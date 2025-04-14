using System.Threading.Tasks;

namespace FileStudio.Ai;

public interface IAiClient
{
    Task<string> GenerateResponseAsync(string prompt);
}