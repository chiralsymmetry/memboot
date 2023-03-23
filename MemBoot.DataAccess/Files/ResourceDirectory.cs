using MemBoot.Core;
using MemBoot.Core.Models;
using System.Diagnostics;

namespace MemBoot.DataAccess.Files;

public static class ResourceDirectory
{
    // TODO: Manage a resource directory.
    private static readonly string resourceDirectory = Path.GetFullPath("Resources");

    public static string GetNewPath(Guid id, string oldPath)
    {
        var guidName = id.ToString().Replace("-", "");
        return Path.Combine(resourceDirectory, Path.ChangeExtension(guidName, Path.GetExtension(oldPath)));
    }

    public static Resource CreateResourceAndAdd(Deck? deck, string path)
    {
        var id = Guid.NewGuid();
        while (deck?.Resources.ContainsKey(id) == true)
        {
            id = Guid.NewGuid();
        }
        var newPath = GetNewPath(id, path);
        Resource output = new(id, newPath, path);
        deck?.Resources.Add(id, output);
        return output;
    }

    public static string GetAbsolutePath(Resource resource)
    {
        return Path.GetFullPath(resource.Path);
    }

    public static void CopyResource(Resource resource, string? basePath)
    {
        Directory.CreateDirectory(resourceDirectory);
        var originalPath = resource.OriginalPath;
        if (!Path.IsPathFullyQualified(originalPath) && !originalPath.StartsWith("..\\"))
        {
            if (basePath != null)
            {
                originalPath = Path.Combine(basePath, resource.OriginalPath);
            }
            else
            {
                originalPath = Path.Combine(Directory.GetCurrentDirectory(), resource.OriginalPath);
            }
        }
        else
        {
            originalPath = Path.GetFullPath(originalPath);
        }
        var newPath = GetAbsolutePath(resource);
        try
        {
            File.Copy(originalPath, newPath, false);
        }
        catch (FileNotFoundException)
        {
            Debug.WriteLine($"File {originalPath} not found.");
        }
    }

    internal static void CopyResources(Deck deck, string? basePath)
    {
        foreach (var resource in deck.Resources.Values)
        {
            CopyResource(resource, basePath);
        }
    }
}
