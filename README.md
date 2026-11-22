# NFA Arithmetic Recognizer School Project – Presentation Notes

## How to run
1. Install the .NET 10 SDK (Windows).
2. Put all the files in one folder.
3. `dotnet run` – the window opens.

## Walk through the files in this order

| # | File | One-sentence pitch |
|---|------|--------------------|
| 1 | `State.cs` | An NFA is states plus arrows; `ε` arrows and multiple matching arrows make it non-deterministic. |
| 2 | `NfaBuilder.cs` | Thompson's construction: build tiny machines and glue them with ε-arrows. The language is defined in `BuildArithmeticExpression()`. |
| 3 | `NfaSimulator.cs` | Track a *set* of active states; each character moves all of them at once, then take the ε-closure. |
| 4 | `OrderOfOperations.cs` | The NFA ignores precedence, so one string has many valid readings – we list them all. |
| 5 | `MainForm.cs` | The window. UI only. |

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
