
// A simple, string-oriented wrapper class for encryption functions, including 
// Hashing, Symmetric Encryption, and Asymmetric Encryption.
//
//  Jeff Atwood
//   http://www.codinghorror.com/
//   http://www.codeproject.com/KB/security/SimpleEncryption.aspx

using System;
using System.Globalization;
using System.Text;

namespace harlam357.Core.Security
{
   /// <summary>
   /// Contains extension methods for hexidecimal and base64 conversions.
   /// </summary>
   public static class Extensions
   {
      /// <summary>
      /// Converts an array of bytes to a Hex string representation.
      /// </summary>
      public static string ToHex(this byte[] value)
      {
         if (value == null || value.Length == 0)
         {
            return String.Empty;
         }

         const string hexFormat = "{0:X2}";
         var sb = new StringBuilder();
         foreach (byte b in value)
         {
            sb.Append(String.Format(CultureInfo.InvariantCulture, hexFormat, b));
         }
         return sb.ToString();
      }

      /// <summary>
      /// Converts a Hex string representation to an array of bytes.
      /// </summary>
      /// <exception cref="T:System.FormatException">The length of value is not zero or a multiple of 2. -or- The format of value is invalid. value contains a non-hex character.</exception>
      public static byte[] FromHex(this string value)
      {
         if (string.IsNullOrEmpty(value))
         {
            return null;
         }

         try
         {
            int byteLength = Convert.ToInt32(value.Length / 2);
            var b = new byte[byteLength];
            for (int i = 0; i < byteLength; i++)
            {
               b[i] = Convert.ToByte(value.Substring(i * 2, 2), 16);
            }
            return b;
         }
         catch (Exception ex)
         {
            throw new FormatException("The provided string does not appear to be Hex encoded.", ex);
         }
      }

      /// <summary>
      /// Converts an array of bytes to a Base64 string representation.
      /// </summary>
      static public string ToBase64(this byte[] value)
      {
         if (value == null || value.Length == 0)
         {
            return String.Empty;
         }
         return Convert.ToBase64String(value);
      }

      /// <summary>
      /// Converts a Base64 string representation to an array of bytes.
      /// </summary>
      /// <exception cref="T:System.FormatException">The length of value, ignoring white-space characters, is not zero or a multiple of 4. -or- The format of value is invalid. value contains a non-base-64 character, more than two padding characters, or a non-white space-character among the padding characters.</exception>
      static public byte[] FromBase64(this string value)
      {
         if (String.IsNullOrEmpty(value))
         {
            return null;
         }

         try
         {
            return Convert.FromBase64String(value);
         }
         catch (FormatException ex)
         {
            throw new FormatException("The provided string does not appear to be Base64 encoded.", ex);
         }
      }
   }
}
