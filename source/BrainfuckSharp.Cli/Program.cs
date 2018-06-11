using System;
using System.Runtime.InteropServices;

namespace BrainfuckSharp.Cli
{
  class Program
  {
    static void Main(string[] args)
    {
      var memory = new byte[1024];
      while (TryReadLine(out var line))
      {
        try
        {
          var script = Brainfuck.Compile(line);
          script.Invoke(memory, Console.In, Console.Out);
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
