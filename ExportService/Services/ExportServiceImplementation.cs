using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ExportService.DTOs;
using System.Linq;

namespace ExportService.Services
{
    public class ExportServiceImplementation : IExportService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExportServiceImplementation(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<byte[]> ExportResumePdf(int resumeId)
        {
            var token = GetBearerToken();
            if (string.IsNullOrEmpty(token)) throw new UnauthorizedAccessException("Missing Authorization header");

            var resumeClient = _httpClientFactory.CreateClient("ResumeService");
            resumeClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var resumeResponse = await resumeClient.GetAsync($"/api/resumes/{resumeId}");
            if (!resumeResponse.IsSuccessStatusCode) throw new Exception("Resume not found");

            var wrappedResume = await resumeResponse.Content.ReadFromJsonAsync<ApiResponse<ResumeDataDto>>();
            if (wrappedResume?.Data?.Resume == null) throw new Exception("Resume data is null");

            var resume = wrappedResume.Data.Resume;
            var sections = wrappedResume.Data.Sections ?? new List<SectionDto>();

            TemplateResponseDto? template = null;
            if (resume.TemplateId > 0)
            {
                var templateClient = _httpClientFactory.CreateClient("TemplateService");
                var templateResponse = await templateClient.GetAsync($"/api/templates/{resume.TemplateId}");
                if (templateResponse.IsSuccessStatusCode)
                {
                    var wrappedTemplate = await templateResponse.Content.ReadFromJsonAsync<ApiResponse<TemplateResponseDto>>();
                    template = wrappedTemplate?.Data;
                }
            }

            try 
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        
                        if (template?.Name == "Professional Executive")
                        {
                            ComposeExecutiveLayout(page, resume, sections);
                        }
                        else if (template?.Name == "Creative Vibrant")
                        {
                            ComposeVibrantLayout(page, resume, sections);
                        }
                        else if (template?.Name == "Modern Minimalist")
                        {
                            ComposeModernLayout(page, resume, sections);
                        }
                        else if (template?.Name == "Classic Royal")
                        {
                            ComposeClassicRoyalLayout(page, resume, sections);
                        }
                        else
                        {
                            ComposeDefaultLayout(page, resume, sections);
                        }
                        
                        page.Footer().Element(ComposeFooter);
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF Generation error: {ex.Message}");
                // Return a very safe fallback if everything else fails
                return Document.Create(c => c.Page(p => { 
                    p.Margin(2, Unit.Centimetre); 
                    p.Content().Column(col => {
                        col.Item().Text("Export Error").FontSize(20).Bold().FontColor(Colors.Red.Medium);
                        col.Item().PaddingTop(10).Text(ex.Message);
                    });
                })).GeneratePdf();
            }
        }

        private void ComposeDefaultLayout(PageDescriptor page, ResumeDto resume, List<SectionDto> sections)
        {
            page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));
            page.Header().Element(c => ComposeHeader(c, resume, sections, Colors.Grey.Darken4, Colors.Blue.Lighten2));
            page.Content().Element(c => ComposeContentStandard(c, resume, sections, Colors.Blue.Darken2));
        }

        private void ComposeModernLayout(PageDescriptor page, ResumeDto resume, List<SectionDto> sections)
        {
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Verdana"));
            page.Header().PaddingVertical(20).PaddingHorizontal(30).Column(col => {
                var name = GetName(sections, resume);
                col.Item().Text(name).FontSize(28).ExtraBold().FontColor(Colors.Blue.Darken2);
                col.Item().Text(resume.TargetJobTitle).FontSize(14).FontColor(Colors.Grey.Darken1);
                col.Item().PaddingTop(5).Text(GetContactString(sections)).FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
            page.Content().PaddingHorizontal(30).Element(c => ComposeContentStandard(c, resume, sections, Colors.Blue.Darken2));
        }

        private void ComposeExecutiveLayout(PageDescriptor page, ResumeDto resume, List<SectionDto> sections)
        {
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Times New Roman"));
            
            page.Content().Row(row => {
                row.ConstantItem(180).Column(col => {
                    col.Item().Background(Colors.Grey.Darken4).Padding(20).Column(sidebarCol => {
                        sidebarCol.Spacing(20);
                        sidebarCol.Item().Text("CONTACT").FontSize(12).SemiBold().FontColor(Colors.White);
                        sidebarCol.Item().Text(GetContactString(sections, true)).FontSize(9).FontColor(Colors.Grey.Lighten3);
                        
                        var skills = sections.FirstOrDefault(s => s.SectionType == "SKILLS");
                        if (skills != null) {
                            sidebarCol.Item().Text("SKILLS").FontSize(12).SemiBold().FontColor(Colors.White);
                            BuildSkills(sidebarCol.Item(), skills.Content, Colors.Grey.Lighten4);
                        }
                    });
                });

                row.RelativeItem().Padding(30).Column(col => {
                    col.Item().Text(GetName(sections, resume)).FontSize(32).Bold();
                    col.Item().Text(resume.TargetJobTitle).FontSize(16).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(20).Element(c => ComposeContentMainOnly(c, resume, sections, Colors.Black));
                });
            });
        }

        private void ComposeVibrantLayout(PageDescriptor page, ResumeDto resume, List<SectionDto> sections)
        {
            page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));
            page.Header().Background(Colors.Red.Darken1).Padding(30).AlignCenter().Column(col => {
                col.Item().Text(GetName(sections, resume)).FontSize(32).ExtraBold().FontColor(Colors.White);
                col.Item().Text(resume.TargetJobTitle).FontSize(14).FontColor(Colors.White);
            });
            page.Content().Padding(30).Element(c => ComposeContentStandard(c, resume, sections, Colors.Red.Darken2));
        }

        private void ComposeClassicRoyalLayout(PageDescriptor page, ResumeDto resume, List<SectionDto> sections)
        {
            page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Times New Roman"));
            
            page.Header().Column(col => {
                col.Item().AlignCenter().Text(GetName(sections, resume)).FontSize(32).Bold();
                col.Item().AlignCenter().Text(resume.TargetJobTitle).FontSize(14).Italic().FontColor(Colors.Grey.Darken1);
                col.Item().AlignCenter().PaddingTop(5).Text(GetContactString(sections)).FontSize(10).FontColor(Colors.Grey.Darken2);
                col.Item().PaddingVertical(10).LineHorizontal(2).LineColor(Colors.Grey.Lighten1);
            });

            page.Content().Column(col => {
                var summary = sections.FirstOrDefault(s => s.SectionType == "SUMMARY");
                if (summary != null) {
                    col.Item().PaddingTop(10).Text("PROFESSIONAL SUMMARY").FontSize(12).Bold();
                    col.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(5).Text(ParseSummary(summary.Content)).FontSize(10);
                }

                var exp = sections.FirstOrDefault(s => s.SectionType == "EXPERIENCE");
                if (exp != null) {
                    col.Item().PaddingTop(20).Text("PROFESSIONAL EXPERIENCE").FontSize(12).Bold();
                    col.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    BuildExperience(col.Item(), exp.Content, Colors.Black);
                }

                var edu = sections.FirstOrDefault(s => s.SectionType == "EDUCATION");
                if (edu != null) {
                    col.Item().PaddingTop(20).Text("EDUCATION").FontSize(12).Bold();
                    col.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    BuildEducation(col.Item(), edu.Content);
                }

                col.Item().PaddingTop(20).Row(row => {
                    row.RelativeItem().Column(subCol => {
                        var skills = sections.FirstOrDefault(s => s.SectionType == "SKILLS");
                        if (skills != null) {
                            subCol.Item().Text("SKILLS").FontSize(12).Bold();
                            subCol.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            BuildSkills(subCol.Item(), skills.Content, Colors.Black);
                        }

                        var lang = sections.FirstOrDefault(s => s.SectionType == "LANGUAGES");
                        if (lang != null) {
                            subCol.Item().PaddingTop(15).Text("LANGUAGES").FontSize(12).Bold();
                            subCol.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            BuildLanguages(subCol.Item(), lang.Content);
                        }
                    });

                    row.ConstantItem(40);

                    row.RelativeItem().Column(subCol => {
                        var cert = sections.FirstOrDefault(s => s.SectionType == "CERTIFICATIONS");
                        if (cert != null) {
                            subCol.Item().Text("CERTIFICATIONS").FontSize(12).Bold();
                            subCol.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            BuildCertifications(subCol.Item(), cert.Content);
                        }

                        var vol = sections.FirstOrDefault(s => s.SectionType == "VOLUNTEER");
                        if (vol != null) {
                            subCol.Item().PaddingTop(15).Text("VOLUNTEER").FontSize(12).Bold();
                            subCol.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            BuildVolunteer(subCol.Item(), vol.Content, Colors.Black);
                        }
                    });
                });

                var proj = sections.FirstOrDefault(s => s.SectionType == "PROJECTS");
                if (proj != null) {
                    col.Item().PaddingTop(20).Text("NOTABLE PROJECTS").FontSize(12).Bold();
                    col.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    BuildProjects(col.Item(), proj.Content);
                }
            });
        }
        private void ComposeContentStandard(IContainer container, ResumeDto resume, List<SectionDto> sections, string accentColor)
        {
            container.PaddingVertical(10).Column(col => {
                col.Item().Row(row => {
                    row.RelativeItem().Column(mainCol => {
                        var summary = sections.FirstOrDefault(s => s.SectionType == "SUMMARY");
                        if (summary != null) {
                            mainCol.Item().PaddingBottom(5).Text("SUMMARY").FontSize(14).SemiBold().FontColor(accentColor);
                            mainCol.Item().Text(ParseSummary(summary.Content)).FontSize(10);
                        }
                        
                        var exp = sections.FirstOrDefault(s => s.SectionType == "EXPERIENCE");
                        if (exp != null) {
                            mainCol.Item().PaddingTop(20).PaddingBottom(5).Text("EXPERIENCE").FontSize(14).SemiBold().FontColor(accentColor);
                            BuildExperience(mainCol.Item(), exp.Content, accentColor);
                        }
                    });
                    
                    row.ConstantItem(20);
                    
                    row.ConstantItem(150).Column(sideCol => {
                        var edu = sections.FirstOrDefault(s => s.SectionType == "EDUCATION");
                        if (edu != null) {
                            sideCol.Item().PaddingBottom(5).Text("EDUCATION").FontSize(14).SemiBold().FontColor(accentColor);
                            BuildEducation(sideCol.Item(), edu.Content);
                        }

                        var skills = sections.FirstOrDefault(s => s.SectionType == "SKILLS");
                        if (skills != null) {
                            sideCol.Item().PaddingTop(20).PaddingBottom(5).Text("SKILLS").FontSize(14).SemiBold().FontColor(accentColor);
                            BuildSkills(sideCol.Item(), skills.Content, Colors.Black);
                        }

                        var lang = sections.FirstOrDefault(s => s.SectionType == "LANGUAGES");
                        if (lang != null) {
                            sideCol.Item().PaddingTop(20).PaddingBottom(5).Text("LANGUAGES").FontSize(14).SemiBold().FontColor(accentColor);
                            BuildLanguages(sideCol.Item(), lang.Content);
                        }
                    });
                });

                var proj = sections.FirstOrDefault(s => s.SectionType == "PROJECTS");
                if (proj != null) {
                    col.Item().PaddingTop(20).PaddingBottom(5).Text("PROJECTS").FontSize(14).SemiBold().FontColor(accentColor);
                    BuildProjects(col.Item(), proj.Content);
                }

                var cert = sections.FirstOrDefault(s => s.SectionType == "CERTIFICATIONS");
                if (cert != null) {
                    col.Item().PaddingTop(20).PaddingBottom(5).Text("CERTIFICATIONS").FontSize(14).SemiBold().FontColor(accentColor);
                    BuildCertifications(col.Item(), cert.Content);
                }

                var vol = sections.FirstOrDefault(s => s.SectionType == "VOLUNTEER");
                if (vol != null) {
                    col.Item().PaddingTop(20).PaddingBottom(5).Text("VOLUNTEER").FontSize(14).SemiBold().FontColor(accentColor);
                    BuildVolunteer(col.Item(), vol.Content, accentColor);
                }
                });
        }

        private void ComposeContentMainOnly(IContainer container, ResumeDto resume, List<SectionDto> sections, string accentColor)
        {
             container.Column(col => {
                    var summary = sections.FirstOrDefault(s => s.SectionType == "SUMMARY");
                    if (summary != null) {
                        col.Item().PaddingBottom(5).Text("SUMMARY").FontSize(14).SemiBold().FontColor(accentColor);
                        col.Item().Text(ParseSummary(summary.Content)).FontSize(10);
                    }
                    
                    var exp = sections.FirstOrDefault(s => s.SectionType == "EXPERIENCE");
                    if (exp != null) {
                        col.Item().PaddingTop(20).PaddingBottom(5).Text("EXPERIENCE").FontSize(14).SemiBold().FontColor(accentColor);
                        BuildExperience(col.Item(), exp.Content, accentColor);
                    }

                    var edu = sections.FirstOrDefault(s => s.SectionType == "EDUCATION");
                    if (edu != null) {
                        col.Item().PaddingTop(20).PaddingBottom(5).Text("EDUCATION").FontSize(14).SemiBold().FontColor(accentColor);
                        BuildEducation(col.Item(), edu.Content);
                    }
             });
        }

        private string GetName(List<SectionDto> sections, ResumeDto resume)
        {
            var personalInfo = sections.FirstOrDefault(s => s.SectionType == "CUSTOM");
            if (personalInfo != null && !string.IsNullOrWhiteSpace(personalInfo.Content))
            {
                try {
                    var doc = JsonDocument.Parse(personalInfo.Content);
                    if (doc.RootElement.TryGetProperty("fullName", out var n)) return n.GetString() ?? resume.Title;
                } catch { }
            }
            return resume.Title;
        }

        private string GetContactString(List<SectionDto> sections, bool multiline = false)
        {
            var personalInfo = sections.FirstOrDefault(s => s.SectionType == "CUSTOM");
            if (personalInfo != null && !string.IsNullOrWhiteSpace(personalInfo.Content))
            {
                try {
                    var doc = JsonDocument.Parse(personalInfo.Content);
                    var e = doc.RootElement.TryGetProperty("email", out var ev) ? ev.GetString() : "";
                    var p = doc.RootElement.TryGetProperty("phone", out var pv) ? pv.GetString() : "";
                    return multiline ? $"{e}\n{p}" : $"{e}  •  {p}";
                } catch { }
            }
            return "";
        }

        private string GetBearerToken()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }
            return string.Empty;
        }

        private void ComposeHeader(IContainer container, ResumeDto resume, List<SectionDto> sections, string bgColor, string accentColor)
        {
            container.Background(bgColor).Padding(30).Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(GetName(sections, resume)).FontSize(32).SemiBold().FontColor(Colors.White);
                    column.Item().PaddingTop(5).Text(resume.TargetJobTitle).FontSize(16).FontColor(accentColor);
                });

                row.ConstantItem(200).AlignRight().Column(column =>
                {
                    column.Item().Text(GetContactString(sections)).FontSize(10).FontColor(Colors.Grey.Lighten2);
                });
            });
        }

        private string ParseSummary(string jsonContent)
        {
            try
            {
                var doc = JsonDocument.Parse(jsonContent);
                if (doc.RootElement.TryGetProperty("summary", out var summaryElement))
                {
                    return summaryElement.GetString() ?? string.Empty;
                }
                return doc.RootElement.ToString();
            }
            catch
            {
                return jsonContent;
            }
        }

        private void BuildExperience(IContainer container, string jsonContent, string accentColor)
        {
            try
            {
                var doc = JsonDocument.Parse(jsonContent);
                container.Column(col =>
                {
                    col.Spacing(15);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in doc.RootElement.EnumerateArray())
                        {
                            var company = item.TryGetProperty("company", out var c) ? c.GetString() : "Company";
                            var title = item.TryGetProperty("title", out var t) ? t.GetString() : "Title";
                            var dates = item.TryGetProperty("dates", out var d) ? d.GetString() : "";
                            var desc = item.TryGetProperty("description", out var de) ? de.GetString() : "";

                            col.Item().Column(jobCol =>
                            {
                                jobCol.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(title).FontSize(12).SemiBold().FontColor(Colors.Black);
                                    r.ConstantItem(100).AlignRight().Text(dates).FontSize(10).FontColor(accentColor);
                                });
                                jobCol.Item().Text(company).FontSize(11).SemiBold().FontColor(Colors.Grey.Darken2);
                                if (!string.IsNullOrEmpty(desc))
                                {
                                    jobCol.Item().PaddingTop(4).Text(desc).FontSize(10).FontColor(Colors.Grey.Darken3).LineHeight(1.4f);
                                }
                            });
                        }
                    }
                });
            }
            catch { }
        }

        private void BuildEducation(IContainer container, string jsonContent)
        {
            try
            {
                var doc = JsonDocument.Parse(jsonContent);
                container.Column(col =>
                {
                    col.Spacing(10);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in doc.RootElement.EnumerateArray())
                        {
                            var school = item.TryGetProperty("school", out var s) ? s.GetString() : "School";
                            var degree = item.TryGetProperty("degree", out var d) ? d.GetString() : "Degree";
                            var year = item.TryGetProperty("year", out var y) ? y.GetString() : "";

                            col.Item().Column(eduCol =>
                            {
                                eduCol.Item().Text(degree).FontSize(11).SemiBold().FontColor(Colors.Black);
                                eduCol.Item().Text(school).FontSize(10).FontColor(Colors.Grey.Darken2);
                                if (!string.IsNullOrEmpty(year)) eduCol.Item().Text(year).FontSize(10).FontColor(Colors.Blue.Darken1);
                            });
                        }
                    }
                });
            }
            catch { }
        }

        private void BuildCertifications(IContainer container, string jsonContent)
        {
            try
            {
                var doc = JsonDocument.Parse(jsonContent);
                container.Column(col =>
                {
                    col.Spacing(5);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in doc.RootElement.EnumerateArray())
                        {
                            var title = item.TryGetProperty("title", out var t) ? t.GetString() : "";
                            var issuer = item.TryGetProperty("issuer", out var i) ? i.GetString() : "";
                            col.Item().Text(x => {
                                x.Span(title).Bold();
                                x.Span(" - " + issuer).FontColor(Colors.Grey.Darken1);
                            });
                        }
                    }
                });
            }
            catch { }
        }

        private void BuildProjects(IContainer container, string jsonContent)
        {
            try
            {
                var doc = JsonDocument.Parse(jsonContent);
                container.Column(col =>
                {
                    col.Spacing(10);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in doc.RootElement.EnumerateArray())
                        {
                            var title = item.TryGetProperty("title", out var t) ? t.GetString() : "";
                            var tech = item.TryGetProperty("technologies", out var tc) ? tc.GetString() : "";
                            var desc = item.TryGetProperty("description", out var de) ? de.GetString() : "";

                            col.Item().Column(pCol => {
                                pCol.Item().Text(title).Bold().FontSize(11);
                                if (!string.IsNullOrEmpty(tech)) pCol.Item().Text(tech).FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                                if (!string.IsNullOrEmpty(desc)) pCol.Item().PaddingTop(2).Text(desc).FontSize(10);
                            });
                        }
                    }
                });
            }
            catch { }
        }

        private void BuildLanguages(IContainer container, string jsonContent)
        {
            try
            {
                var doc = JsonDocument.Parse(jsonContent);
                container.Column(col =>
                {
                    col.Spacing(4);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in doc.RootElement.EnumerateArray())
                        {
                            var lang = item.TryGetProperty("language", out var l) ? l.GetString() : "";
                            var level = item.TryGetProperty("level", out var lv) ? lv.GetString() : "";
                            col.Item().Text($"{lang}: {level}").FontSize(10);
                        }
                    }
                });
            }
            catch { }
        }

        private void BuildVolunteer(IContainer container, string jsonContent, string accentColor)
        {
            try
            {
                var doc = JsonDocument.Parse(jsonContent);
                container.Column(col =>
                {
                    col.Spacing(10);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in doc.RootElement.EnumerateArray())
                        {
                            var org = item.TryGetProperty("organization", out var o) ? o.GetString() : "";
                            var role = item.TryGetProperty("role", out var r) ? r.GetString() : "";
                            var dates = item.TryGetProperty("dates", out var d) ? d.GetString() : "";
                            var desc = item.TryGetProperty("description", out var de) ? de.GetString() : "";

                            col.Item().Column(vCol => {
                                vCol.Item().Row(row => {
                                    row.RelativeItem().Text(org).Bold();
                                    row.ConstantItem(100).AlignRight().Text(dates).FontSize(10).FontColor(accentColor);
                                });
                                vCol.Item().Text(role).Italic().FontSize(10).FontColor(Colors.Grey.Darken2);
                                if (!string.IsNullOrEmpty(desc)) vCol.Item().PaddingTop(2).Text(desc).FontSize(10);
                            });
                        }
                    }
                });
            }
            catch { }
        }

        private void BuildSkills(IContainer container, string jsonContent, string fontColor)
        {
            try
            {
                var doc = JsonDocument.Parse(jsonContent);
                container.Column(col =>
                {
                    col.Spacing(4);
                    JsonElement skills;
                    if (doc.RootElement.ValueKind == JsonValueKind.Array) skills = doc.RootElement;
                    else if (doc.RootElement.TryGetProperty("skills", out var s)) skills = s;
                    else return;

                    foreach (var item in skills.EnumerateArray())
                    {
                        col.Item().Text("• " + item.GetString()).FontSize(9).FontColor(fontColor);
                    }
                });
            }
            catch { }
        }

        private void ComposeFooter(IContainer container)
        {
            container.PaddingHorizontal(30).PaddingBottom(20).AlignRight().Text(x =>
            {
                x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Lighten1);
                x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Lighten1);
            });
        }
    }
}
