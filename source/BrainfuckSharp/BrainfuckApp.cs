using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrainfuckSharp
{
  public delegate void BrainfuckApp(byte[] memory, TextReader @in, TextWriter @out);
}
