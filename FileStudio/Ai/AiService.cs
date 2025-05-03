using System.Threading.Tasks;
using FileStudio.Repositories; // Added repository namespace

namespace FileStudio.Ai;

// Interface remains the same, defining the service contract
public interface IAiService
{
    Task<string> GenerateResponseAsync(string prompt);
}

// Service now depends on the repository interface
public class AiService : IAiService
{
    private readonly IAiRepository _aiRepository;

    // Constructor injection for IAiRepository
    public AiService(IAiRepository aiRepository)
    {
        _aiRepository = aiRepository;
    }

    // Delegate the call to the injected repository
    public async Task<string> GenerateResponseAsync(string prompt)
    {
        return await _aiRepository.GenerateAiResponseAsync(prompt);
    }
}
