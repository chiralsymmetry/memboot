using MemBoot.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MemBoot.WPF
{
    public partial class Editor : Window
    {
        private readonly DeckViewModel deckViewModel;
        public Editor(DeckViewModel deckViewModel)
        {
            InitializeComponent();
            this.deckViewModel = deckViewModel;
            DataContext = deckViewModel;
            RemakeColumns();
        }

        private void AddFieldButton_Click(object sender, RoutedEventArgs e)
        {
            deckViewModel.CreateNewField();
            RemakeColumns();
        }

        private void RemoveFieldButton_Click(object sender, RoutedEventArgs e)
        {
            if (FieldsListBox.SelectedItem is Field field)
            {
                var result = MessageBox.Show("This will remove information from any fact currently using this field. Continue?", "Remove Field", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    deckViewModel.RemoveField(field);
                    RemakeColumns();
                }
            }
        }

        private void AddCardTypeButton_Click(object sender, RoutedEventArgs e)
        {
            deckViewModel.CreateCardType();
        }

        private void RemoveCardTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (CardTypesListBox.SelectedItem is CardType cardType)
            {
                deckViewModel.RemoveCardType(cardType);
            }
        }

        private void RefreshColumnHeaders()
        {
            foreach (var column in FactsDataGrid.Columns.Cast<FieldColumn>().ToList())
            {
                column.RefreshHeader();
            }
        }

        private void RemakeColumns()
        {
            var availableFields = new HashSet<Field>(deckViewModel.Fields);
            var existingColumns = FactsDataGrid.Columns.Cast<DataGridColumn>().ToList();
            foreach (var existingColumn in existingColumns)
            {
                if (existingColumn is FieldColumn fieldColumn)
                {
                    if (availableFields.Contains(fieldColumn.OriginalField))
                    {
                        fieldColumn.RefreshHeader();
                        availableFields.Remove(fieldColumn.OriginalField);
                    }
                    else
                    {
                        FactsDataGrid.Columns.Remove(existingColumn);
                    }
                }
            }
            foreach (var unusedField in availableFields)
            {
                FactsDataGrid.Columns.Add(new FieldColumn(unusedField));
            }
        }

        private void FieldNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshColumnHeaders();
        }
    }
}
