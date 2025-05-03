using System.Threading.Tasks;

namespace FileStudio.Repositories;

/// <summary>
/// Defines the contract for repositories that handle AI model interactions.
/// </summary>
public interface IAiRepository
{
    /// <summary>
    /// Asynchronously generates a response from the AI model based on the provided prompt.
    /// </summary>
    /// <param name="prompt">The input prompt for the AI model.</param>
    /// <returns>A task representing the asynchronous operation, containing the AI-generated response string.</returns>
    Task<string> GenerateAiResponseAsync(string prompt);
}
