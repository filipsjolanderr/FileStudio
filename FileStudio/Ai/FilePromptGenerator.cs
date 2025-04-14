using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json; // Required for JSON serialization
using System.Text.Json.Serialization;
using FileStudio.FileManagement; // Required for attributes like JsonPropertyName

namespace FileStudio.Ai;

// Define DTOs (Data Transfer Objects) for clarity and serialization

// Represents a single file in the input JSON
public class FileInputData
{
    [JsonPropertyName("name")] // Ensures the JSON property name is "name"
    public string Name { get; set; }

    [JsonPropertyName("type")] // Ensures the JSON property name is "type"
    public string Type { get; set; }

    // Include "text_content" only if it's not null
    [JsonPropertyName("text_content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string TextContent { get; set; }
}

// Represents the top-level structure of the input JSON
public class InputPayload
{
    [JsonPropertyName("files")]
    public List<FileInputData> Files { get; set; }
}


// Represents a single processed file's information in the output
public class OutputFileResult
{
    [JsonPropertyName("original_name")] // Maps JSON "original_name" to this property
    public string OriginalName { get; set; }

    [JsonPropertyName("new_name")] // Maps JSON "new_name" to this property
    public string NewName { get; set; }

    [JsonPropertyName("summary")] // Maps JSON "summary" to this property
    public string Summary { get; set; }
}

// Represents a folder containing processed files in the output
public class OutputFolderResult
{
    [JsonPropertyName("folder_name")] // Maps JSON "folder_name" to this property
    public string FolderName { get; set; }

    [JsonPropertyName("files")] // Maps JSON "files" array to this list
    public List<OutputFileResult> Files { get; set; }
}

// Represents the top-level structure of the expected JSON output
public class AnalysisOutputPayload
{
    [JsonPropertyName("folders")] // Maps JSON "folders" array to this list
    public List<OutputFolderResult> Folders { get; set; }
}


public class FilePromptGenerator : IPromptGenerator
{
    // Configure JSON serialization options (optional but good practice)
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true, // Makes the JSON in the prompt easier to read
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, // Use this if your C# props are PascalCase but you need snake_case JSON
        // Since we used JsonPropertyName, we don't strictly need the policy here for this specific case.
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Handles the optional text_content
    };

    public string GeneratePrompt(List<CustomStorageFile> fileInfoList)
    {
        // 1. Map the input List<FileInfo> to the DTO structure
        var inputPayload = new InputPayload
        {
            Files = fileInfoList.Select(f => new FileInputData
            {
                Name = f.Name,
                Type = f.Type,
                TextContent = f.TextContent
            }).ToList()
        };

        // 2. Serialize the input data DTO to a JSON string
        var inputJson = JsonSerializer.Serialize(inputPayload, JsonSerializerOptions);

        // 3. Build the prompt using StringBuilder and improved formatting
        var prompt = new StringBuilder();

        // Use Markdown for better structure within the prompt
        prompt.AppendLine("## Role: File Analysis Assistant");
        prompt.AppendLine("You are an AI assistant specializing in file analysis and organization.");
        prompt.AppendLine();
        prompt.AppendLine("## Task");
        prompt.AppendLine("Analyze the list of files provided in the JSON input below. For each file:");
        prompt.AppendLine("* Generate a concise summary of its content.");
        prompt.AppendLine("* Suggest an improved, descriptive file name (keeping the original extension if applicable).");
        prompt.AppendLine("* Categorize the file into a new folder structure based on content, type, or name.");
        prompt.AppendLine("* Ensure you keep track of the original file name.");
        prompt.AppendLine();
        prompt.AppendLine("## Guidelines");
        prompt.AppendLine("* **Summarization:** If `text_content` is available, base the summary on it. Otherwise, infer the summary from the `name` and `type`.");
        prompt.AppendLine("* **Categorization:** Create meaningful and specific folder names. Group related files together.");
        prompt.AppendLine("* **Output:** Your response **MUST** be a single, valid JSON object conforming to the specified output format. Do not include any introductory text, explanations, or comments outside the JSON structure.");
        prompt.AppendLine();
        prompt.AppendLine("## Input Data Format");
        prompt.AppendLine("The file information is provided as a JSON object with a `files` array:");
        prompt.AppendLine("```json");
        prompt.AppendLine(inputJson); // Inject the cleanly serialized JSON input
        prompt.AppendLine("```");
        prompt.AppendLine();
        prompt.AppendLine("## Required Output JSON Format");
        prompt.AppendLine("Provide your analysis in the following JSON structure ONLY:");
        // Using verbatim string literal for the example structure
        prompt.Append(@"```json
{
  ""folders"": [
    {
      ""folder_name"": ""<Generated Folder Name>"",
      ""files"": [
        {
          ""original_name"": ""<Original File Name>"",
          ""new_name"": ""<Suggested New File Name With Extension>"",
          ""summary"": ""<Generated Summary of File Content>""
        }
        // ... potentially more files in this folder
      ]
    }
    // ... potentially more folders
  ]
}
```");

        return prompt.ToString();
    }


}
