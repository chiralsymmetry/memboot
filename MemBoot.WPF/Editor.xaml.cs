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

        public Editor(Deck deck, DeckViewModel deckViewModel, IDeckStorage deckStorage)
        {
            InitializeComponent();

            DataContext = this;
            Decks = new ObservableCollection<Deck>(deckStorage.GetDecks());
            DeckListBox.SelectedItem = deck;

            CurrentDeckEditor.DataContext = deckViewModel;
            CurrentFieldEditor.DataContext = deckViewModel;
            CurrentCardTypeEditor.DataContext = deckViewModel;
            CurrentFactEditor.DataContext = deckViewModel;
            CurrentResourceManager.DataContext = deckViewModel;

            CurrentDeckEditor.StoreChanges = () => deckStorage.AddOrReplaceDeck(deckViewModel.CurrentDeck);

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
