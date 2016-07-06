#define UTILIZA_DESCONEXION_AUTOMATICA
using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using Hik.Communication.Scs.Communication.EndPoints;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Timer = Hik.Threading.Timer;

namespace Hik.Communication.Scs.Communication.Channels.Tcp
{
    internal class TcpSslCommunicationChannel : CommunicationChannelBase
    {
        #region Constructor

        /// <summary>
        ///     Creates a new TcpCommunicationChannel object.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="client"></param>
        /// <param name="stream"></param>
        public TcpSslCommunicationChannel(ScsTcpEndPoint endpoint, TcpClient client, SslStream stream)
        {
            _client = client;
            _client.NoDelay = true;

            _sslStream = stream;

            _remoteEndPoint = endpoint;

            _buffer = new byte[_receiveBufferSize];
            _syncLock = new object();
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Disconnects from remote application and closes channel.
        /// </summary>
        public override void Disconnect()
        {
#if UTILIZA_DESCONEXION_AUTOMATICA
            if (_timerTimeout != null)
            {
                _timerTimeout.Stop();
                _timerTimeout = null; //????
            }
#endif

            if (CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            _running = false;
            try
            {
                if (_client.Connected)
                {
                    _client.Close();
                }

                //_client.Dispose();
            }
            catch
            {
                // ignored
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
        private void ReceiveCallback(IAsyncResult ar)
        {
            if (!_running)
            {
                return;
            }

#if UTILIZA_DESCONEXION_AUTOMATICA
            //int valorAnterior = Interlocked.CompareExchange(ref timeoutFlag, 2, 1);
            if (Interlocked.CompareExchange(ref _timeoutFlag, 2, 1) /*valorAnterior*/!= 0)
            {
                //El flag ya ha sido seteado con lo cual nada!!
                return;
            }

            _timerTimeout?.Stop();
#endif

            try
            {
                //Get received bytes count
                var bytesRead = _sslStream.EndRead(ar);
                if (bytesRead > 0)
                {
                    LastReceivedMessageTime = DateTime.Now;

                    //Copy received bytes to a new byte array
                    var receivedBytes = new byte[bytesRead];
                    Array.Copy(_buffer, 0, receivedBytes, 0, bytesRead);

                    //Read messages according to current wire protocol
                    var messages = WireProtocol.CreateMessages(receivedBytes);

                    //Raise MessageReceived event for all received messages
                    foreach (var message in messages)
                    {
                        OnMessageReceived(message);
                    }
                }
                else
                {
                    throw new CommunicationException("Tcp socket is closed");
                }

                //Read more bytes if still running
                if (_running)
                {
                    _sslStream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, null);

#if UTILIZA_DESCONEXION_AUTOMATICA
                    _timerTimeout?.Start();
#endif
                }
            }
            catch
            {
                Disconnect();
            }
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
        private const int _receiveBufferSize = 4*1024; //4KB

        /// <summary>
        ///     This buffer is used to receive bytes
        /// </summary>
        private readonly byte[] _buffer;

        /// <summary>
        ///     Underlying socket
        /// </summary>
        private readonly TcpClient _client;

        /// <summary>
        ///     Secure stream to transmit / receive over
        /// </summary>
        private readonly SslStream _sslStream;

        /// <summary>
        ///     A flag to control thread's running
        /// </summary>
        private volatile bool _running;

        /// <summary>
        ///     This object is just used for thread synchronizing (locking).
        /// </summary>
        private readonly object _syncLock;

#if UTILIZA_DESCONEXION_AUTOMATICA
        private Timer _timerTimeout;
        private int _timeoutFlag;
#endif

        #endregion

        #region Protected methods

        /// <summary>
        ///     Starts the thread to receive messages from socket.
        /// </summary>
        protected override void StartInternal()
        {
            _running = true;
            _sslStream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, null);

#if UTILIZA_DESCONEXION_AUTOMATICA
            //  if (res.IsCompleted)
            {
                _timerTimeout = new Timer(120000);
                _timerTimeout.Elapsed += timerTimeout_Elapsed;
                _timerTimeout.Start();
            }
#endif
        }

#if UTILIZA_DESCONEXION_AUTOMATICA
        private void timerTimeout_Elapsed(object sender, EventArgs e)
        {
            _timerTimeout.Stop();

            //int valorAnterior = Interlocked.CompareExchange(ref timeoutFlag, 1, 0);
            if (Interlocked.CompareExchange(ref _timeoutFlag, 1, 0) /*valorAnterior*/!= 0)
            {
                //El flag ya ha sido seteado con lo cual nada!!
                return;
            }

            Disconnect();
        }
#endif

        /// <summary>
        ///     Sends a message to the remote application.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        protected override void SendMessageInternal(IScsMessage message)
        {
            //Send message
            var totalSent = 0;
            lock (_syncLock)
            {
                //Create a byte array from message according to current protocol
                var messageBytes = WireProtocol.GetBytes(message);
                //Send all bytes to the remote application

                try
                {
                    _sslStream.Write(messageBytes, totalSent, messageBytes.Length);
                }
                catch
                {
                    throw new CommunicationException("Message could not be sent via SSL stream");
                }

                LastSentMessageTime = DateTime.Now;
                OnMessageSent(message);
            }
        }

        #endregion
    }
}