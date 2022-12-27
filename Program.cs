var lines = File.ReadAllLines("code.ev");
var code = "";

for (int i = 0; i < lines.Length; ++i)
    code += lines[i] + " ";

Console.WriteLine(Compiler.ToRPN(code).Print());
Compiler.Compile(code);
