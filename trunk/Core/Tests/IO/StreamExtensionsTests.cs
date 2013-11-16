
using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace harlam357.Core.IO
{
   [TestFixture]
   public class StreamExtensionsTests
   {
      #region CopyTo

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

      #endregion

      #region FindLastIndex

      [Test]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Stream_FindLastIndex_ArgumentNullException_Test1()
      {
         StreamExtensions.FindLastIndex(null, value => true);
      }
      
      [Test]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Stream_FindLastIndex_ArgumentNullException_Test2()
      {
         using (var ms = new MemoryStream())
         {
            StreamExtensions.FindLastIndex(ms, null);
         }
      }

      [Test]
      [ExpectedException(typeof(ArgumentNullException))]
      public void Stream_FindLastIndex_ArgumentNullException_Test3()
      {
         StreamExtensions.FindLastIndex(null, 0, value => true);
      }

      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void Stream_FindLastIndex_ArgumentOutOfRangeException_Test1()
      {
         using (var ms = new MemoryStream())
         {
            StreamExtensions.FindLastIndex(ms, -1, null);
         }
      }

      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void Stream_FindLastIndex_ArgumentOutOfRangeException_Test2()
      {
         using (var ms = new MemoryStream())
         {
            StreamExtensions.FindLastIndex(ms, 1, null);
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

      #endregion
   }
}
