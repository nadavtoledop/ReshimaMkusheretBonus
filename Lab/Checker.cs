using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Hidden;

// =============================================================
//  הבודק. אין צורך לקרוא או לשנות את הקובץ הזה.
//  מריצים מהטרמינל:   dotnet run --project Lab
// =============================================================

public class Case
{
    public int Task;
    public int Index;
    public string ListText;
    public int[] List;
    public int X;
    public int Y;
    public string Expected;
    public bool Readable;
}

// The object the student writes cases into (see MyCases.cs).
public class Cases
{
    private List<Case> items = new List<Case>();
    private Dictionary<int, int> counters = new Dictionary<int, int>();

    public void T1(string list, int x, int expected)
    {
        Add(1, list, x, 0, expected.ToString(), true);
    }

    public void T2(string list, string expected)
    {
        Add(2, list, 0, 0, expected, false);
    }

    public void T3(string list, int x, int y, string expected)
    {
        Add(3, list, x, y, expected, false);
    }

    private void Add(int task, string list, int x, int y, string expected, bool expectedIsNumber)
    {
        if (!counters.ContainsKey(task))
            counters[task] = 0;
        counters[task]++;

        Case c = new Case();
        c.Task = task;
        c.Index = counters[task];
        c.ListText = list;
        c.X = x;
        c.Y = y;

        int[] parsedList;
        c.Readable = Lists.TryParse(list, out parsedList);
        c.List = parsedList;

        if (expectedIsNumber)
        {
            c.Expected = expected;
        }
        else
        {
            int[] parsedExpected;
            if (Lists.TryParse(expected, out parsedExpected))
                c.Expected = Lists.Join(parsedExpected);
            else
                c.Readable = false;
        }
        items.Add(c);
    }

    public List<Case> ForTask(int task)
    {
        List<Case> result = new List<Case>();
        foreach (Case c in items)
            if (c.Task == task)
                result.Add(c);
        return result;
    }
}

public static class Lists
{
    public static bool TryParse(string text, out int[] result)
    {
        result = null;
        if (text == null)
            return false;
        string t = text.Trim().Trim('[', ']').Trim();
        if (t == "" || t.ToLower() == "null")
        {
            result = new int[0];
            return true;
        }
        string[] parts = t.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        List<int> values = new List<int>();
        foreach (string part in parts)
        {
            int v;
            if (!int.TryParse(part, out v))
                return false;
            values.Add(v);
        }
        result = values.ToArray();
        return true;
    }

    public static string Join(int[] a)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < a.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append(a[i]);
        }
        return sb.ToString();
    }

    public static Node<int> Build(int[] a)
    {
        Node<int> first = null;
        for (int i = a.Length - 1; i >= 0; i--)
            first = new Node<int>(a[i], first);
        return first;
    }

    public static string Show(Node<int> first)
    {
        StringBuilder sb = new StringBuilder();
        int count = 0;
        Node<int> p = first;
        while (p != null)
        {
            if (count > 10000)
                return "LOOP";
            if (count > 0)
                sb.Append(", ");
            sb.Append(p.GetValue());
            count++;
            p = p.GetNext();
        }
        return sb.ToString();
    }
}

public static class Checker
{
    private const int TimeoutMs = 2000;
    private const int MaxCases = 4;

    // Runs a function safely: catches crashes, stops endless loops.
    private static string Call(Func<string> f)
    {
        string result = null;
        string error = null;
        System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                result = f();
            }
            catch (NotImplementedException)
            {
                error = "TODO";
            }
            catch (Exception)
            {
                error = "CRASH";
            }
        });
        if (!t.Wait(TimeoutMs))
            return "TIMEOUT";
        if (error != null)
            return error;
        return result;
    }

    private static string RunStudent(int task, int[] a, int x, int y)
    {
        switch (task)
        {
            case 1:
                return Tasks.CountBigger(Lists.Build(a), x).ToString();
            case 2:
                return Lists.Show(Tasks.DeleteOdd(Lists.Build(a)));
            case 3:
                {
                    Node<int> first = Lists.Build(a);
                    Tasks.InsertAfter(first, x, y);
                    return Lists.Show(first);
                }
        }
        return "?";
    }

    private static string Pretty(string result)
    {
        if (result == "TODO")
            return "not written yet";
        if (result == "CRASH")
            return "crashed";
        if (result == "TIMEOUT")
            return "endless loop";
        if (result == "LOOP")
            return "a list that loops forever";
        return "[" + result + "]";
    }

    private static string Dots(string name, int width)
    {
        string s = "  " + name + " ";
        while (s.Length < width)
            s += ".";
        return s + " ";
    }

    public static string Run()
    {
        StringBuilder r = new StringBuilder();
        Cases cases = new Cases();
        MyCases.Task1(cases);
        MyCases.Task2(cases);
        MyCases.Task3(cases);

        int totalCaught = 0;
        int functionsDone = 0;
        TaskDef[] tasks = Catalog.All();

        r.AppendLine("==================================================");
        r.AppendLine(" INTERNS LAB - linked lists");
        r.AppendLine(" Student: " + StudentName());
        r.AppendLine("==================================================");

        foreach (TaskDef task in tasks)
        {
            r.AppendLine();
            r.AppendLine("Task " + task.Number + " - " + task.Title);

            // 1. The student's cases: readable? expected result correct?
            List<Case> mine = cases.ForTask(task.Number);
            List<Case> valid = new List<Case>();
            List<string> caseNotes = new List<string>();
            foreach (Case c in mine)
            {
                string label = "case #" + c.Index + " (" + c.ListText + ")";
                if (!c.Readable)
                {
                    caseNotes.Add(label + ": can't read it - check the format");
                    continue;
                }
                Case cc = c;
                string correct = Call(() => task.Reference((int[])cc.List.Clone(), cc.X, cc.Y));
                if (correct != c.Expected)
                {
                    caseNotes.Add(label + ": the expected result is wrong - trace it again by hand");
                    continue;
                }
                valid.Add(c);
            }
            r.AppendLine(Dots("your cases", 22) + mine.Count + " written, " + valid.Count + " correct");
            if (mine.Count > MaxCases)
                r.AppendLine("      only cases #1-#" + MaxCases + " hunt interns; the rest only test your function");
            foreach (string note in caseNotes)
                r.AppendLine("      " + note);

            // 2. The student's own function: hidden checks + the student's valid cases.
            int passed = 0;
            bool notWritten = false;
            bool timedOut = false;
            List<string> failingHints = new List<string>();
            foreach (HiddenCheck check in task.Checks)
            {
                HiddenCheck hc = check;
                string want = Call(() => task.Reference((int[])hc.List.Clone(), hc.X, hc.Y));
                string got = Call(() => RunStudent(task.Number, (int[])hc.List.Clone(), hc.X, hc.Y));
                if (got == "TODO")
                {
                    notWritten = true;
                    break;
                }
                if (got == want)
                {
                    passed++;
                }
                else
                {
                    failingHints.Add(hc.Hint);
                    if (got == "TIMEOUT")
                    {
                        timedOut = true;
                        failingHints.Add("endless loop - stopped checking");
                        break;
                    }
                }
            }
            if (notWritten)
            {
                r.AppendLine(Dots("your function", 22) + "not written yet");
            }
            else
            {
                r.AppendLine(Dots("your function", 22) + "hidden checks " + passed + "/" + task.Checks.Length);
                if (failingHints.Count > 0)
                {
                    r.AppendLine("      fails on: " + string.Join("; ", failingHints));
                    r.AppendLine("      (this says WHERE it broke, not WHY - the bug may be somewhere else)");
                }
                foreach (Case c in valid)
                {
                    if (timedOut)
                        break;
                    Case cc = c;
                    string got = Call(() => RunStudent(task.Number, (int[])cc.List.Clone(), cc.X, cc.Y));
                    if (got != c.Expected)
                        r.AppendLine("      case #" + c.Index + ": your function gave " + Pretty(got) + ", expected [" + c.Expected + "]");
                }
                if (passed == task.Checks.Length)
                    functionsDone++;
            }

            // 3. The interns: caught by the first valid case that exposes them.
            int caught = 0;
            List<Case> hunters = new List<Case>();
            foreach (Case c in valid)
                if (c.Index <= MaxCases)
                    hunters.Add(c);
            r.AppendLine("  interns:");
            foreach (Intern intern in task.Interns)
            {
                string by = null;
                foreach (Case c in hunters)
                {
                    Case cc = c;
                    Intern ii = intern;
                    string got = Call(() => ii.Run((int[])cc.List.Clone(), cc.X, cc.Y));
                    if (got != c.Expected)
                    {
                        by = "case #" + c.Index;
                        break;
                    }
                }
                if (by != null)
                {
                    caught++;
                    r.AppendLine(Dots("  " + intern.Name, 22) + "CAUGHT by " + by);
                }
                else
                {
                    r.AppendLine(Dots("  " + intern.Name, 22) + "still alive");
                }
            }
            r.AppendLine(Dots("caught", 22) + caught);
            totalCaught += caught;
        }

        r.AppendLine();
        r.AppendLine("==================================================");
        r.AppendLine(" TOTAL");
        r.AppendLine("   functions fully correct : " + functionsDone + "/" + tasks.Length);
        r.AppendLine("   interns caught          : " + totalCaught);
        r.AppendLine("==================================================");
        return r.ToString();
    }

    private static string StudentName()
    {
        string file = FindAnswers();
        if (file == null)
            return "(Answers.md not found)";
        foreach (string line in File.ReadAllLines(file))
        {
            int i = line.IndexOf("שם:", StringComparison.Ordinal);
            if (i >= 0)
            {
                string name = line.Substring(i + 3).Replace("*", "").Trim();
                if (name.Length > 0)
                    return name;
            }
        }
        return "(write your name in Answers.md)";
    }

    private static string FindAnswers()
    {
        string[] starts = { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (string start in starts)
        {
            DirectoryInfo dir = new DirectoryInfo(start);
            for (int level = 0; level < 7 && dir != null; level++)
            {
                string candidate = Path.Combine(dir.FullName, "Answers.md");
                if (File.Exists(candidate))
                    return candidate;
                dir = dir.Parent;
            }
        }
        return null;
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = Encoding.UTF8;
        }
        catch (Exception)
        {
        }
        string report = Checker.Run();
        Console.WriteLine(report);

        // On GitHub, the same report is shown on the run's summary page.
        string summary = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
        if (!string.IsNullOrEmpty(summary))
            File.AppendAllText(summary, "```\n" + report + "\n```\n");
    }
}
