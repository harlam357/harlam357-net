
using System;
using System.IO;

namespace harlam357.Core.IO
{
   public static class StreamExtensions
   {
      #region CopyTo

      // Implementation by Nicholas Carey
      // http://stackoverflow.com/questions/1540658/net-asynchronous-stream-read-write

      /// <summary>
      /// Copies the input stream to the output stream.
      /// </summary>
      /// <param name="input">The input stream.</param>
      /// <param name="output">The output stream.</param>
      /// <exception cref="T:System.ArgumentNullException">input -or- output is null.</exception>
      /// <exception cref="T:System.InvalidOperationException">input is not open for reading.</exception>
      /// <exception cref="T:System.InvalidOperationException">output is not open for writing.</exception>
      public static void CopyTo(this Stream input, Stream output)
      {
         input.CopyTo(output, 0);
      }

      /// <summary>
      /// Copies the input stream to the output stream using the supplied buffer size.
      /// </summary>
      /// <param name="input">The input stream.</param>
      /// <param name="output">The output stream.</param>
      /// <param name="bufferSize">The size of the copy buffer.</param>
      /// <exception cref="T:System.ArgumentNullException">input -or- output is null.</exception>
      /// <exception cref="T:System.ArgumentException">bufferSize is negative.</exception>
      /// <exception cref="T:System.InvalidOperationException">input is not open for reading.</exception>
      /// <exception cref="T:System.InvalidOperationException">output is not open for writing.</exception>
      public static void CopyTo(this Stream input, Stream output, int bufferSize)
      {
         if (input == null) throw new ArgumentNullException("input");
         if (output == null) throw new ArgumentNullException("output");
         if (bufferSize < 0) throw new ArgumentException("bufferSize cannot be negative.", "bufferSize");

         if (!input.CanRead) throw new InvalidOperationException("input must be open for reading.");
         if (!output.CanWrite) throw new InvalidOperationException("output must be open for writing.");

         if (bufferSize == 0)
         {
            // 32,767
            bufferSize = Int16.MaxValue;
         }

         byte[][] buf = { new byte[bufferSize], new byte[bufferSize] };
         int[] bufl = { 0, 0 };
         int bufno = 0;

         IAsyncResult read = input.BeginRead(buf[bufno], 0, buf[bufno].Length, null, null);
         IAsyncResult write = null;

         while (true)
         {
            // wait for the read operation to complete
            read.AsyncWaitHandle.WaitOne();
            bufl[bufno] = input.EndRead(read);

            // if zero bytes read, the copy is complete
            if (bufl[bufno] == 0)
            {
               break;
            }

            // wait for the in-flight write operation, if one exists, to complete
            // the only time one won't exist is after the very first read operation completes
            if (write != null)
            {
               write.AsyncWaitHandle.WaitOne();
               output.EndWrite(write);
            }

            // start the new write operation
            write = output.BeginWrite(buf[bufno], 0, bufl[bufno], null, null);

            // toggle the current, in-use buffer
            // and start the read operation on the new buffer.
            //
            // Changed to use XOR to toggle between 0 and 1.
            // A little speedier than using a ternary expression.
            bufno ^= 1; // bufno = ( bufno == 0 ? 1 : 0 ) ;
            read = input.BeginRead(buf[bufno], 0, buf[bufno].Length, null, null);
         }

         // wait for the final in-flight write operation, if one exists, to complete
         // the only time one won't exist is if the input stream is empty.
         if (write != null)
         {
            write.AsyncWaitHandle.WaitOne();
            output.EndWrite(write);
         }

         output.Flush();
      }

      #endregion

      #region FindLastIndex

      /// <summary>
      /// Searches for a byte that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire System.IO.Stream.
      /// </summary>
      /// <param name="stream">The stream instance.</param>
      /// <param name="match">The predicate delegate that defines the conditions of the byte to search for.</param>
      /// <returns>The zero-based index of the last occurrence of a byte that matches the conditions defined by match, if found; otherwise, –1.</returns>
      public static long FindLastIndex(this Stream stream, Predicate<int> match)
      {
         return stream.FindLastIndex(stream.Length, match);
      }

      /// <summary>
      /// Searches for a byte that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of bytes in the System.IO.Stream that extends from the first byte to the specified index.
      /// </summary>
      /// <param name="stream">The stream instance.</param>
      /// <param name="startIndex">The zero-based starting index of the backward search.</param>
      /// <param name="match">The predicate delegate that defines the conditions of the byte to search for.</param>
      /// <returns>The zero-based index of the last occurrence of a byte that matches the conditions defined by match, if found; otherwise, –1.</returns>
      public static long FindLastIndex(this Stream stream, long startIndex, Predicate<int> match)
      {
         // read the stream backwards using SeekOrigin.Current
         stream.Seek(startIndex, SeekOrigin.Begin);
         for (long i = 0; i < startIndex; i++)
         {
            stream.Seek(-1, SeekOrigin.Current);
            int value = stream.ReadByte();
            if (match(value))
            {
               return stream.Position;
            }
            stream.Seek(-1, SeekOrigin.Current);
         }

         return -1;
      }

      #endregion
   }
}
