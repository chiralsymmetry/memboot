using MemBoot.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemBoot.Core.Models
{
    public class Fact
    {
        public Guid Id { get; set; }
        public IDictionary<Field, string> FieldsContents { get; set; } = new Dictionary<Field, string>();

        public string this[Field field]
        {
            get
            {
                return FieldsContents[field];
            }
            set
            {
                FieldsContents[field] = value;
            }
        }

        public Fact(Guid id, IEnumerable<Field> fields)
        {
            Id = id;
            foreach (var field in fields)
            {
                FieldsContents[field] = string.Empty;
            }
        }

        public Fact(Guid id, IList<Field> fields, IList<string> contents)
        {
            Id = id;
            if (fields.Count != contents.Count)
            {
                throw new ArgumentException("if supplying contents, all fields must be accounted for exactly");
            }
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                var content = contents[i];
                FieldsContents[field] = content;
            }
        }

        public Fact(Guid id, IDictionary<Field, string> fields)
        {
            Id = id;
            foreach (var kvp in fields)
            {
                FieldsContents[kvp.Key] = kvp.Value;
            }
        }

        public bool IsFunctionallyEqualTo(Fact other)
        {
            if (this == other) { return true; }
            if (this == null || other == null) { return false; }
            if (Id != other.Id) { return false; }
            if (!FieldsContents.IsFunctionallyEqualTo(other.FieldsContents)) { return false; }
            return true;
        }
    }
}
