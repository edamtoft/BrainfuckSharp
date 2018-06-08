using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BrainfuckSharp
{
  public static class Brainfuck
  {
    public static Action<int[]> Compile(string source)
    {
      var dataArray = Expression.Parameter(typeof(int[]), "dataArray");
      var dataPointer = Expression.Variable(typeof(int), "dataPointer");
      var expressions = Parse(source, dataArray, dataPointer);
      var block = Expression.Block(new[] { dataPointer }, expressions);
      return Expression.Lambda<Action<int[]>>(block, dataArray).Compile();
    }

    private static IEnumerable<Expression> Parse(string source, ParameterExpression dataArray, ParameterExpression dataPointer)
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
            yield return Expression.Call(typeof(Console), nameof(Console.Write), Type.EmptyTypes, Expression.Convert(data, typeof(char)));
            break;
          case ',':
            yield return Expression.Assign(data, Expression.Call(typeof(Console), nameof(Console.Read), Type.EmptyTypes));
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
          default:
            break;
        }
      }
    }
  }
}
