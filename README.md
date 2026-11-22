# NFA Arithmetic Recognizer – Presentation Notes
# School Project - BSCS 2K

## How to run
1. Install the .NET 10 SDK (Windows).
2. Keep the folder layout shown below.
3. `dotnet run` – the window opens.

## Project structure
```
NfaCalculator/
├── NfaCalculator.csproj
├── Program.cs                  ← entry point, opens the window
├── PRESENTATION_NOTES.md
│
├── Automata/                   ← the theory
│   ├── State.cs                  (1. states, transitions, Nfa)
│   ├── NfaBuilder.cs             (2. Thompson construction + the grammar)
│   └── NfaSimulator.cs           (3. running the NFA)
│
├── Analysis/
│   └── OrderOfOperations.cs      (4. ambiguity / groupings)
│
└── UI/
    └── MainForm.cs               (5. the window)
```

**Important:** every `.cs` file must keep `namespace NfaDemo;` on line 3. Some editors rewrite the namespace to match the folder when a file is moved. If you see "type or namespace 'MainForm' could not be found", check that first.

## Walk through the folders in this order

| # | Folder / File | One-sentence pitch |
|---|---------------|--------------------|
| 1 | `Automata/State.cs` | An NFA is states plus arrows; `ε` arrows and multiple matching arrows make it non-deterministic. |
| 2 | `Automata/NfaBuilder.cs` | Thompson's construction: build tiny machines and glue them with ε-arrows. The language is defined in `BuildArithmeticExpression()`. |
| 3 | `Automata/NfaSimulator.cs` | Track a *set* of active states; each character moves all of them at once, then take the ε-closure. |
| 4 | `Analysis/OrderOfOperations.cs` | The NFA ignores precedence, so one string has many valid readings – we list them all. |
| 5 | `UI/MainForm.cs` | The window. UI only, no automata logic. |
| – | `Program.cs` | Entry point. Opens `MainForm`. |

Suggested line for the audience: *"Automata is the theory, Analysis is the ambiguity, UI is just the window."*

## The language
```
expression = spaces number ( spaces operator spaces number )* spaces
number     = digit+ ( '.' digit+ )?
operator   = + | - | * | /
```

## Key talking points
- **Why NFA?** Union, Star and Plus are one line each with ε-arrows. No need to work out the deterministic version by hand.
- **Where is the non-determinism?** After a digit the machine is in several states at once: still in the number, or ready for an operator, or in trailing spaces. The trace panel shows this.
- **Order of operations / ambiguity:** `3 + 4 * 2` is accepted once, but can be grouped `(3+4)*2 = 14` or `3+(4*2) = 11`. The "Orders of operations" tab lists every grouping (Catalan-many).
- **Limitation (good question to expect):** Parentheses are *not* supported. Matching brackets needs a counter, and a finite automaton can't count – that needs a pushdown automaton. This is a regular language.

## Demo inputs
| Input | Result | Why |
|-------|--------|-----|
| `3 + 4 * 2 - 6 / 3` | Accepted | Many groupings, several answers |
| `1.5*2` | Accepted | Decimal number |
| `  7  ` | Accepted | Spaces allowed at the ends |
| `3 +` | Rejected (input ended) | Ends before a number |
| `+3` | Rejected at char 1 | No unary sign |
| `4 5` | Rejected at char 3 | Two numbers, no operator |
| `2 ** 3` | Rejected at char 4 | Operator after operator |
| `8 / 0` | Accepted | Valid shape; groupings show "undefined" |
