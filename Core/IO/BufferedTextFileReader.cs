
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace harlam357.Core.IO
{
   public sealed class BufferedTextFileReader
   {
      private StreamPosition _position;
      private readonly List<string> _items;

      public BufferedTextFileReader()
      {
         _position = StreamPosition.Empty;
         _items = new List<string>();
      }

      public IEnumerable<string> ReadAllLines(string path)
      {
         return ReadAllLines(path, Encoding.UTF8);
      }

      public IEnumerable<string> ReadAllLines(string path, Encoding encoding)
      {
         using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
         {
            return ReadAllLines(fs, encoding);
         }
      }

      public IEnumerable<string> ReadAllLines(Stream stream)
      {
         return ReadAllLines(stream, Encoding.UTF8);
      }

      public IEnumerable<string> ReadAllLines(Stream stream, Encoding encoding)
      {
         using (var sr = new StreamReader(stream, encoding))
         {
            if (stream.SetStreamPosition(_position))
            {
               if (!IsEndOfLine(_position.CheckBuffer))
               {
                  ResetStream(_items, stream);
               }
            }
            else
            {
               _items.Clear();
            }

            while (!sr.EndOfStream)
            {
               _items.Add(sr.ReadLine());
            }

            _position = stream.GetStreamPosition(128);
         }

         return _items;
      }

      private static bool IsEndOfLine(byte[] buffer)
      {
         if (buffer.Length < 2)
         {
            return false;
         }
         return (buffer[buffer.Length - 2] == Convert.ToByte('\r') &&
                 buffer[buffer.Length - 1] == Convert.ToByte('\n')) ||
                 buffer[buffer.Length - 2] == Convert.ToByte('\n');
      }

      private static void ResetStream(IList<string> lines, Stream stream)
      {
         long streamPosition = stream.Position;
         long crLfPosition = stream.FindLastIndex(streamPosition, value => value == Convert.ToInt32('\n'));
         if (crLfPosition >= 0 && streamPosition != crLfPosition)
         {
            lines.RemoveAt(lines.Count - 1);
            stream.Position = crLfPosition;
         }
      }
   }
}
