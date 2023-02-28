﻿using MemBoot.Core.Models;
using MemBoot.DataAccess;
using MemBoot.DataAccess.Json;
using MemBoot.DataAccess.Sqlite;
using MemBoot.WPF;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

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

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Tuple<string, Guid> pick)
            {
                var deck = deckStorage.GetDeckFromCardTypeId(pick.Item2);
                if (deck != null)
                {
                    var editWindow = new Editor(new(deck));
                    editWindow.ShowDialog();
                    RefillStoredCardTypes();
                }
            }
        }
    }
}
