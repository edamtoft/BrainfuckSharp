using System;

namespace BrainfuckSharp.Cli
{
  class Program
  {
    static void Main(string[] args)
    {
      var memory = new int[1024];
      while (true)
      {
        var script = Brainfuck.Compile(Console.ReadLine());
        script.Invoke(memory);
      }
    }
  }
}
