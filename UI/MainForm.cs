using System.Text;

namespace NfaDemo;

// ─────────────────────────────────────────────────────────────
// PART 5 – THE WINDOW
// Pure UI: build the controls, call the NFA, print the results.
// No automata logic lives here.
// ─────────────────────────────────────────────────────────────
public sealed class MainForm : Form
{
    private readonly Nfa _nfa;
    private readonly NfaSimulator _simulator;

    private readonly TextBox _input = new();
    private readonly Label _verdict = new();
    private readonly TextBox _trace = new();
    private readonly TextBox _orders = new();
    private readonly TextBox _table = new();

    public MainForm()
    {
        // Build the machine once, reuse it for every keystroke.
        _nfa = new NfaBuilder().BuildArithmeticExpression();
        _simulator = new NfaSimulator(_nfa);

        BuildLayout();
        _table.Text = DescribeNfa();

        _input.Text = "3 + 4 * 2 - 6 / 3";   // starter example
        Evaluate();
    }

    // ── Layout ─────────────────────────────────────────────────

    private void BuildLayout()
    {
        Text = "NFA – Arithmetic Expression Recognizer";
        Size = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);

        // Top bar: label + input box
        var prompt = new Label
        {
            Text = "Expression:",
            AutoSize = true,
            Dock = DockStyle.Left,
            Padding = new Padding(0, 8, 8, 0)
        };
        _input.Dock = DockStyle.Fill;
        _input.Font = new Font("Consolas", 14);
        _input.TextChanged += (_, _) => Evaluate();   // live feedback

        var inputRow = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(10, 6, 10, 0) };
        inputRow.Controls.Add(_input);
        inputRow.Controls.Add(prompt);

        // Verdict banner
        _verdict.Dock = DockStyle.Top;
        _verdict.Height = 44;
        _verdict.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        _verdict.TextAlign = ContentAlignment.MiddleCenter;

        // Left: step-by-step trace
        StyleOutput(_trace);
        var traceGroup = new GroupBox { Text = "Step-by-step: which states are active?", Dock = DockStyle.Fill };
        traceGroup.Controls.Add(_trace);

        // Right: tabs
        StyleOutput(_orders);
        StyleOutput(_table);
        var tabs = new TabControl { Dock = DockStyle.Fill };
        var ordersTab = new TabPage("Orders of operations");
        var tableTab = new TabPage("NFA transition table");
        ordersTab.Controls.Add(_orders);
        tableTab.Controls.Add(_table);
        tabs.TabPages.Add(ordersTab);
        tabs.TabPages.Add(tableTab);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 520
        };
        split.Panel1.Controls.Add(traceGroup);
        split.Panel2.Controls.Add(tabs);

        // Order matters for docking: Fill first, then Top controls.
        Controls.Add(split);
        Controls.Add(_verdict);
        Controls.Add(inputRow);
    }

    private static void StyleOutput(TextBox box)
    {
        box.Multiline = true;
        box.ReadOnly = true;
        box.ScrollBars = ScrollBars.Both;
        box.WordWrap = false;
        box.Dock = DockStyle.Fill;
        box.Font = new Font("Consolas", 10);
        box.BackColor = Color.White;
    }

    // ── Reacting to the input ──────────────────────────────────

    private void Evaluate()
    {
        var result = _simulator.Run(_input.Text);

        ShowVerdict(result);
        _trace.Text = DescribeTrace(result);
        _orders.Text = result.Accepted
            ? DescribeOrders(_input.Text)
            : "Enter a valid expression to see how its operations can be grouped.";
    }

    private void ShowVerdict(SimulationResult result)
    {
        if (result.Accepted)
        {
            _verdict.Text = "✔ ACCEPTED – valid arithmetic expression";
            _verdict.BackColor = Color.FromArgb(200, 240, 200);
            _verdict.ForeColor = Color.DarkGreen;
        }
        else if (result.FailedAtIndex >= 0)
        {
            _verdict.Text = $"✘ REJECTED – every path died at character {result.FailedAtIndex + 1}";
            _verdict.BackColor = Color.FromArgb(250, 210, 210);
            _verdict.ForeColor = Color.DarkRed;
        }
        else
        {
            _verdict.Text = "✘ REJECTED – input ended too early (accept state not reached)";
            _verdict.BackColor = Color.FromArgb(250, 235, 200);
            _verdict.ForeColor = Color.SaddleBrown;
        }
    }

    // ── Text builders ──────────────────────────────────────────

    private static string DescribeTrace(SimulationResult result)
    {
        var sb = new StringBuilder();
        int n = 0;
        foreach (var step in result.Steps)
        {
            string states = step.Active.Count == 0
                ? "∅  (no path left)"
                : "{ " + string.Join(", ", step.Active) + " }";

            sb.AppendLine($"{n++,2}. {step.Description}");
            sb.AppendLine($"    {step.Active.Count} state(s) active: {states}");
        }
        return sb.ToString();
    }

    private static string DescribeOrders(string input)
    {
        var (nums, ops) = OrderOfOperations.Tokenize(input);
        var sb = new StringBuilder();

        double standard = OrderOfOperations.WithPrecedence(nums, ops);
        sb.AppendLine($"Normal precedence (* and / first):  {OrderOfOperations.Format(standard)}");
        sb.AppendLine();

        if (ops.Count > OrderOfOperations.MaxOperators)
        {
            sb.AppendLine($"{ops.Count} operators – too many to list every grouping");
            sb.AppendLine($"(limit is {OrderOfOperations.MaxOperators}).");
            return sb.ToString();
        }

        var groupings = OrderOfOperations.AllGroupings(nums, ops);
        int distinct = groupings.Select(g => g.Value).Distinct().Count();

        sb.AppendLine($"The NFA accepts this one string, but it can be read");
        sb.AppendLine($"{groupings.Count} different ways → {distinct} different answer(s):");
        sb.AppendLine();
        foreach (var g in groupings)
            sb.AppendLine($"  {g.Text,-35} = {OrderOfOperations.Format(g.Value)}");

        return sb.ToString();
    }

    private string DescribeNfa()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"States: {_nfa.States.Count}");
        sb.AppendLine($"Start : {_nfa.Start}");
        sb.AppendLine($"Accept: {_nfa.Accept}");
        sb.AppendLine();

        foreach (var s in _nfa.States)
        {
            string tag = s == _nfa.Start ? " (start)" : s.IsAccepting ? " (accept)" : "";
            sb.AppendLine($"{s}{tag}");
            foreach (var t in s.Transitions)
                sb.AppendLine($"    --{t.Label}--> {t.Target}");
        }
        return sb.ToString();
    }
}
