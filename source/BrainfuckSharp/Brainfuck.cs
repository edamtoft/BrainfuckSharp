using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace BrainfuckSharp
{
  public static class Brainfuck
  {
    /// <summary>
    /// Compiles a brainfuck string to an executable Linq expression
    /// </summary>
    /// <param name="source">Brainfuck source</param>
    public static BrainfuckApp Compile(ReadOnlySpan<char> source, int memorySize = 256)
    {
      var signature = typeof(BrainfuckApp).GetMethod(nameof(BrainfuckApp.Invoke));
      var method = new DynamicMethod("BrainfuckApp",
        returnType: signature.ReturnType,
        parameterTypes: signature.GetParameters().Select(p => p.ParameterType).ToArray());

      var il = method.GetILGenerator();

      il.DeclareLocal(typeof(byte[]));
      il.DeclareLocal(typeof(int));

      il.Emit(OpCodes.Ldc_I4, memorySize);
      il.Emit(OpCodes.Newarr, typeof(byte));
      il.Emit(OpCodes.Stloc_0);

      var labels = new Stack<Label>();

      for (var i = 0; i < source.Length; i++)
      {
        var c = source[i];
        switch (c)
        {
          case '>':
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_1);
            break;
          case '<':
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Stloc_1);
            break;
          case '+':
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldelem_U1, typeof(byte));
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stelem, typeof(byte));
            break;
          case '-':
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldelem, typeof(byte));
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Stelem, typeof(byte));
            break;
          case '.':
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldelem, typeof(byte));
            il.Emit(OpCodes.Callvirt, typeof(TextWriter).GetMethod(nameof(TextWriter.Write), new[] { typeof(char) }));
            break;
          case ',':
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Callvirt, typeof(TextReader).GetMethod(nameof(TextReader.Read), Type.EmptyTypes));
            il.Emit(OpCodes.Stelem, typeof(byte));
            break;
          case '[':
            labels.Push(il.DefineLabel());
            labels.Push(il.DefineLabel());
            il.MarkLabel(labels.Peek());
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldelem, typeof(byte));
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Beq, labels.Peek());
            break;
          case ']':
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldelem, typeof(byte));
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Bne_Un, labels.Pop());
            il.MarkLabel(labels.Pop());
            break;
          case ' ':
            break;
          default:
            throw new BrainfuckException($"Syntax Error: Unexpected character '{c}'");
        }
      }

      il.Emit(OpCodes.Ret);

      if (labels.Count > 0)
      {
        throw new BrainfuckException($"Syntax Error: Unmatched brackets");
      }

      return method.CreateDelegate(typeof(BrainfuckApp)) as BrainfuckApp;
    }
  }
}
