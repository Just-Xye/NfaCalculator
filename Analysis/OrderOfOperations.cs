using System.Globalization;
using System.Text.RegularExpressions;

namespace NfaDemo;

// ─────────────────────────────────────────────────────────────
// PART 4 – "DIFFERENT ORDERS OF OPERATIONS"  (the ambiguity)
//
// The NFA only checks that the SHAPE is valid: number (op number)*.
// It says nothing about precedence. So  3 + 4 * 2  can legally be
// read as  (3 + 4) * 2   or   3 + (4 * 2).
// That is exactly the classic ambiguity in  E -> E op E.
//
// Here we list EVERY way to parenthesise an accepted expression,
// plus the answer you get from normal precedence (* and / first).
// For n operators there are Catalan(n) groupings, so we cap n.
// ─────────────────────────────────────────────────────────────
public sealed record Grouping(string Text, double Value);

public static class OrderOfOperations
{
    public const int MaxOperators = 7;   // Catalan(7) = 429 groupings

    private static readonly Regex TokenPattern =
        new(@"\d+(\.\d+)?|[+\-*/]", RegexOptions.Compiled);

    /// <summary>Split an ACCEPTED expression into numbers and operators.</summary>
    public static (List<double> Numbers, List<char> Ops) Tokenize(string input)
    {
        var numbers = new List<double>();
        var ops = new List<char>();

        foreach (Match m in TokenPattern.Matches(input))
        {
            if (char.IsDigit(m.Value[0]))
                numbers.Add(double.Parse(m.Value, CultureInfo.InvariantCulture));
            else
                ops.Add(m.Value[0]);
        }
        return (numbers, ops);
    }

    /// <summary>Normal school rules: * and / first (left to right), then + and -.</summary>
    public static double WithPrecedence(List<double> nums, List<char> ops)
    {
        var values = new List<double> { nums[0] };
        var lowOps = new List<char>();

        for (int i = 0; i < ops.Count; i++)
        {
            if (ops[i] is '*' or '/')
                values[^1] = Apply(ops[i], values[^1], nums[i + 1]);
            else
            {
                lowOps.Add(ops[i]);
                values.Add(nums[i + 1]);
            }
        }

        double result = values[0];
        for (int i = 0; i < lowOps.Count; i++)
            result = Apply(lowOps[i], result, values[i + 1]);
        return result;
    }

    /// <summary>Every possible parenthesisation (every parse tree).</summary>
    public static List<Grouping> AllGroupings(List<double> nums, List<char> ops)
    {
        var memo = new Dictionary<(int, int), List<Grouping>>();
        var all = Groupings(0, nums.Count - 1, nums, ops, memo);

        // The outermost brackets are noise – strip them for display.
        return all.Select(g => ops.Count > 0
                ? g with { Text = g.Text[1..^1] }
                : g).ToList();
    }

    // Numbers lo..hi; operator k sits between number k and number k+1.
    private static List<Grouping> Groupings(
        int lo, int hi, List<double> nums, List<char> ops,
        Dictionary<(int, int), List<Grouping>> memo)
    {
        if (memo.TryGetValue((lo, hi), out var cached)) return cached;

        var result = new List<Grouping>();

        if (lo == hi)
        {
            result.Add(new Grouping(Format(nums[lo]), nums[lo]));
        }
        else
        {
            // Choose which operator is applied LAST (the root of the tree).
            for (int k = lo; k < hi; k++)
            {
                foreach (var left in Groupings(lo, k, nums, ops, memo))
                    foreach (var right in Groupings(k + 1, hi, nums, ops, memo))
                        result.Add(new Grouping(
                            $"({left.Text} {ops[k]} {right.Text})",
                            Apply(ops[k], left.Value, right.Value)));
            }
        }

        memo[(lo, hi)] = result;
        return result;
    }

    private static double Apply(char op, double a, double b) => op switch
    {
        '+' => a + b,
        '-' => a - b,
        '*' => a * b,
        '/' => b == 0 ? double.NaN : a / b,
        _ => throw new ArgumentException($"Unknown operator {op}")
    };

    public static string Format(double v) =>
        double.IsNaN(v) ? "undefined (÷0)" : v.ToString("G6", CultureInfo.InvariantCulture);
}
