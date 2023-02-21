using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBoot.Core.Models
{
    public class Field
    {
        public string Name { get; set; }
        public bool AllowHTML { get; set; }
        public Field(string name, bool allowHTML)
        {
            Name = name;
            AllowHTML = allowHTML;
        }
        public Field(string name) : this(name, false)
        {
        }

        public bool IsFunctionallyEqualTo(Field other)
        {
            if (this == other) { return true; }
            if (this == null || other == null) { return false; }
            if (this.Name != other.Name) { return false; }
            if (this.AllowHTML != other.AllowHTML) { return false; }
            return true;
        }
    }
}
