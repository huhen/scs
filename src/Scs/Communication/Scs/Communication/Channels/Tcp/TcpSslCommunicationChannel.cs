using System;
using System.Net.Security;
using Hik.Communication.Scs.Communication.EndPoints;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using System.IO;
using System.Threading;

namespace Hik.Communication.Scs.Communication.Channels.Tcp
{
    internal class TcpSslCommunicationChannel : CommunicationChannelBase
    {
        #region Constructor

        /// <summary>
        ///     Creates a new TcpCommunicationChannel object.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="stream"></param>
        public TcpSslCommunicationChannel(ScsTcpEndPoint endPoint, SslStream stream)
        {
            _sslStream = stream;

            _remoteEndPoint = endPoint;

            _buffer = new byte[_receiveBufferSize];
        }

        private volatile int disposed;

        ~TcpSslCommunicationChannel()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Interlocked.Increment(ref disposed) == 1)
            {
                _sslStream?.Dispose();
                GC.SuppressFinalize(this);
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

            if (_sslStream != null)
            {
                _sslStream.Dispose();
                _sslStream = null;
            }

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
            var _tcpSslCommunicationChannel = ar.AsyncState as TcpSslCommunicationChannel;
            try
            {
                //Get received bytes count
                var bytesRead = _tcpSslCommunicationChannel._sslStream.EndRead(ar);
                if (bytesRead > 0)
                {
                    _tcpSslCommunicationChannel.LastReceivedMessageTime = DateTime.Now;

                    //Read messages according to current wire protocol
                    var messages = _tcpSslCommunicationChannel.WireProtocol.CreateMessages(_tcpSslCommunicationChannel._buffer, bytesRead);

                    //Raise MessageReceived event for all received messages
                    foreach (var message in messages)
                    {
                        _tcpSslCommunicationChannel.OnMessageReceived(message);
                    }
                    _tcpSslCommunicationChannel._sslStream.BeginRead(_tcpSslCommunicationChannel._buffer, 0, _tcpSslCommunicationChannel._buffer.Length, ReceiveCallback, _tcpSslCommunicationChannel);
                    return;
                }
            }
            catch
            {
                //ignored
            }
            _tcpSslCommunicationChannel.Disconnect();
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
        private SslStream _sslStream;

        /// <summary>
        ///     lockSend
        /// </summary>
        private object _lockSend = new object();

        #endregion

        #region Protected methods

        /// <summary>
        ///     Starts the thread to receive messages from socket.
        /// </summary>
        protected override void StartInternal()
        {
            _sslStream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, this);
        }

        /// <summary>
        ///     Sends a message to the remote application.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        protected override void SendMessageInternal(IScsMessage message)
        {
            //Create a byte array from message according to current protocol
            var messageBytes = WireProtocol.GetBytes(message);

            Monitor.Enter(_lockSend);
            try
            {
                //Send all bytes to the remote application
                _sslStream.Write(messageBytes);
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