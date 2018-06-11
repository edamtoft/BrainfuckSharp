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
    public static Expression<BrainfuckApp> Parse(string source)
    {
      var memory = Expression.Parameter(typeof(byte[]), "memory");
      var input = Expression.Parameter(typeof(TextReader), "input");
      var output = Expression.Parameter(typeof(TextWriter), "output");

      var pointer = Expression.Variable(typeof(int), "pointer");

      var block = Expression.Block(new[] { pointer }, Parse(source, memory, pointer, input, output));

      return Expression.Lambda<BrainfuckApp>(block, memory, input, output);
    }

    public static BrainfuckApp Compile(string source) => Parse(source).Compile();

    public static void Run(string source, byte[] memory = null, TextReader @in = null, TextWriter @out = null)
    {
      Compile(source)(
        memory ?? new byte[1024], 
        @in ?? Console.In, 
        @out ?? Console.Out);
    }

    private static IEnumerable<Expression> Parse(string source, Expression memory, Expression pointer, Expression @in, Expression @out)
    {
      var labels = new Stack<(LabelTarget start, LabelTarget end)>();
      foreach (var c in source)
      {
        switch (c)
        {
          case '>':
            yield return Expression.PreIncrementAssign(pointer);
            break;
          case '<':
            yield return Expression.PreDecrementAssign(pointer);
            break;
          case '+':
            yield return Expression.Call(typeof(Ops), nameof(Ops.IncrementData), Type.EmptyTypes, memory, pointer);
            break;
          case '-':
            yield return Expression.Call(typeof(Ops), nameof(Ops.DecrementData), Type.EmptyTypes, memory, pointer);
            break;
          case '.':
            yield return Expression.Call(typeof(Ops), nameof(Ops.Write), Type.EmptyTypes, memory, pointer, @out);
            break;
          case ',':
            yield return Expression.Call(typeof(Ops), nameof(Ops.Read), Type.EmptyTypes, memory, pointer, @in);
            break;
          case '[':
            {
              var start = Expression.Label("start");
              var end = Expression.Label("end");
              labels.Push((start, end));
              yield return Expression.Label(start);
              yield return Expression.IfThen(
                Expression.Equal(Expression.ArrayAccess(memory, pointer), Expression.Constant((byte)0b0)),
                Expression.Goto(end));
            }
            break;
          case ']':
            {
              var (start, end) = labels.Pop();
              yield return Expression.IfThen(
                Expression.NotEqual(Expression.ArrayAccess(memory, pointer), Expression.Constant((byte)0b0)),
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
