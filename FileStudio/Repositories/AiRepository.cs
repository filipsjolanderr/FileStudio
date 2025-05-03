using System.Threading.Tasks;
using FileStudio.Ai; // Assuming IAiClient is in this namespace

namespace FileStudio.Repositories;

/// <summary>
/// Concrete implementation of IAiRepository for handling AI model interactions.
/// </summary>
public class AiRepository : IAiRepository
{
    private readonly IAiClient _aiClient;

    // Constructor injection for IAiClient
    public AiRepository(IAiClient aiClient)
    {
        _aiClient = aiClient;
    }

    /// <inheritdoc />
    public async Task<string> GenerateAiResponseAsync(string prompt)
    {
        // Delegate the call to the injected AI client
        return await _aiClient.GenerateResponseAsync(prompt);
    }
}
