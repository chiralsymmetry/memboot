namespace MemBoot.Core;

public class Resource
{
    public Guid Id { get; set; }
    public string Path { get; set; }
    public string OriginalPath { get; set; }
    public Resource(Guid id, string path, string originalPath)
    {
        Id = id;
        Path = path;
        OriginalPath = originalPath;
    }
}
