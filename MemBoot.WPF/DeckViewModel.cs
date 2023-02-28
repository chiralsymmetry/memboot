using MemBoot.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace MemBoot.WPF
{
    public class DeckViewModel
    {
        private readonly Deck deck;

        public DeckViewModel(Deck deck)
        {
            this.deck = deck;
            Fields = new(deck.Fields);
        }

        public string Name
        {
            get
            {
                return deck.Name;
            }
            set
            {
                deck.Name = value;
            }
        }

        public string Description
        {
            get
            {
                return deck.Description;
            }
            set
            {
                deck.Description = value;
            }
        }

        public ObservableFieldCollection Fields { get; }

        internal void CreateNewField()
        {
            string newName = string.Empty;
            {
                string nameBase = "New Field";
                int number = 1;
                newName = nameBase;
                var occupied = deck.Fields.Where(f => f.Name == newName).Any();
                while (occupied)
                {
                    newName = $"{nameBase} {number++}";
                    occupied = deck.Fields.Where(f => f.Name == newName).Any();
                }
            }
            var newField = new Field(newName);
            deck.Fields.Add(newField);
            Fields.Add(newField);
        }
    }
    public class ObservableFieldCollection : ObservableCollection<Field>
    {
        public ObservableFieldCollection(IEnumerable<Field> collection) : base(collection)
        {
        }

        protected override void InsertItem(int index, Field item)
        {
            base.InsertItem(index, item);
            Debug.WriteLine("Insert");
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            Debug.WriteLine("Remove");
        }

        protected override void SetItem(int index, Field item)
        {
            base.SetItem(index, item);
            Debug.WriteLine("Set");
        }
    }
}
