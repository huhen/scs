using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using System;

namespace Hik.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    ///     This class is used to listen and accept incoming TCP
    ///     connection requests on a TCP port.
    /// </summary>
    internal class TcpSslConnectionListener : ConnectionListenerBase
    {
        /// <summary>
        ///     The endpoint address of the server to listen incoming connections.
        /// </summary>
        private readonly ScsTcpEndPoint _endPoint;

        private readonly X509Certificate _serverCert;

        /// <summary>
        ///     Server socket to listen incoming connection requests.
        /// </summary>
        private TcpListener _listenerSocket;

        /// <summary>
        ///     A flag to control thread's running
        /// </summary>
        private volatile bool _running;

        private volatile int disposed;

        ~TcpSslConnectionListener()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Interlocked.Increment(ref disposed) == 1)
            {
                _listenerSocket?.Stop();
                _listenerSocket = null;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="serverCert"></param>
        public TcpSslConnectionListener(ScsTcpEndPoint endPoint, X509Certificate serverCert)
        {
            _endPoint = endPoint;
            _serverCert = serverCert;
        }

        /// <summary>
        ///     Starts listening incoming connections.
        /// </summary>
        public override void Start()
        {
            if (_running) return;

            var localIP = new IPEndPoint(IPAddress.Any, _endPoint.TcpPort);
            _listenerSocket = new TcpListener(localIP);
            _listenerSocket.Start();
            _listenerSocket.BeginAcceptTcpClient(OnAcceptConnection, this);
            _running = true;
        }

        /// <summary>
        ///     Stops listening incoming connections.
        /// </summary>
        public override void Stop()
        {
            if (!_running) return;

            _running = false;
            _listenerSocket?.Stop();
        }

        private static void OnAcceptConnection(IAsyncResult result)
        {
            var _tcpSslConnectionListener = result.AsyncState as TcpSslConnectionListener;
            SslStream sslStream = null;

            try
            {
                if (_tcpSslConnectionListener._running)
                {
                    //start accepting the next connection…
                    _tcpSslConnectionListener._listenerSocket.BeginAcceptTcpClient(OnAcceptConnection, _tcpSslConnectionListener);
                }
                else
                {
                    //someone called Stop() – don’t call EndAcceptTcpClient because
                    //it will throw an ObjectDisposedException
                    return;
                }

                //complete the last operation…
                var client = _tcpSslConnectionListener._listenerSocket.EndAcceptTcpClient(result);
                var ipEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                sslStream = new SslStream(client.GetStream(), false);
                var asyncState = new object[] { _tcpSslConnectionListener, sslStream, new ScsTcpEndPoint(ipEndPoint.Address.ToString(), ipEndPoint.Port) };
                sslStream.BeginAuthenticateAsServer(_tcpSslConnectionListener._serverCert, false, SslProtocols.Tls, false, OnAuthenticateAsServer, asyncState);
            }
            catch
            {
                if (sslStream != null)
                {
                    sslStream.Dispose();
                    sslStream = null;
                }
            }
        }

        private static void OnAuthenticateAsServer(IAsyncResult result)
        {
            var asyncState = result.AsyncState as object[];
            if (asyncState == null && asyncState.Length != 3) return;
            var _tcpSslConnectionListener = asyncState[0] as TcpSslConnectionListener;
            var sslStream = asyncState[1] as SslStream;
            var scsTcpEndPoint = asyncState[2] as ScsTcpEndPoint;

            try
            {
                sslStream.EndAuthenticateAsServer(result);
                _tcpSslConnectionListener.OnCommunicationChannelConnected(new TcpSslCommunicationChannel(scsTcpEndPoint, sslStream));
            }
            catch
            {
                if (sslStream != null)
                {
                    sslStream.Dispose();
                    sslStream = null;
                }
            }
        }
    }
}