using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Math : Library
{
    public Math() : base("Math")
    {
        Functions = new string[] { "sin", "cos", "tg", "ctg", "log", "ln", "round" };
        Constants = new Variable[]
        {
            new Variable("num", "pi", MathF.PI.ToString()),
            new Variable("num", "e", MathF.E.ToString()),
        };
    }

    public override dynamic ExecuteFunction(string name, ref Stack<string> stack)
    {
        var a = "";
        var b = "";

        switch (name)
        {
            case "round":
                b = stack.Pop();
                a = stack.Pop();
                return MathF.Round(float.Parse(a), int.Parse(b));

            case "sin":
                a = stack.Pop();
                return MathF.Sin(float.Parse(a));

            case "cos":
                a = stack.Pop();
                return MathF.Cos(float.Parse(a));

            case "tg":
                a = stack.Pop();
                return MathF.Tan(float.Parse(a));

            case "ctg":
                a = stack.Pop();
                return 1 / MathF.Tan(float.Parse(a));

            case "ln":
                a = stack.Pop();
                return MathF.Log(float.Parse(a));

            case "log":
                a = stack.Pop();
                b = stack.Pop();
                return MathF.Log(float.Parse(a), float.Parse(b));
        }

        throw new Exception("WRONG FUNCTION NAME!");
    }
}
