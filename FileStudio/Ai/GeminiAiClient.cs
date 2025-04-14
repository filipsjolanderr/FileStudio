using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Mscc.GenerativeAI;

namespace FileStudio.Ai;

public class GeminiAiClient : IAiClient
{
    private readonly GenerativeModel _model;

    public GeminiAiClient(string apiKey)
    {
        var googleAi = new GoogleAI(apiKey: apiKey);
        _model = googleAi.GenerativeModel(model: Model.Gemini20FlashLite);
    }

    public async Task<string> GenerateResponseAsync(string prompt)
    {
        try
        {
            var response = await _model.GenerateContent(prompt);
            return response.Text;
        }
        catch (Exception e)
        {
            return $"An error occurred while generating the response: {e.Message}";
        }
    }
}
