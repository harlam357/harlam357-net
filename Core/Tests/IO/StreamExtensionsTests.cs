
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace harlam357.Core.IO
{
   [TestFixture]
   public class StreamExtensionsTests
   {
      [Test]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Stream_CopyTo_ArgumentNullException_Test1()
      {
         using (var ms = new MemoryStream())
         {
            StreamExtensions.CopyTo(null, ms);
         }
      }

      [Test]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Stream_CopyTo_ArgumentNullException_Test2()
      {
         using (var ms = new MemoryStream())
         {
            StreamExtensions.CopyTo(ms, null);
         }
      }

      [Test]
      [ExpectedException(typeof(ArgumentException))]
      public void Stream_CopyTo_ArgumentException_Test1()
      {
         using (var ms1 = new MemoryStream())
         using (var ms2 = new MemoryStream())
         {
            StreamExtensions.CopyTo(ms1, ms2, -1);
         }
      }

      [Test]
      [ExpectedException(typeof(InvalidOperationException))]
      public void Stream_CopyTo_InvalidOperationException_Test1()
      {
         MemoryStream ms1;
         MemoryStream ms2;
         using (ms1 = new MemoryStream())
         {
            
         }
         using (ms2 = new MemoryStream())
         {
            // input not open for reading
            StreamExtensions.CopyTo(ms1, ms2);
         }
      }

      [Test]
      [ExpectedException(typeof(InvalidOperationException))]
      public void Stream_CopyTo_InvalidOperationException_Test2()
      {
         MemoryStream ms1;
         MemoryStream ms2;
         using (ms2 = new MemoryStream())
         {

         }
         using (ms1 = new MemoryStream())
         {
            // output not open for writing
            StreamExtensions.CopyTo(ms1, ms2);
         }
      }

      [Test]
      public void Stream_CopyTo_Test1()
      {
         var buffer = new byte[Int16.MaxValue * 3];
         var random = new Random();
         random.NextBytes(buffer);
         using (var ms1 = new MemoryStream(buffer))
         using (var ms2 = new MemoryStream())
         {
            ms1.CopyTo(ms2);
            Assert.IsTrue(ms1.ToArray().SequenceEqual(ms2.ToArray()));
         }
      }

      [Test]
      public void Stream_FindLastIndex_Test1()
      {
         using (var fs = new FileStream(@"..\..\TestFiles\gpl-3.0-1.txt", FileMode.Open, FileAccess.Read))
         {
            Assert.AreEqual(1108, fs.FindLastIndex(value => value == Convert.ToInt32('\n')));
         }
      }

      [Test]
      public void Stream_FindLastIndex_Test2()
      {
         using (var ms = new MemoryStream())
         {
            Assert.AreEqual(-1, ms.FindLastIndex(value => value == Convert.ToInt32('\n')));
         }
      }

      [Test]
      public void Test1()
      {
         var lines = new List<string>();
         long position = 0;

         position = ReadFile(@"..\..\TestFiles\gpl-3.0-1.txt", position, lines);
         position = ReadFile(@"..\..\TestFiles\gpl-3.0-2.txt", position, lines);
         position = ReadFile(@"..\..\TestFiles\gpl-3.0-3.txt", position, lines);

         Assert.AreEqual(48, lines.Count);
      }

      private static long ReadFile(string path, long position, IList<string> lines)
      {
         using (var stream = new StreamReader(path))
         {
            long newPosition = stream.BaseStream.FindLastIndex(position, Predicate);
            if (newPosition >= 0)
            {
               stream.BaseStream.Position = newPosition + 1;
               lines.RemoveAt(lines.Count - 1);
            }

            while (!stream.EndOfStream)
            {
               lines.Add(stream.ReadLine());
            }

            return stream.BaseStream.Position;
         }
      }

      private static bool Predicate(int value)
      {
         return value == Convert.ToInt32('\r') || value == Convert.ToInt32('\n');
      }

      //[Test]
      //public void Test2()
      //{
      //   var list = new List<int> {1, 2, 4, 3, 1, 2, 4, 3, 2, 1, 4};
      //   Assert.AreEqual(10, list.FindLastIndex(x => x == 4));
      //   Assert.AreEqual(2, list.FindLastIndex(3, x => x == 4));
      //}
   }
}
