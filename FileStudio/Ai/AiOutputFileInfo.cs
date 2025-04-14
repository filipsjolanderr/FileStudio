using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FileStudio.Ai;


public class AiOutputFileInfo
{
    [JsonPropertyName("original_name")] public string OriginalName { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("summary")] public string Summary { get; set; }
}

public class AiOutputFileInfoList
{
    [JsonPropertyName("files")] public List<AiOutputFileInfo> Files { get; set; }
}
