using System.Collections.Generic;

namespace Maggsoft.Core.IO.Model;

public class FileManagerResponse
{
    public FileManagerDirectoryContent CWD { get; set; }
    public IEnumerable<FileManagerDirectoryContent> Files { get; set; }
    public ErrorProperty Error { get; set; }
    public FileDetails Details { get; set; }
}
