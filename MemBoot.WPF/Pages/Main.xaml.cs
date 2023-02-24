using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
using MemBoot.Core.Models;
using MemBoot.DataAccess;
using MemBoot.DataAccess.Json;
using MemBoot.DataAccess.Sqlite;

namespace MemBoot.Pages
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        private readonly IDeckStorage deckStorage;
        public ObservableCollection<Tuple<string, Guid>> StoredCardTypes { get; }

        public Main()
        {
            InitializeComponent();

            this.deckStorage = new SqliteDeckStorage("Data Source=MemBoot.db;Version=3;");

            StoredCardTypes = new ObservableCollection<Tuple<string, Guid>>();

            RefillStoredCardTypes();
        }

        private void RefillStoredCardTypes()
        {
            StoredCardTypes.Clear();
            foreach (var cardTypeNameId in deckStorage.GetCardTypeIds())
            {
                StoredCardTypes.Add(cardTypeNameId);
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "Document",
                DefaultExt = ".json",
                Filter = "MemBoot deck (.json)|*.json"
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                Deck? newDeck = JsonDeck.ImportFile(dialog.FileName);
                if (newDeck != null)
                {
                    var success = deckStorage.AddDeck(newDeck);
                    if (success)
                    {
                        RefillStoredCardTypes();
                    }
                }
            }
        }

        private void CardTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Tuple<string, Guid> pick)
            {
                var flashcard = deckStorage.GetFlashcard(pick.Item2);
                if (flashcard != null)
                {
                    var deckViewModel = new FlashcardViewModel(flashcard);
                    FlashcardPage page = new(deckViewModel);
                    NavigationService.Navigate(page);
                    while (NavigationService.CanGoBack)
                    {
                        NavigationService.RemoveBackEntry();
                    }
                }
            }
        }
    }
}
