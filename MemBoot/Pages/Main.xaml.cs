using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MemBoot.Pages
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        private readonly DeckStorage deckStorage = new();
        public ObservableCollection<Tuple<Deck, CardType>> StoredCardTypes { get; }

        public Main()
        {
            InitializeComponent();
            StoredCardTypes = new ObservableCollection<Tuple<Deck, CardType>>(deckStorage.GetCardTypes());
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
                deckStorage.ImportFile(dialog.FileName);
                var cardTypes = deckStorage.GetCardTypes();
                foreach (var cardType in cardTypes)
                {
                    if (!StoredCardTypes.Contains(cardType))
                    {
                        StoredCardTypes.Add(cardType);
                    }
                }
            }
        }

        private void CardTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Tuple<Deck, CardType> pick)
            {
                var (deck, cardType) = pick;
                var deckViewModel = new DeckViewModel(deck!, cardType!);

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
