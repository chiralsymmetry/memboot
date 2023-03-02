using MemBoot.Core.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace MemBoot.WPF
{
    public class FieldColumn : DataGridColumn, IEquatable<FieldColumn>
    {
        public readonly Field OriginalField;
        private static readonly Brush UsedCellBackground = Brushes.White;
        private static readonly Brush UnusedCellBackground = new SolidColorBrush(new Color() { A = 0xFF, R = 0xEE, G = 0xEE, B = 0xEE });
        private static readonly Brush ActiveCellBackground = new SolidColorBrush(Color.FromRgb(224, 255, 255));

        public FieldColumn(Field field)
        {
            this.OriginalField = field;
            RefreshHeader();
        }

        public void RefreshHeader()
        {
            Header = OriginalField.Name.Replace("_", "__");
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var textBox = new TextBox
            {
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            if (dataItem is Fact fact)
            {
                if (fact.FieldsContents.ContainsKey(OriginalField))
                {
                    textBox.Text = fact.FieldsContents[OriginalField];
                    textBox.Background = UsedCellBackground;
                }
                else
                {
                    textBox.Background = UnusedCellBackground;
                }
                textBox.GotFocus += (sender, e) =>
                {
                    if (sender is TextBox tb)
                    {
                        tb.Background = ActiveCellBackground;
                    }
                };
                cell.GotFocus += (sender, e) =>
                {
                    textBox.Focus();
                    textBox.CaretIndex = textBox.Text.Length;
                };
                textBox.LostFocus += (sender, e) =>
                {
                    if (sender is TextBox tb)
                    {
                        if (tb.Text.Length > 0)
                        {
                            fact.FieldsContents[OriginalField] = tb.Text;
                            tb.Background = UsedCellBackground;
                        }
                        else
                        {
                            fact.FieldsContents.Remove(OriginalField);
                            tb.Background = UnusedCellBackground;
                        }
                    }
                };
            }
            else
            {
                textBox.Background = UnusedCellBackground;
            }
            textBox.IsReadOnly = false;
            return textBox;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return GenerateElement(cell, dataItem);
        }

        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            var textBox = (TextBox)editingElement;
            textBox.IsReadOnly = false;
            textBox.Background = Brushes.White;
            return base.PrepareCellForEdit(editingElement, editingEventArgs);
        }

        public bool Equals(FieldColumn? other)
        {
            var output = false;

            if (other != null)
            {
                output = OriginalField.Equals(other.OriginalField);
            }

            return output;
        }
    }
}
