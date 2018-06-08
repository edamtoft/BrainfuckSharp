using System;
using System.Collections.Generic;
using System.Text;

namespace BrainfuckSharp
{
  /// <summary>
  /// Represents an error during compilation of brainfuck
  /// </summary>
  public class BrainfuckException : Exception
  {
    public BrainfuckException(string message) : base(message)
    {
    }
  }
}
