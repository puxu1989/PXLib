using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;

namespace PXLib.Net.Email
{
    public sealed class MessageHeader
    {
        #region 属性字段
        public string Subject
        {
            get;
            private set;
        }
        public string MessageId
        {
            get;
            private set;
        }

        public DateTime DateSent
        {
            get;
            private set;
        }
        public string Date
        {
            get;
            private set;
        }
        public string ContentId
        {
            get;
            private set;
        }
        public string ContentDescription
        {
            get;
            private set;
        }
        public RfcMailAddress From
        {
            get;
            private set;
        }
        public RfcMailAddress ReplyTo
        {
            get;
            private set;
        }
        public RfcMailAddress Sender
        {
            get;
            private set;
        }
        public List<RfcMailAddress> To
        {
            get;
            private set;
        }
        public List<RfcMailAddress> Cc
        {
            get;
            private set;
        }
        public List<RfcMailAddress> Bcc
        {
            get;
            private set;
        }
        public List<Received> Received
        {
            get;
            private set;
        }
        public List<string> Keywords
        {
            get;
            private set;
        }
        /// <summary>
        /// The message identifier(s) of the original message(s) to which the current message is a reply
        /// The list will be empty if no In-Reply-To header was present in the message
        /// </summary>
        public List<string> InReplyTo
        {
            get;
            private set;
        }
        /// <summary>
        /// The message identifier(s) of other message(s) to which the current message is related to.
        /// The list will be empty if no References header was present in the message
        /// </summary>
        public List<string> References
        {
            get;
            private set;
        }
        /// <summary>
        /// A List of emails to people who wishes to be notified when some event happens.These events could be email:
        /// <summary>
        public List<RfcMailAddress> DispositionNotificationTo
        {
            get;
            private set;
        }
        public NameValueCollection UnknownHeaders
        {
            get;
            private set;
        }
        public MailPriority Importance
        {
            get;
            private set;
        }
        public ContentTransferEncoding ContentTransferEncoding
        {
            get;
            private set;
        }
        public ContentType ContentType
        {
            get;
            private set;
        }
        /// <summary>
        /// The Mime Version.This field will almost always show 1.0
        /// </summary>
        public string MimeVersion
        {
            get;
            private set;
        }
        public RfcMailAddress ReturnPath
        {
            get;
            private set;
        }
        public ContentDisposition ContentDisposition
        {
            get;
            private set;
        }

        #endregion
        internal MessageHeader(NameValueCollection headers)
        {
            if (headers == null)
                throw new ArgumentNullException("headers");

            // Create empty lists as defaults. We do not like null values
            // List with an initial capacity set to zero will be replaced
            // when a corrosponding header is found
            To = new List<RfcMailAddress>(0);
            Cc = new List<RfcMailAddress>(0);
            Bcc = new List<RfcMailAddress>(0);
            Received = new List<Received>();
            Keywords = new List<string>();
            InReplyTo = new List<string>(0);
            References = new List<string>(0);
            DispositionNotificationTo = new List<RfcMailAddress>();
            UnknownHeaders = new NameValueCollection();

            // Default importancetype is Normal (assumed if not set)
            Importance = MailPriority.Normal;

            // 7BIT is the default ContentTransferEncoding (assumed if not set)
            ContentTransferEncoding = ContentTransferEncoding.SevenBit;

            // text/plain; charset=us-ascii is the default ContentType
            ContentType = new ContentType("text/plain; charset=us-ascii");

            // Now parse the actual headers
            ParseHeaders(headers);
        }
        private void ParseHeaders(NameValueCollection headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }
            foreach (string headerName in headers.Keys)
            {
                string[] headerValues = headers.GetValues(headerName);
                if (headerValues != null)
                {
                    string[] array = headerValues;
                    for (int i = 0; i < array.Length; i++)
                    {
                        string headerValue = array[i];
                        this.ParseHeader(headerName, headerValue);
                    }
                }
            }
        }
        private void ParseHeader(string headerName, string headerValue)
        {
            if (headerName == null)
            {
                throw new ArgumentNullException("headerName");
            }
            if (headerValue == null)
            {
                throw new ArgumentNullException("headerValue");
            }
            string text = headerName.ToUpperInvariant();
            switch (text)
            {
                case "TO":
                    this.To = RfcMailAddress.ParseMailAddresses(headerValue);
                    return;
                case "CC":
                    this.Cc = RfcMailAddress.ParseMailAddresses(headerValue);
                    return;
                case "BCC":
                    this.Bcc = RfcMailAddress.ParseMailAddresses(headerValue);
                    return;
                case "FROM":
                    this.From = RfcMailAddress.ParseMailAddress(headerValue);
                    return;
                case "REPLY-TO":
                    this.ReplyTo = RfcMailAddress.ParseMailAddress(headerValue);
                    return;
                case "SENDER":
                    this.Sender = RfcMailAddress.ParseMailAddress(headerValue);
                    return;
                case "KEYWORDS":
                    {
                        string[] keywordsTemp = headerValue.Split(new char[]{','});
                        string[] array = keywordsTemp;
                        for (int i = 0; i < array.Length; i++)
                        {
                            string keyword = array[i];
                            this.Keywords.Add(Utils.RemoveQuotesIfAny(keyword.Trim()));
                        }
                        return;
                    }
                case "RECEIVED":
                    this.Received.Add(new Received(headerValue.Trim()));
                    return;
                case "IMPORTANCE":
                    this.Importance = HeaderFieldParser.ParseImportance(headerValue.Trim());
                    return;
                case "DISPOSITION-NOTIFICATION-TO":
                    this.DispositionNotificationTo = RfcMailAddress.ParseMailAddresses(headerValue);
                    return;
                case "MIME-VERSION":
                    this.MimeVersion = headerValue.Trim();
                    return;
                case "SUBJECT":
                    this.Subject = Utils.Decode(headerValue);
                    return;
                case "RETURN-PATH":
                    this.ReturnPath = RfcMailAddress.ParseMailAddress(headerValue);
                    return;
                case "MESSAGE-ID":
                    this.MessageId = HeaderFieldParser.ParseId(headerValue);
                    return;
                case "IN-REPLY-TO":
                    this.InReplyTo = HeaderFieldParser.ParseMultipleIDs(headerValue);
                    return;
                case "REFERENCES":
                    this.References = HeaderFieldParser.ParseMultipleIDs(headerValue);
                    return;
                case "DATE":
                    this.Date = headerValue.Trim();
                    this.DateSent = Utils.StringToDate(headerValue);
                    return;
                case "CONTENT-TRANSFER-ENCODING":
                    this.ContentTransferEncoding = HeaderFieldParser.ParseContentTransferEncoding(headerValue.Trim());
                    return;
                case "CONTENT-DESCRIPTION":
                    this.ContentDescription = Utils.Decode(headerValue.Trim());
                    return;
                case "CONTENT-TYPE":
                    this.ContentType = HeaderFieldParser.ParseContentType(headerValue);
                    return;
                case "CONTENT-DISPOSITION":
                    this.ContentDisposition = HeaderFieldParser.ParseContentDisposition(headerValue);
                    return;
                case "CONTENT-ID":
                    this.ContentId = HeaderFieldParser.ParseId(headerValue);
                    return;
            }
            this.UnknownHeaders.Add(headerName, headerValue);
        }

    }
    /// <summary>
    /// hold information about one "Received:" header line
    /// </summary>
    public class Received 
    {
      
        public DateTime Date
        {
            get;
            private set;
        }

        /// <summary>
        /// A dictionary that contains the names and values of the received header line.
        /// <summary>    
        public Dictionary<string, string> Names
        {
            get;
            private set;
        }

        /// <summary>
        /// The raw input string that was parsed into this class.
        /// </summary>
        public string Raw
        {
            get;
            private set;
        }

        /// <summary>
        /// Parses a Received header value.
        /// </summary>
        /// <param name="headerValue">The value for the header to be parsed</param>
        public Received(string headerValue)
        {
            if (headerValue == null)
            {
                throw new ArgumentNullException("headerValue");
            }
            this.Raw = headerValue;
            this.Date = DateTime.MinValue;
            if (headerValue.Contains(";"))
            {
                string datePart = headerValue.Substring(headerValue.LastIndexOf(";") + 1);

                this.Date = Utils.StringToDate(datePart);
            }
            this.Names = Received.ParseDictionary(headerValue);
        }

        /// <summary>
        /// Parses the Received header name-value-list into a dictionary.
        /// </summary>
        /// <param name="headerValue">The full header value for the Received header</param>
        /// <returns>A dictionary where the name-value-list has been parsed into</returns>
        private static Dictionary<string, string> ParseDictionary(string headerValue)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string headerValueWithoutDate = headerValue;
            if (headerValue.Contains(";"))
            {
                headerValueWithoutDate = headerValue.Substring(0, headerValue.LastIndexOf(";"));
            }
            headerValueWithoutDate = Regex.Replace(headerValueWithoutDate, "\\s+", " ");
            MatchCollection matches = Regex.Matches(headerValueWithoutDate, "(?<name>[^\\s]+)\\s(?<value>[^\\s]+(\\s\\(.+?\\))*)");
            foreach (Match match in matches)
            {
                string name = match.Groups["name"].Value;
                string value = match.Groups["value"].Value;
                if (!name.StartsWith("("))
                {
                    if (!dictionary.ContainsKey(name))
                    {
                        dictionary.Add(name, value);
                    }
                }
            }
            return dictionary;
        }
    }
    public enum ContentTransferEncoding
    {
        /// <summary>
        /// 7 bit Encoding
        /// </summary>
        SevenBit,
        /// <summary>
        /// 8 bit Encoding
        /// </summary>
        EightBit,
        /// <summary>
        /// Quoted Printable Encoding
        /// </summary>
        QuotedPrintable,
        /// <summary>
        /// Base64 Encoding
        /// </summary>
        Base64,
        /// <summary>
        /// Binary Encoding
        /// </summary>
        Binary
    }

   internal static class HeaderFieldParser
	{
		/// <summary>
		/// Parses the Content-Transfer-Encoding header.
		/// </summary>
		/// <param name="headerValue">The value for the header to be parsed</param>
		/// <returns>A <see cref="T:OpenPop.Mime.Header.ContentTransferEncoding" /></returns>
		/// <exception cref="T:System.ArgumentNullException">If <paramref name="headerValue" /> is <see langword="null" /></exception>
		/// <exception cref="T:System.ArgumentException">If the <paramref name="headerValue" /> could not be parsed to a <see cref="T:OpenPop.Mime.Header.ContentTransferEncoding" /></exception>
		public static ContentTransferEncoding ParseContentTransferEncoding(string headerValue)
		{
			if (headerValue == null)
			{
				throw new ArgumentNullException("headerValue");
			}
			string text = headerValue.Trim().ToUpperInvariant();
			ContentTransferEncoding result;
			if (text != null)
			{
				if (text == "7BIT")
				{
					result = ContentTransferEncoding.SevenBit;
					return result;
				}
				if (text == "8BIT")
				{
					result = ContentTransferEncoding.EightBit;
					return result;
				}
				if (text == "QUOTED-PRINTABLE")
				{
					result = ContentTransferEncoding.QuotedPrintable;
					return result;
				}
				if (text == "BASE64")
				{
					result = ContentTransferEncoding.Base64;
					return result;
				}
				if (text == "BINARY")
				{
					result = ContentTransferEncoding.Binary;
					return result;
				}
			}
			result = ContentTransferEncoding.SevenBit;
			return result;
		}

		/// <summary>
		/// Parses an ImportanceType from a given Importance header value.
		/// </summary>
		/// <param name="headerValue">The value to be parsed</param>
		/// <returns>A <see cref="T:System.Net.Mail.MailPriority" />. If the <paramref name="headerValue" /> is not recognized, Normal is returned.</returns>
		/// <exception cref="T:System.ArgumentNullException">If <paramref name="headerValue" /> is <see langword="null" /></exception>
		public static MailPriority ParseImportance(string headerValue)
		{
			if (headerValue == null)
			{
				throw new ArgumentNullException("headerValue");
			}
			string text = headerValue.ToUpperInvariant();
			MailPriority result;
			switch (text)
			{
			case "5":
			case "HIGH":
				result = MailPriority.High;
				return result;
			case "3":
			case "NORMAL":
				result = MailPriority.Normal;
				return result;
			case "1":
			case "LOW":
				result = MailPriority.Low;
				return result;
			}
			result = MailPriority.Normal;
			return result;
		}

		/// <summary>
		/// Parses a the value for the header Content-Type to  a  object.
		/// </summary>
        public static ContentType ParseContentType(string headerValue)
        {
            if (headerValue == null)
                throw new ArgumentNullException("headerValue");

            // We create an empty Content-Type which we will fill in when we see the values
            ContentType contentType = new ContentType();

            // Now decode the parameters
            List<KeyValuePair<string, string>> parameters = Utils.Rfc2231Decode(headerValue);

            foreach (KeyValuePair<string, string> keyValuePair in parameters)
            {
                string key = keyValuePair.Key.ToUpperInvariant().Trim();
                string value = Utils.RemoveQuotesIfAny(keyValuePair.Value.Trim());
                switch (key)
                {
                    case "":
                        if (value.ToUpperInvariant().Equals("TEXT"))
                            value = "text/plain";

                        contentType.MediaType = value;
                        break;

                    case "BOUNDARY":
                        contentType.Boundary = value;
                        break;

                    case "CHARSET":
                        contentType.CharSet = value;
                        break;

                    case "NAME":
                        contentType.Name = Utils.Decode(value);
                        break;

                    default:
                        // This is to shut up the code help that is saying that contentType.Parameters
                        // can be null - which it cant!
                        if (contentType.Parameters == null)
                            throw new Exception("The ContentType parameters property is null. This will never be thrown.");

                        // We add the unknown value to our parameters list
                        // "Known" unknown values are:
                        // - title
                        // - report-type
                        contentType.Parameters.Add(key, value);
                        break;
                }
            }
            return contentType;
        }
		/// <summary>
		/// Parses a the value for the header Content-Disposition to a <see cref="T:System.Net.Mime.ContentDisposition" /> object.
		/// </summary>
        public static ContentDisposition ParseContentDisposition(string headerValue)
        {
            if (headerValue == null) 
            {
                throw new ArgumentNullException("headerValue");
            }
            ContentDisposition contentDisposition = new ContentDisposition();
            List<KeyValuePair<string, string>> parameters = Utils.Rfc2231Decode(headerValue);
            foreach (KeyValuePair<string, string> keyValuePair in parameters)
            {
                string key = keyValuePair.Key.ToUpperInvariant().Trim();
                string value = Utils.RemoveQuotesIfAny(keyValuePair.Value.Trim());
                switch (key)
                {
                    case "":
                        // This is the DispisitionType - it has no key since it is the first one
                        // and has no = in it.
                        contentDisposition.DispositionType = value;
                        break;

                    // The correct name of the parameter is filename, but some emails also contains the parameter
                    // name, which also holds the name of the file. Therefore we use both names for the same field.
                    case "NAME":
                    case "FILENAME":
                        // The filename might be in qoutes, and it might be encoded-word encoded
                        contentDisposition.FileName = Utils.Decode(value);
                        break;

                    case "CREATION-DATE":
                        // Notice that we need to create a new DateTime because of a failure in .NET 2.0.
                        // The failure is: you cannot give contentDisposition a DateTime with a Kind of UTC
                        // It will set the CreationDate correctly, but when trying to read it out it will throw an exception.
                        // It is the same with ModificationDate and ReadDate.
                        // This is fixed in 4.0 - maybe in 3.0 too.
                        // Therefore we create a new DateTime which have a DateTimeKind set to unspecified
                        DateTime creationDate = new DateTime(Utils.StringToDate(value).Ticks);
                        contentDisposition.CreationDate = creationDate;
                        break;

                    case "MODIFICATION-DATE":
                        DateTime midificationDate = new DateTime(Utils.StringToDate(value).Ticks);
                        contentDisposition.ModificationDate = midificationDate;
                        break;

                    case "READ-DATE":
                        DateTime readDate = new DateTime(Utils.StringToDate(value).Ticks);
                        contentDisposition.ReadDate = readDate;
                        break;

                    case "SIZE":
                        contentDisposition.Size = Utils.SizeParse(value);
                        break;

                    default:
                        if (key.StartsWith("X-"))
                        {
                            contentDisposition.Parameters.Add(key, value);
                            break;
                        }

                        throw new ArgumentException("Unknown parameter in Content-Disposition. Ask developer to fix! Parameter: " + key);
                }
            }

            return contentDisposition;
        }

		/// <summary>
		/// Parses an ID like Message-Id and Content-Id.<br />
		/// Example:<br />
		/// <c>&lt;test@test.com&gt;</c><br />
		/// into<br />
		/// <c>test@test.com</c>
		/// </summary>
		/// <param name="headerValue">The id to parse</param>
		/// <returns>A parsed ID</returns>
		public static string ParseId(string headerValue)
		{
			return headerValue.Trim().TrimEnd(new char[]
			{
				'>'
			}).TrimStart(new char[]
			{
				'<'
			});
		}

		/// <summary>
		/// Parses multiple IDs from a single string like In-Reply-To.
		/// </summary>
		/// <param name="headerValue">The value to parse</param>
		/// <returns>A list of IDs</returns>
		public static List<string> ParseMultipleIDs(string headerValue)
		{
			List<string> returner = new List<string>();
			string[] ids = headerValue.Trim().Split(new char[]
			{
				'>'
			}, StringSplitOptions.RemoveEmptyEntries);
			string[] array = ids;
			for (int i = 0; i < array.Length; i++)
			{
				string id = array[i];
				returner.Add(HeaderFieldParser.ParseId(id));
			}
			return returner;
		}
	}
}
