using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace BrainfuckSharp.Tests
{
  [TestClass]
  public class CompilerTests
  {
    [TestMethod]
    public void HelloWorld()
    {
      using (var output = new StringWriter())
      {
        var action = Brainfuck.Compile("++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.");
        action.Invoke(new int[7], null, output);
        Assert.AreEqual("Hello World!", output.ToString());
      }
    }

    [TestMethod]
    [ExpectedException(typeof(BrainfuckException))]
    public void UnmatchedBracketThrows()
    {
      Brainfuck.Compile("+++[+++");
    }

    [TestMethod]
    [ExpectedException(typeof(BrainfuckException))]
    public void InvalidCharactersThrow()
    {
      Brainfuck.Compile("abc123");
    }
  }
}
