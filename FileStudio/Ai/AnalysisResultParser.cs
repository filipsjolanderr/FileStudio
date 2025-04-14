namespace FileStudio.Ai;

using System;
using System.Text.Json;
using System.Collections.Generic; // If not already included via other DTOs

public class AnalysisResultParser
{
    // Optional: Configure options if needed (e.g., case insensitivity, though
    // JsonPropertyName makes it explicit and preferred)
    private static readonly JsonSerializerOptions _deserializeOptions = new JsonSerializerOptions
    {
        // If the AI *might* not strictly adhere to snake_case, this can help,
        // but relying on JsonPropertyName is more robust.
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Deserializes the JSON string output from the file analysis AI.
    /// </summary>
    /// <param name="aiJsonOutput">The JSON string received from the AI.</param>
    /// <returns>An AnalysisOutputPayload object, or null if deserialization fails.</returns>
    public AnalysisOutputPayload DeserializeAnalysisResult(string aiJsonOutput)
    {
        if (string.IsNullOrWhiteSpace(aiJsonOutput))
        {
            Console.Error.WriteLine("Error: AI output JSON string is null or empty.");
            return null;
        }

        try
        {
            // Attempt to deserialize the JSON into our defined C# object structure
            AnalysisOutputPayload result = JsonSerializer.Deserialize<AnalysisOutputPayload>(aiJsonOutput, _deserializeOptions);

            // Basic validation: Check if the top-level 'folders' list was parsed
            if (result?.Folders == null)
            {
                Console.Error.WriteLine("Warning: JSON deserialized, but the 'folders' array is missing or null.");
                // Depending on requirements, you might still return the 'result'
                // or return null if 'folders' is mandatory. Let's return result for now.
            }

            return result;
        }
        catch (JsonException jsonEx)
        {
            // Handle errors specifically related to JSON parsing
            Console.Error.WriteLine($"Error deserializing AI output JSON: {jsonEx.Message}");
            Console.Error.WriteLine($"Problematic JSON snippet (check logs for full): {aiJsonOutput.Substring(0, Math.Min(aiJsonOutput.Length, 100))}"); // Log safely
            return null;
        }
        catch (Exception ex)
        {
            // Handle any other unexpected errors during deserialization
            Console.Error.WriteLine($"An unexpected error occurred during deserialization: {ex.Message}");
            return null;
        }
    }
}