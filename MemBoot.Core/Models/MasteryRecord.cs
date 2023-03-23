using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace MemBoot.Core.Models;

public class MasteryRecord : IDictionary<Fact, double>
{
    private IDictionary<Fact, double> BackingDictionary { get; } = new Dictionary<Fact, double>();
    public double this[Fact fact] { get => BackingDictionary[fact]; set => BackingDictionary[fact] = value; }

    public ICollection<Fact> Keys => BackingDictionary.Keys;

    public ICollection<double> Values => BackingDictionary.Values;

    public int Count => BackingDictionary.Count;

    public bool IsReadOnly => BackingDictionary.IsReadOnly;

    public void Add(Fact fact, double mastery)
    {
        BackingDictionary.Add(fact, mastery);
    }

    public void Add(KeyValuePair<Fact, double> item)
    {
        BackingDictionary.Add(item);
    }

    public void Clear()
    {
        BackingDictionary.Clear();
    }

    public bool Contains(KeyValuePair<Fact, double> item)
    {
        return BackingDictionary.Contains(item);
    }

    public bool ContainsKey(Fact fact)
    {
        return BackingDictionary.ContainsKey(fact);
    }

    public void CopyTo(KeyValuePair<Fact, double>[] array, int arrayIndex)
    {
        BackingDictionary.CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<Fact, double>> GetEnumerator()
    {
        return BackingDictionary.GetEnumerator();
    }

    public bool Remove(Fact fact)
    {
        return BackingDictionary.Remove(fact);
    }

    public bool Remove(KeyValuePair<Fact, double> item)
    {
        return BackingDictionary.Remove(item);
    }

    public bool TryGetValue(Fact key, [MaybeNullWhen(false)] out double mastery)
    {
        return BackingDictionary.TryGetValue(key, out mastery);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return BackingDictionary.GetEnumerator();
    }
}
