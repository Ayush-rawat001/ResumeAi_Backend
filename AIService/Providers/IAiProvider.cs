namespace AIService.Providers
{
    public interface IAiProvider
    {
        Task<string> GenerateAsync(string prompt);
    }
}
