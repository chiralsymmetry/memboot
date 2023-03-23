using MemBoot.Core.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MemBoot.WPF.Controls;

public partial class FieldEditor : UserControl
{
    public Action? OnFieldAdded { get; set; }
    public Action? OnFieldRemoved { get; set; }
    public Action? OnFieldRenamed { get; set; }

    public FieldEditor()
    {
        InitializeComponent();
    }

    private void AddFieldButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is DeckViewModel deckViewModel)
        {
            deckViewModel.CreateNewField();
            OnFieldAdded?.Invoke();
        }
    }

    private void RemoveFieldButton_Click(object sender, RoutedEventArgs e)
    {
        if (FieldsListBox.SelectedItem is Field field && DataContext is DeckViewModel deckViewModel)
        {
            var removeField = false;
            var useCount = deckViewModel.Facts.Count(f => f.FieldsContents.ContainsKey(field));
            if (useCount == 0)
            {
                removeField = true;
            }
            else
            {
                var result = MessageBox.Show($"This will remove information from {useCount} fact{(useCount > 0 ? "s" : "")} currently using this field. Continue?", "Remove Field", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                removeField = (result == MessageBoxResult.Yes);
            }
            if (removeField)
            {
                deckViewModel.RemoveField(field);
                OnFieldRemoved?.Invoke();
            }
        }
    }

    private void FieldNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        OnFieldRenamed?.Invoke();
    }
}
