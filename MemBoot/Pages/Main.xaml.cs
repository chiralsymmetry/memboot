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

namespace MemBoot.Pages
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        public Main()
        {
            InitializeComponent();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var json = System.IO.File.ReadAllText("sample.json");
            var deck = DeckProcessor.FromJson(json);
            var cardType = deck!.CardTypes.First();
            var deckViewModel = new DeckViewModel(deck, cardType);

            FlashcardPage page = new(deckViewModel);
            NavigationService.Navigate(page);
            while (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }
        }
    }
}
