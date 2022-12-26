using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Parser
{
    private static void AddSpaces(ref string code, char operand)
    {
        var s1 = "";

        for (int i = 0; i < code.Length; ++i)
        {
            if (code[i] != operand)
                s1 += code[i];
            else
                s1 += " " + code[i] + " ";
        }

        code = s1;
    }

    public static Queue<string> Parse(string code)
    {
        code = code.Replace("\n", "").Replace("\t", "");
        var result = new Queue<string>();

        var operators = new char[] { '"', '\'', '{', '}', '(', ')', ';', '&' };

        for (int i = 0; i < operators.Length; ++i)
            AddSpaces(ref code, operators[i]);

        var tokens = code.Split(' ');
        var firstChar = ' ';
        var token = "";
        bool stringFound = false;
        int n = 0;

        for (int i = 0; i < tokens.Length; ++i)
        {
            if (tokens[i] != "" && tokens[i] != " ")
            {
                if (stringFound)
                {
                    if (firstChar == '}')
                    {
                        if (tokens[i].Contains('{'))
                            ++n;

                        if (tokens[i].Contains(firstChar))
                        {
                            if (n != 0)
                            {
                                token += tokens[i] + " ";
                                --n;
                            }
                            else
                            {
                                token = token.Remove(token.Length - 1) + tokens[i];
                                result.Enqueue(token);
                                stringFound = false;
                                token = "";
                            }
                        }
                        else
                            token += tokens[i] + " ";
                    }
                    else
                    {
                        if (tokens[i].Contains(firstChar))
                        {
                            token = token.Remove(token.Length - 1) + tokens[i];
                            result.Enqueue(token);
                            stringFound = false;
                            token = "";
                        }
                        else
                            token += tokens[i] + " ";
                    }
                }
                else
                {
                    if (tokens[i].Contains("\"") || tokens[i].Contains("'"))
                    {
                        token = tokens[i];
                        firstChar = tokens[i][0];
                        stringFound = true;
                        continue;
                    }
                    else if (tokens[i].Contains('{'))
                    {
                        token = tokens[i];
                        firstChar = '}';
                        stringFound = true;
                        continue;
                    }

                    result.Enqueue(tokens[i]);
                }
            }
        }

        return result;
    }

    public static string Print<T>(this Queue<T> queue)
    {
        var result = "";

        for (int i = 0; i < queue.Count; ++i)
            result += queue.ElementAt(i).ToString() + " ";

        return result;
    }

    public static string Print<T>(this Stack<T> stack)
    {
        var result = "";

        for (int i = 0; i < stack.Count; ++i)
            result += stack.ElementAt(i).ToString() + "\n";

        return result;
    }
}
