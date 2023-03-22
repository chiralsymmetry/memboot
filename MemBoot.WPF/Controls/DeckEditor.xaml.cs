﻿using MemBoot.Core.Models;
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
    /// Interaction logic for DeckEditor.xaml
    /// </summary>
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
}
