using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBoot
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
    }
}
