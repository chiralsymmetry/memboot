using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBoot
{
    public class Deck
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<Field> Fields { get; set; } = new List<Field>();
        public ICollection<Fact> Facts { get; set; } = new List<Fact>();
        public ICollection<CardType> CardTypes { get; set; } = new List<CardType>();
        public IDictionary<string, byte[]> Resources { get; set; } = new Dictionary<string, byte[]>();
        public IDictionary<CardType, MasteryRecord> MasteryRecords { get; set; } = new Dictionary<CardType, MasteryRecord>();
    }
}
