using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BrainfuckSharp.Cli
{
  class Program
  {
    static void Main(string[] args)
    {
      while (TryReadLine(out var line))
      {
        try
        {
          var script = Brainfuck.Compile(line.AsReadOnlySpan());
          script(Console.In, Console.Out);
        }
        catch (BrainfuckException err)
        {
          Console.Error.WriteLine($"Parser Exception: {err.Message}");
        }
      }
    }

    static bool TryReadLine(out string line)
    {
      line = Console.ReadLine();
      return !string.IsNullOrEmpty(line);
    }
  }
}
