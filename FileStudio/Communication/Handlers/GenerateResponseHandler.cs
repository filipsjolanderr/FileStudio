using System;
using System.Linq;
using System.Threading.Tasks;
using FileStudio.Ai;
using FileStudio.Communication.Messages;

namespace FileStudio.Communication.Handlers
{
    /// <summary>
    /// Handles the GenerateResponseRequest.
    /// </summary>
    public class GenerateResponseHandler : IRequestHandler<GenerateResponseRequest, GenerateResponseResponse>
    {
        private readonly IAiService _aiService;
        private readonly IPromptGenerator _promptGenerator;

        // Inject necessary services
        public GenerateResponseHandler(IAiService aiService, IPromptGenerator promptGenerator)
        {
            _aiService = aiService;
            _promptGenerator = promptGenerator;
        }

        public async Task<GenerateResponseResponse> HandleAsync(GenerateResponseRequest request)
        {
            if (request.CurrentFolder == null || !request.Files.Any())
            {
                return new GenerateResponseResponse(null, false, "Folder not selected or no files loaded.");
            }

            try
            {
                var prompt = _promptGenerator.GeneratePrompt(request.Files, request.SubFolders);
                var generatedResponse = await _aiService.GenerateResponseAsync(prompt);
                return new GenerateResponseResponse(generatedResponse, !string.IsNullOrEmpty(generatedResponse));
            }
            catch (Exception ex)
            {
                // Log the exception details here if a logging service exists
                return new GenerateResponseResponse(null, false, $"Error generating AI response: {ex.Message}");
            }
        }
    }
}
