
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace harlam357.Core.IO
{
   public sealed class BufferedTextFileReader
   {
      private readonly List<string> _items;

      public BufferedTextFileReader()
      {
         _items = new List<string>();
      }

      public IEnumerable<string> ReadAllLines(string path, ref StreamPosition position)
      {
         return ReadAllLines(path, Encoding.UTF8, ref position);
      }

      public IEnumerable<string> ReadAllLines(string path, Encoding encoding, ref StreamPosition position)
      {
         using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
         using (var sr = new StreamReader(fs, encoding))
         {
            if (!fs.SetStreamPosition(position))
            {
               _items.Clear();
            }

            while (!sr.EndOfStream)
            {
               _items.Add(sr.ReadLine());
            }

            ResetEndOfStream(_items, fs);
            position = fs.GetStreamPosition(128);
         }

         return _items;
      }

      private static void ResetEndOfStream(IList<string> lines, Stream fs)
      {
         long streamPosition = fs.Position;
         long lastCrLfPosition = fs.FindLastIndex(Predicate);
         if (streamPosition != lastCrLfPosition)
         {
            lines.RemoveAt(lines.Count - 1);
            fs.Position = lastCrLfPosition;
         }
      }

      private static bool Predicate(int value)
      {
         return value == Convert.ToInt32('\r') || value == Convert.ToInt32('\n');
      }
   }
}
