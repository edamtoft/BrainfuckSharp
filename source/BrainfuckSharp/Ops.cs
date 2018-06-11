using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrainfuckSharp
{
  static class Ops
  {
    public static void IncrementData(byte[] memory, int pointer) => memory[pointer]++;
    public static void DecrementData(byte[] memory, int pointer) => memory[pointer]--;
    public static void Write(byte[] memory, int pointer, TextWriter @out) => @out.Write((char)memory[pointer]);
    public static void Read(byte[] memory, int pointer, TextReader @in) => memory[pointer] = (byte)@in.Read();
  }
}
