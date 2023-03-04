using MemBoot.Core.Models;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for CardTypeEditor.xaml
    /// </summary>
    public partial class CardTypeEditor : UserControl
    {
        public CardTypeEditor()
        {
            InitializeComponent();
        }

        private void AddCardTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DeckViewModel deckViewModel)
            {
                deckViewModel.CreateCardType();
            }
        }

        private void RemoveCardTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (CardTypesListBox.SelectedItem is CardType cardType && DataContext is DeckViewModel deckViewModel)
            {
                deckViewModel.RemoveCardType(cardType);
            }
        }
    }
}
