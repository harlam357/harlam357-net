
// A simple, string-oriented wrapper class for encryption functions, including 
// Hashing, Symmetric Encryption, and Asymmetric Encryption.
//
//  Jeff Atwood
//   http://www.codinghorror.com/
//   http://www.codeproject.com/KB/security/SimpleEncryption.aspx

using System;
using System.Text;

namespace harlam357.Core.Security
{
   /// <summary>
   /// Represents data to encrypt or decrypt.
   /// </summary>
   /// <remarks>
   /// Use the Text property to get/set a string representation.
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
         get { return _encoding ?? (_encoding = DefaultEncoding); }
         set { _encoding = value; }
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
      /// Gets or sets the byte representation of the data.
      /// </summary>
      public virtual byte[] Bytes
      {
         get { return _data; }
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
