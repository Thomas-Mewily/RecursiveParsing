using System;
using System.Collections.Immutable;
using System.Text;

namespace RecursiveParsing;

public static class CharExtension
{
    public static int? TryAsNumber(this char c) => c >= '0' && c <= '9' ? c - '0' : null;
    public static bool IsUseless(this char c) => c == ' ' || c == '\t' || c == '\r' || c == '\n';
}

public static class Extension
{
    public static void ReplaceLast<T>(this IList<T> l, T val) => l[l.Count - 1] = val;
    public static void Push<T>(this IList<T> l, T val) => l.Add(val);
    public static bool IsEmpty<T>(this IList<T> l) => l.Count == 0;
    public static T Peek<T>(this IList<T> l) => l[l.Count - 1];
    public static T Pop<T>(this IList<T> l)
    {
        T t = l.Peek();
        l.RemoveAt(l.Count - 1);
        return t;
    }

    // Thank to https://stackoverflow.com/questions/7278136/create-hash-value-on-a-list
    public static int GetSequenceHashCode<T>(this IList<T> sequence)
    {
        const int seed = 487;
        const int modifier = 31;

        unchecked
        {
            int hash = sequence.Aggregate(seed, (current, item) =>
                (current * modifier) + item!.GetHashCode());
            return hash;
        }
    }

    // Thank to https://stackoverflow.com/questions/17590528/pad-left-pad-right-pad-center-string
    public static string PadMiddle(this string str, int length)
    {
        int spaces = length - str.Length;
        int padLeft = spaces / 2 + str.Length;
        return str.PadLeft(padLeft).PadRight(length);
    }

    public static void Todo(this object? o) => throw new Exception(o == null ? "?" : o.ToString());
    public static void MustBeTrue(this bool b) { if (b == false) { throw new Exception(); } }

    public static T Unwrap<T>(this T t)  => t ?? throw new ArgumentNullException(nameof(t));
}