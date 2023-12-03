using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Maggsoft.Core.IO.Model;

public class FEParams
{
    public string Action { get; set; }
    public string Path { get; set; }
    public string TargetPath { get; set; }
    public bool ShowHiddenItems { get; set; }
    public string[] ItemNames { get; set; }
    public string Name { get; set; }
    public bool CaseSensitive { get; set; }
    public string[] CommonFiles { get; set; }
    public string SearchString { get; set; }
    public string ItemNewName { get; set; }
    public IList<IFormFile> UploadFiles { get; set; }
    public object[] Data { get; set; }
}
