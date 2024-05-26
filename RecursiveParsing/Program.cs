using System.Collections;
using static RecursiveParsing.Operator;

namespace RecursiveParsing;

public abstract class Expr
{
    public string Lexem = "";
    public Expr() { }
    public Expr(string lexem)
    {
        Lexem = lexem;
    }

    public override string ToString() => Lexem;
}

public class IntValue : Expr 
{
    public int Val { get; set; }

    public IntValue(int val, string lexem) : base(lexem)
    {
        Val = val;
        (val.ToString() == lexem).MustBeTrue();
    }
}

public class OperatorExpr : Expr 
{
    public Operator Op;
    public int Precedence => Op.Precedence;
    public AssociativityKind Associativity => Op.Associativity;

    public OperatorExpr(Operator op, string lexem) : base(lexem)
    {
        Op = op;
    }
}

public class BinaryExpr : Expr
{
    public Expr Operator;
    public Expr Left;
    public Expr Right;

    public BinaryExpr(Expr op, Expr left, Expr right)
    {
        Operator = op;
        Left = left;
        Right = right;
    }
    public override string ToString() => "(" + Left + " " + Operator + " " + Right + ")";
}

public class OperatorManager : IEnumerable<Operator>
{
    private List<Operator> All = new();

    public void Clear() 
    {
        All.Clear();
    }

    public OperatorManager() { }

    public Operator Add(int precedence, AssociativityKind associativity, params string[] lexems) => Add(new Operator(precedence, associativity, lexems));
    public Operator Add(Operator op) 
    {
        All.Add(op);
        All = All.OrderBy(a => a.Precedence).ToList();
        return op;
    }

    public IEnumerator<Operator> GetEnumerator() => ((IEnumerable<Operator>)All).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)All).GetEnumerator();
}

public class Operator 
{
    public List<string> Lexem { get; private set; }
    public AssociativityKind Associativity { get; private set; }
    public int Precedence { get; private set; }

    public Operator(int precedence, AssociativityKind associativity, params string[] lexems) : this(precedence, associativity, lexems.ToList()) { }
    public Operator(int precedence, AssociativityKind associativity, List<string> lexems)
    {
        Precedence = precedence;
        Associativity = associativity;
        Lexem = lexems;
    }

    public enum AssociativityKind { LeftToRight = 1, RightToLeft = 0, };
    public override string ToString() => "operator (" + string.Join(", ", Lexem) + ") with precedence " + Precedence + " and " + (Associativity == AssociativityKind.LeftToRight ? ">>>" : "<<<");
}

/// <summary>
/// Inspired by https://www.youtube.com/watch?v=fIPO4G42wYE&t=4683s
/// Thank you Jonathan Blow and Casey Muratori
/// </summary>
public class Parser 
{
    private string Input = "";
    private int Idx = 0;
    private List<int> Cp = new();

    public void AddCp() { Cp.Add(Idx); }
    public void RollBackCp() { RollBackCp(); PopCp(); }
    public void RollBackAndRemoveCp() { Idx = Cp.Peek(); }
    public void PopCp() { Cp.Pop();  }

    /// <summary>  Current Idx is excluded </summary>
    public string CpLexem() => Input.Substring(Cp.Peek(), Idx - Cp.Peek());

    public bool IdxValid => Idx >= 0 && Idx < Input.Length;
    public char Observe  => IdxValid ? Input[Idx] : '\0';
    public void Move()   => Idx++;
    public char Read()   => Input[Idx++];

    public OperatorManager Operators { get; private set; } = new();

    public Parser() { }

    public bool ParseUseless() 
    {
        if (Observe.IsUseless()) 
        {
            do
            {
                Idx++;
            } while (Observe.IsUseless());
            return true;
        }
        return false;
    }

    public bool MatchKeywordNoCp(string kw)
    {
        foreach (var c in kw)
        {
            if (Read() != c) { return false; }
        }
        return true;
    }
    public bool MatchKeyword(string kw)
    {
        if (kw.Length == 0) { return true; }
        if (Observe == kw[0])
        {
            AddCp();
            if (MatchKeywordNoCp(kw))
            {
                PopCp();
                return true;
            }
            RollBackAndRemoveCp();
        }
        return false;
    }

    public OperatorExpr? ParseOperator()
    {
        // Can do a better structure to handle it within the OperatorManager but this is a toy project
        AddCp();

        foreach (var op in Operators)
        {
            foreach (var lexem in op.Lexem)
            {
                if (MatchKeyword(lexem))
                {
                    PopCp();
                    return JustParsed(new OperatorExpr(op, lexem));
                }
            }
        }

        PopCp();
        return null;
    }


    public Expr? Parse(string input)
    {
        Input = input;
        Idx = 0;
        Cp.Clear();

        ParseUseless();
        return ParseExpr();
    }

    public T JustParsed<T>(T val) where T : Expr 
    {
        Console.WriteLine("Parsed " + val.ToString());
        ParseUseless(); 
        return val; 
    }

    public Expr? ParseExpr() => ParseIncreasingPrecedence();

    public Expr? ParseTerminal()
    {
        var nb = Observe.TryAsNumber();
        if (nb.HasValue) 
        {
            AddCp();
            int val = 0;
            do
            {

                // Simple (poor) way to handle overflow
                try
                {
                    checked { val = val * 10 + nb.Value; }
                }
                catch (Exception)
                {
                    PopCp();
                    throw;
                }

                Move();
                nb = Observe.TryAsNumber();
                
            }while(nb.HasValue);

            var parsed = new IntValue(val, CpLexem());
            PopCp();
            return JustParsed(parsed);
        }
        return null;
    }

    public Expr? ParseWithCustomPrecedence(bool IncreasingPrecedence, int previousPrecedence = int.MinValue)
    {
        Expr? left = ParseTerminal();
        if (left == null) { return null; }

        while (true)
        {
            AddCp();
            var op = ParseOperator();
            if (op == null) { PopCp(); return left; }

            //Console.WriteLine(left +" " +op+" "+(IncreasingPrecedence ? "++" : "--") + "precedence : " + op.Op);

            if(
                (  IncreasingPrecedence  && !(op.Precedence >= previousPrecedence)) 
                //|| ((!IncreasingPrecedence) && !(op.Precedence <= previousPrecedence))
                )
            { RollBackAndRemoveCp(); return left; }

            Expr? right = IncreasingPrecedence ? ParseWithCustomPrecedence(op.Precedence >= previousPrecedence, op.Precedence + (int)op.Associativity) : ParseTerminal();
            PopCp();
            if (right == null) { return left; } 

            left = new BinaryExpr(op, left, right);
        }
    }

    public Expr? ParseIncreasingPrecedence() => ParseWithCustomPrecedence(true);
    public Expr? ParseDecreasingPrecedence() => ParseWithCustomPrecedence(false);
}

public class Program
{
    public Program() { }

    public void LoadOperator(OperatorManager operators) 
    {
        int precedence = 0;
        operators.Add(precedence, AssociativityKind.RightToLeft, "=");

        precedence++;
        operators.Add(precedence, AssociativityKind.LeftToRight, "+");
        operators.Add(precedence, AssociativityKind.LeftToRight, "-");

        precedence++;
        operators.Add(precedence, AssociativityKind.LeftToRight, "*");
        operators.Add(precedence, AssociativityKind.LeftToRight, "/");

        precedence++;
        operators.Add(precedence, AssociativityKind.RightToLeft, "^");
    }

    public void Run() 
    {
        var p = new Parser();
        LoadOperator(p.Operators);

        //var r = p.Parse("1234 + 42");
        //var r = p.Parse("1 + 2 * 3 + 4 + 5");

        //string input = "1 + 2 + 3 + 4 + 5 + 6 + 7 + 8 + 9";
        //string input = "1 = 2 = 3 = 4 = 5 = 6 = 7 = 8 = 9";

        //string input = "1 + 2 * 3 + 10 / 7 + 4 + 5";
        //string input = "1 + 2 * 4 + 5 ^ 7 ^ 8 * 9 + 2";
        string input = "1 + 2 * 4 + 12 * 5 ^ 7 ^ 8 + 123 ^ 8 * 9 + 2";
        //string input = "1 ^ 2 ^ 3 ^ 4";
        //string input = "12345678901234567891234567890123456789";
        //string input = "4  * 1 ^ 2";


        // Todo : add support for parenthesis
        //string input = "1 + 2 * 3 + (4 + 5 * 6 + 7)";

        Console.WriteLine(input);
        Console.WriteLine(p.Parse(input));
    }

    static void Main(string[] args)
    {
        var p = new Program();
        p.Run();
        Console.WriteLine("Hello, World!");
    }
}
