
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace harlam357.Core.IO
{
   [TestFixture]
   public class BufferedTextFileReaderTests
   {
      [Test]
      public void BufferedTextFileReader_ReadAllLines_Test1()
      {
         var reader = new BufferedTextFileReader();
         List<string> lines = reader.ReadAllLines(Path.Combine(Environment.CurrentDirectory, @"TestFiles\gpl-3.0-1.txt")).ToList();
         Assert.AreEqual(24, lines.Count);
         lines = reader.ReadAllLines(Path.Combine(Environment.CurrentDirectory, @"TestFiles\gpl-3.0-2.txt")).ToList();
         Assert.AreEqual(40, lines.Count);
         lines = reader.ReadAllLines(Path.Combine(Environment.CurrentDirectory, @"TestFiles\gpl-3.0-3.txt")).ToList();
         Assert.AreEqual(48, lines.Count);
         var compareLines = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, @"TestFiles\gpl-3.0-3.txt"));
         Assert.IsTrue(lines.SequenceEqual(compareLines));
      }

      [Test]
      public void BufferedTextFileReader_ReadAllLines_Test2()
      {
         ReadAllLines_Internal_Test("First line.\r\n", "Second line.", "\r\nThird line.\r\n", "Fourth line.\r\n");
      }

      [Test]
      public void BufferedTextFileReader_ReadAllLines_Test3()
      {
         ReadAllLines_Internal_Test("First line.\n", "Second line.", "\nThird line.\n", "Fourth line.\n");
      }

      [Test]
      public void BufferedTextFileReader_ReadAllLines_Test4()
      {
         ReadAllLines_Internal_Test("First lin", "e.\r\nSecond line.\r\n", "Third", " line.\r\nFourth line.");
      }

      [Test]
      public void BufferedTextFileReader_ReadAllLines_Test5()
      {
         ReadAllLines_Internal_Test("First lin", "e.\nSecond line.\n", "Third", " line.\nFourth line.");
      }

      public void ReadAllLines_Internal_Test(params string[] input)
      {
         List<string> lines;
         var reader = new BufferedTextFileReader();

         byte[] buffer1 = Encoding.UTF8.GetBytes(input[0]);
         byte[] buffer2 = Encoding.UTF8.GetBytes(input[1]);
         byte[] buffer3 = Encoding.UTF8.GetBytes(input[2]);
         byte[] buffer4 = Encoding.UTF8.GetBytes(input[3]);

         using (var ms = new MemoryStream())
         {
            ms.Write(buffer1, 0, buffer1.Length);
            ms.Write(buffer2, 0, buffer2.Length);

            lines = reader.ReadAllLines(ms).ToList();
            Assert.AreEqual(2, lines.Count);
         }

         using (var ms = new MemoryStream())
         {
            ms.Write(buffer1, 0, buffer1.Length);
            ms.Write(buffer2, 0, buffer2.Length);
            ms.Write(buffer3, 0, buffer3.Length);
            ms.Write(buffer4, 0, buffer4.Length);

            lines = reader.ReadAllLines(ms).ToList();
            Assert.AreEqual(4, lines.Count);
         }

         Assert.IsTrue(lines.SequenceEqual(CreateCompareLines()));
      }

      private static IEnumerable<string> CreateCompareLines()
      {
         var sb = new StringBuilder();
         sb.AppendLine("First line.");
         sb.AppendLine("Second line.");
         sb.AppendLine("Third line.");
         sb.Append("Fourth line.");
         return sb.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
      }
   }
}
