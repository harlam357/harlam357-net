
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
         StreamPosition position = StreamPosition.Empty;
         List<string> lines = reader.ReadAllLines(@"..\..\TestFiles\gpl-3.0-1.txt", ref position).ToList();
         Assert.AreEqual(23, lines.Count);
         lines = reader.ReadAllLines(@"..\..\TestFiles\gpl-3.0-2.txt", ref position).ToList();
         Assert.AreEqual(39, lines.Count);
         lines = reader.ReadAllLines(@"..\..\TestFiles\gpl-3.0-3.txt", ref position).ToList();
         Assert.AreEqual(48, lines.Count);
         var compareLines = File.ReadAllLines(@"..\..\TestFiles\gpl-3.0-3.txt");
         Assert.IsTrue(lines.SequenceEqual(compareLines));
      }
   }
}
