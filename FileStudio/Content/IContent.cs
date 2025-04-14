using System.Collections.Generic;
using FileStudio.FileManagement;

namespace FileStudio.Content;

public interface IContent
{
    public List<string> GetContent(string path);
}