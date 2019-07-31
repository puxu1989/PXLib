using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PXLib.Net.Email
{
    internal static class Utils
    {
    
            public static string Decode(string encodedWords)
		    {
                if (encodedWords == null)
                    throw new ArgumentNullException("encodedWords");
			const string encodedWordRegex = @"\=\?(?<Charset>\S+?)\?(?<Encoding>\w)\?(?<Content>.+?)\?\=";
			const string replaceRegex = @"(?<first>" + encodedWordRegex + @")\s+(?<second>" + encodedWordRegex + ")";
			encodedWords = Regex.Replace(encodedWords, replaceRegex, "${first}${second}");
			encodedWords = Regex.Replace(encodedWords, replaceRegex, "${first}${second}");
			string decodedWords = encodedWords;
			MatchCollection matches = Regex.Matches(encodedWords, encodedWordRegex);
			foreach (Match match in matches)
			{
				if (!match.Success) continue;
				string fullMatchValue = match.Value;
				string encodedText = match.Groups["Content"].Value;
				string encoding = match.Groups["Encoding"].Value;
				string charset = match.Groups["Charset"].Value;				
				Encoding charsetEncoding = EncodingFinder.FindEncoding(charset);
				string decodedText;
				switch (encoding.ToUpperInvariant())
				{
					case "B":
						decodedText = Base64Decode(encodedText, charsetEncoding);
						break;
					case "Q":
						decodedText = QuotedPrintable.DecodeEncodedWord(encodedText, charsetEncoding);
						break;

					default:
						throw new ArgumentException("The encoding " + encoding + " was not recognized");
				}
				decodedWords = decodedWords.Replace(fullMatchValue, decodedText);
			}
			return decodedWords;
		}
        

        public static string Base64Decode(string base64Encoded, Encoding encoding)
        {
            if (base64Encoded == null)
                throw new ArgumentNullException("base64Encoded");

            if (encoding == null)
                throw new ArgumentNullException("encoding");
            return encoding.GetString(Base64Decode(base64Encoded));
        }
        public static byte[] Base64Decode(string base64Encoded)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    base64Encoded = base64Encoded.Replace("\r\n", "");

                    byte[] inputBytes = Encoding.ASCII.GetBytes(base64Encoded);

                    using (FromBase64Transform transform = new FromBase64Transform(FromBase64TransformMode.DoNotIgnoreWhiteSpaces))
                    {
                        byte[] outputBytes = new byte[transform.OutputBlockSize];

                        // Transform the data in chunks the size of InputBlockSize.
                        const int inputBlockSize = 4;
                        int currentOffset = 0;
                        while (inputBytes.Length - currentOffset > inputBlockSize)
                        {
                            transform.TransformBlock(inputBytes, currentOffset, inputBlockSize, outputBytes, 0);
                            currentOffset += inputBlockSize;
                            memoryStream.Write(outputBytes, 0, transform.OutputBlockSize);
                        }

                        // Transform the final block of data.
                        outputBytes = transform.TransformFinalBlock(inputBytes, currentOffset, inputBytes.Length - currentOffset);
                        memoryStream.Write(outputBytes, 0, outputBytes.Length);
                    }

                    return memoryStream.ToArray();
                }
            }
            catch (FormatException e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Remove quotes, if found, around the string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveQuotesIfAny(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            string result;
            if (text.Length > 1 && text[0] == '"' && text[text.Length - 1] == '"')
            {
                result = text.Substring(1, text.Length - 2);
            }
            else
            {
                result = text;
            }
            return result;
        }
        public static List<string> SplitStringWithCharNotInsideQuotes(string input, char toSplitAt)
        {
            List<string> elements = new List<string>();
            int lastSplitLocation = 0;
            bool insideQuote = false;
            char[] characters = input.ToCharArray();
            for (int i = 0; i < characters.Length; i++)
            {
                char character = characters[i];
                if (character == '"')
                {
                    insideQuote = !insideQuote;
                }
                if (character == toSplitAt && !insideQuote)
                {
                    int length = i - lastSplitLocation;
                    elements.Add(input.Substring(lastSplitLocation, length));
                    lastSplitLocation = i + 1;
                }
            }
            elements.Add(input.Substring(lastSplitLocation, input.Length - lastSplitLocation));
            return elements;
        }

        /// <summary>
        /// Read a line from the stream.
        /// A line is interpreted as all the bytes read until a CRLF or LF is encountered.<br/>
        /// CRLF pair or LF is not included in the string.
        /// </summary>
        /// <param name="stream">The stream from which the line is to be read</param>
        /// <returns>A line read from the stream returned as a byte array or <see langword="null"/> if no bytes were readable from the stream</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <see langword="null"/></exception>
        public static byte[] ReadLineAsBytes(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                while (true)
                {
                    int justRead = stream.ReadByte();
                    if (justRead == -1 && memoryStream.Length > 0)
                        break;

                    // Check if we started at the end of the stream we read from
                    // and we have not read anything from it yet
                    if (justRead == -1 && memoryStream.Length == 0)
                        return null;

                    char readChar = (char)justRead;

                    // Do not write \r or \n
                    if (readChar != '\r' && readChar != '\n')
                        memoryStream.WriteByte((byte)justRead);

                    // Last point in CRLF pair
                    if (readChar == '\n')
                        break;
                }

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Read a line from the stream. <see cref="ReadLineAsBytes"/> for more documentation.
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <returns>A line read from the stream or <see langword="null"/> if nothing could be read from the stream</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <see langword="null"/></exception>
        public static string ReadLineAsAscii(Stream stream)
        {
            byte[] readFromStream = ReadLineAsBytes(stream);
            return readFromStream != null ? Encoding.ASCII.GetString(readFromStream) : null;
        }
        #region Size转换
        private static readonly Dictionary<string, long> UnitsToMultiplicator = InitializeSizes();

        private static Dictionary<string, long> InitializeSizes()
        {
            return new Dictionary<string, long>
			{ 
				{ "", 1L },  // No unit is the same as a byte
				{ "B", 1L }, // Byte
				{ "KB", 1024L }, // Kilobyte
				{ "MB", 1024L * 1024L}, // Megabyte
				{ "GB", 1024L * 1024L * 1024L}, // Gigabyte
				{ "TB", 1024L * 1024L * 1024L * 1024L} // Terabyte
			};
        }

        public static long SizeParse(string value)
        {
            value = value.Trim();

            string unit = ExtractUnit(value);
            string valueWithoutUnit = value.Substring(0, value.Length - unit.Length).Trim();

            long multiplicatorForUnit = MultiplicatorForUnit(unit);

            double size = double.Parse(valueWithoutUnit, NumberStyles.Number, CultureInfo.InvariantCulture);

            return (long)(multiplicatorForUnit * size);
        }

        private static string ExtractUnit(string sizeWithUnit)
        {
            // start right, end at the first digit
            int lastChar = sizeWithUnit.Length - 1;
            int unitLength = 0;

            while (unitLength <= lastChar
                && sizeWithUnit[lastChar - unitLength] != ' '       // stop when a space
                && !IsDigit(sizeWithUnit[lastChar - unitLength]))   // or digit is found
            {
                unitLength++;
            }

            return sizeWithUnit.Substring(sizeWithUnit.Length - unitLength).ToUpperInvariant();
        }

        private static bool IsDigit(char value)
        {
            // we don't want to use char.IsDigit since it would accept esoterical unicode digits
            return value >= '0' && value <= '9';
        }

        private static long MultiplicatorForUnit(string unit)
        {
            unit = unit.ToUpperInvariant();

            if (!UnitsToMultiplicator.ContainsKey(unit))
                throw new ArgumentException("illegal or unknown unit: \"" + unit + "\"", "unit");

            return UnitsToMultiplicator[unit];
        }
        #endregion


        #region 日期处理
        public static DateTime StringToDate(string inputDate)
        {
            if (inputDate == null)
            {
                throw new ArgumentNullException("inputDate");
            }
            inputDate = StripCommentsAndExcessWhitespace(inputDate);
            DateTime result;
            try
            {
                DateTime dateTime = ExtractDateTime(inputDate);
                if (dateTime == DateTime.MinValue)
                {
                    result = dateTime;
                }
                else
                {
                    ValidateDayNameIfAny(dateTime, inputDate);
                    dateTime = new DateTime(dateTime.Ticks, DateTimeKind.Utc);
                    dateTime = AdjustTimezone(dateTime, inputDate);
                    result = dateTime;
                }
            }
            catch (FormatException e)
            {
                throw new ArgumentException(string.Concat(new string[]
		{
			"Could not parse date: ",
			e.Message,
			". Input was: \"",
			inputDate,
			"\""
		}), e);
            }
            catch (ArgumentException e2)
            {
                throw new ArgumentException(string.Concat(new string[]
		{
			"Could not parse date: ",
			e2.Message,
			". Input was: \"",
			inputDate,
			"\""
		}), e2);
            }
            return result;
        }

        private static string StripCommentsAndExcessWhitespace(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            input = Regex.Replace(input, "(\\((?>\\((?<C>)|\\)(?<-C>)|.?)*(?(C)(?!))\\))", "");
            input = Regex.Replace(input, "\\s+", " ");
            input = Regex.Replace(input, "^\\s+", "");
            input = Regex.Replace(input, "\\s+$", "");
            input = Regex.Replace(input, " ?: ?", ":");
            return input;
        }
        private static DateTime ExtractDateTime(string dateInput)
        {
            if (dateInput == null)
            {
                throw new ArgumentNullException("dateInput");
            }
            Match match = Regex.Match(dateInput, "(\\d\\d? .+ (\\d\\d\\d\\d|\\d\\d) \\d?\\d:\\d?\\d(:\\d?\\d)?)|((\\d\\d\\d\\d|\\d\\d)-\\d?\\d-\\d?\\d \\d?\\d:\\d?\\d(:\\d?\\d)?)|(\\d\\d?-[A-Za-z]{3}-(\\d\\d\\d\\d|\\d\\d) \\d?\\d:\\d?\\d(:\\d?\\d)?)");
            DateTime result;
            if (match.Success)
            {
                result = Convert.ToDateTime(match.Value, CultureInfo.InvariantCulture);
            }
            else
            {
                result = DateTime.MinValue;
            }
            return result;
        }
        private static void ValidateDayNameIfAny(DateTime dateTime, string dateInput)
        {
            if (dateInput.Length >= 4 && dateInput[3] == ',')
            {
                string dayName = dateInput.Substring(0, 3);
                if ((dateTime.DayOfWeek == DayOfWeek.Monday && !dayName.Equals("Mon")) || (dateTime.DayOfWeek == DayOfWeek.Tuesday && !dayName.Equals("Tue")) || (dateTime.DayOfWeek == DayOfWeek.Wednesday && !dayName.Equals("Wed")) || (dateTime.DayOfWeek == DayOfWeek.Thursday && !dayName.Equals("Thu")) || (dateTime.DayOfWeek == DayOfWeek.Friday && !dayName.Equals("Fri")) || (dateTime.DayOfWeek == DayOfWeek.Saturday && !dayName.Equals("Sat")) || (dateTime.DayOfWeek == DayOfWeek.Sunday && !dayName.Equals("Sun")))
                {

                }
            }
        }

        private static DateTime AdjustTimezone(DateTime dateTime, string dateInput)
        {
            string[] parts = dateInput.Split(new char[] { ' ' });
            string lastPart = parts[parts.Length - 1];
            lastPart = Regex.Replace(lastPart, "UT|GMT|EST|EDT|CST|CDT|MST|MDT|PST|PDT|[A-I]|[K-Y]|Z", new MatchEvaluator(MatchEvaluator));
            Match match = Regex.Match(lastPart, "[\\+-](?<hours>\\d\\d)(?<minutes>\\d\\d)");
            DateTime result;
            if (match.Success)
            {
                int hours = int.Parse(match.Groups["hours"].Value);
                int minutes = int.Parse(match.Groups["minutes"].Value);
                int factor = (match.Value[0] == '+') ? -1 : 1;
                dateTime = dateTime.AddHours((double)(factor * hours));
                dateTime = dateTime.AddMinutes((double)(factor * minutes));
                result = dateTime;
            }
            else
            {

                result = dateTime;
            }
            return result;
        }


        private static string MatchEvaluator(Match match)
        {
            if (!match.Success)
            {
                throw new ArgumentException("Match success are always true");
            }

            switch (match.Value)
            {
                // "A" through "I"
                // are equivalent to "+0100" through "+0900" respectively
                case "A": return "+0100";
                case "B": return "+0200";
                case "C": return "+0300";
                case "D": return "+0400";
                case "E": return "+0500";
                case "F": return "+0600";
                case "G": return "+0700";
                case "H": return "+0800";
                case "I": return "+0900";

                // "K", "L", and "M"
                // are equivalent to "+1000", "+1100", and "+1200" respectively
                case "K": return "+1000";
                case "L": return "+1100";
                case "M": return "+1200";

                // "N" through "Y"
                // are equivalent to "-0100" through "-1200" respectively
                case "N": return "-0100";
                case "O": return "-0200";
                case "P": return "-0300";
                case "Q": return "-0400";
                case "R": return "-0500";
                case "S": return "-0600";
                case "T": return "-0700";
                case "U": return "-0800";
                case "V": return "-0900";
                case "W": return "-1000";
                case "X": return "-1100";
                case "Y": return "-1200";

                // "Z", "UT" and "GMT"
                // is equivalent to "+0000"
                case "Z":
                case "UT":
                case "GMT":
                    return "+0000";

                // US time zones
                case "EDT": return "-0400"; // EDT is semantically equivalent to -0400
                case "EST": return "-0500"; // EST is semantically equivalent to -0500
                case "CDT": return "-0500"; // CDT is semantically equivalent to -0500
                case "CST": return "-0600"; // CST is semantically equivalent to -0600
                case "MDT": return "-0600"; // MDT is semantically equivalent to -0600
                case "MST": return "-0700"; // MST is semantically equivalent to -0700
                case "PDT": return "-0700"; // PDT is semantically equivalent to -0700
                case "PST": return "-0800"; // PST is semantically equivalent to -0800

                default:
                    throw new ArgumentException("Unexpected input");
            }
        }

        #endregion

        public static List<KeyValuePair<string, string>> Rfc2231Decode(string toDecode)
            {
                if (toDecode == null)
                {
                    throw new ArgumentNullException("toDecode");
                }
                toDecode = Regex.Replace(toDecode, "=\\s*\"(?<value>[^\"]*)\"\\s", "=\"${value}\"; ");
                toDecode = Regex.Replace(toDecode, "^(?<first>[^;\\s]+)\\s(?<second>[^;\\s]+)", "${first}; ${second}");
                List<string> splitted = SplitStringWithCharNotInsideQuotes(toDecode.Trim(), ';');
                List<KeyValuePair<string, string>> collection = new List<KeyValuePair<string, string>>(splitted.Count);
                foreach (string part in splitted)
                {
                    if (part.Trim().Length != 0)
                    {
                        string[] keyValue = part.Trim().Split(new char[]
					{
						'='
					}, 2);
                        if (keyValue.Length == 1)
                        {
                            collection.Add(new KeyValuePair<string, string>("", keyValue[0]));
                        }
                        else
                        {
                            if (keyValue.Length != 2)
                            {
                                throw new ArgumentException(string.Concat(new object[]
							{
								"When splitting the part \"",
								part,
								"\" by = there was ",
								keyValue.Length,
								" parts. Only 1 and 2 are supported"
							}));
                            }
                            collection.Add(new KeyValuePair<string, string>(keyValue[0], keyValue[1]));
                        }
                    }
                }
                return DecodePairs(collection);
            }

            /// <summary>
            /// Decodes the list of key value pairs into a decoded list of key value pairs.<br />
            /// There may be less keys in the decoded list, but then the values for the lost keys will have been appended
            /// to the new key.
            /// </summary>
            /// <param name="pairs">The pairs to decode</param>
            /// <returns>A decoded list of pairs</returns>
            private static List<KeyValuePair<string, string>> DecodePairs(List<KeyValuePair<string, string>> pairs)
            {
                if (pairs == null)
                {
                    throw new ArgumentNullException("pairs");
                }
                List<KeyValuePair<string, string>> resultPairs = new List<KeyValuePair<string, string>>(pairs.Count);
                int pairsCount = pairs.Count;
                for (int i = 0; i < pairsCount; i++)
                {
                    KeyValuePair<string, string> currentPair = pairs[i];
                    string key = currentPair.Key;
                    string value = RemoveQuotesIfAny(currentPair.Value);
                    if (key.EndsWith("*0", StringComparison.OrdinalIgnoreCase) || key.EndsWith("*0*", StringComparison.OrdinalIgnoreCase))
                    {
                        string encoding = "notEncoded - Value here is never used";
                        if (key.EndsWith("*0*", StringComparison.OrdinalIgnoreCase))
                        {
                            value = DecodeSingleValue(value, out encoding);
                            key = key.Replace("*0*", "");
                        }
                        else
                        {
                            key = key.Replace("*0", "");
                        }
                        StringBuilder builder = new StringBuilder();
                        builder.Append(value);
                        int j = i + 1;
                        int continuationCount = 1;
                        while (j < pairsCount)
                        {
                            string jKey = pairs[j].Key;
                            string valueJKey = RemoveQuotesIfAny(pairs[j].Value);
                            if (jKey.Equals(key + "*" + continuationCount))
                            {
                                builder.Append(valueJKey);
                                i++;
                            }
                            else
                            {
                                if (!jKey.Equals(string.Concat(new object[]
							{
								key,
								"*",
								continuationCount,
								"*"
							})))
                                {
                                    break;
                                }
                                if (encoding != null)
                                {
                                    valueJKey = DecodeSingleValue(valueJKey, encoding);
                                }
                                builder.Append(valueJKey);
                                i++;
                            }
                            j++;
                            continuationCount++;
                        }
                        value = builder.ToString();
                        resultPairs.Add(new KeyValuePair<string, string>(key, value));
                    }
                    else if (key.EndsWith("*", StringComparison.OrdinalIgnoreCase))
                    {
                        key = key.Replace("*", "");
                        string throwAway;
                        value = DecodeSingleValue(value, out throwAway);
                        resultPairs.Add(new KeyValuePair<string, string>(key, value));
                    }
                    else
                    {
                        resultPairs.Add(currentPair);
                    }
                }
                return resultPairs;
            }
            private static string DecodeSingleValue(string toDecode, out string encodingUsed)
            {
                if (toDecode == null)
                {
                    throw new ArgumentNullException("toDecode");
                }
                string result;
                if (toDecode.IndexOf('\'') == -1)
                {
                    encodingUsed = null;
                    result = toDecode;
                }
                else
                {
                    encodingUsed = toDecode.Substring(0, toDecode.IndexOf('\''));
                    toDecode = toDecode.Substring(toDecode.LastIndexOf('\'') + 1);
                    result = DecodeSingleValue(toDecode, encodingUsed);
                }
                return result;
            }

            /// <summary>
            /// This will decode a single value of the form: %3D%3DIamHere
            /// Which is basically a <see cref="T:OpenPop.Mime.Decode.EncodedWord" /> form just using % instead of =
            /// </summary>
            /// <param name="valueToDecode">The value to decode</param>
            /// <param name="encoding">The encoding used to decode with</param>
            /// <returns>The decoded value that corresponds to <paramref name="valueToDecode" /></returns>
            /// <exception cref="T:System.ArgumentNullException">If <paramref name="valueToDecode" /> is <see langword="null" /></exception>
            /// <exception cref="T:System.ArgumentNullException">If <paramref name="encoding" /> is <see langword="null" /></exception>
            private static string DecodeSingleValue(string valueToDecode, string encoding)
            {
                if (valueToDecode == null)
                {
                    throw new ArgumentNullException("valueToDecode");
                }
                if (encoding == null)
                {
                    throw new ArgumentNullException("encoding");
                }
                valueToDecode = string.Concat(new string[]
			{
				"=?",
				encoding,
				"?Q?",
				valueToDecode.Replace("%", "="),
				"?="
			});
                return Utils.Decode(valueToDecode);
            }

            /// <summary>
            /// Create the digest for the APOP command so that the server can validate we know the password for some user.
            /// </summary>
          
            public static string ComputeDigest(string password, string serverTimestamp)
            {
                if (password == null)
                {
                    throw new ArgumentNullException("password");
                }
                if (serverTimestamp == null)
                {
                    throw new ArgumentNullException("serverTimestamp");
                }
                byte[] digestToHash = Encoding.ASCII.GetBytes(serverTimestamp + password);
                string result2;
                using (MD5 md5 = new MD5CryptoServiceProvider())
                {
                    byte[] result = md5.ComputeHash(digestToHash);
                    result2 = BitConverter.ToString(result).Replace("-", "").ToLowerInvariant();
                }
                return result2;
            }
            #region 使用CramMd5加密

            private static readonly byte[] ipad;
            private static readonly byte[] opad;
           static Utils() //静态构造 最多只运行一次，用于初始化静态变量
           {
               Utils.ipad = new byte[64];
               Utils.opad = new byte[64];
               for (int i = 0; i < Utils.ipad.Length; i++)
               {
                   Utils.ipad[i] = 54;
                   Utils.opad[i] = 92;
               }
           }
            internal static string ComputeDigest(string username, string password, string challenge)
            {
                
                if (username == null)
                {
                    throw new ArgumentNullException("username");
                }
                if (password == null)
                {
                    throw new ArgumentNullException("password");
                }
                if (challenge == null)
                {
                    throw new ArgumentNullException("challenge");
                }
                byte[] passwordBytes = GetSharedSecretInBytes(password);
                byte[] challengeBytes = Convert.FromBase64String(challenge);
                byte[] passwordOpad = Utils.Xor(passwordBytes, Utils.opad);
                byte[] passwordIpad = Utils.Xor(passwordBytes, Utils.ipad);
                byte[] digestValue = Utils.Hash(Utils.Concatenate(passwordOpad, Utils.Hash(Utils.Concatenate(passwordIpad, challengeBytes))));
                string hex = BitConverter.ToString(digestValue).Replace("-", "").ToLowerInvariant();
                return Convert.ToBase64String(Encoding.ASCII.GetBytes(username + " " + hex));
            }
            /// <summary>
            /// Hashes a byte array using the MD5 algorithm.
            /// </summary>
            private static byte[] Hash(byte[] toHash)
            {
                if (toHash == null)
                {
                    throw new ArgumentNullException("toHash");
                }
                byte[] result;
                using (MD5 md5 = new MD5CryptoServiceProvider())
                {
                    result = md5.ComputeHash(toHash);
                }
                return result;
            }
            /// <summary>
            /// Concatenates two byte arrays into one
            /// </summary>
            private static byte[] Concatenate(byte[] one, byte[] two)
            {
                if (one == null)
                {
                    throw new ArgumentNullException("one");
                }
                if (two == null)
                {
                    throw new ArgumentNullException("two");
                }
                byte[] concatenated = new byte[one.Length + two.Length];
                Buffer.BlockCopy(one, 0, concatenated, 0, one.Length);
                Buffer.BlockCopy(two, 0, concatenated, one.Length, two.Length);
                return concatenated;
            }

            private static byte[] GetSharedSecretInBytes(string password)
            {
                if (password == null)
                {
                    throw new ArgumentNullException("password");
                }
                byte[] passwordBytes = Encoding.ASCII.GetBytes(password);
                if (passwordBytes.Length > 64)
                {
                    passwordBytes = new MD5CryptoServiceProvider().ComputeHash(passwordBytes);
                }
                byte[] result;
                if (passwordBytes.Length != 64)
                {
                    byte[] returner = new byte[64];
                    for (int i = 0; i < passwordBytes.Length; i++)
                    {
                        returner[i] = passwordBytes[i];
                    }
                    result = returner;
                }
                else
                {
                    result = passwordBytes;
                }
                return result;
            }
            private static byte[] Xor(byte[] toXor, byte[] toXorWith)
            {
                if (toXor == null)
                {
                    throw new ArgumentNullException("toXor");
                }
                if (toXorWith == null)
                {
                    throw new ArgumentNullException("toXorWith");
                }
                if (toXor.Length != toXorWith.Length)
                {
                    throw new ArgumentException("The lengths of the arrays must be equal");
                }
                byte[] xored = new byte[toXor.Length];
                for (int i = 0; i < toXor.Length; i++)
                {
                    xored[i] = toXor[i];
                    byte[] expr_5F_cp_0 = xored;
                    int expr_5F_cp_1 = i;
                    expr_5F_cp_0[expr_5F_cp_1] ^= toXorWith[i];
                }
                return xored;
            }

            #endregion
    }
    public static class EncodingFinder
    {
        /// <summary>
        /// Delegate that is used when the EncodingFinder is unable to find an encoding by
        /// using the <see cref="EncodingFinder.EncodingMap"/> or general code.<br/>
        /// This is used as a last resort and can be used for setting a default encoding or
        /// for finding an encoding on runtime for some <paramref name="characterSet"/>.
        /// </summary>
        /// <param name="characterSet">The character set to find an encoding for.</param>
        /// <returns>An encoding for the <paramref name="characterSet"/> or <see langword="null"/> if none could be found.</returns>
        public delegate Encoding FallbackDecoderDelegate(string characterSet);

        /// <summary>
        /// Last resort decoder.
        /// </summary>
        public static FallbackDecoderDelegate FallbackDecoder { private get; set; }

        /// <summary>
        /// Mapping from charactersets to encodings.
        /// </summary>
        private static Dictionary<string, Encoding> EncodingMap { get; set; }

        /// <summary>
        /// Initialize the EncodingFinder
        /// </summary>
        static EncodingFinder()
        {
            Reset();
        }

        /// <summary>
        /// Used to reset this static class to facilite isolated unit testing.
        /// </summary>
        internal static void Reset()
        {
            EncodingMap = new Dictionary<string, Encoding>();
            FallbackDecoder = null;

            // Some emails incorrectly specify the encoding as utf8, but it should have been utf-8.
            AddMapping("utf8", Encoding.UTF8);
        }

        /// <summary>
        /// Parses a character set into an encoding.
        /// </summary>
        /// <param name="characterSet">The character set to parse</param>
        /// <returns>An encoding which corresponds to the character set</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="characterSet"/> is <see langword="null"/></exception>
        internal static Encoding FindEncoding(string characterSet)
        {
            if (characterSet == null)
                throw new ArgumentNullException("characterSet");

            string charSetUpper = characterSet.ToUpperInvariant();

            // Check if the characterSet is explicitly mapped to an encoding
            if (EncodingMap.ContainsKey(charSetUpper))
                return EncodingMap[charSetUpper];

            // Try to generally find the encoding
            try
            {
                if (charSetUpper.Contains("WINDOWS") || charSetUpper.Contains("CP"))
                {
                    // It seems the characterSet contains an codepage value, which we should use to parse the encoding
                    charSetUpper = charSetUpper.Replace("CP", ""); // Remove cp
                    charSetUpper = charSetUpper.Replace("WINDOWS", ""); // Remove windows
                    charSetUpper = charSetUpper.Replace("-", ""); // Remove - which could be used as cp-1554

                    // Now we hope the only thing left in the characterSet is numbers.
                    int codepageNumber = int.Parse(charSetUpper, CultureInfo.InvariantCulture);

                    return Encoding.GetEncoding(codepageNumber);
                }

                // It seems there is no codepage value in the characterSet. It must be a named encoding
                return Encoding.GetEncoding(characterSet);
            }
            catch (ArgumentException)
            {
                // The encoding could not be found generally. 
                // Try to use the FallbackDecoder if it is defined.

                // Check if it is defined
                if (FallbackDecoder == null)
                    throw; // It was not defined - throw catched exception

                // Use the FallbackDecoder
                Encoding fallbackDecoderResult = FallbackDecoder(characterSet);

                // Check if the FallbackDecoder had a solution
                if (fallbackDecoderResult != null)
                    return fallbackDecoderResult;

                // If no solution was found, throw catched exception
                throw;
            }
        }

        /// <summary>
        /// Puts a mapping from <paramref name="characterSet"/> to <paramref name="encoding"/>
        /// into the <see cref="EncodingFinder"/>'s internal mapping Dictionary.
        /// </summary>
        /// <param name="characterSet">The string that maps to the <paramref name="encoding"/></param>
        /// <param name="encoding">The <see cref="Encoding"/> that should be mapped from <paramref name="characterSet"/></param>
        /// <exception cref="ArgumentNullException">If <paramref name="characterSet"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException">If <paramref name="encoding"/> is <see langword="null"/></exception>
        public static void AddMapping(string characterSet, Encoding encoding)
        {
            if (characterSet == null)
                throw new ArgumentNullException("characterSet");

            if (encoding == null)
                throw new ArgumentNullException("encoding");

            // Add the mapping using uppercase
            EncodingMap.Add(characterSet.ToUpperInvariant(), encoding);
        }
    }
}
