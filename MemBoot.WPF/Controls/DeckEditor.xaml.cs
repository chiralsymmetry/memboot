using System;
using System.Windows;
using System.Windows.Controls;

namespace MemBoot.WPF.Controls;

public partial class DeckEditor : UserControl
{
    public Action? StoreChanges { get; set; }
    public Action? CreateDeck { get; set; }
    public Action? DeleteDeck { get; set; }

    public DeckEditor()
    {
        InitializeComponent();
    }

    private void StoreChangesButton_Click(object sender, RoutedEventArgs e)
    {
        StoreChanges?.Invoke();
    }

    private void CreateDeckButton_Click(object sender, RoutedEventArgs e)
    {
        CreateDeck?.Invoke();
    }

    private void DeleteDeckButton_Click(object sender, RoutedEventArgs e)
    {
        DeleteDeck?.Invoke();
    }
}
