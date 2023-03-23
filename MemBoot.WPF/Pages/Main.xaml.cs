using MemBoot.Core.Models;
using MemBoot.DataAccess;
using MemBoot.DataAccess.Json;
using MemBoot.DataAccess.Sqlite;
using MemBoot.WPF;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MemBoot.Pages
{
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
            var newDeck = ImportExportHelpers.ImportDeckFromJson();
            if (newDeck != null)
            {
                var success = deckStorage.AddDeck(newDeck);
                if (success)
                {
                    RefillStoredCardTypes();
                }
            }
        }

        private void CardTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: Tuple<string, Guid> pick })
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

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Deck deck = new()
            {
                Id = Guid.NewGuid(),
                Name = "New Deck"
            };
            var decks = deckStorage.GetDecks().ToList();
            if (decks.Any())
            {
                deck = decks[0];
            }
            var editWindow = new Editor(deck, new(deck), deckStorage);
            editWindow.ShowDialog();
            RefillStoredCardTypes();
        }
    }
}
