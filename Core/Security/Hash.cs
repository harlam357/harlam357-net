
// A simple, string-oriented wrapper class for encryption functions, including 
// Hashing, Symmetric Encryption, and Asymmetric Encryption.
//
//  Jeff Atwood
//   http://www.codinghorror.com/
//   http://www.codeproject.com/KB/security/SimpleEncryption.aspx

using System;
using System.Security.Cryptography;

namespace harlam357.Core.Security
{
   /// <summary>
   /// Specifies the type of hash.
   /// </summary>
   public enum HashProvider
   {
      // ReSharper disable InconsistentNaming

      /// <summary>
      /// Cyclic Redundancy Check provider, 32-bit.
      /// </summary>
      CRC32,
      /// <summary>
      /// Secure Hashing Algorithm provider, SHA-1 variant, 160-bit.
      /// </summary>
      SHA1,
      /// <summary>
      /// Secure Hashing Algorithm provider, SHA-2 variant, 256-bit.
      /// </summary>
      SHA256,
      /// <summary>
      /// Secure Hashing Algorithm provider, SHA-2 variant, 384-bit.
      /// </summary>
      SHA384,
      /// <summary>
      /// Secure Hashing Algorithm provider, SHA-2 variant, 512-bit.
      /// </summary>
      SHA512,
      /// <summary>
      /// Message Digest algorithm 5, 128-bit.
      /// </summary>
      MD5

      // ReSharper restore InconsistentNaming
   }

   /// <summary>
   /// Provides access to factory methods for creating HashAlgorithm instances.
   /// </summary>
   public static class HashAlgorithmFactory
   {
      /// <summary>
      /// Creates a new instance of the HashAlgorithm class based on the specified provider.
      /// </summary>
      /// <param name="provider">Provides the type of hash algorithm to create.</param>
      /// <returns>The HashAlgorithm object.</returns>
      /// <exception cref="T:System.ArgumentException">The provider is unknown.</exception>
      public static HashAlgorithm Create(HashProvider provider)
      {
         switch (provider)
         {
            case HashProvider.CRC32:
               return new CRC32();
            case HashProvider.SHA1:
               return SHA1.Create();
            case HashProvider.SHA256:
               return SHA256.Create();
            case HashProvider.SHA384:
               return SHA384.Create();
            case HashProvider.SHA512:
               return SHA512.Create();
            case HashProvider.MD5:
               return MD5.Create();
         }

         throw new ArgumentException("Unknown HashProvider.", "provider");
      }

      #region CRC32 HashAlgorithm

      // ReSharper disable InconsistentNaming
      private sealed class CRC32 : HashAlgorithm
      // ReSharper restore InconsistentNaming
      {
         private const UInt32 DefaultPolynomial = 0xedb88320;
         private const UInt32 DefaultSeed = 0xffffffff;

         private UInt32 _hash;
         private readonly UInt32 _seed;
         private readonly UInt32[] _table;
         private static UInt32[] DefaultTable;

         public CRC32()
         {
            _table = InitializeTable(DefaultPolynomial);
            _seed = DefaultSeed;
            Initialize();
         }

         //public CRC32(UInt32 polynomial, UInt32 seed)
         //{
         //   _table = InitializeTable(polynomial);
         //   _seed = seed;
         //   Initialize();
         //}

         public override void Initialize()
         {
            _hash = _seed;
         }

         protected override void HashCore(byte[] buffer, int start, int length)
         {
            _hash = CalculateHash(_table, _hash, buffer, start, length);
         }

         protected override byte[] HashFinal()
         {
            byte[] hashBuffer = UInt32ToBigEndianBytes(~_hash);
            HashValue = hashBuffer;
            return hashBuffer;
         }

         public override int HashSize
         {
            get { return 32; }
         }

         //public static UInt32 Compute(byte[] buffer)
         //{
         //   return ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
         //}

         //public static UInt32 Compute(UInt32 seed, byte[] buffer)
         //{
         //   return ~CalculateHash(InitializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length);
         //}

         //public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer)
         //{
         //   return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
         //}

         private static UInt32[] InitializeTable(UInt32 polynomial)
         {
            if (polynomial == DefaultPolynomial && DefaultTable != null)
            {
               return DefaultTable;
            }

            var createTable = new UInt32[256];
            for (int i = 0; i < 256; i++)
            {
               var entry = (UInt32)i;
               for (int j = 0; j < 8; j++)
               {
                  if ((entry & 1) == 1)
                  {
                     entry = (entry >> 1) ^ polynomial;
                  }
                  else
                  {
                     entry = entry >> 1;
                  }
               }
               
               createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
            {
               DefaultTable = createTable;
            }

            return createTable;
         }

         private static UInt32 CalculateHash(UInt32[] table, UInt32 seed, byte[] buffer, int start, int size)
         {
            UInt32 crc = seed;
            for (int i = start; i < size; i++)
            {
               unchecked
               {
                  crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
               }
            }
            return crc;
         }

         private static byte[] UInt32ToBigEndianBytes(UInt32 x)
         {
            return new[] 
            {
               (byte)((x >> 24) & 0xff),
               (byte)((x >> 16) & 0xff),
               (byte)((x >> 8) & 0xff),
               (byte)(x & 0xff)
            };
         }

      }

      #endregion
   }

   // Hash functions are fundamental to modern cryptography. These functions map binary 
   // strings of an arbitrary length to small binary strings of a fixed length, known as 
   // hash values. A cryptographic hash function has the property that it is computationally
   // infeasible to find two distinct inputs that hash to the same value. Hash functions 
   // are commonly used with digital signatures and for data integrity.

   /// <summary>
   /// Represents an object that performs hashing.
   /// </summary>
   public class Hash : IDisposable
   {
      private readonly HashAlgorithm _hash;
      private readonly Data _hashValue = new Data();

      /// <summary>
      /// Initializes a new instance of the Hash class with the specified hash provider.
      /// </summary>
      public Hash(HashProvider provider)
      {
         _hash = HashAlgorithmFactory.Create(provider);
      }

      /// <summary>
      /// Gets the previously calculated hash value.
      /// </summary>
      public Data Value
      {
         get { return _hashValue; }
      }

      /// <summary>
      /// Calculates the hash on a stream of arbitrary length.
      /// </summary>
      public Data Calculate(System.IO.Stream stream)
      {
         _hashValue.Bytes = _hash.ComputeHash(stream);
         return _hashValue;
      }

      /// <summary>
      /// Calculates the hash for fixed length data.
      /// </summary>
      /// <exception cref="T:System.ArgumentNullException">data is null.</exception>
      public Data Calculate(Data data)
      {
         if (data == null) throw new ArgumentNullException("data");
         return CalculatePrivate(data.Bytes);
      }

      /// <summary>
      /// Calculates the hash for fixed length data with a prefixed salt value.
      ///  </summary>
      /// <exception cref="T:System.ArgumentNullException">data or salt is null.</exception>
      /// <remarks>A "salt" value is random data prefixed to every hashed value to prevent common dictionary attacks.</remarks>
      public Data Calculate(Data data, Data salt)
      {
         if (data == null) throw new ArgumentNullException("data");
         if (salt == null) throw new ArgumentNullException("salt");

         var value = new byte[data.Bytes.Length + salt.Bytes.Length];
         salt.Bytes.CopyTo(value, 0);
         data.Bytes.CopyTo(value, salt.Bytes.Length);
         return CalculatePrivate(value);
      }

      private Data CalculatePrivate(byte[] value)
      {
         _hashValue.Bytes = _hash.ComputeHash(value);
         return _hashValue;
      }

      /// <summary>
      /// Calculates the hash on a seekable stream while reporting progress.
      /// </summary>
      public Data Calculate(System.IO.Stream stream, IProgress<int> progress)
      {
         if (stream == null) throw new ArgumentNullException("stream");
         if (progress == null) throw new ArgumentNullException("progress");
         if (!stream.CanSeek) throw new ArgumentException("stream must support seeking.", "stream");

         const int bufferLength = 1048576;
         long totalBytesRead = 0;
         long size = stream.Length;
         var buffer = new byte[bufferLength];

         int bytesRead = stream.Read(buffer, 0, buffer.Length);
         totalBytesRead += bytesRead;

         do
         {
            int oldBytesRead = bytesRead;
            byte[] oldBuffer = buffer;

            buffer = new byte[bufferLength];
            bytesRead = stream.Read(buffer, 0, buffer.Length);

            totalBytesRead += bytesRead;

            if (bytesRead == 0)
            {
               _hash.TransformFinalBlock(oldBuffer, 0, oldBytesRead);
            }
            else
            {
               _hash.TransformBlock(oldBuffer, 0, oldBytesRead, oldBuffer, 0);
            }

            progress.Report((int)((double)totalBytesRead * 100 / size));

         } while (bytesRead != 0);

         _hashValue.Bytes = _hash.Hash;
         return _hashValue;
      }

      #region IDisposable Implementation

      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
      /// </summary>
      /// <filterpriority>2</filterpriority>
      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
      /// </summary>
      /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
      /// <filterpriority>2</filterpriority>
      protected virtual void Dispose(bool disposing)
      {
         if (!_disposed)
         {
            if (disposing)
            {
               // clean managed resources
               ((IDisposable)_hash).Dispose();
            }
            // clean unmanaged resources
         }

         _disposed = true;
      }

      private bool _disposed;

      #endregion
   }
}
