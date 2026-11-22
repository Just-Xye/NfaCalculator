namespace NfaDemo;

// ─────────────────────────────────────────────────────────────
// PART 2 – BUILDING THE NFA  (Thompson's construction)
//
// Instead of drawing every state by hand, we build small machines
// ("fragments") and glue them together with ε-arrows.
// Each fragment has exactly ONE start and ONE end state.
//
//   Symbol   : start --c--> end
//   Concat   : A then B          (A.end --ε--> B.start)
//   Union    : A or B            (new start branches to both)
//   Star     : zero or more A    (loop back + skip)
//   Plus     : one or more A
//   Optional : zero or one A
// ─────────────────────────────────────────────────────────────
public sealed class NfaBuilder
{
    private readonly List<State> _states = new();

    /// <summary>A partially-built machine with one entry and one exit.</summary>
    public readonly record struct Fragment(State Start, State End);

    private State NewState()
    {
        var s = new State(_states.Count);
        _states.Add(s);
        return s;
    }

    // ── The six building blocks ────────────────────────────────

    public Fragment Symbol(Func<char, bool> matches, string label)
    {
        var s = NewState();
        var e = NewState();
        s.AddSymbol(e, matches, label);
        return new Fragment(s, e);
    }

    public Fragment Concat(params Fragment[] parts)
    {
        for (int i = 0; i < parts.Length - 1; i++)
            parts[i].End.AddEpsilon(parts[i + 1].Start);
        return new Fragment(parts[0].Start, parts[^1].End);
    }

    public Fragment Union(params Fragment[] options)
    {
        var s = NewState();
        var e = NewState();
        foreach (var o in options)
        {
            s.AddEpsilon(o.Start);   // branch: try every option at once
            o.End.AddEpsilon(e);     // rejoin
        }
        return new Fragment(s, e);
    }

    public Fragment Star(Fragment f)
    {
        var s = NewState();
        var e = NewState();
        s.AddEpsilon(f.Start);       // enter once...
        s.AddEpsilon(e);             // ...or skip entirely
        f.End.AddEpsilon(f.Start);   // repeat
        f.End.AddEpsilon(e);         // leave
        return new Fragment(s, e);
    }

    public Fragment Plus(Fragment f)
    {
        var s = NewState();
        var e = NewState();
        s.AddEpsilon(f.Start);
        f.End.AddEpsilon(f.Start);   // repeat
        f.End.AddEpsilon(e);
        return new Fragment(s, e);
    }

    public Fragment Optional(Fragment f)
    {
        var s = NewState();
        var e = NewState();
        s.AddEpsilon(f.Start);
        s.AddEpsilon(e);             // skip
        f.End.AddEpsilon(e);
        return new Fragment(s, e);
    }

    // ── Reusable pieces of the arithmetic language ─────────────

    private Fragment Digit() =>
        Symbol(c => c >= '0' && c <= '9', "digit");

    private Fragment Spaces() =>
        Star(Symbol(char.IsWhiteSpace, "space"));

    /// <summary>number  =  digit+  ( '.' digit+ )?</summary>
    private Fragment Number() =>
        Concat(
            Plus(Digit()),
            Optional(Concat(Symbol(c => c == '.', "'.'"), Plus(Digit()))));

    /// <summary>operator  =  '+' | '-' | '*' | '/'</summary>
    private Fragment Operator() =>
        Union(
            Symbol(c => c == '+', "'+'"),
            Symbol(c => c == '-', "'-'"),
            Symbol(c => c == '*', "'*'"),
            Symbol(c => c == '/', "'/'"));

    // ── The whole language ─────────────────────────────────────

    /// <summary>
    /// expression  =  spaces number ( spaces operator spaces number )* spaces
    ///
    /// Examples accepted:  7   |   3 + 4   |   1.5*2 - 8/4
    /// Examples rejected:  +3  |   3 +      |   4 5   |   2 ** 3
    /// </summary>
    public Nfa BuildArithmeticExpression()
    {
        var term = Concat(Spaces(), Operator(), Spaces(), Number());

        var expression = Concat(
            Spaces(),
            Number(),
            Star(term),
            Spaces());

        expression.End.IsAccepting = true;
        return new Nfa(expression.Start, expression.End, _states);
    }
}
