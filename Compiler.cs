using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Variable
{
    private string Type;
    public string Name;
    private string Value;

    public Variable(string type, string name, string value)
    {
        Type = type;
        Name = name;
        Value = value;
    }

    public void SetValue(string value)
    {
        Value = value;
    }

    public dynamic GetValue()
    {
        switch (Type)
        {
            case "num":
                return float.Parse(Value);

            case "text":
                return Value;

            case "char":
                return char.Parse(Value);

            case "flag":
                return bool.Parse(Value);
        }

        throw new Exception("NON EXISTING DATA TYPE!");
    }

    public override string ToString()
    {
        return Type + " " + Name + " = " + Value;
    }
}

public class Function
{
    public string Type;
    public string Name;
    public string[] Args;
    public string Code;

    public Function(string type, string name, string[] args, string code)
    {
        Type = type;
        Name = name;
        Args = args;
        Code = code;
    }
}

public static class Compiler
{
    private static List<string> DataTypes = new() { "num", "text", "char", "flag" };
    private static List<string> Functions = new() { "array", "exec", "func", "while", "if", "elseif", "else", "equals", "get", "set", "print", "read" };
    private static List<string> Operators = new() { "<", ">", "<=", ">=", "==", "!=", "^^", "||", "&", "^", "*", "/", "+", "-" };
    private static List<string> KeyWords = new() { "break", "continue" };

    private static Dictionary<string, int> ExecutionOrder = new()
    {
        { "&", 3 },
        { "<", 2 },
        { ">", 2 },
        { "<=", 2 },
        { ">=", 2 },
        { "==", 2 },
        { "!=", 2 },
        { "^", 2 },
        { "^^", 1 },
        { "||", 1 },
        { "*", 1 },
        { "/", 1 },
        { "+", 0 },
        { "-", 0 }
    };

    private static bool LastConditionFalse;
    private static List<bool> BreakLoop = new();
    private static List<bool> ContinueLoop = new();
    private static int CurrentLoop = -1;

    private static List<Variable> Variables = new();
    private static List<Function> CustomFunctions = new();
    private static List<Variable[]> Arrays = new();

    public static Queue<string> ToRPN(string code)
    {
        var tokens = Parser.Parse(code);
        var stack = new Stack<string>();
        var output = new Queue<string>();

        var token = "";
        var o2 = "";
        var number = 0f;

        while (tokens.TryDequeue(out token))
        {
            switch (token)
            {
                case string s when float.TryParse(s, out number):
                    output.Enqueue(s);
                    break;

                case string s when s.Contains('"') || s.Contains('\'') || s.Contains('{'):
                    output.Enqueue(s);
                    break;

                case string s when KeyWords.Contains(s):
                    output.Enqueue(s);
                    break;

                case string s when DataTypes.Contains(s):
                    stack.Push(s);
                    break;

                case string s when Functions.Contains(s):
                    stack.Push(s);
                    break;

                case string s when Operators.Contains(s):
                    while (stack.TryPeek(out o2) && Operators.Contains(o2) && ExecutionOrder[s] <= ExecutionOrder[o2])
                        output.Enqueue(stack.Pop());

                    stack.Push(s);
                    break;

                case string s when s == "(":
                    stack.Push(s);
                    break;

                case string s when s == ";":
                    while (stack.TryPeek(out o2))
                    {
                        if (o2 == "(")
                            break;

                        output.Enqueue(stack.Pop());
                    }

                    break;

                case string s when s == ")":
                    while (stack.TryPop(out o2))
                    {
                        if (o2 == "(")
                        {
                            if (stack.TryPeek(out o2) && Functions.Contains(o2))
                                output.Enqueue(stack.Pop());
                            else if (stack.TryPeek(out o2) && DataTypes.Contains(o2))
                                output.Enqueue(stack.Pop());
                            else if (stack.TryPeek(out o2) && Operators.Contains(o2))
                                output.Enqueue(stack.Pop());

                            break;
                        }

                        output.Enqueue(o2);
                    }

                    break;

                default:
                    output.Enqueue(token);
                    break;
            }
        }

        while (stack.TryPop(out token))
            output.Enqueue(token);

        return output;
    }

    public static void Compile(string code)
    {
        var tokens = ToRPN(code);
        var token = "";
        var number = 0f;

        var stack = new Stack<string>();

        while (tokens.TryDequeue(out token))
        {
            switch (token)
            {
                case string s when s.Contains('"') || s.Contains('\'') || s.Contains('{'):
                    stack.Push(s);
                    break;

                case string s when float.TryParse(s, out number):
                    stack.Push(s);
                    break;

                case string s when KeyWords.Contains(s):
                    try
                    {
                        ExecuteFunction(s, ref stack);
                    }
                    catch (Exception e)
                    {
                        if (e.Message == "break" || e.Message == "Continue")
                            return;
                    }
                    break;

                case string s when Operators.Contains(s):
                    var a = "";
                    var b = "";

                    switch (s)
                    {
                        case "&":
                            a = stack.Pop();

                            if (Variables.Find(var => var.Name == a) == null && Arrays.Find(arr => arr[0].Name == a) == null)
                                throw new Exception("VARIABLE DOESN'T EXIST!");
                            else if (Variables.Find(var => var.Name == a) != null)
                                stack.Push(Variables.Find(var => var.Name == a).GetValue().ToString());
                            else if (Arrays.Find(arr => arr[0].Name == a) != null)
                            {
                                var i = int.Parse(stack.Pop());
                                stack.Push(Arrays.Find(arr => arr[0].Name == a)[i].GetValue().ToString());
                            }

                            break;

                        default:
                            a = stack.Pop();
                            b = stack.Pop();

                            stack.Push(ExecuteOperation(float.Parse(b), float.Parse(a), token).ToString());
                            break;
                    }

                    break;

                case string s when DataTypes.Contains(s):
                    var value = stack.Pop();
                    var name = stack.Pop();

                    if (name.Contains('"'))
                        name = name.Replace("\"", "");

                    if (value.Contains('"'))
                        value = value.Replace("\"", "");

                    if (Variables.Find(var => var.Name == name) != null)
                        Variables.Find(var => var.Name == name).SetValue(value);
                    else
                        Variables.Add(new Variable(s, name, value));

                    break;

                case string s when Functions.Contains(s):
                    try
                    {
                        stack.Push(ExecuteFunction(s, ref stack).ToString());
                    }
                    catch (Exception e)
                    {
                        if (e.Message != "void")
                            Console.WriteLine(e.Message);
                    }

                    break;

                default:
                    stack.Push(token);
                    break;
            }
        }
    }

    public static void Compile(string code, ref Stack<string> stack)
    {
        var tokens = ToRPN(code);
        var token = "";
        var number = 0f;

        while (tokens.TryDequeue(out token))
        {
            switch (token)
            {
                case string s when s.Contains('"') || s.Contains('\'') || s.Contains('{'):
                    stack.Push(s);
                    break;

                case string s when float.TryParse(s, out number):
                    stack.Push(s);
                    break;

                case string s when KeyWords.Contains(s):
                    try
                    {
                        ExecuteFunction(s, ref stack);
                    }
                    catch (Exception e)
                    {
                        if (e.Message == "break" || e.Message == "Continue")
                            return;
                    }
                    break;

                case string s when Operators.Contains(s):
                    var a = "";
                    var b = "";

                    switch (s)
                    {
                        case "&":
                            a = stack.Pop();

                            if (Variables.Find(var => var.Name == a) == null && Arrays.Find(arr => arr[0].Name == a) == null)
                                throw new Exception("VARIABLE DOESN'T EXIST!");
                            else if (Variables.Find(var => var.Name == a) != null)
                                stack.Push(Variables.Find(var => var.Name == a).GetValue().ToString());
                            else if (Arrays.Find(arr => arr[0].Name == a) != null)
                            {
                                var i = int.Parse(stack.Pop());
                                stack.Push(Arrays.Find(arr => arr[0].Name == a)[i].GetValue().ToString());
                            }

                            break;

                        default:
                            a = stack.Pop();
                            b = stack.Pop();

                            stack.Push(ExecuteOperation(float.Parse(b), float.Parse(a), token).ToString());
                            break;
                    }

                    break;

                case string s when DataTypes.Contains(s):
                    var value = stack.Pop();
                    var name = stack.Pop();

                    if (name.Contains('"'))
                        name = name.Replace("\"", "");

                    if (value.Contains('"'))
                        value = value.Replace("\"", "");

                    if (Variables.Find(var => var.Name == name) != null)
                        Variables.Find(var => var.Name == name).SetValue(value);
                    else
                        Variables.Add(new Variable(s, name, value));

                    break;

                case string s when Functions.Contains(s):
                    try
                    {
                        stack.Push(ExecuteFunction(s, ref stack).ToString());
                    }
                    catch (Exception e) 
                    {
                        if (e.Message != "void")
                            Console.WriteLine(e.Message);
                    }
                    
                    break;

                default:
                    stack.Push(token);
                    break;
            }
        }
    }

    public static float ExecuteOperation(float a, float b, string operand)
    {
        switch (operand)
        {
            case "==":
                return a == b ? 1.0f : 0.0F;

            case "!=":
                return a != b ? 1.0f : 0.0F;

            case "<":
                return a < b ? 1.0f : 0.0F;

            case ">":
                return a > b ? 1.0f : 0.0F;

            case ">=":
                return a >= b ? 1.0f : 0.0F;

            case "<=":
                return a <= b ? 1.0f : 0.0F;

            case "^^":
                return ((a == 1.0f && b == 1.0f) || (a == 0.0f && b == 0.0f)) ? 1.0f : 0.0f;

            case "||":
                return (!(a == 0.0f && b == 0.0f)) ? 1.0f : 0.0f;

            case "^":
                return MathF.Pow(a, b);

            case "*":
                return a * b;

            case "/":
                return a / b;

            case "+":
                return a + b;

            case "-":
                return a - b;
        }

        throw new Exception("WRONG OPERATOR!");
    }

    private static void BreakCurrentLoop()
    {
        BreakLoop[CurrentLoop] = true;
    }

    private static void ContinueCurrentLoop()
    {
        ContinueLoop[CurrentLoop] = true;
    }

    public static dynamic ExecuteFunction(string name, ref Stack<string> stack)
    {
        var a = "";
        var b = "";
        var code = "";
        var condition = "";

        switch (name)
        {
            case "break":
                BreakCurrentLoop();
                throw new Exception("break");

            case "continue":
                ContinueCurrentLoop();
                throw new Exception("continue");

            case "array":
                var length = int.Parse(stack.Pop());
                var arrayName = stack.Pop();
                var arrayType = stack.Pop().Replace("\"", "");

                var array = new Variable[length];

                for (int i = 0; i < length; ++i)
                    array[i] = new Variable(arrayType, arrayName, "");

                Arrays.Add(array);
                throw new Exception("void");

            case "exec":
                a = stack.Pop();

                var func = CustomFunctions.Find(var => var.Name == a);

                for (int i = 0; i < func.Args.Length; ++i)
                {
                    b = string.Format(func.Args[i].Replace("\"", "").Replace(" ", ""), stack.Pop());
                    Compile(b);
                }

                Compile(func.Code.Remove(0, 1).Remove(func.Code.Length - 2), ref stack);

                for (int i = 0; i < func.Args.Length; ++i)
                    Variables.RemoveAt(Variables.Count - 2);

                if (func.Type != "void")
                {
                    var r = Variables[Variables.Count - 1].GetValue();
                    Variables.RemoveAt(Variables.Count - 1);
                    return r;
                }

                throw new Exception("void");

            case "func":
                var execute = stack.Pop();

                var args = new List<string>();

                while (stack.Count > 2)
                    args.Add(stack.Pop());

                var function = stack.Pop().Replace("\"", ""); 
                var type = stack.Pop().Replace("\"", "");

                CustomFunctions.Add(new Function(type, function, args.ToArray(), execute));
                throw new Exception("void");

            case "equals":
                a = stack.Pop();
                b = stack.Pop();

                return b.Equals(a) ? 1.0f : 0.0f;

            case "while":
                code = stack.Pop();
                condition = stack.Pop();
                var c = "1";
                var keyword = "";
                ++CurrentLoop;
                BreakLoop.Add(false);
                ContinueLoop.Add(false);

                do
                {
                    Compile(condition.Replace("\"", ""), ref stack);
                    c = stack.Pop();

                    if (c != "1")
                        break;

                    Compile(code.Remove(0, 1).Remove(code.Length - 2));

                    if (BreakLoop[CurrentLoop])
                        break;

                    if (ContinueLoop[CurrentLoop])
                    {
                        ContinueLoop[CurrentLoop] = false;
                        continue;
                    }
                }
                while (c == "1");

                BreakLoop.RemoveAt(CurrentLoop);
                ContinueLoop.RemoveAt(CurrentLoop);
                --CurrentLoop;
                throw new Exception("void");


            case "if":
                code = stack.Pop();
                condition = stack.Pop();

                if (condition == "1")
                {
                    Compile(code.Remove(0, 1).Remove(code.Length - 2), ref stack);
                    LastConditionFalse = false;
                }
                else
                    LastConditionFalse = true;

                throw new Exception("void");

            case "elseif":
                if (!LastConditionFalse)
                    throw new Exception("void");

                code = stack.Pop();
                condition = stack.Pop();

                if (condition == "1")
                {
                    Compile(code.Remove(0, 1).Remove(code.Length - 2), ref stack);
                    LastConditionFalse = false;
                }
                else
                    LastConditionFalse = true;

                throw new Exception("void");

            case "else":
                if (!LastConditionFalse)
                    throw new Exception("void");

                code = stack.Pop();
                Compile(code.Remove(0, 1).Remove(code.Length - 2), ref stack);

                throw new Exception("void");

            case "get":
                a = stack.Pop();

                if (a.Contains('"'))
                    a = a.Replace("\"", "");

                if (Variables.Find(var => var.Name == a) == null && Arrays.Find(arr => arr[0].Name == a) == null)
                    throw new Exception("VARIABLE DOESN'T EXIST!");
                else if (Variables.Find(var => var.Name == a) != null)
                    return Variables[Variables.FindIndex(var => var.Name == a)].GetValue();
                else
                    return Arrays[Arrays.FindIndex(arr => arr[0].Name == a)][int.Parse(stack.Pop())].GetValue();

                throw new Exception("void");

            case "set":
                a = stack.Pop();
                b = stack.Pop();

                if (a.Contains('"'))
                    a = a.Replace("\"", "");

                if (b.Contains('"'))
                    b = b.Replace("\"", "");

                if (Variables.Find(var => var.Name == b) == null && Arrays.Find(arr => arr[0].Name == b) == null)
                    throw new Exception("VARIABLE DOESN'T EXIST!");
                else if (Variables.Find(var => var.Name == b) != null)
                    Variables[Variables.FindIndex(var => var.Name == b)].SetValue(a);
                else
                    Arrays[Arrays.FindIndex(arr => arr[0].Name == b)][int.Parse(a)].SetValue(stack.Pop());

                throw new Exception("void");

            case "print":
                var argument = stack.Pop();
                
                if (argument.Contains('"'))
                    argument = argument.Replace("\"", "");

                if (argument.Contains('\''))
                    argument = argument.Replace("\'", "");

                Console.WriteLine(argument);
                throw new Exception("void");

            case "read":
                var arg = stack.Pop();

                if (arg == "line")
                    return Console.ReadLine();
                else if (arg == "char")
                    return Console.Read();
                
                throw new Exception("WRONG ARGUMENT!");
        }

        throw new Exception("WRONG FUNCTION NAME!");
    }

}