using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using Hik.Communication.Scs.Communication.EndPoints;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using System.Threading;

namespace Hik.Communication.Scs.Communication.Channels.Tcp
{
    internal class TcpSslCommunicationChannel : CommunicationChannelBase
    {
        private readonly Aes _aes;
        #region Constructor

        /// <summary>
        ///     Creates a new TcpCommunicationChannel object.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="stream"></param>
        /// <param name="aes"></param>
        public TcpSslCommunicationChannel(ScsTcpEndPoint endPoint, NetworkStream stream, Aes aes)
        {
            _sslStream = stream;

            _remoteEndPoint = endPoint;

            _buffer = new byte[_receiveBufferSize];

            _aes = aes;
        }

        private int _disposed;

        ~TcpSslCommunicationChannel()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposed) == 1)
            {
                DisposeVars();
                GC.SuppressFinalize(this);
            }
        }

        private void DisposeVars()
        {
            try
            {
                _sslStream?.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Disconnects from remote application and closes channel.
        /// </summary>
        public override void Disconnect()
        {
            if (CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            DisposeVars();

            CommunicationState = CommunicationStates.Disconnected;
            OnDisconnected();
        }

        #endregion

        #region Private methods

        /// <summary>
        ///     This method is used as callback method in _clientSocket's BeginReceive method.
        ///     It reveives bytes from socker.
        /// </summary>
        /// <param name="ar">Asyncronous call result</param>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            var tcpSslCommunicationChannel = ar.AsyncState as TcpSslCommunicationChannel;
            if (tcpSslCommunicationChannel == null) return;
            try
            {
                //Get received bytes count
                var bytesRead = tcpSslCommunicationChannel._sslStream.EndRead(ar);
                if (bytesRead > 0)
                {
                    tcpSslCommunicationChannel.LastReceivedMessageTime = DateTime.Now;

                    //Read messages according to current wire protocol
                    var messages = tcpSslCommunicationChannel.WireProtocol.CreateMessages(tcpSslCommunicationChannel._buffer, bytesRead);

                    //Raise MessageReceived event for all received messages
                    foreach (var message in messages)
                    {
                        tcpSslCommunicationChannel.OnMessageReceived(message);
                    }
                    tcpSslCommunicationChannel._sslStream.BeginRead(tcpSslCommunicationChannel._buffer, 0, tcpSslCommunicationChannel._buffer.Length, ReceiveCallback, tcpSslCommunicationChannel);
                    return;
                }
            }
            catch
            {
                //ignored
            }
            tcpSslCommunicationChannel.Disconnect();
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     Gets the endpoint of remote application.
        /// </summary>
        public override ScsEndPoint RemoteEndPoint => _remoteEndPoint;

        private readonly ScsTcpEndPoint _remoteEndPoint;

        #endregion

        #region Private fields

        /// <summary>
        ///     Size of the buffer that is used to receive bytes from TCP socket.
        /// </summary>
        private const int _receiveBufferSize = 4 * 1024; //4KB

        /// <summary>
        ///     This buffer is used to receive bytes
        /// </summary>
        private readonly byte[] _buffer;

        /// <summary>
        ///     Secure stream to transmit / receive over
        /// </summary>
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private NetworkStream _sslStream;

        /*/// <summary>
        /// AES
        /// </summary>
        private Aes _aes;

        /// <summary>
        /// Encryptor
        /// </summary>
        private ICryptoTransform _encryptor;

        /// <summary>
        /// Decryptor
        /// </summary>
        private ICryptoTransform _decryptor;*/


        /// <summary>
        ///     lockSend
        /// </summary>
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private object _lockSend = new object();

        #endregion

        #region Protected methods

        /// <summary>
        ///     Starts the thread to receive messages from socket.
        /// </summary>
        protected override void StartInternal()
        {
            WireProtocol.SetAes(_aes);
            _sslStream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, this);
        }

        /// <summary>
        ///     Sends a message to the remote application.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        protected override void SendMessageInternal(IScsMessage message)
        {
            Monitor.Enter(_lockSend);
            try
            {
                //Create a byte array from message according to current protocol
                var messageBytes = WireProtocol.GetBytes(message);

                //Send all bytes to the remote application
                _sslStream.Write(messageBytes, 0, messageBytes.Length);
            }
            catch
            {
                Disconnect();
                return;
            }
            finally
            {
                Monitor.Exit(_lockSend);
            }

            LastSentMessageTime = DateTime.Now;
            OnMessageSent(message);
        }

        #endregion
    }
}