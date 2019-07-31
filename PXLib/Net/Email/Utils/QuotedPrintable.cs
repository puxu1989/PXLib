using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PXLib.Net.Email
{
    internal static class QuotedPrintable
    {
        /// <summary>
        /// Decodes a Quoted-Printable string according to <a href="http://tools.ietf.org/html/rfc2047">RFC 2047</a>.<br />
        /// RFC 2047 is used for decoding Encoded-Word encoded strings.
        /// </summary>
        /// <param name="toDecode">Quoted-Printable encoded string</param>
        /// <param name="encoding">Specifies which encoding the returned string will be in</param>
        /// <returns>A decoded string in the correct encoding</returns>
        /// <exception cref="T:System.ArgumentNullException">If <paramref name="toDecode" /> or <paramref name="encoding" /> is <see langword="null" /></exception>
        public static string DecodeEncodedWord(string toDecode, Encoding encoding)
        {
            if (toDecode == null)
            {
                throw new ArgumentNullException("toDecode");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            return encoding.GetString(QuotedPrintable.Rfc2047QuotedPrintableDecode(toDecode, true));
        }

        /// <summary>
        /// Decodes a Quoted-Printable string according to <a href="http://tools.ietf.org/html/rfc2045">RFC 2045</a>.<br />
        /// RFC 2045 specifies the decoding of a body encoded with Content-Transfer-Encoding of quoted-printable.
        /// </summary>
        /// <param name="toDecode">Quoted-Printable encoded string</param>
        /// <returns>A decoded byte array that the Quoted-Printable encoded string described</returns>
        /// <exception cref="T:System.ArgumentNullException">If <paramref name="toDecode" /> is <see langword="null" /></exception>
        public static byte[] DecodeContentTransferEncoding(string toDecode)
        {
            if (toDecode == null)
            {
                throw new ArgumentNullException("toDecode");
            }
            return QuotedPrintable.Rfc2047QuotedPrintableDecode(toDecode, false);
        }

        /// <summary>
        /// This is the actual decoder.
        /// </summary>
        /// <param name="toDecode">The string to be decoded from Quoted-Printable</param>
        /// <param name="encodedWordVariant">
        /// If <see langword="true" />, specifies that RFC 2047 quoted printable decoding is used.<br />
        /// This is for quoted-printable encoded words<br />
        /// <br />
        /// If <see langword="false" />, specifies that RFC 2045 quoted printable decoding is used.<br />
        /// This is for quoted-printable Content-Transfer-Encoding
        /// </param>
        /// <returns>A decoded byte array that was described by <paramref name="toDecode" /></returns>
        /// <exception cref="T:System.ArgumentNullException">If <paramref name="toDecode" /> is <see langword="null" /></exception>
        /// <remarks>See <a href="http://tools.ietf.org/html/rfc2047#section-4.2">RFC 2047 section 4.2</a> for RFC details</remarks>
        private static byte[] Rfc2047QuotedPrintableDecode(string toDecode, bool encodedWordVariant)
        {
            if (toDecode == null)
            {
                throw new ArgumentNullException("toDecode");
            }
            byte[] result;
            using (MemoryStream byteArrayBuilder = new MemoryStream())
            {
                toDecode = QuotedPrintable.RemoveIllegalControlCharacters(toDecode);
                for (int i = 0; i < toDecode.Length; i++)
                {
                    char currentChar = toDecode[i];
                    if (currentChar == '=')
                    {
                        if (toDecode.Length - i < 3)
                        {
                            QuotedPrintable.WriteAllBytesToStream(byteArrayBuilder, QuotedPrintable.DecodeEqualSignNotLongEnough(toDecode.Substring(i)));
                            break;
                        }
                        string quotedPrintablePart = toDecode.Substring(i, 3);
                        QuotedPrintable.WriteAllBytesToStream(byteArrayBuilder, QuotedPrintable.DecodeEqualSign(quotedPrintablePart));
                        i += 2;
                    }
                    else if (currentChar == '_' && encodedWordVariant)
                    {
                        byteArrayBuilder.WriteByte(32);
                    }
                    else
                    {
                        byteArrayBuilder.WriteByte((byte)currentChar);
                    }
                }
                result = byteArrayBuilder.ToArray();
            }
            return result;
        }

        /// <summary>
        /// Writes all bytes in a byte array to a stream
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        /// <param name="toWrite">The bytes to write to the <paramref name="stream" /></param>
        private static void WriteAllBytesToStream(Stream stream, byte[] toWrite)
        {
            stream.Write(toWrite, 0, toWrite.Length);
        }

        /// <summary>
        /// RFC 2045 states about robustness:<br />
        /// <code>
        /// Control characters other than TAB, or CR and LF as parts of CRLF pairs,
        /// must not appear. The same is true for octets with decimal values greater
        /// than 126.  If found in incoming quoted-printable data by a decoder, a
        /// robust implementation might exclude them from the decoded data and warn
        /// the user that illegal characters were discovered.
        /// </code>
        /// Control characters are defined in RFC 2396 as<br />
        /// <c>control = US-ASCII coded characters 00-1F and 7F hexadecimal</c>
        /// </summary>
        /// <param name="input">String to be stripped from illegal control characters</param>
        /// <returns>A string with no illegal control characters</returns>
        /// <exception cref="T:System.ArgumentNullException">If <paramref name="input" /> is <see langword="null" /></exception>
        private static string RemoveIllegalControlCharacters(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            input = QuotedPrintable.RemoveCarriageReturnAndNewLinewIfNotInPair(input);
            return Regex.Replace(input, "[\0-\b\v\f\u000e-\u001f\u007f]", "");
        }

        /// <summary>
        /// This method will remove any \r and \n which is not paired as \r\n
        /// </summary>
        /// <param name="input">String to remove lonely \r and \n's from</param>
        /// <returns>A string without lonely \r and \n's</returns>
        /// <exception cref="T:System.ArgumentNullException">If <paramref name="input" /> is <see langword="null" /></exception>
        private static string RemoveCarriageReturnAndNewLinewIfNotInPair(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            StringBuilder newString = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] != '\r' || (i + 1 < input.Length && input[i + 1] == '\n'))
                {
                    if (input[i] != '\n' || (i - 1 >= 0 && input[i - 1] == '\r'))
                    {
                        newString.Append(input[i]);
                    }
                }
            }
            return newString.ToString();
        }

        /// <summary>
        /// RFC 2045 says that a robust implementation should handle:<br />
        /// <code>
        /// An "=" cannot be the ultimate or penultimate character in an encoded
        /// object. This could be handled as in case (2) above.
        /// </code>
        /// Case (2) is:<br />
        /// <code>
        /// An "=" followed by a character that is neither a
        /// hexadecimal digit (including "abcdef") nor the CR character of a CRLF pair
        /// is illegal.  This case can be the result of US-ASCII text having been
        /// included in a quoted-printable part of a message without itself having
        /// been subjected to quoted-printable encoding.  A reasonable approach by a
        /// robust implementation might be to include the "=" character and the
        /// following character in the decoded data without any transformation and, if
        /// possible, indicate to the user that proper decoding was not possible at
        /// this point in the data.
        /// </code>
        /// </summary>
        /// <param name="decode">
        /// The string to decode which cannot have length above or equal to 3
        /// and must start with an equal sign.
        /// </param>
        /// <returns>A decoded byte array</returns>
        /// <exception cref="T:System.ArgumentNullException">If <paramref name="decode" /> is <see langword="null" /></exception>
        /// <exception cref="T:System.ArgumentException">Thrown if a the <paramref name="decode" /> parameter has length above 2 or does not start with an equal sign.</exception>
        private static byte[] DecodeEqualSignNotLongEnough(string decode)
        {
            if (decode == null)
            {
                throw new ArgumentNullException("decode");
            }
            if (decode.Length >= 3)
            {
                throw new ArgumentException("decode must have length lower than 3", "decode");
            }
            if (decode.Length <= 0)
            {
                throw new ArgumentException("decode must have length lower at least 1", "decode");
            }
            if (decode[0] != '=')
            {
                throw new ArgumentException("First part of decode must be an equal sign", "decode");
            }
            return Encoding.ASCII.GetBytes(decode);
        }

        /// <summary>
        /// This helper method will decode a string of the form "=XX" where X is any character.<br />
        /// This method will never fail, unless an argument of length not equal to three is passed.
        /// </summary>
        /// <param name="decode">The length 3 character that needs to be decoded</param>
        /// <returns>A decoded byte array</returns>
        /// <exception cref="T:System.ArgumentNullException">If <paramref name="decode" /> is <see langword="null" /></exception>
        /// <exception cref="T:System.ArgumentException">Thrown if a the <paramref name="decode" /> parameter does not have length 3 or does not start with an equal sign.</exception>
        private static byte[] DecodeEqualSign(string decode)
        {
            if (decode == null)
            {
                throw new ArgumentNullException("decode");
            }
            if (decode.Length != 3)
            {
                throw new ArgumentException("decode must have length 3", "decode");
            }
            if (decode[0] != '=')
            {
                throw new ArgumentException("decode must start with an equal sign", "decode");
            }
            byte[] result;
            if (decode.Contains("\r\n"))
            {
                result = new byte[0];
            }
            else
            {
                try
                {
                    string numberString = decode.Substring(1);
                    byte[] oneByte = new byte[]
					{
						Convert.ToByte(numberString, 16)
					};
                    result = oneByte;
                }
                catch (FormatException)
                {
                    result = Encoding.ASCII.GetBytes(decode);
                }
            }
            return result;
        }
    }
}
