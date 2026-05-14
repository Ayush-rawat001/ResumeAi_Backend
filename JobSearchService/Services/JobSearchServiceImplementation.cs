using System.Net.Http.Json;
using JobSearchService.Data;
using JobSearchService.DTOs;
using JobSearchService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobSearchService.Services
{
    public class JobSearchServiceImplementation : IJobSearchService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<JobSearchServiceImplementation> _logger;

        public JobSearchServiceImplementation(AppDbContext context, IHttpClientFactory httpClientFactory, ILogger<JobSearchServiceImplementation> logger)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient("HimalayasApi");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ResumeAI-JobMatcher/1.0");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _logger = logger;
        }

        public async Task<ApiResponse<List<JobSearchResultDto>>> SearchAndMatchJobs(int userId, AnalyzeJobFitRequest request)
        {
            try
            {
                var userSkills = request.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToLower())
                    .ToList();

                if (!userSkills.Any())
                    return ApiResponse<List<JobSearchResultDto>>.Fail("No skills provided for matching.");

                // Broaden search: search for each of the top 3 skills individually and combine results
                var allJobs = new List<HimalayasJob>();
                var searchSkills = userSkills.Take(3).ToList();
                
                // Add dotnet if .net is present
                if (userSkills.Contains(".net") && !searchSkills.Contains("dotnet"))
                    searchSkills.Add("dotnet");

                foreach (var skill in searchSkills)
                {
                    try 
                    {
                        var searchUrl = $"https://himalayas.app/jobs/api/search?q={Uri.EscapeDataString(skill)}";
                        var searchResponse = await _httpClient.GetFromJsonAsync<HimalayasJobResponse>(searchUrl);
                        
                        if (searchResponse?.Jobs != null)
                        {
                            allJobs.AddRange(searchResponse.Jobs);
                        }
                    }
                    catch (Exception ex) { _logger.LogWarning($"Failed to search for skill: {skill}. {ex.Message}"); }
                }

                // Deduplicate by ApplicationLink
                var uniqueJobs = allJobs.GroupBy(j => j.ApplicationLink).Select(g => g.First()).ToList();

                if (!uniqueJobs.Any())
                    return ApiResponse<List<JobSearchResultDto>>.Ok(new List<JobSearchResultDto>(), "No jobs found for these skills.");

                var results = new List<JobSearchResultDto>();

                foreach (var job in uniqueJobs)
                {
                    // Clean up job excerpt (remove HTML tags if any)
                    var excerptLower = (job.Excerpt ?? "").ToLower();
                    var titleLower = job.Title.ToLower();
                    
                    var matchingSkills = new List<string>();
                    var missingSkills = new List<string>();

                    foreach (var skill in userSkills)
                    {
                        if (titleLower.Contains(skill) || excerptLower.Contains(skill))
                        {
                            matchingSkills.Add(skill);
                        }
                        else
                        {
                            missingSkills.Add(skill);
                        }
                    }

                    // Combined weighted score
                    int titleMatchCount = userSkills.Count(s => titleLower.Contains(s));
                    int excerptMatchCount = userSkills.Count(s => excerptLower.Contains(s));
                    
                    int finalScore = (titleMatchCount * 40) + (excerptMatchCount * 10);
                    finalScore = Math.Min(100, finalScore);

                    // Skip if no match at all
                    if (finalScore == 0) continue;

                    results.Add(new JobSearchResultDto
                    {
                        Title = job.Title,
                        Company = job.CompanyName,
                        Url = job.ApplicationLink,
                        Location = job.Location,
                        MatchScore = finalScore,
                        MatchingSkills = matchingSkills,
                        MissingSkills = missingSkills
                    });
                }

                // Return top 5 matches
                var topMatches = results.OrderByDescending(r => r.MatchScore).Take(5).ToList();

                // Save these matches to DB for history
                foreach (var match in topMatches)
                {
                    var entity = new JobMatch
                    {
                        UserId = userId,
                        ResumeId = request.ResumeId,
                        JobTitle = match.Title,
                        CompanyName = match.Company,
                        JobUrl = match.Url,
                        MatchScore = match.MatchScore,
                        MissingSkills = string.Join(", ", match.MissingSkills),
                        MatchedAt = DateTime.UtcNow
                    };
                    _context.JobMatches.Add(entity);
                }
                await _context.SaveChangesAsync();

                return ApiResponse<List<JobSearchResultDto>>.Ok(topMatches, "Found top job matches based on your skills.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching jobs on Himalayas");
                return ApiResponse<List<JobSearchResultDto>>.Fail("Failed to fetch jobs from external provider.");
            }
        }

        public async Task<ApiResponse<List<JobMatch>>> GetSavedMatches(int userId)
        {
            var matches = await _context.JobMatches
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.MatchedAt)
                .ToListAsync();
            return ApiResponse<List<JobMatch>>.Ok(matches);
        }

        public async Task<ApiResponse<bool>> BookmarkMatch(int matchId)
        {
            var match = await _context.JobMatches.FindAsync(matchId);
            if (match == null) return ApiResponse<bool>.Fail("Match not found.");

            match.IsBookmarked = !match.IsBookmarked;
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(match.IsBookmarked, "Bookmark updated.");
        }
    }
}
