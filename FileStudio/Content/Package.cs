using System;
using System.Collections.Generic;
using System.Linq;
using FileStudio.FileManagement;
using s4pi.Interfaces;
using s4pi.Package;

namespace FileStudio.Content;

public class PackageContent : IContent
{ 
    public List<string> GetContent(string path)
    {
        var package = Package.OpenPackage(1, path);

        return package.ContentFields.ToList();
    }
}