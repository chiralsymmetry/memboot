using MemBoot.Core.Models;
using System.Windows;
using System.Windows.Controls;

namespace MemBoot.WPF.Controls;

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
