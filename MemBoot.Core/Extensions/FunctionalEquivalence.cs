using MemBoot.Core.Models;
using System.Collections;

namespace MemBoot.Core.Extensions;

internal static class FunctionalEquivalence
{
    internal static bool IsFunctionallyEqualTo<T>(this ICollection<T> one, ICollection<T> other)
    {
        if (one == other) { return true; }
        if (one == null || other == null) { return false; }
        if (one.Count != other.Count) { return false; }
        foreach (var oneItem in one)
        {
            if (oneItem is Field)
            {
                var oneField = oneItem as Field;
                var otherFields = other as ICollection<Field>;
                if (!otherFields!.Any(otherField => oneField!.IsFunctionallyEqualTo(otherField))) { return false; }
            }
            else if (oneItem is Fact)
            {
                var oneFact = oneItem as Fact;
                var otherFacts = other as ICollection<Fact>;
                if (!otherFacts!.Any(otherFact => oneFact!.IsFunctionallyEqualTo(otherFact))) { return false; }
            }
            else if (oneItem is CardType)
            {
                var oneCardType = oneItem as CardType;
                var otherCardTypes = other as ICollection<CardType>;
                if (!otherCardTypes!.Any(otherCardType => oneCardType!.IsFunctionallyEqualTo(otherCardType))) { return false; }
            }
            else
            {
                if (oneItem == null)
                {
                    var nullSeen = false;
                    foreach (var otherItem in other)
                    {
                        if (otherItem == null)
                        {
                            nullSeen = true;
                            break;
                        }
                    }
                    if (!nullSeen)
                    {
                        return false;
                    }
                }
                else if (!other.Any(otherItem => oneItem.Equals(otherItem))) { return false; }
            }
        }
        return true;
    }

    internal static bool IsFunctionallyEqualTo(this IList one, IList other)
    {
        if (one == other) { return true; }
        if (one == null || other == null) { return false; }
        if (one.Count != other.Count) { return false; }
        for (int i = 0; i < one.Count; i++)
        {
            var oneItem = one[i];
            var otherItem = other[i];
            if (oneItem is Field)
            {
                var oneField = oneItem as Field;
                var otherField = otherItem as Field;
                if (!oneField!.IsFunctionallyEqualTo(otherField!)) { return false; }
            }
            else if (oneItem is Fact)
            {
                var oneFact = oneItem as Fact;
                var otherFact = otherItem as Fact;
                if (!oneFact!.IsFunctionallyEqualTo(otherFact!)) { return false; }
            }
            else if (oneItem is CardType)
            {
                var oneCardType = oneItem as CardType;
                var otherCardType = otherItem as CardType;
                if (!oneCardType!.IsFunctionallyEqualTo(otherCardType!)) { return false; }
            }
            else
            {
                if (oneItem == null && otherItem == null) { continue; }
                else if (oneItem == null || otherItem == null) { return false; }
                else if (!oneItem.Equals(otherItem)) { return false; }
            }
        }
        return true;
    }

    internal static bool IsFunctionallyEqualTo<T, U>(this IDictionary<T, U> one, IDictionary<T, U> other)
    {
        if (one == other) { return true; }
        if (one == null || other == null) { return false; }
        if (one.Count != other.Count) { return false; }
        if (one.Keys is ICollection<Field>)
        {
            var oneFields = one.Keys as ICollection<Field>;
            var otherFields = other.Keys as ICollection<Field>;
            if (!oneFields!.IsFunctionallyEqualTo(otherFields!)) { return false; }
        }
        else if (one.Keys is ICollection<Fact>)
        {
            var oneFacts = one.Keys as ICollection<Fact>;
            var otherFacts = other.Keys as ICollection<Fact>;
            if (!oneFacts!.IsFunctionallyEqualTo(otherFacts!)) { return false; }
        }
        else if (one.Keys is ICollection<CardType>)
        {
            var oneCardTypes = one.Keys as ICollection<CardType>;
            var otherCardTypes = other.Keys as ICollection<CardType>;
            if (!oneCardTypes!.IsFunctionallyEqualTo(otherCardTypes!)) { return false; }
        }
        else if (!one.Keys.IsFunctionallyEqualTo(other.Keys)) { return false; }

        // Keys are confirmed equal.
        // TODO: Use EqualityCheck(one.Keys, other.Keys) directly?

        foreach (var kvp in one)
        {
            var oneKey = kvp.Key;
            var oneValue = kvp.Value;
            var keyValueMatch = false;

            foreach (var otherKvp in other)
            {
                var otherKey = otherKvp.Key;
                var otherValue = otherKvp.Value;

                var bothKeysAreNull = oneKey == null && otherKey == null;
                var bothValuesAreNull = oneValue == null && otherValue == null;
                var neitherKeyIsNull = oneKey != null && otherKey != null;
                var neitherValueIsNull = oneValue != null && otherValue != null;
                var keysAreEqual = false;
                var valuesAreEqual = false;

                if (neitherKeyIsNull && oneKey is Field)
                {
                    var oneField = oneKey as Field;
                    var otherField = otherKey as Field;
                    keysAreEqual = oneField!.IsFunctionallyEqualTo(otherField!);
                }
                else if (neitherKeyIsNull && oneKey is Fact)
                {
                    var oneFact = oneKey as Fact;
                    var otherFact = otherKey as Fact;
                    keysAreEqual = oneFact!.IsFunctionallyEqualTo(otherFact!);
                }
                else if (neitherKeyIsNull && oneKey is CardType)
                {
                    var oneCardType = oneKey as CardType;
                    var otherCardType = otherKey as CardType;
                    keysAreEqual = oneCardType!.IsFunctionallyEqualTo(otherCardType!);
                }
                else
                {
                    keysAreEqual = bothKeysAreNull || (neitherKeyIsNull && oneKey!.Equals(otherKey));
                }

                if (otherValue is MasteryRecord)
                {
                    var oneRecord = oneValue as IDictionary<Fact, double>;
                    var otherRecord = otherValue as IDictionary<Fact, double>;
                    valuesAreEqual = oneRecord!.IsFunctionallyEqualTo(otherRecord!);
                }
                else if (neitherValueIsNull && otherValue is Array)
                {
                    var oneArray = oneValue as Array;
                    var otherArray = otherValue as Array;
                    valuesAreEqual = oneArray!.IsFunctionallyEqualTo(otherArray!);
                }
                else
                {
                    valuesAreEqual = bothValuesAreNull || (neitherValueIsNull && oneValue!.Equals(otherValue));
                }

                if (keysAreEqual && valuesAreEqual)
                {
                    keyValueMatch = true;
                    break;
                }
            }
            if (!keyValueMatch) { return false; }
        }
        return true;
    }
}
