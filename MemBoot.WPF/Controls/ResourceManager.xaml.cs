using MemBoot.Core;
using MemBoot.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MemBoot.WPF.Controls
{
    /// <summary>
    /// Interaction logic for ResourceManager.xaml
    /// </summary>
    public partial class ResourceManager : UserControl
    {
        public ResourceManager()
        {
            InitializeComponent();
        }

        private void ImportResourceButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DeckViewModel deckViewModel)
            {
                deckViewModel.ImportNewResource();
            }
        }

        private void RemoveResourceButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResourcesListBox.SelectedItem is Resource resource && DataContext is DeckViewModel deckViewModel)
            {
                var removeResource = false;
                var oldPath = resource.OriginalPath.Replace("]", "\\]");
                var pattern = $":{oldPath}]";
                var useCount = deckViewModel.Facts.Count(f => f.FieldsContents.Values.Any(c => c.Contains(pattern)));
                if (useCount == 0)
                {
                    removeResource = true;
                }
                else
                {
                    var result = MessageBox.Show($"This will remove a resource currently used by {useCount} fact{(useCount > 0 ? "s" : "")}. Continue?", "Remove Resource", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                    removeResource = (result == MessageBoxResult.Yes);
                }
                if (removeResource)
                {
                    deckViewModel.RemoveResource(resource);
                }
            }
        }

        private void ShowResourceButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResourcesListBox.SelectedItem is Resource resource)
            {
                string path = DataAccess.Files.ResourceDirectory.GetAbsolutePath(resource);
                Process.Start("explorer.exe", $"/select, {path}");
            }
        }
    }
}
