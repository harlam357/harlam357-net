
// A simple, string-oriented wrapper class for encryption functions, including 
// Hashing, Symmetric Encryption, and Asymmetric Encryption.
//
//  Jeff Atwood
//   http://www.codinghorror.com/
//   http://www.codeproject.com/KB/security/SimpleEncryption.aspx

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace harlam357.Core.Security.Cryptography
{
   /// <summary>
   /// Specifies the symmetric encryption algorithm.
   /// </summary>
   public enum SymmetricProvider
   {
      // ReSharper disable InconsistentNaming

      /// <summary>
      /// The Data Encryption Standard provider supports a 64 bit key only.
      /// </summary>
      DES,
      /// <summary>
      /// The Rivest Cipher 2 provider supports keys ranging from 40 to 128 bits, default is 128 bits.
      /// </summary>
      RC2,
      /// <summary>
      /// The Rijndael (also known as AES) provider supports keys of 128, 192, or 256 bits with a default of 256 bits.
      /// </summary>
      Rijndael,
      /// <summary>
      /// The TripleDES provider (also known as 3DES) supports keys of 128 or 192 bits with a default of 192 bits.
      /// </summary>
      TripleDES

      // ReSharper restore InconsistentNaming
   }

   /// <summary>
   /// Provides access to factory methods for creating SymmetricAlgorithm instances.
   /// </summary>
   public static class SymmetricAlgorithmFactory
   {
      /// <summary>
      /// Creates a new instance of the SymmetricAlgorithm class based on the specified provider.
      /// </summary>
      /// <param name="provider">Provides the type of symmetric algorithm to create.</param>
      /// <returns>The SymmetricAlgorithm object.</returns>
      /// <exception cref="T:System.ArgumentException">The provider is unknown.</exception>
      public static SymmetricAlgorithm Create(SymmetricProvider provider)
      {
         switch (provider)
         {
            case SymmetricProvider.DES:
               return DES.Create();
            case SymmetricProvider.RC2:
               return RC2.Create();
            case SymmetricProvider.Rijndael:
               return Rijndael.Create();
            case SymmetricProvider.TripleDES:
               return TripleDES.Create();
         }

         throw new ArgumentException("Unknown SymmetricProvider.", "provider");
      }
   }

   /// <summary>
   /// Represents an object that performs symmetric encryption.
   /// </summary>
   /// <remarks>Symmetric encryption uses a single key to encrypt and decrypt. Both parties (encryptor and decryptor) must share the same secret key.</remarks>
   public class Symmetric : IDisposable
   {
      private const string DefaultIntializationVector = "439#k5;&+";
      private const int BufferSize = 2048;

      private SymmetricKeyData _key;
      private SymmetricKeyData _iv;
      private readonly SymmetricAlgorithm _crypto;

      /// <summary>
      /// Initializes a new instance of the Symmetric class based on the specified provider.
      /// </summary>
      public Symmetric(SymmetricProvider provider)
         : this(provider, true)
      {

      }

      /// <summary>
      /// Initializes a new instance of the Symmetric class based on the specified provider.
      /// </summary>
      public Symmetric(SymmetricProvider provider, bool useDefaultInitializationVector)
      {
         _crypto = SymmetricAlgorithmFactory.Create(provider);

         // make sure key and IV are always set, no matter what
         Key = RandomKey();
         IntializationVector = useDefaultInitializationVector ? new SymmetricKeyData(DefaultIntializationVector) : RandomInitializationVector();
      }

      /// <summary>
      /// Gets or sets the key used to encrypt or decrypt data.
      /// </summary>
      /// <remarks>Setting a null value will be ignored.</remarks>
      public SymmetricKeyData Key
      {
         get { return _key; }
         set
         {
            // don't allow a null to be set here
            if (value == null) return;
            _key = value;
            _key.MaxBytes = _crypto.LegalKeySizes[0].MaxSize / 8;
            _key.MinBytes = _crypto.LegalKeySizes[0].MinSize / 8;
         }
      }

      // Using the default Cipher Block Chaining (CBC) mode, all data blocks are processed using
      // the value derived from the previous block.  The first data block has no previous data block
      // to use, so it needs an InitializationVector to feed the first block.

      /// <summary>
      /// Gets or sets the initialization vector.
      /// </summary>
      /// <remarks>Setting a null value will be ignored.</remarks>
      public SymmetricKeyData IntializationVector
      {
         get { return _iv; }
         set
         {
            // don't allow a null to be set here
            if (value == null) return;
            _iv = value;
            _iv.MaxBytes = _crypto.BlockSize / 8;
            _iv.MinBytes = _crypto.BlockSize / 8;
         }
      }

      /// <summary>
      /// Generates a random initialization vector.
      /// </summary>
      private SymmetricKeyData RandomInitializationVector()
      {
         _crypto.GenerateIV();
         return new SymmetricKeyData(_crypto.IV);
      }

      /// <summary>
      /// Generates a random key.
      /// </summary>
      private SymmetricKeyData RandomKey()
      {
         _crypto.GenerateKey();
         return new SymmetricKeyData(_crypto.Key);
      }

      /// <summary>
      /// Ensures that the _crypto object has valid Key and IV prior to any attempt to encrypt or decrypt.
      /// </summary>
      private void ValidateKeyAndIv(bool isEncrypting)
      {
         if (_key.IsEmpty)
         {
            if (isEncrypting)
            {
               _key = RandomKey();
            }
            else
            {
               throw new CryptographicException("No key was provided for the decryption operation!");
            }
         }
         if (_iv.IsEmpty)
         {
            if (isEncrypting)
            {
               _iv = RandomInitializationVector();
            }
            else
            {
               throw new CryptographicException("No initialization vector was provided for the decryption operation!");
            }
         }
         _crypto.Key = _key.Bytes;
         _crypto.IV = _iv.Bytes;
      }

      /// <summary>
      /// Encrypts the specified data using the provided key.
      /// </summary>
      /// <exception cref="T:System.ArgumentNullException">data is null.</exception>
      public Data Encrypt(Data d, SymmetricKeyData key)
      {
         Key = key;
         return Encrypt(d);
      }

      /// <summary>
      /// Encrypts the specified data using the preset key and initialization vector.
      /// </summary>
      /// <exception cref="T:System.ArgumentNullException">data is null.</exception>
      public Data Encrypt(Data data)
      {
         if (data == null) throw new ArgumentNullException("data");
         ValidateKeyAndIv(true);

         using (var ms = new MemoryStream())
         {
            using (var cs = new CryptoStream(ms, _crypto.CreateEncryptor(), CryptoStreamMode.Write))
            {
               cs.Write(data.Bytes, 0, data.Bytes.Length);
            }
            return new Data(ms.ToArray());
         }
      }

      /// <summary>
      /// Encrypts the specified stream to memory using the provided key and initialization vector.
      /// </summary>
      /// <exception cref="T:System.ArgumentNullException">stream is null.</exception>
      public Data Encrypt(Stream stream, SymmetricKeyData key, SymmetricKeyData iv)
      {
         IntializationVector = iv;
         Key = key;
         return Encrypt(stream);
      }

      /// <summary>
      /// Encrypts the specified stream to memory using the provided key.
      /// </summary>
      /// <exception cref="T:System.ArgumentNullException">stream is null.</exception>
      public Data Encrypt(Stream stream, SymmetricKeyData key)
      {
         Key = key;
         return Encrypt(stream);
      }

      /// <summary>
      /// Encrypts the specified stream to memory using the preset key and initialization vector.
      /// </summary>
      /// <exception cref="T:System.ArgumentNullException">stream is null.</exception>
      public Data Encrypt(Stream stream)
      {
         if (stream == null) throw new ArgumentNullException("stream");
         ValidateKeyAndIv(true);

         using (var ms = new MemoryStream())
         {
            using (var cs = new CryptoStream(ms, _crypto.CreateEncryptor(), CryptoStreamMode.Write))
            {
               var b = new byte[BufferSize + 1];
               int i = stream.Read(b, 0, BufferSize);
               while (i > 0)
               {
                  cs.Write(b, 0, i);
                  i = stream.Read(b, 0, BufferSize);
               }
            }
            return new Data(ms.ToArray());
         }
      }

      /// <summary>
      /// Decrypts the specified stream using the provided key and preset initialization vector.
      /// </summary>
      /// <exception cref="T:System.ArgumentNullException">stream is null.</exception>
      public Data Decrypt(Stream stream, SymmetricKeyData key)
      {
         Key = key;
         return Decrypt(stream);
      }

      /// <summary>
      /// Decrypts the specified stream using the preset key and initialization vector.
      /// </summary>
      /// <exception cref="T:System.ArgumentNullException">stream is null.</exception>
      public Data Decrypt(Stream stream)
      {
         if (stream == null) throw new ArgumentNullException("stream");
         ValidateKeyAndIv(false);

         using (var ms = new MemoryStream())
         {
            using (var cs = new CryptoStream(stream, _crypto.CreateDecryptor(), CryptoStreamMode.Read))
            {
               var b = new byte[BufferSize + 1];
               int i = cs.Read(b, 0, BufferSize);
               while (i > 0)
               {
                  ms.Write(b, 0, i);
                  i = cs.Read(b, 0, BufferSize);
               }
            }
            return new Data(ms.ToArray());
         }
      }

      /// <summary>
      /// Decrypts the specified data using provided key and preset initialization vector.
      /// </summary>
      /// <exception cref="T:System.ArgumentNullException">data is null.</exception>
      /// <exception cref="T:System.Security.Cryptography.CryptographicException">Unable to decrypt data.</exception>
      public Data Decrypt(Data data, SymmetricKeyData key)
      {
         Key = key;
         return Decrypt(data);
      }

      /// <summary>
      /// Decrypts the specified data using preset key and initialization vector.
      /// </summary>
      /// <exception cref="T:System.ArgumentNullException">data is null.</exception>
      /// <exception cref="T:System.Security.Cryptography.CryptographicException">Unable to decrypt data.</exception>
      public Data Decrypt(Data data)
      {
         if (data == null) throw new ArgumentNullException("data");
         ValidateKeyAndIv(false);

         using (var ms = new MemoryStream(data.Bytes, 0, data.Bytes.Length))
         {
            var b = new byte[data.Bytes.Length];
            using (var cs = new CryptoStream(ms, _crypto.CreateDecryptor(), CryptoStreamMode.Read))
            {
               try
               {
                  cs.Read(b, 0, data.Bytes.Length - 1);
               }
               catch (CryptographicException ex)
               {
                  throw new CryptographicException("Unable to decrypt data. The provided key may be invalid.", ex);
               }
            }
            return new Data(b);
         }
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
               ((IDisposable)_crypto).Dispose();
            }
            // clean unmanaged resources
         }

         _disposed = true;
      }

      private bool _disposed;

      #endregion
   }

   /// <summary>
   /// Represents symmetric key data used to encrypt or decrypt data.
   /// </summary>
   public class SymmetricKeyData : Data
   {
      /// <summary>
      /// Initializes a new instance of the SymmetricKeyData class that is empty.
      /// </summary>
      public SymmetricKeyData()
      {

      }

      /// <summary>
      /// Initializes a new instance of the SymmetricKeyData class with the byte array value.
      /// </summary>
      public SymmetricKeyData(byte[] value)
         : base(value)
      {
         
      }

      /// <summary>
      /// Initializes a new instance of the SymmetricKeyData class with the string value.
      /// </summary>
      public SymmetricKeyData(string value)
         : base(value)
      {
         
      }

      /// <summary>
      /// Initializes a new instance of the SymmetricKeyData class with the string value.
      /// </summary>
      public SymmetricKeyData(string value, Encoding encoding)
         : base(value, encoding)
      {
         
      }

      /// <summary>
      /// Gets or sets the minimum number of bytes allowed for this data; if 0, no limit.
      /// </summary>
      public int MinBytes { get; set; }

      /// <summary>
      /// Gets or sets the maximum number of bytes allowed for this data; if 0, no limit.
      /// </summary>
      public int MaxBytes { get; set; }

      /// <summary>
      /// Gets or sets the byte representation of the data. This will be padded to MinBytes and trimmed to MaxBytes as necessary.
      /// </summary>
      public override byte[] Bytes
      {
         get
         {
            if (MaxBytes > 0)
            {
               if (base.Bytes.Length > MaxBytes)
               {
                  var b = new byte[MaxBytes];
                  Array.Copy(base.Bytes, b, b.Length);
                  base.Bytes = b;
               }
            }
            if (MinBytes > 0)
            {
               if (base.Bytes.Length < MinBytes)
               {
                  var b = new byte[MinBytes];
                  Array.Copy(base.Bytes, b, base.Bytes.Length);
                  base.Bytes = b;
               }
            }
            return base.Bytes;
         }
         set { base.Bytes = value; }
      }
   }
}
