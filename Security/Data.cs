
// A simple, string-oriented wrapper class for encryption functions, including 
// Hashing, Symmetric Encryption, and Asymmetric Encryption.
//
//  Jeff Atwood
//   http://www.codinghorror.com/
//   http://www.codeproject.com/KB/security/SimpleEncryption.aspx

using System;
using System.Text;

namespace harlam357.Security
{
   /// <summary>
   /// Represents Hex, Byte, Base64, or String data to encrypt or decrypt.
   ///  </summary>
   /// <remarks>
   /// Use the Text property to get/set a string representation.
   /// Use the Hex property to get/set a string-based Hexadecimal representation.
   /// Use the Base64 to get/set a string-based Base64 representation.
   /// </remarks>
   public class Data
   {
      #region Fields

      private byte[] _data;

      private static readonly Encoding DefaultEncodingValue = Encoding.GetEncoding("Windows-1252");
      /// <summary>
      /// Gets the default text encoding for all Data instances.
      /// </summary>
      public static Encoding DefaultEncoding
      {
         get { return DefaultEncodingValue; }
      }

      private Encoding _encoding = DefaultEncoding;
      /// <summary>
      /// Gets or sets the text encoding for this Data instance.
      /// </summary>
      public Encoding Encoding
      {
         get { return _encoding; }
         set
         {
            // don't allow a null to be set here
            if (value == null) return;
            _encoding = value;
         }
      }

      #endregion

      #region Constructors

      /// <summary>
      /// Initializes a new instance of the Data class that is empty.
      /// </summary>
      public Data()
      {

      }

      /// <summary>
      /// Initializes a new instance of the Data class with the byte array value.
      /// </summary>
      public Data(byte[] value)
      {
         _data = value;
      }

      /// <summary>
      /// Initializes a new instance of the Data class with the string value.
      /// </summary>
      public Data(string value)
      {
         Text = value;
      }

      /// <summary>
      /// Initializes a new instance of the Data class with the string value.
      /// </summary>
      public Data(string value, Encoding encoding)
      {
         // encoding must be set BEFORE value
         Encoding = encoding;
         Text = value;
      }

      #endregion

      #region Properties

      /// <summary>
      /// Indicates if no data is present in this instance.
      /// </summary>
      public bool IsEmpty
      {
         get { return _data == null || _data.Length == 0; }
      }

      /// <summary>
      /// Gets or sets the allowed step interval, in bytes, for this data; if 0, no limit.
      /// </summary>
      public int StepBytes { get; set; }

      ///// <summary>
      ///// Gets or sets the allowed step interval, in bits, for this data; if 0, no limit.
      ///// </summary>
      //public int StepBits
      //{
      //   get { return StepBytes * 8; }
      //   set { StepBytes = value / 8; }
      //}

      /// <summary>
      /// Gets or sets the minimum number of bytes allowed for this data; if 0, no limit.
      /// </summary>
      public int MinBytes { get; set; }

      ///// <summary>
      ///// Gets or sets the minimum number of bits allowed for this data; if 0, no limit.
      ///// </summary>
      //public int MinBits
      //{
      //   get { return _minBytes * 8; }
      //   set { _minBytes = value / 8; }
      //}

      /// <summary>
      /// Gets or sets the maximum number of bytes allowed for this data; if 0, no limit.
      /// </summary>
      public int MaxBytes { get; set; }

      /// <summary>
      /// Gets or sets the maximum number of bits allowed for this data; if 0, no limit.
      /// </summary>
      public int MaxBits
      {
         get { return MaxBytes * 8; }
         set { MaxBytes = value / 8; }
      }

      /// <summary>
      /// Gets or sets the byte representation of the data. This will be padded to MinBytes and trimmed to MaxBytes as necessary.
      /// </summary>
      public byte[] Bytes
      {
         get
         {
            if (MaxBytes > 0)
            {
               if (_data.Length > MaxBytes)
               {
                  var b = new byte[MaxBytes];
                  Array.Copy(_data, b, b.Length);
                  _data = b;
               }
            }
            if (MinBytes > 0)
            {
               if (_data.Length < MinBytes)
               {
                  var b = new byte[MinBytes];
                  Array.Copy(_data, b, _data.Length);
                  _data = b;
               }
            }
            return _data;
         }
         set { _data = value; }
      }

      /// <summary>
      /// Gets or sets the text representation of the data using the Encoding value.
      /// </summary>
      public string Text
      {
         get
         {
            if (_data == null)
            {
               return String.Empty;
            }

            // need to handle nulls here
            // oddly, C# will happily convert nulls into the string
            // whereas VB stops converting at the first null
            int i = Array.IndexOf(_data, (byte)0);
            if (i >= 0)
            {
               return Encoding.GetString(_data, 0, i);
            }
            return Encoding.GetString(_data);
         }
         set { _data = Encoding.GetBytes(value); }
      }

      /// <summary>
      /// Gets or sets the Hex string representation of this data.
      /// </summary>
      public string Hex
      {
         get { return _data.ToHex(); }
         set { _data = value.FromHex(); }
      }

      /// <summary>
      /// Gets or sets the Base64 string representation of this data.
      /// </summary>
      public string Base64
      {
         get { return _data.ToBase64(); }
         set { _data = value.FromBase64(); }
      }

      #endregion

      /// <summary>
      /// Returns a string that represents the current object.
      /// </summary>
      /// <returns>A string that represents the current object.</returns>
      /// <filterpriority>2</filterpriority>
      public override string ToString()
      {
         return Text;
      }
   }
}
