using MemBoot.Core.Models;
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
using System.Windows.Shapes;

namespace MemBoot.WPF
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : Window
    {
        private DeckViewModel deckViewModel;
        public Editor(DeckViewModel deckViewModel)
        {
            InitializeComponent();
            this.deckViewModel = deckViewModel;
            DataContext = deckViewModel;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListBox)?.SelectedItem is Field field)
            {
               // deckViewModel.CurrentField = field;
            }
        }

        private void AddFieldButton_Click(object sender, RoutedEventArgs e)
        {
            deckViewModel.CreateNewField();
        }

        private void RemoveFieldButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
