namespace NfaDemo;

// ─────────────────────────────────────────────────────────────
// PART 1 – THE DATA MODEL
// An NFA is just: states + labelled arrows between them.
// The "N" (non-deterministic) comes from two things:
//   1. ε-transitions  : arrows that consume NO input
//   2. a state may have several arrows for the same character
// ─────────────────────────────────────────────────────────────

/// <summary>One arrow from a state to another state.</summary>
public sealed class Transition
{
    public State Target { get; }

    /// <summary>Which characters this arrow accepts. null means ε (free move).</summary>
    public Func<char, bool>? Matches { get; }

    /// <summary>Text shown in the transition table, e.g. "digit" or "'+'" or "ε".</summary>
    public string Label { get; }

    public bool IsEpsilon => Matches is null;

    public Transition(State target, Func<char, bool>? matches, string label)
    {
        Target = target;
        Matches = matches;
        Label = label;
    }
}

/// <summary>A circle in the NFA diagram.</summary>
public sealed class State
{
    public int Id { get; }
    public string Name => $"q{Id}";
    public bool IsAccepting { get; set; }
    public List<Transition> Transitions { get; } = new();

    public State(int id) => Id = id;

    public void AddEpsilon(State to) =>
        Transitions.Add(new Transition(to, null, "ε"));

    public void AddSymbol(State to, Func<char, bool> matches, string label) =>
        Transitions.Add(new Transition(to, matches, label));

    public override string ToString() => Name;
}

/// <summary>A finished machine: where it starts, where it accepts, and every state.</summary>
public sealed class Nfa
{
    public State Start { get; }
    public State Accept { get; }
    public IReadOnlyList<State> States { get; }

    public Nfa(State start, State accept, IReadOnlyList<State> states)
    {
        Start = start;
        Accept = accept;
        States = states;
    }
}
