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
    /// Interaction logic for FactEditor.xaml
    /// </summary>
    public partial class FactEditor : UserControl
    {
        public FactEditor()
        {
            InitializeComponent();
        }

        internal void RefreshColumnHeaders()
        {
            foreach (var column in FactsDataGrid.Columns.Cast<FieldColumn>().ToList())
            {
                column.RefreshHeader();
            }
        }

        internal void RemakeColumns()
        {
            if (DataContext is DeckViewModel deckViewModel)
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
        }

        private void AddFactButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DeckViewModel deckViewModel)
            {
                deckViewModel.CreateNewFact();
            }
        }

        private void RemoveFactButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DeckViewModel deckViewModel)
            {
                deckViewModel.RemoveFacts(FactsDataGrid.SelectedItems.Cast<Fact>().ToList());
            }
        }
    }
}
