using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace PXLib.Net.Email
{
    /// <summary>
    /// EmailClient基于OpenPop开源的电子邮件框架
    /// </summary>
    public class EmailClient : BaseDisposable
    {
        #region 字段
        /// <summary>
        /// The stream used to communicate with the server
        /// </summary>
        private Stream Stream;

        /// <summary>
        /// This is the last response the server sent back when a command was issued to it
        /// </summary>
        private string LastServerResponse;

        /// <summary>
        /// The APOP time stamp sent by the server in it's welcome message if APOP is supported.
        /// </summary>
        private string ApopTimeStamp;
        /// <summary>
        /// Describes what state the EmailClient is in
        /// </summary>
        private ConnectionState State;
        /// <summary>
        /// Tells whether the EmailClient is connected to a POP server or not
        /// </summary>
        public bool Connected { get; private set; }
        /// <summary>
        /// the server tells in its welcome message if APOP is supported //包括其他绑定的邮箱
        /// </summary>
        public bool ApopSupported { get; private set; }
        #endregion
        public EmailClient()
        {
            this.ApopTimeStamp = null;
            this.Connected = false;
            this.State = ConnectionState.Disconnected;
            this.ApopSupported = false;
        }
        #region 连接
        /// <summary>
        /// Connects to a remote POP3 server using default timeouts of 60.000 millisecond
        /// </summary>
        public void Connect(string hostname, int port, bool useSsl)
        {
            this.Connect(hostname, port, useSsl, 60000, 60000, null);
        }
        /// <summary>
        /// Connects to a remote POP3 server
        /// </summary>
        public void Connect(string hostname, int port, bool useSsl, int receiveTimeout, int sendTimeout, RemoteCertificateValidationCallback certificateValidator)
        {
            base.CheckDisposed();
            if (hostname == null)
            {
                throw new ArgumentNullException("hostname");
            }
            if (hostname.Length == 0)
            {
                throw new ArgumentException("hostname cannot be empty", "hostname");
            }
            if (port > 65535 || port < 0)
            {
                throw new ArgumentOutOfRangeException("port");
            }
            if (receiveTimeout < 0)
            {
                throw new ArgumentOutOfRangeException("receiveTimeout");
            }
            if (sendTimeout < 0)
            {
                throw new ArgumentOutOfRangeException("sendTimeout");
            }
            if (this.State != ConnectionState.Disconnected)
            {
                throw new Exception("You cannot ask to connect to a POP3 server, when we are already connected to one. Disconnect first.");
            }
            TcpClient clientSocket = new TcpClient();
            clientSocket.ReceiveTimeout = receiveTimeout;
            clientSocket.SendTimeout = sendTimeout;
            try
            {
                clientSocket.Connect(hostname, port);
            }
            catch (SocketException e)
            {
                clientSocket.Close();
                throw new Exception("Server not found", e);
            }
            Stream stream;
            if (useSsl)
            {
                SslStream sslStream;
                if (certificateValidator == null)
                {
                    sslStream = new SslStream(clientSocket.GetStream(), false);
                }
                else
                {
                    sslStream = new SslStream(clientSocket.GetStream(), false, certificateValidator);
                }
                sslStream.ReadTimeout = receiveTimeout;
                sslStream.WriteTimeout = sendTimeout;
                sslStream.AuthenticateAsClient(hostname);
                stream = sslStream;
            }
            else
            {
                stream = clientSocket.GetStream();
            }
            this.Connect(stream);
        }
        /// <summary>
        /// Connect to the server using user supplied stream
        /// </summary>
        /// <param name="stream">The stream used to communicate with the server</param>
        /// <exception cref="T:System.ArgumentNullException">If <paramref name="stream" /> is <see langword="null" /></exception>
        public void Connect(Stream stream)
        {
            base.CheckDisposed();
            if (this.State != ConnectionState.Disconnected)
            {
                throw new Exception("You cannot ask to connect to a POP3 server, when we are already connected to one. Disconnect first.");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this.Stream = stream;
            string response = Utils.ReadLineAsAscii(this.Stream);
            try
            {
                this.State = ConnectionState.Authorization;
                IsOkResponse(response);
                this.ExtractApopTimestamp(response);
                this.Connected = true;
            }
            catch (Exception e)
            {
                this.DisconnectStreams();
                throw new Exception("Server is not available" + this.LastServerResponse, e);
            }
        }
        /// <summary>
        /// Examines string to see if it contains a time stamp to use with the APOP command
        /// </summary>
        private void ExtractApopTimestamp(string response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            Match match = Regex.Match(response, "<.+>");
            if (match.Success)
            {
                this.ApopTimeStamp = match.Value;
                this.ApopSupported = true;
            }
        }
        #endregion
        #region 授权
        public void Authenticate(string username, string password)
        {

            this.Authenticate(username, password, AuthenticationMethod.Auto);
        }
        public void Authenticate(string username, string password, AuthenticationMethod authenticationMethod)
        {
            base.CheckDisposed();
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            if (this.State != ConnectionState.Authorization)
            {
                throw new Exception("You have to be connected and not authorized when trying to authorize yourself");
            }
            try
            {
                switch (authenticationMethod)
                {
                    case AuthenticationMethod.UsernameAndPassword:
                        this.AuthenticateUsingUserAndPassword(username, password);
                        break;
                    case AuthenticationMethod.Apop:
                        this.AuthenticateUsingApop(username, password);
                        break;
                    case AuthenticationMethod.Auto:
                        if (this.ApopSupported)
                        {
                            this.AuthenticateUsingApop(username, password);
                        }
                        else
                        {
                            this.AuthenticateUsingUserAndPassword(username, password);
                        }
                        break;
                    case AuthenticationMethod.CramMd5:
                        this.AuthenticateUsingCramMd5(username, password);
                        break;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Problem logging in using method .Server response was: " + this.LastServerResponse, e);
            }
            this.State = ConnectionState.Transaction;
        }

        /// <summary>
        /// Authenticates using the CRAM-MD5 authentication method
        /// </summary>
        private void AuthenticateUsingCramMd5(string username, string password)
        {
            try
            {
                this.SendCommand("AUTH CRAM-MD5");
            }
            catch (Exception e)
            {
                throw new NotSupportedException("CRAM-MD5 authentication not supported", e);
            }
            string challenge = this.LastServerResponse.Substring(2);
            string response = Utils.ComputeDigest(username, password, challenge);
            this.SendCommand(response);
        }
        /// <summary>
        /// Authenticates a user towards the POP server using the USER and PASSWORD commands
        /// </summary>
        private void AuthenticateUsingUserAndPassword(string username, string password)
        {
            this.SendCommand("USER " + username);
            this.SendCommand("PASS " + password);
        }
        /// <summary>
        /// Authenticates a user towards the POP server using APOP
        /// </summary>
        private void AuthenticateUsingApop(string username, string password)
        {
            if (!this.ApopSupported)
            {
                throw new NotSupportedException("APOP is not supported on this server");
            }
            this.SendCommand("APOP " + username + " " + Utils.ComputeDigest(password, this.ApopTimeStamp));
        }

        #endregion
        #region Email消息操作
        /// <summary>
        /// Get the number of messages on the server using a STAT command
        /// </summary>
        public int GetMessageCount()
        {
            base.CheckDisposed();
            if (this.State != ConnectionState.Transaction)
            {
                throw new Exception("You cannot get the message count without authenticating yourself towards the server first");
            }
            return this.SendCommandIntResponse("STAT", 1);
        }
        public void DeleteMessage(int messageNumber)
        {
            base.CheckDisposed();
            ValidateMessageNumber(messageNumber);
            if (this.State != ConnectionState.Transaction)
            {
                throw new Exception("You cannot delete any messages without authenticating yourself towards the server first");
            }
            this.SendCommand("DELE " + messageNumber);
        }
        public void DeleteAllMessages()
        {
            base.CheckDisposed();
            int messageCount = this.GetMessageCount();
            for (int messageItem = messageCount; messageItem > 0; messageItem--)
            {
                this.DeleteMessage(messageItem);
            }
        }
        public string GetMessageUid(int messageNumber)
        {
            base.CheckDisposed();
            ValidateMessageNumber(messageNumber);
            if (this.State != ConnectionState.Transaction)
            {
                throw new Exception("Cannot get message ID, when the user has not been authenticated yet");
            }
            this.SendCommand("UIDL " + messageNumber);
            return this.LastServerResponse.Split(new char[]
			{
				' '
			})[2];
        }
        public List<string> GetMessageUids()
        {
            base.CheckDisposed();
            if (this.State != ConnectionState.Transaction)
            {
                throw new Exception("Cannot get message IDs, when the user has not been authenticated yet");
            }
            this.SendCommand("UIDL");
            List<string> uids = new List<string>();
            string response;
            while (!IsLastLineInMultiLineResponse(response = Utils.ReadLineAsAscii(this.Stream)))
            {
                uids.Add(response.Split(new char[]
				{
					' '
				})[1]);
            }
            return uids;
        }
        /// <summary>
        ///  Gets the size in bytes of a single message
        /// </summary>
        /// <param name="messageNumber"></param>
        /// <returns></returns>
        public int GetMessageSize(int messageNumber)
        {
            base.CheckDisposed();
            ValidateMessageNumber(messageNumber);
            if (this.State != ConnectionState.Transaction)
            {
                throw new Exception("Cannot get message size, when the user has not been authenticated yet");
            }
            return this.SendCommandIntResponse("LIST " + messageNumber, 2);
        }
        /// <summary>
        /// Get the sizes in bytes of all the messages.<br />
        /// Messages marked as deleted are not listed.
        /// </summary>
        /// <returns>Size of each message excluding deleted ones</returns>
        /// <exception cref="T:OpenPop.Pop3.Exceptions.PopServerException">If the server did not accept the LIST command</exception>
        public List<int> GetMessageSizes()
        {
            base.CheckDisposed();
            if (this.State != ConnectionState.Transaction)
            {
                throw new Exception("Cannot get message sizes, when the user has not been authenticated yet");
            }
            this.SendCommand("LIST");
            List<int> sizes = new List<int>();
            string response;
            while (!".".Equals(response = Utils.ReadLineAsAscii(this.Stream)))
            {
                sizes.Add(int.Parse(response.Split(new char[]
				{
					' '
				})[1], CultureInfo.InvariantCulture));
            }
            return sizes;
        }

        /// <summary>
        /// Fetches a message from the server and parses it
        /// </summary>
        public PopMessage GetMessage(int messageNumber)
        {
            base.CheckDisposed();
            ValidateMessageNumber(messageNumber);
            if (this.State != ConnectionState.Transaction)
            {
                throw new Exception("Cannot fetch a message, when the user has not been authenticated yet");
            }
            byte[] messageContent = this.GetMessageAsBytes(messageNumber);
            return new PopMessage(messageContent,true);
        }
        public MessageHeader GetMessageHeaders(int messageNumber)
        {
            base.CheckDisposed();
            ValidateMessageNumber(messageNumber);
            if (this.State != ConnectionState.Transaction)
            {
                throw new Exception("Cannot fetch a message, when the user has not been authenticated yet");
            }
            byte[] messageContent = this.GetMessageAsBytes(messageNumber, true);
            return new PopMessage(messageContent, false).Headers;
        }
        /// <summary>
        ///download all messages
        /// </summary>
        /// <returns></returns>
        public List<PopMessage> GetAllMessages() 
        {
            int messageCount = this.GetMessageCount();
            base.CheckDisposed();
            ValidateMessageNumber(messageCount);
            List<PopMessage> allMessages = new List<PopMessage>(messageCount);
            // Most servers give the latest message the highest number 大多数邮件服务器把最近的一条邮件排为最大
            for (int i = messageCount; i > 0; i--)
            {
                allMessages.Add(this.GetMessage(i));
            }
            return allMessages;     
        }
        /// <summary>
        /// Asks the server to return it's capability listing
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<string>> Capabilities()
        {
            base.CheckDisposed();
            if (this.State != ConnectionState.Authorization && this.State != ConnectionState.Transaction)
            {
                throw new Exception("Capability command only available while connected or authenticated");
            }
            this.SendCommand("CAPA");
            Dictionary<string, List<string>> capabilities = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            string lineRead;
            while (!IsLastLineInMultiLineResponse(lineRead = Utils.ReadLineAsAscii(this.Stream)))
            {
                string[] splitted = lineRead.Split(new char[]
				{
					' '
				});
                string capabilityName = splitted[0];
                List<string> capabilityArguments = new List<string>();
                for (int i = 1; i < splitted.Length; i++)
                {
                    capabilityArguments.Add(splitted[i]);
                }
                capabilities.Add(capabilityName, capabilityArguments);
            }
            return capabilities;
        }
        public byte[] GetMessageAsBytes(int messageNumber)
        {
            ValidateMessageNumber(messageNumber);
            if (this.State != ConnectionState.Transaction)
            {
                throw new Exception("Cannot fetch a message, when the user has not been authenticated yet");
            }
            return this.GetMessageAsBytes(messageNumber, false);
        }
        private byte[] GetMessageAsBytes(int messageNumber, bool askOnlyForHeaders)
        {
            base.CheckDisposed();
            ValidateMessageNumber(messageNumber);
            if (this.State != ConnectionState.Transaction)
            {
                throw new Exception("Cannot fetch a message, when the user has not been authenticated yet");
            }
            if (askOnlyForHeaders)
            {
                this.SendCommand("TOP " + messageNumber + " 0");
            }
            else
            {
                this.SendCommand("RETR " + messageNumber);
            }
            byte[] result;
            using (MemoryStream byteArrayBuilder = new MemoryStream())
            {
                bool first = true;
                byte[] lineRead;
                while (!IsLastLineInMultiLineResponse(lineRead = Utils.ReadLineAsBytes(this.Stream)))
                {
                    if (!first)
                    {
                        byte[] crlfPair = Encoding.ASCII.GetBytes("\r\n");
                        byteArrayBuilder.Write(crlfPair, 0, crlfPair.Length);
                    }
                    else
                    {
                        first = false;
                    }
                    if (lineRead.Length > 0 && lineRead[0] == 46)
                    {
                        byteArrayBuilder.Write(lineRead, 1, lineRead.Length - 1);
                    }
                    else
                    {
                        byteArrayBuilder.Write(lineRead, 0, lineRead.Length);
                    }
                }
                if (askOnlyForHeaders)
                {
                    byte[] crlfPair = Encoding.ASCII.GetBytes("\r\n");
                    byteArrayBuilder.Write(crlfPair, 0, crlfPair.Length);
                }
                byte[] receivedBytes = byteArrayBuilder.ToArray();
                result = receivedBytes;
            }
            return result;
        }

        private static void ValidateMessageNumber(int messageNumber) 
        {
            if (messageNumber <= 0)
            {
                throw new Exception("The messageNumber argument cannot have a value of zero or less. Valid messageNumber is in the range [1, messageCount]");
            }
        }
        private static bool IsLastLineInMultiLineResponse(string lineReceived)
        {
            if (lineReceived == null)
            {
                throw new ArgumentNullException("lineReceived");
            }
            return lineReceived.Length == 1 && IsLastLineInMultiLineResponse(Encoding.ASCII.GetBytes(lineReceived));
        }
        private static bool IsLastLineInMultiLineResponse(byte[] bytesReceived)
        {
            if (bytesReceived == null)
            {
                throw new ArgumentNullException("bytesReceived");
            }
            return bytesReceived.Length == 1 && bytesReceived[0] == 46;
        }
        /// <summary>
        /// Keep server active by sending a NOOP command.
        /// </summary>
        public void NoOperation()
        {
            base.CheckDisposed();
            if (this.State != ConnectionState.Transaction)
            {
                throw new Exception("You cannot use the NOOP command unless you are authenticated to the server");
            }
            this.SendCommand("NOOP");
        }
        /// <summary>
        /// Send a reset command to the server.
        /// </summary>
        public void Reset()
        {
            base.CheckDisposed();
            if (this.State != ConnectionState.Transaction)
            {
                throw new Exception("You cannot use the RSET command unless you are authenticated to the server");
            }
            this.SendCommand("RSET");
        }
        #endregion
        #region 发送命令
        /// <summary>
        /// Sends a command to the POP server.If this fails, an exception is thrown.
        /// </summary>
        private void SendCommand(string command)
        {
            byte[] commandBytes = Encoding.ASCII.GetBytes(command + "\r\n");
            this.Stream.Write(commandBytes, 0, commandBytes.Length);
            this.Stream.Flush();
            this.LastServerResponse = Utils.ReadLineAsAscii(this.Stream);
            EmailClient.IsOkResponse(this.LastServerResponse);
        }
        /// <summary>
        /// Sends a command to the POP server, expects an integer reply in the response
        /// </summary>
        private int SendCommandIntResponse(string command, int location)
        {
            this.SendCommand(command);
            return int.Parse(this.LastServerResponse.Split(new char[]
			{
				' '
			})[location], CultureInfo.InvariantCulture);
        }
        private static void IsOkResponse(string response)
        {
            if (response == null)
            {
                throw new Exception("The stream used to retrieve responses from was closed");
            }
            if (response.StartsWith("+", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            throw new Exception("The server did not respond with a + response. The response was: \"" + response + "\"");
        }
        #endregion
        #region 断开/释放资源
        /// <summary>
        /// Closes down the streams and sets the EmailClient into the initial configuration
        /// </summary>
        private void DisconnectStreams()
        {
            try
            {
                this.Stream.Close();
            }
            finally
            {
                this.ApopTimeStamp = null;
                this.Connected = false;
                this.State = ConnectionState.Disconnected;
                this.ApopSupported = false;
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && base._isDisposed != 1)
            {
                if (this.Connected)
                {
                    this.Disconnect();
                }
            }
            base.Dispose(disposing);
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            base.CheckDisposed();
            if (this.State == ConnectionState.Disconnected)
            {
                throw new Exception("You cannot disconnect a connection which is already disconnected");
            }
            try
            {
                this.SendCommand("QUIT");
            }
            finally
            {
                this.DisconnectStreams();
            }
        }
        #endregion
    }
}
