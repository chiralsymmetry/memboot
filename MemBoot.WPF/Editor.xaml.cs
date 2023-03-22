using MemBoot.Core.Models;
using MemBoot.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MemBoot.WPF
{
    public partial class Editor : Window
    {
        public ObservableCollection<Deck> Decks { get; set; }

        public Editor(Deck _, DeckViewModel deckViewModel, IDeckStorage deckStorage)
        {
            InitializeComponent();

            DataContext = this;
            {
                var decks = deckStorage.GetDecks();
                Decks = new ObservableCollection<Deck>(decks);
                DeckListBox.SelectedItem = decks.FirstOrDefault(new Deck()
                {
                    Id = Guid.NewGuid(),
                    Name = "New Deck"
                });
            }

            CurrentDeckEditor.DataContext = deckViewModel;
            CurrentFieldEditor.DataContext = deckViewModel;
            CurrentCardTypeEditor.DataContext = deckViewModel;
            CurrentFactEditor.DataContext = deckViewModel;
            CurrentResourceManager.DataContext = deckViewModel;

            CurrentDeckEditor.StoreChanges = () =>
            {
                deckStorage.AddOrReplaceDeck(deckViewModel.CurrentDeck);
                if (!Decks.Contains(deckViewModel.CurrentDeck))
                {
                    Decks.Add(deckViewModel.CurrentDeck);
                }
                DeckListBox.Items.Refresh();
            };

            CurrentDeckEditor.CreateDeck = () =>
            {
                Deck newDeck = new Deck()
                {
                    Id = Guid.NewGuid(),
                    Name = "New Deck"
                };
                deckViewModel.ChangeDeck(newDeck);
                DeckListBox.SelectedIndex = -1;
                CurrentFactEditor.RemakeColumns();
            };

            CurrentDeckEditor.DeleteDeck = () =>
            {
                Deck deckToRemove = deckViewModel.CurrentDeck;
                deckStorage.RemoveDeck(deckToRemove);
                Decks.Remove(deckToRemove);

                Deck replacementDeck = deckStorage.GetDecks().FirstOrDefault(new Deck()
                {
                    Id = Guid.NewGuid(),
                    Name = "New Deck"
                });
                if (Decks.Contains(replacementDeck))
                {
                    DeckListBox.SelectedItem = replacementDeck;
                }
                deckViewModel.ChangeDeck(replacementDeck);
                DeckListBox.Items.Refresh();
                CurrentFactEditor.RemakeColumns();
            };

            CurrentFieldEditor.OnFieldAdded = () => CurrentFactEditor.RemakeColumns();
            CurrentFieldEditor.OnFieldRemoved = () => CurrentFactEditor.RemakeColumns();
            CurrentFieldEditor.OnFieldRenamed = () => CurrentFactEditor.RefreshColumnHeaders();

            CurrentFactEditor.RemakeColumns();
        }

        private void DeckListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListBox)?.SelectedItem is Deck newlySelectedDeck && CurrentFactEditor.DataContext is DeckViewModel deckViewModel)
            {
                deckViewModel.ChangeDeck(newlySelectedDeck);
                CurrentFactEditor.RemakeColumns();
            }
        }
    }
}
