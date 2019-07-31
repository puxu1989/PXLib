using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace PXLib.Net.Email
{
    /// <summary>
    ///  Utility class that divides a message into a body and a header
    /// </summary>
    internal static class HeaderExtractor
    {
        private static int FindHeaderEndPosition(byte[] messageContent)
        {
            if (messageContent == null)
            {
                throw new ArgumentNullException("messageContent");
            }
            int result;
            using (Stream stream = new MemoryStream(messageContent))
            {
                string line;
                do
                {
                    line = Utils.ReadLineAsAscii(stream);
                }
                while (!string.IsNullOrEmpty(line));
                result = (int)stream.Position;
            }
            return result;
        }

        /// <summary>
        /// Extract the header part and body part of a message.<br />
        /// The headers are then parsed to a strongly typed <see cref="T:OpenPop.Mime.Header.MessageHeader" /> object.
        /// </summary>
        /// <param name="fullRawMessage">The full message in bytes where header and body needs to be extracted from</param>
        /// <param name="headers">The extracted header parts of the message</param>
        /// <param name="body">The body part of the message</param>
        /// <exception cref="T:System.ArgumentNullException">If <paramref name="fullRawMessage" /> is <see langword="null" /></exception>
        public static void ExtractHeadersAndBody(byte[] fullRawMessage, out MessageHeader headers, out byte[] body)
        {
            if (fullRawMessage == null)
            {
                throw new ArgumentNullException("fullRawMessage");
            }
            int endOfHeaderLocation = HeaderExtractor.FindHeaderEndPosition(fullRawMessage);
            string headersString = Encoding.ASCII.GetString(fullRawMessage, 0, endOfHeaderLocation);
            NameValueCollection headersUnparsedCollection = HeaderExtractor.ExtractHeaders(headersString);
            headers = new MessageHeader(headersUnparsedCollection);
            body = new byte[fullRawMessage.Length - endOfHeaderLocation];
            Array.Copy(fullRawMessage, endOfHeaderLocation, body, 0, body.Length);
        }

        /// <summary>
        /// Method that takes a full message and extract the headers from it.
        /// </summary>
        /// <param name="messageContent">The message to extract headers from. Does not need the body part. Needs the empty headers end line.</param>
        /// <returns>A collection of Name and Value pairs of headers</returns>
        /// <exception cref="T:System.ArgumentNullException">If <paramref name="messageContent" /> is <see langword="null" /></exception>
        private static NameValueCollection ExtractHeaders(string messageContent)
        {
            if (messageContent == null)
            {
                throw new ArgumentNullException("messageContent");
            }
            NameValueCollection headers = new NameValueCollection();
            using (StringReader messageReader = new StringReader(messageContent))
            {
                string line;
                while (!string.IsNullOrEmpty(line = messageReader.ReadLine()))
                {
                    KeyValuePair<string, string> header = HeaderExtractor.SeparateHeaderNameAndValue(line);
                    string headerName = header.Key;
                    StringBuilder headerValue = new StringBuilder(header.Value);
                    while (HeaderExtractor.IsMoreLinesInHeaderValue(messageReader))
                    {
                        string moreHeaderValue = messageReader.ReadLine();
                        if (moreHeaderValue == null)
                        {
                            throw new ArgumentException("This will never happen");
                        }
                        headerValue.Append(moreHeaderValue);
                    }
                    headers.Add(headerName, headerValue.ToString());
                }
            }
            return headers;
        }

        /// <summary>
        /// Check if the next line is part of the current header value we are parsing by
        /// peeking on the next character of the <see cref="T:System.IO.TextReader" />.<br />
        /// This should only be called while parsing headers.
        /// </summary>
        /// <param name="reader">The reader from which the header is read from</param>
        /// <returns><see langword="true" /> if multi-line header. <see langword="false" /> otherwise</returns>
        private static bool IsMoreLinesInHeaderValue(TextReader reader)
        {
            int peek = reader.Peek();
            bool result;
            if (peek == -1)
            {
                result = false;
            }
            else
            {
                char peekChar = (char)peek;
                result = (peekChar == ' ' || peekChar == '\t');
            }
            return result;
        }

        /// <summary>
        /// Separate a full header line into a header name and a header value.
        /// </summary>
        /// <param name="rawHeader">The raw header line to be separated</param>
        /// <exception cref="T:System.ArgumentNullException">If <paramref name="rawHeader" /> is <see langword="null" /></exception>
        internal static KeyValuePair<string, string> SeparateHeaderNameAndValue(string rawHeader)
        {
            if (rawHeader == null)
            {
                throw new ArgumentNullException("rawHeader");
            }
            string key = string.Empty;
            string value = string.Empty;
            int indexOfColon = rawHeader.IndexOf(':');
            if (indexOfColon >= 0 && rawHeader.Length >= indexOfColon + 1)
            {
                key = rawHeader.Substring(0, indexOfColon).Trim();
                value = rawHeader.Substring(indexOfColon + 1).Trim();
            }
            return new KeyValuePair<string, string>(key, value);
        }
    }
}
