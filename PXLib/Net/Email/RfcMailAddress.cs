using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace PXLib.Net.Email
{
  public  class RfcMailAddress
    {
        public string Address
        {
            get;
            private set;
        }

        public string DisplayName
        {
            get;
            private set;
        }

        /// <summary>
        /// This is the Raw string used to describe the <see cref="T:OpenPop.Mime.Header.RfcMailAddress" />.
        /// </summary>
        public string Raw
        {
            get;
            private set;
        }
        public MailAddress MailAddress
        {
            get;
            private set;
        }

        public bool HasValidMailAddress
        {
            get
            {
                return this.MailAddress != null;
            }
        }
        private RfcMailAddress(MailAddress mailAddress, string raw)
        {
            if (mailAddress == null)
            {
                throw new ArgumentNullException("mailAddress");
            }
            if (raw == null)
            {
                throw new ArgumentNullException("raw");
            }
            this.MailAddress = mailAddress;
            this.Address = mailAddress.Address;
            this.DisplayName = mailAddress.DisplayName;
            this.Raw = raw;
        }
        private RfcMailAddress(string raw)
        {
            if (raw == null)
            {
                throw new ArgumentNullException("raw");
            }
            this.MailAddress = null;
            this.Address = string.Empty;
            this.DisplayName = raw;
            this.Raw = raw;
        }
        public override string ToString()
        {
            string result;
            if (this.HasValidMailAddress)
            {
                result = this.MailAddress.ToString();
            }
            else
            {
                result = this.Raw;
            }
            return result;
        }

      /// <summary>
      /// Parses an email address from a MIME header
      /// </summary>
      /// <param name="input"></param>
      /// <returns></returns>
        internal static RfcMailAddress ParseMailAddress(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            input = Utils.Decode(input.Trim());
            int indexStartEmail = input.LastIndexOf('<');
            int indexEndEmail = input.LastIndexOf('>');
            RfcMailAddress result;
            try
            {
                if (indexStartEmail >= 0 && indexEndEmail >= 0)
                {
                    string username;
                    if (indexStartEmail > 0)
                    {
                        username = input.Substring(0, indexStartEmail).Trim();
                    }
                    else
                    {
                        username = string.Empty;
                    }
                    indexStartEmail++;
                    int emailLength = indexEndEmail - indexStartEmail;
                    string emailAddress = input.Substring(indexStartEmail, emailLength).Trim();
                    if (!string.IsNullOrEmpty(emailAddress))
                    {
                        result = new RfcMailAddress(new MailAddress(emailAddress, username), input);
                        return result;
                    }
                }
                if (input.Contains("@"))
                {
                    result = new RfcMailAddress(new MailAddress(input), input);
                    return result;
                }
            }
            catch (FormatException)
            {
                throw new Exception("RfcMailAddress: Improper mail address: \"" + input + "\"");
            }
            result = new RfcMailAddress(input);
            return result;
        }
        internal static List<RfcMailAddress> ParseMailAddresses(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            List<RfcMailAddress> returner = new List<RfcMailAddress>();
            IEnumerable<string> mailAddresses = Utils.SplitStringWithCharNotInsideQuotes(input, ',');
            foreach (string mailAddress in mailAddresses)
            {
                returner.Add(RfcMailAddress.ParseMailAddress(mailAddress));
            }
            return returner;
        }
    }
}
