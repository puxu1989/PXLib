using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;

namespace PXLib.Net.Email
{
    public class MessagePart
    {
        #region 字段
        public ContentType ContentType
        {
            get;
            private set;
        }
        public string ContentDescription
        {
            get;
            private set;
        }
        public ContentTransferEncoding ContentTransferEncoding
        {
            get;
            private set;
        }
        public string ContentId
        {
            get;
            private set;
        }
        public ContentDisposition ContentDisposition
        {
            get;
            private set;
        }
        public Encoding BodyEncoding
        {
            get;
            private set;
        }
        public byte[] Body
        {
            get;
            private set;
        }
        public bool IsMultiPart
        {
            get
            {
                return this.ContentType.MediaType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase);
            }
        }
        public bool IsText
        {
            get
            {
                string mediaType = this.ContentType.MediaType;
                return mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) || mediaType.Equals("message/rfc822", StringComparison.OrdinalIgnoreCase);
            }
        }
        public bool IsAttachment
        {
            get
            {
                return (!this.IsText && !this.IsMultiPart) || (this.ContentDisposition != null && !this.ContentDisposition.Inline);
            }
        }
        public string FileName
        {
            get;
            private set;
        }
        public List<MessagePart> MessageParts
        {
            get;
            private set;
        }
        #endregion
        public MessagePart(byte[] rawBody, MessageHeader headers)
        {
            if (rawBody == null)
                throw new ArgumentNullException("rawBody");
            if (headers == null)
                throw new ArgumentNullException("headers");
            ContentType = headers.ContentType;
            ContentDescription = headers.ContentDescription;
            ContentTransferEncoding = headers.ContentTransferEncoding;
            ContentId = headers.ContentId;
            ContentDisposition = headers.ContentDisposition;
            FileName = FindFileName(ContentType, ContentDisposition, "(no name)");
            BodyEncoding = ParseBodyEncoding(ContentType.CharSet);
            ParseBody(rawBody);
        }
        #region ParseBody
        private void ParseBody(byte[] rawBody)
        {
            if (this.IsMultiPart)
            {
                this.ParseMultiPartBody(rawBody);
            }
            else
            {
                this.Body = MessagePart.DecodeBody(rawBody, this.ContentTransferEncoding);
            }
        }
        private static byte[] DecodeBody(byte[] messageBody, ContentTransferEncoding contentTransferEncoding)
        {
            if (messageBody == null)
                throw new ArgumentNullException("messageBody");

            switch (contentTransferEncoding)
            {
                case ContentTransferEncoding.QuotedPrintable:
                    // If encoded in QuotedPrintable, everything in the body is in US-ASCII
                    return QuotedPrintable.DecodeContentTransferEncoding(Encoding.ASCII.GetString(messageBody));

                case ContentTransferEncoding.Base64:
                    // If encoded in Base64, everything in the body is in US-ASCII
                    return Utils.Base64Decode(Encoding.ASCII.GetString(messageBody));

                case ContentTransferEncoding.SevenBit:
                case ContentTransferEncoding.Binary:
                case ContentTransferEncoding.EightBit:
                    // We do not have to do anything
                    return messageBody;

                default:
                    throw new ArgumentOutOfRangeException("contentTransferEncoding");
            }
        }
        private void ParseMultiPartBody(byte[] rawBody)
        {
            string multipartBoundary = this.ContentType.Boundary;
            List<byte[]> bodyParts = MessagePart.GetMultiPartParts(rawBody, multipartBoundary);
            this.MessageParts = new List<MessagePart>(bodyParts.Count);
            foreach (byte[] bodyPart in bodyParts)
            {
                MessagePart messagePart = MessagePart.GetMessagePart(bodyPart);
                this.MessageParts.Add(messagePart);
            }
        }
        private static MessagePart GetMessagePart(byte[] rawMessageContent)
        {
            MessageHeader headers;
            byte[] body;
            HeaderExtractor.ExtractHeadersAndBody(rawMessageContent, out headers, out body);
            return new MessagePart(body, headers);
        }
        private static List<byte[]> GetMultiPartParts(byte[] rawBody, string multipPartBoundary)
        {
            if (rawBody == null)
            {
                throw new ArgumentNullException("rawBody");
            }
            List<byte[]> messageBodies = new List<byte[]>();
            using (MemoryStream stream = new MemoryStream(rawBody))
            {
                bool lastMultipartBoundaryEncountered;
                int startLocation = MessagePart.FindPositionOfNextMultiPartBoundary(stream, multipPartBoundary, out lastMultipartBoundaryEncountered) + ("--" + multipPartBoundary + "\r\n").Length;
                while (!lastMultipartBoundaryEncountered)
                {
                    int stopLocation = MessagePart.FindPositionOfNextMultiPartBoundary(stream, multipPartBoundary, out lastMultipartBoundaryEncountered) - "\r\n".Length;
                    if (stopLocation <= -1)
                    {
                        stopLocation = (int)stream.Length - "\r\n".Length;
                        lastMultipartBoundaryEncountered = true;
                        if (startLocation >= stopLocation)
                        {
                            break;
                        }
                    }
                    int length = stopLocation - startLocation;
                    byte[] messageBody = new byte[length];
                    Array.Copy(rawBody, startLocation, messageBody, 0, length);
                    messageBodies.Add(messageBody);
                    startLocation = stopLocation + ("\r\n--" + multipPartBoundary + "\r\n").Length;
                }
            }
            return messageBodies;
        }
        private static int FindPositionOfNextMultiPartBoundary(Stream stream, string multiPartBoundary, out bool lastMultipartBoundaryFound)
        {
            lastMultipartBoundaryFound = false;
            while (true)
            {
                int currentPos = (int)stream.Position;
                string line = Utils.ReadLineAsAscii(stream);
                if (line == null)
                    return -1;
                if (line.StartsWith("--" + multiPartBoundary, StringComparison.Ordinal))
                {
                    lastMultipartBoundaryFound = line.StartsWith("--" + multiPartBoundary + "--", StringComparison.OrdinalIgnoreCase);
                    return currentPos;
                }
            }
        }

        #endregion

        private static Encoding ParseBodyEncoding(string characterSet)
        {
            Encoding encoding = Encoding.ASCII;
            if (!string.IsNullOrEmpty(characterSet))
            {
                encoding = EncodingFinder.FindEncoding(characterSet);
            }
            return encoding;
        }
        /// <summary>
        ///  Figures out the filename of this message part from some headers.
        /// </summary>
        private static string FindFileName(ContentType contentType, ContentDisposition contentDisposition, string defaultName)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            string result;
            if (contentDisposition != null && contentDisposition.FileName != null)
            {
                result = contentDisposition.FileName;
            }
            else if (contentType.Name != null)
            {
                result = contentType.Name;
            }
            else
            {
                result = defaultName;
            }
            return result;
        }
        public string GetBodyAsText()
        {
            return this.BodyEncoding.GetString(this.Body);
        }


    }
}
