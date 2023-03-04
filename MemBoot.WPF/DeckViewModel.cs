using MemBoot.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MemBoot.WPF
{
    public class DeckViewModel : INotifyPropertyChanged
    {
        private Deck deck;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal Deck CurrentDeck { get => deck; }

        public DeckViewModel(Deck deck)
        {
            this.deck = deck;
            Fields = new(deck.Fields);
            CardTypes = new(deck.CardTypes);
            Facts = new(deck.Facts);
        }

        public void ChangeDeck(Deck deck)
        {
            Fields.Clear();
            CardTypes.Clear();
            Facts.Clear();
            this.deck = deck;
            foreach (var field in deck.Fields)
            {
                Fields.Add(field);
            }
            foreach (var cardType in deck.CardTypes)
            {
                CardTypes.Add(cardType);
            }
            foreach (var fact in deck.Facts)
            {
                Facts.Add(fact);
            }
            NotifyPropertyChanged(nameof(Name));
            NotifyPropertyChanged(nameof(Description));
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

        public ObservableCollection<CardType> CardTypes { get; }

        internal void CreateCardType()
        {
            string newName = string.Empty;
            {
                const string nameBase = "New Card Type";
                int number = 1;
                newName = nameBase;
                var occupied = deck.CardTypes.Any(ct => ct.Name == newName);
                while (occupied)
                {
                    newName = $"{nameBase} {number++}";
                    occupied = deck.CardTypes.Any(ct => ct.Name == newName);
                }
            }
            var newCardType = new CardType(Guid.NewGuid(), newName, "", "");
            deck.CardTypes.Add(newCardType);
            CardTypes.Add(newCardType);
        }

        internal void RemoveCardType(CardType cardType)
        {
            CardTypes.Remove(cardType);
            deck.MasteryRecords.Remove(cardType);
            deck.CardTypes.Remove(cardType);
        }

        public ObservableCollection<Fact> Facts { get; }

        internal void CreateNewFact()
        {
            var newFact = new Fact(Guid.NewGuid());
            deck.Facts.Add(newFact);
            Facts.Add(newFact);
        }

        internal void RemoveFacts(IEnumerable<Fact> selectedFacts)
        {
            foreach (var fact in selectedFacts)
            {
                deck.Facts.Remove(fact);
                Facts.Remove(fact);
            }
        }
    }
}
