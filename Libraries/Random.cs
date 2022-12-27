using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Random : Library
{
    private System.Random Rand = new System.Random();

    public Random() : base("Random")
    {
        Functions = new[] { "rand", "randValue" };
        Constants = new Variable[0];
    }

    public override dynamic ExecuteFunction(string name, ref Stack<string> stack)
    {
        switch (name)
        {
            case "rand":
                var b = stack.Pop();
                var a = stack.Pop();

                return float.Parse(a) + (float)Rand.NextDouble() * float.Parse(b);

            case "randValue":
                return (float)Rand.NextDouble();
        }

        throw new Exception("WRONG FUNCTION NAME!");
    }
}
