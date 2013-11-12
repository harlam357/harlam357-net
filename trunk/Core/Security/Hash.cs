
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
         switch (provider)
         {
            case HashProvider.CRC32:
               _hash = new CRC32();
               break;
            case HashProvider.SHA1:
               _hash = new SHA1Managed();
               break;
            case HashProvider.SHA256:
               _hash = new SHA256Managed();
               break;
            case HashProvider.SHA384:
               _hash = new SHA384Managed();
               break;
            case HashProvider.SHA512:
               _hash = new SHA512Managed();
               break;
            case HashProvider.MD5:
               _hash = new MD5CryptoServiceProvider();
               break;
         }
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
