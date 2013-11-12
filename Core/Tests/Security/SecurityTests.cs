
// A simple, string-oriented wrapper class for encryption functions, including 
// Hashing, Symmetric Encryption, and Asymmetric Encryption.
//
//  Jeff Atwood
//   http://www.codinghorror.com/
//   http://www.codeproject.com/KB/security/SimpleEncryption.aspx
//
//  Contributions by Ryan Harlamert (harlam357) 2009-2010
//   http://code.google.com/p/harlam357-net/

using System.IO;
using System.Text;

using NUnit.Framework;

using harlam357.Core.Security.Cryptography;

namespace harlam357.Core.Security
{
   /// <summary>
   /// unit tests for the Encryption class to verify correct operation
   /// </summary>
   /// <remarks>
   ///   Jeff Atwood
   ///   http://www.codinghorror.com/
   /// </remarks>
   [TestFixture]
   public class SecurityTests
   {
      private string _targetString;
      private string _targetString2;

      [SetUp]
      public void Setup()
      {
         _targetString = "The instinct of nearly all societies is to lock up anybody who is truly free. " + 
            "First, society begins by trying to beat you up. If this fails, they try to poison you. " + 
            "If this fails too, they finish by loading honors on your head." +
            " - Jean Cocteau (1889-1963)";

         _targetString2 = "Everything should be made as simple as possible, but not simpler. - Albert Einstein";
      }

      #region Hash Tests

      [Test, Category("Hash")]
      public void Hash_Salted_Test()
      {
         Assert.AreEqual("6CD9DD96", DoSaltedHash(HashProvider.CRC32, new Data("Shazam!")).Hex);
         Assert.AreEqual("4F7FA9C182C5FA60F9197F4830296685", DoSaltedHash(HashProvider.MD5, new Data("SnapCracklePop")).Hex);
         Assert.AreEqual("3DC330B4E4E61C8DF039EAE93EC16412E22425FB", DoSaltedHash(HashProvider.SHA1, new Data("全球最大的華文新聞網站", Encoding.Unicode)).Hex);
         Assert.AreEqual("EFAE307AEE511D6078FDF0D4372F4D0C8135170C5F7626CB19B04BFDBABBBDB2", DoSaltedHash(HashProvider.SHA256, new Data("!@#$%^&*()_-+=", Encoding.ASCII)).Hex);
         Assert.AreEqual("582B31C13EF16D706EC2514FDA08316A369DF1F130D34A0A2A16B065D82662A1101EA01110AB7C8F9022A1CEA76FD6B9", DoSaltedHash(HashProvider.SHA384, new Data("supercalifragilisticexpialidocious", Encoding.ASCII)).Hex);
         Assert.AreEqual("44FAA06E8E80666408304E3458621769699A76B591C6389F958C0DDA1D80A82965D169E8AA7D3C1A0637BCB7B0F45D420389C629D19E255D64A923F6C4F87FD8", DoSaltedHash(HashProvider.SHA512, new Data("42", Encoding.ASCII)).Hex);
      }

      [Test, Category("Hash")]
      public void Hash_File_Test()
      {
         string hashHex;

         using (var h1 = new Hash(HashProvider.CRC32))
         using (var stream = File.OpenRead(@"..\..\TestFiles\gettysburg.txt"))
         {
            hashHex = h1.Calculate(stream).Hex;
         }
         Assert.AreEqual(hashHex, "E37F6423");

         using (var h2 = new Hash(HashProvider.MD5))
         using (var stream = File.OpenRead(@"..\..\TestFiles\sample.doc"))
         {
            hashHex = h2.Calculate(stream).Hex;
         }
         Assert.AreEqual(hashHex, "4F32AB797F0FCC782AAC0B4F4E5B1693");
      }

      [Test, Category("Hash")]
      public void Hash_Test()
      {
         Assert.AreEqual("AA692113", DoHash(HashProvider.CRC32).Hex);
         Assert.AreEqual("44D36517B0CCE797FF57118ABE264FD9", DoHash(HashProvider.MD5).Hex);
         Assert.AreEqual("9E93AB42BCC8F738C7FBB6CCA27A902DC663DBE1", DoHash(HashProvider.SHA1).Hex);
         Assert.AreEqual("40AF07ABFE970590B2C313619983651B1E7B2F8C2D855C6FD4266DAFD7A5E670", DoHash(HashProvider.SHA256).Hex);
         Assert.AreEqual("9FC0AFB3DA61201937C95B133AB397FE62C329D6061A8768DA2B9D09923F07624869D01CD76826E1152DAB7BFAA30915", DoHash(HashProvider.SHA384).Hex);
         Assert.AreEqual("2E7D4B051DD528F3E9339E0927930007426F4968B5A4A08349472784272F17DA5C532EDCFFE14934988503F77DEF4AB58EB05394838C825632D04A10F42A753B", DoHash(HashProvider.SHA512).Hex);
      }

      private Data DoSaltedHash(HashProvider p, Data salt)
      {
         using (var h = new Hash(p))
         {
            return h.Calculate(new Data(_targetString), salt);
         }
      }

      private Data DoHash(HashProvider p)
      {
         using (var h = new Hash(p))
         {
            return h.Calculate(new Data(_targetString));
         }
      } 

      #endregion

      #region Asymmetric Tests

      [Test, Category("Asymmetric")]
      public void Asymmetric_Test()
      {
         string secret = "Pack my box with five dozen liquor jugs.";
         Assert.AreEqual(secret, AsymmetricNewKey(secret));
         Assert.AreEqual(secret, AsymmetricNewKey(secret, 384));
         Assert.AreEqual(secret, AsymmetricNewKey(secret, 512));
         Assert.AreEqual(secret, AsymmetricNewKey(secret, 1024));
         Assert.AreEqual(secret, AsymmetricConfigKey(secret));
         Assert.AreEqual(secret, AsymmetricXmlKey(secret));
      }

      private string AsymmetricXmlKey(string secret)
      {
         string publicKeyXml = "<RSAKeyValue>" + "<Modulus>0D59Km2Eo9oopcm7Y2wOXx0TRRXQFybl9HHe/ve47Qcf2EoKbs9nkuMmhCJlJzrq6ZJzgQSEbpVyaWn8OHq0I50rQ13dJsALEquhlfwVWw6Hit7qRvveKlOAGfj8xdkaXJLYS1tA06tKHfYxgt6ysMBZd0DIedYoE1fe3VlLZyE=</Modulus>" + "<Exponent>AQAB</Exponent>" + "</RSAKeyValue>";

         string privateKeyXml = "<RSAKeyValue>" + "<Modulus>0D59Km2Eo9oopcm7Y2wOXx0TRRXQFybl9HHe/ve47Qcf2EoKbs9nkuMmhCJlJzrq6ZJzgQSEbpVyaWn8OHq0I50rQ13dJsALEquhlfwVWw6Hit7qRvveKlOAGfj8xdkaXJLYS1tA06tKHfYxgt6ysMBZd0DIedYoE1fe3VlLZyE=</Modulus>" + "<Exponent>AQAB</Exponent>" + "<P>/1cvDks8qlF1IXKNwcXW8tjTlhjidjGtbT9k7FCYug+P6ZBDfqhUqfvjgLFF/+dAkoofNqliv89b8DRy4gS4qQ==</P>" + "<Q>0Mgq7lyvmVPR1r197wnba1bWbJt8W2Ki8ilUN6lX6Lkk04ds9y3A0txy0ESya7dyg9NLscfU3NQMH8RRVnJtuQ==</Q>" + "<DP>+uwfRumyxSDlfSgInFqh/+YKD5+GtGXfKtO4hu4xF+8BGqJ1YXtkL+Njz2zmADOt5hOr1tigPSQ2EhhIqUnAeQ==</DP>" + "<DQ>M5Ofd28SOjCIwCHjwG+Q8v1qzz3CBNljI6uuEGoXO3ixbkggVRfKcMzg2C6AXTfeZE6Ifoy9OyhvLlHTPiXakQ==</DQ>" + "<InverseQ>yQIJMLdL6kU4VK7M5b5PqWS8XzkgxfnaowRs9mhSEDdwwWPtUXO8aQ9G3zuiDUqNq9j5jkdt77+c2stBdV97ew==</InverseQ>" + "<D>HOpQXu/OFyJXuo2EY43BgRt8bX9V4aEZFRQqrqSfHOp8VYASasiJzS+VTYupGAVqUPxw5V1HNkOyG0kIKJ+BG6BpIwLIbVKQn/ROs7E3/vBdg2+QXKhikMz/4gYx2oEsXW2kzN1GMRop2lrrJZJNGE/eG6i4lQ1/inj1Tk/sqQE=</D>" + "</RSAKeyValue>";

         Data encryptedData;
         Data decryptedData;
         Asymmetric asym = new Asymmetric();
         Asymmetric asym2 = new Asymmetric();

         encryptedData = asym.Encrypt(new Data(secret), publicKeyXml);
         decryptedData = asym2.Decrypt(encryptedData, privateKeyXml);

         return decryptedData.ToString();
      }

      private string AsymmetricConfigKey(string secret)
      {
         Data encryptedData;
         Data decryptedData;
         Asymmetric asym = new Asymmetric();
         Asymmetric asym2 = new Asymmetric();

         encryptedData = asym.Encrypt(new Data(secret));
         decryptedData = asym2.Decrypt(encryptedData);

         return decryptedData.ToString();
      }

      private string AsymmetricNewKey(string secret)
      {
         return AsymmetricNewKey(secret, 0);
      }

      private string AsymmetricNewKey(string secret, int keysize)
      {
         Asymmetric asym;
         Asymmetric asym2;
         if (keysize == 0)
         {
            asym = new Asymmetric();
            asym2 = new Asymmetric();
         }
         else
         {
            asym = new Asymmetric(keysize);
            asym2 = new Asymmetric(keysize);
         }
         
         var keyPair = Asymmetric.GenerateNewKeyset();
         var pubkey = keyPair.Key;
         var privkey = keyPair.Value;

         Data encryptedData = asym.Encrypt(new Data(secret), pubkey);
         Data decryptedData = asym2.Decrypt(encryptedData, privkey);

         return decryptedData.ToString();
      } 

      #endregion

      #region Symmetric Tests

      [Test, Category("Symmetric")]
      public void Symmetric_Test()
      {
         Assert.AreEqual(_targetString, SymmetricLoopback(SymmetricProvider.DES, _targetString));
         Assert.AreEqual(_targetString, SymmetricWithKey(SymmetricProvider.DES, _targetString));
         Assert.AreEqual(_targetString, SymmetricLoopback(SymmetricProvider.RC2, _targetString));
         Assert.AreEqual(_targetString, SymmetricWithKey(SymmetricProvider.RC2, _targetString));
         Assert.AreEqual(_targetString, SymmetricLoopback(SymmetricProvider.Rijndael, _targetString));
         Assert.AreEqual(_targetString, SymmetricWithKey(SymmetricProvider.Rijndael, _targetString));
         Assert.AreEqual(_targetString, SymmetricLoopback(SymmetricProvider.TripleDES, _targetString));
         Assert.AreEqual(_targetString, SymmetricWithKey(SymmetricProvider.TripleDES, _targetString));

         Assert.AreEqual(_targetString2, SymmetricLoopback(SymmetricProvider.DES, _targetString2));
         Assert.AreEqual(_targetString2, SymmetricWithKey(SymmetricProvider.DES, _targetString2));
         Assert.AreEqual(_targetString2, SymmetricLoopback(SymmetricProvider.RC2, _targetString2));
         Assert.AreEqual(_targetString2, SymmetricWithKey(SymmetricProvider.RC2, _targetString2));
         Assert.AreEqual(_targetString2, SymmetricLoopback(SymmetricProvider.Rijndael, _targetString2));
         Assert.AreEqual(_targetString2, SymmetricWithKey(SymmetricProvider.Rijndael, _targetString2));
         Assert.AreEqual(_targetString2, SymmetricLoopback(SymmetricProvider.TripleDES, _targetString2));
         Assert.AreEqual(_targetString2, SymmetricWithKey(SymmetricProvider.TripleDES, _targetString2));
      }

      [Test, Category("Symmetric")]
      public void Symmetric_File_Test()
      {
         //-- compare the hash of the decrypted file to what it should be after encryption/decryption
         //-- using pure file streams
         Assert.AreEqual("AC27F132E6728E4F8FA3B054013D3456", SymmetricFilePrivate(SymmetricProvider.TripleDES, @"..\..\TestFiles\gettysburg.txt", "Password, Yo!"));
         Assert.AreEqual("4F32AB797F0FCC782AAC0B4F4E5B1693", SymmetricFilePrivate(SymmetricProvider.RC2, @"..\..\TestFiles\sample.doc", "0nTheDownLow1"));
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
      private static string SymmetricFilePrivate(SymmetricProvider p, string fileName, string key)
      {
         string encryptedFilePath = Path.GetFileNameWithoutExtension(fileName) + ".encrypted";
         string decryptedFilePath = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(fileName)) + "-decrypted" + Path.GetExtension(fileName);

         // encrypt the file to memory
         Data encryptedData;
         using (var sym = new Symmetric(p))
         {
            sym.Key = new Data(key);
            using (var stream = File.OpenRead(fileName))
            {
               encryptedData = sym.Encrypt(stream);
            }
         }

         // write encrypted data to a new binary file
         using (var stream = File.Open(encryptedFilePath, FileMode.Create))
         using (var bw = new BinaryWriter(stream))
         {
            bw.Write(encryptedData.Bytes);
         }

         // decrypt this binary file
         Data decryptedData;
         using (var sym2 = new Symmetric(p))
         {
            sym2.Key = new Data(key);
            using (var stream = File.OpenRead(encryptedFilePath))
            {
               decryptedData = sym2.Decrypt(stream);
            }
         }

         // write decrypted data to a new binary file
         using (var stream = File.Open(decryptedFilePath, FileMode.Create))
         using (var bw = new BinaryWriter(stream))
         {
            bw.Write(decryptedData.Bytes);
         }

         // get the MD5 hash of the returned data
         using (var h = new Hash(HashProvider.MD5))
         {
            return h.Calculate(decryptedData).Hex;
         }
      }

      /// <summary>
      /// test using user-provided keys and init vectors
      /// </summary>
      private static string SymmetricWithKey(SymmetricProvider p, string targetString)
      {
         var keyData = new Data("MySecretPassword");
         var ivData = new Data("MyInitializationVector");

         Data encryptedData;
         using (var sym = new Symmetric(p, false))
         {
            sym.IntializationVector = ivData;
            encryptedData = sym.Encrypt(new Data(targetString), keyData);
         }

         Data decryptedData;
         using (var sym2 = new Symmetric(p, false))
         {
            sym2.IntializationVector = ivData;
            decryptedData = sym2.Decrypt(encryptedData, keyData);
         }

         ////-- the data will be padded to the encryption blocksize, so we need to trim it back down.
         //return decryptedData.ToString().Substring(0, _TargetData.Bytes.Length);

         return decryptedData.ToString();
      }

      /// <summary>
      /// test using auto-generated keys
      /// </summary>
      private static string SymmetricLoopback(SymmetricProvider p, string targetString)
      {
         Data decryptedData;
         using (var sym = new Symmetric(p))
         {
            Data encryptedData = sym.Encrypt(new Data(targetString));
            using (var sym2 = new Symmetric(p))
            {
               decryptedData = sym2.Decrypt(encryptedData, sym.Key);
            }
         }

         ////-- the data will be padded to the encryption blocksize, so we need to trim it back down.
         //return decryptedData.ToString().Substring(0, _TargetData.Bytes.Length);

         return decryptedData.ToString();
      } 

      #endregion
   }
}
