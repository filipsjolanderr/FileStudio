using System.Threading.Tasks;

namespace FileStudio.Ai;

public interface IAiService
{
    Task<string> GenerateResponseAsync(string prompt);
}

public class AiService(IAiClient aiClient) : IAiService
{
    public async Task<string> GenerateResponseAsync(string prompt)
    {
        return await aiClient.GenerateResponseAsync(prompt);
    }
}