namespace NfaDemo;

// ─────────────────────────────────────────────────────────────
// PART 3 – RUNNING THE NFA
//
// A DFA is in ONE state at a time. An NFA is in a SET of states
// at a time – every possible "guess" is followed in parallel.
//
// Algorithm for each input character c:
//   1. For every active state, follow every arrow that accepts c.
//   2. Take the ε-closure (follow all free ε-arrows).
//   3. The result is the new active set.
// The input is accepted if the accept state is active at the end.
// ─────────────────────────────────────────────────────────────

/// <summary>A snapshot shown in the trace window.</summary>
public sealed record Step(string Description, IReadOnlyList<State> Active);

public sealed record SimulationResult(
    bool Accepted,
    IReadOnlyList<Step> Steps,
    int FailedAtIndex);   // -1 when the machine never died

public sealed class NfaSimulator
{
    private readonly Nfa _nfa;

    public NfaSimulator(Nfa nfa) => _nfa = nfa;

    public SimulationResult Run(string input)
    {
        var steps = new List<Step>();

        // Start in the ε-closure of the start state.
        var active = EpsilonClosure(new[] { _nfa.Start });
        steps.Add(new Step("start (ε-closure)", Ordered(active)));

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            // 1. consume c from every active state
            var reached = new List<State>();
            foreach (var state in active)
                foreach (var t in state.Transitions)
                    if (!t.IsEpsilon && t.Matches!(c))
                        reached.Add(t.Target);

            // 2. ε-closure
            active = EpsilonClosure(reached);
            steps.Add(new Step($"read '{c}'", Ordered(active)));

            // Every guess died → no way to continue.
            if (active.Count == 0)
                return new SimulationResult(false, steps, i);
        }

        bool accepted = active.Contains(_nfa.Accept);
        return new SimulationResult(accepted, steps, -1);
    }

    /// <summary>All states reachable using only ε-arrows (including the starting ones).</summary>
    private static HashSet<State> EpsilonClosure(IEnumerable<State> seeds)
    {
        var closure = new HashSet<State>(seeds);
        var stack = new Stack<State>(closure);

        while (stack.Count > 0)
        {
            var s = stack.Pop();
            foreach (var t in s.Transitions)
                if (t.IsEpsilon && closure.Add(t.Target))
                    stack.Push(t.Target);
        }
        return closure;
    }

    private static List<State> Ordered(HashSet<State> set) =>
        set.OrderBy(s => s.Id).ToList();
}
