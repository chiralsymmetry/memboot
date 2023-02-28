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

        public ObservableCollection<Field> Fields { get; }

        internal void CreateNewField()
        {
            string newName = string.Empty;
            {
                const string nameBase = "New Field";
                int number = 1;
                newName = nameBase;
                var occupied = deck.Fields.Any(f => f.Name == newName);
                while (occupied)
                {
                    newName = $"{nameBase} {number++}";
                    occupied = deck.Fields.Any(f => f.Name == newName);
                }
            }
            var newField = new Field(newName);
            deck.Fields.Add(newField);
            Fields.Add(newField);
        }

        internal void RemoveField(Field field)
        {
            Fields.Remove(field);
            foreach (var fact in deck.Facts)
            {
                fact.FieldsContents.Remove(field);
            }
            deck.Fields.Remove(field);
        }
    }
}
