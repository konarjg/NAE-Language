using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class Library
{
    public string Name;
    public string[] Functions;
    public Variable[] Constants;
    public bool Imported;

    public Library(string name)
    {
        Name = name;
    }

    public abstract dynamic ExecuteFunction(string name, ref Stack<string> stack);
}
