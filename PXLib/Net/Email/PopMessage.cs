using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace PXLib.Net.Email
{
    public class PopMessage
    {
        public MessageHeader Headers { get; private set; }//邮件的消息头

        public MessagePart MessagePart { get; private set; }    //邮件的Body

        public byte[] RawMessage { get; private set; }//构造消息的原始内容

        public MessagePart FindFirstPlainTextVersion()
        {
            return this.FindFirstMessagePartWithMediaType("text/plain");
        }
        public MessagePart FindFirstHtmlVersion()
        {
            return this.FindFirstMessagePartWithMediaType("text/html");
        }
        public PopMessage(byte[] rawMessageContent,bool getBody)
        {
            this.RawMessage = rawMessageContent;
            MessageHeader headersTemp;
            byte[] body;
            HeaderExtractor.ExtractHeadersAndBody(rawMessageContent, out headersTemp, out body);
            this.Headers = headersTemp;
            if (getBody)
			{
				this.MessagePart = new MessagePart(body, this.Headers);
			}
        }
        public MessagePart FindFirstMessagePartWithMediaType(string mediaType) 
        {
            return new FindFirstMessagePartWithMediaType().VisitMessage(this, mediaType);
        }
        public static PopMessage Load(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            if (!file.Exists)
            {
                throw new FileNotFoundException("Cannot load message from non-existent file", file.FullName);
            }
            PopMessage result;
            using (FileStream stream = new FileStream(file.FullName, FileMode.Open))
            {
                result = Load(stream);
            }
            return result;
        }
        /// <summary>
        /// 从stream
        /// </summary>
        /// <param name="messageStream"></param>
        /// <returns></returns>
        public static PopMessage Load(Stream messageStream)
        {
            if (messageStream == null)
            {
                throw new ArgumentNullException("messageStream");
            }
            PopMessage result;
            using (MemoryStream outStream = new MemoryStream())
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = messageStream.Read(buffer, 0, 4096)) > 0)
                {
                    outStream.Write(buffer, 0, bytesRead);
                }
                byte[] content = outStream.ToArray();
                result = new PopMessage(content,true);
            }
            return result;
        }
        public void Save(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            using (FileStream stream = new FileStream(file.FullName, FileMode.Create))
            {
                this.Save(stream);
            }
        }
        public void Save(Stream messageStream)
        {
            if (messageStream == null)
            {
                throw new ArgumentNullException("messageStream");
            }
            messageStream.Write(this.RawMessage, 0, this.RawMessage.Length);
        }
        /// <summary>
        /// 转换成.Net 自带的MailMessage
        /// </summary>
        /// <returns></returns>
        public MailMessage ToMailMessage()
        {
            MailMessage message = new MailMessage();
            message.Subject = this.Headers.Subject;
            message.SubjectEncoding = Encoding.UTF8;
            MessagePart preferredVersion = this.FindFirstHtmlVersion();
            if (preferredVersion != null)
            {
                message.IsBodyHtml = true;
            }
            else
            {
                preferredVersion = this.FindFirstPlainTextVersion();
            }
            if (preferredVersion != null)
            {
                message.Body = preferredVersion.GetBodyAsText();
                message.BodyEncoding = preferredVersion.BodyEncoding;
            }
            IEnumerable<MessagePart> textVersions = this.FindAllTextVersions();
            foreach (MessagePart textVersion in textVersions)
            {
                if (textVersion != preferredVersion)
                {
                    MemoryStream stream = new MemoryStream(textVersion.Body);
                    AlternateView alternative = new AlternateView(stream);
                    alternative.ContentId = textVersion.ContentId;
                    alternative.ContentType = textVersion.ContentType;
                    message.AlternateViews.Add(alternative);
                }
            }
            IEnumerable<MessagePart> attachments = this.FindAllAttachments();
            foreach (MessagePart attachmentMessagePart in attachments)
            {
                MemoryStream stream = new MemoryStream(attachmentMessagePart.Body);
                Attachment attachment = new Attachment(stream, attachmentMessagePart.ContentType);
                attachment.ContentId = attachmentMessagePart.ContentId;
                message.Attachments.Add(attachment);
            }
            if (this.Headers.From != null && this.Headers.From.HasValidMailAddress)
            {
                message.From = this.Headers.From.MailAddress;
            }
            if (this.Headers.ReplyTo != null && this.Headers.ReplyTo.HasValidMailAddress)
            {
                message.ReplyToList.Add(this.Headers.ReplyTo.MailAddress);
            }
            if (this.Headers.Sender != null && this.Headers.Sender.HasValidMailAddress)
            {
                message.Sender = this.Headers.Sender.MailAddress;
            }
            foreach (RfcMailAddress to in this.Headers.To)
            {
                if (to.HasValidMailAddress)
                {
                    message.To.Add(to.MailAddress);
                }
            }
            foreach (RfcMailAddress cc in this.Headers.Cc)
            {
                if (cc.HasValidMailAddress)
                {
                    message.CC.Add(cc.MailAddress);
                }
            }
            foreach (RfcMailAddress bcc in this.Headers.Bcc)
            {
                if (bcc.HasValidMailAddress)
                {
                    message.Bcc.Add(bcc.MailAddress);
                }
            }
            return message;
        }
        public List<MessagePart> FindAllTextVersions()
        {
            return new TextVersionFinder().VisitMessage(this);
        }

        /// <summary>
        /// Finds all the <see cref="P:OpenPop.Mime.Message.MessagePart" />'s which are attachments to this message.<br />
        /// <br />
        /// <see cref="P:OpenPop.Mime.MessagePart.IsAttachment" /> for MessageParts which are considered to be attachments.
        /// </summary>
        /// <returns>A List of MessageParts where each is considered an attachment</returns>
        public List<MessagePart> FindAllAttachments()
        {
            return new TextVersionFinder().VisitMessage(this);
        }
    }
}
