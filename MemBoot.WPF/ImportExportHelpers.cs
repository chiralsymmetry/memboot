using MemBoot.Core.Models;
using MemBoot.DataAccess.Json;
using MemBoot.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemBoot.Core;
using System.IO;
using MemBoot.DataAccess.Files;

namespace MemBoot.WPF;

internal static class ImportExportHelpers
{
    internal static Deck? ImportDeckFromJson()
    {
        Deck? output = null;
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            FileName = "Document",
            DefaultExt = ".json",
            Filter = "MemBoot deck (.json)|*.json"
        };

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            output = JsonDeck.ImportFile(dialog.FileName);
        }

        return output;
    }

    internal static Resource? ImportResource(Deck? deck)
    {
        Resource? output = null;
        var dialog = new Microsoft.Win32.OpenFileDialog();

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            output = ResourceDirectory.CreateResourceAndAdd(deck, dialog.FileName);
            ResourceDirectory.CopyResource(output, Directory.GetCurrentDirectory());
            if (Path.IsPathFullyQualified(output.OriginalPath) || output.OriginalPath.StartsWith("..\\"))
            {
                // Basically if importing a file that's not in a subdirectory, assume we want it relative, as if it's imported together with a JSON file.
                output.OriginalPath = Path.GetFileName(output.OriginalPath);
            }
        }

        return output;
    }
}
