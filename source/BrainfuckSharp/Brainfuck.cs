using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace BrainfuckSharp
{
  public static class Brainfuck
  {
    /// <summary>
    /// Compiles a brainfuck string to an executable Linq expression
    /// </summary>
    /// <param name="source">Brainfuck source</param>
    public static Action<int[],TextReader,TextWriter> Compile(string source)
    {
      var dataArray = Expression.Parameter(typeof(int[]), "dataArray");
      var dataPointer = Expression.Variable(typeof(int), "dataPointer");
      var input = Expression.Variable(typeof(TextReader), "input");
      var output = Expression.Variable(typeof(TextWriter), "output");

      var expressions = Parse(source, dataArray, dataPointer, input, output);

      var block = Expression.Block(new[] { dataPointer }, expressions);
      return Expression.Lambda<Action<int[], TextReader, TextWriter>>(block, dataArray, input, output).Compile();
    }

    private static readonly MethodInfo Read = typeof(TextReader).GetMethod(nameof(TextReader.Read), Type.EmptyTypes);
    private static readonly MethodInfo Write = typeof(TextWriter).GetMethod(nameof(TextWriter.Write), new[] { typeof(char) });

    private static IEnumerable<Expression> Parse(string source, Expression dataArray, Expression dataPointer, Expression input, Expression output)
    {
      var labels = new Stack<(LabelTarget start, LabelTarget end)>();

      foreach (var c in source)
      {
        var data = Expression.ArrayAccess(dataArray, dataPointer);
        switch (c)
        {
          case '>':
            yield return Expression.PostIncrementAssign(dataPointer);
            break;
          case '<':
            yield return Expression.PostDecrementAssign(dataPointer);
            break;
          case '+':
            yield return Expression.PostIncrementAssign(data);
            break;
          case '-':
            yield return Expression.PostDecrementAssign(data);
            break;
          case '.':
            yield return Expression.Call(output, Write, Expression.Convert(data, typeof(char)));
            break;
          case ',':
            yield return Expression.Assign(data, Expression.Call(input, Read));
            break;
          case '[':
            {
              var start = Expression.Label();
              var end = Expression.Label();
              labels.Push((start, end));
              yield return Expression.Label(start);
              yield return Expression.IfThen(
                Expression.Equal(data, Expression.Constant(0)),
                Expression.Goto(end));
            }
            break;
          case ']':
            {
              var (start, end) = labels.Pop();
              yield return Expression.IfThen(
                Expression.NotEqual(data, Expression.Constant(0)),
                Expression.Goto(start));
              yield return Expression.Label(end);
            }
            break;
          case ' ':
            break;
          default:
            throw new BrainfuckException($"Syntax Error: Unexpected character '{c}'");
        }
      }

      if (labels.Count > 0)
      {
        throw new BrainfuckException($"Syntax Error: Unmatched open bracket");
      }
    }
  }
}
