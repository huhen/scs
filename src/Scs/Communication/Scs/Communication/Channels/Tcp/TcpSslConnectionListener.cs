using System.Net;
using System.Net.Sockets;
using System.Threading;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using System;
using System.Security.Cryptography;

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

        //private readonly X509Certificate _serverCert;

        private readonly RSACryptoServiceProvider _rsa;

        /// <summary>
        ///     Server socket to listen incoming connection requests.
        /// </summary>
        private TcpListener _listenerSocket;

        /// <summary>
        ///     A flag to control thread's running
        /// </summary>
        private volatile bool _running;

        private int _disposed;

        ~TcpSslConnectionListener()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposed) == 1)
            {
                _rsa?.Dispose();
                _listenerSocket?.Stop();
                _listenerSocket = null;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="privateKey"></param>
        public TcpSslConnectionListener(ScsTcpEndPoint endPoint, byte[] privateKey)
        {
            _endPoint = endPoint;
            //_serverCert = serverCert;
            _rsa = new RSACryptoServiceProvider();
            _rsa.ImportCspBlob(privateKey);
        }

        /// <summary>
        ///     Starts listening incoming connections.
        /// </summary>
        public override void Start()
        {
            if (_running) return;

            var localIp = new IPEndPoint(IPAddress.Any, _endPoint.TcpPort);
            _listenerSocket = new TcpListener(localIp);
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
            var tcpSslConnectionListener = result.AsyncState as TcpSslConnectionListener;
            if (tcpSslConnectionListener == null) return;

            NetworkStream sslStream = null;

            try
            {
                if (tcpSslConnectionListener._running)
                {
                    //start accepting the next connection…
                    tcpSslConnectionListener._listenerSocket.BeginAcceptTcpClient(OnAcceptConnection, tcpSslConnectionListener);
                }
                else
                {
                    //someone called Stop() – don’t call EndAcceptTcpClient because
                    //it will throw an ObjectDisposedException
                    return;
                }

                //complete the last operation…
                var client = tcpSslConnectionListener._listenerSocket.EndAcceptTcpClient(result);
                var ipEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                if (ipEndPoint == null)
                {
                    client.Close();
                    return;
                }

                //sslStream = new SslStream(client.GetStream(), false);
                sslStream = client.GetStream();
                var remain = 0x200;
                var buff = new byte[remain];
                var asyncState = new object[] { tcpSslConnectionListener, sslStream, new ScsTcpEndPoint(ipEndPoint.Address.ToString(), ipEndPoint.Port), buff, remain };

                sslStream.BeginRead(buff, 0, remain, OnAuthenticateAsServer, asyncState);
                //var asyncState = new object[] { tcpSslConnectionListener, sslStream, new ScsTcpEndPoint(ipEndPoint.Address.ToString(), ipEndPoint.Port) };
                //sslStream.BeginAuthenticateAsServer(tcpSslConnectionListener._serverCert, false, SslProtocols.Tls, false, OnAuthenticateAsServer, asyncState);
            }
            catch
            {
                sslStream?.Dispose();
            }
        }

        private static void OnAuthenticateAsServer(IAsyncResult result)
        {
            var asyncState = result.AsyncState as object[];
            if (asyncState == null || asyncState.Length != 5) return;
            var tcpSslConnectionListener = asyncState[0] as TcpSslConnectionListener;
            var sslStream = asyncState[1] as NetworkStream;
            var scsTcpEndPoint = asyncState[2] as ScsTcpEndPoint;
            var buff = asyncState[3] as byte[];
            var remain = asyncState[4] as int? ?? 0;

            if (tcpSslConnectionListener == null || sslStream == null || scsTcpEndPoint == null || buff == null || remain == 0) return;

            try
            {
                var bytesRead = sslStream.EndRead(result);
                if (bytesRead > 0)
                {
                    remain -= bytesRead;
                    if (remain == 0)
                    {
                        var aes = Aes.Create();
                        //var aes = new RijndaelManaged();
                        if (aes == null)
                        {
                            sslStream.Dispose();
                            return;
                        }
                        var data = new byte[0x100];
                        Array.Copy(buff, data, 0x100);
                        aes.Key = tcpSslConnectionListener._rsa.Decrypt(data, false);
                        Array.Copy(buff, 0x100, data, 0, 0x100);
                        aes.IV = tcpSslConnectionListener._rsa.Decrypt(data, false);

                        tcpSslConnectionListener.OnCommunicationChannelConnected(new TcpSslCommunicationChannel(scsTcpEndPoint, sslStream, aes));
                        return;
                    }
                    var newAsyncState = new object[] { tcpSslConnectionListener, sslStream, scsTcpEndPoint, buff, remain };
                    sslStream.BeginRead(buff, 0x200 - remain, remain, OnAuthenticateAsServer, newAsyncState);
                    return;
                }
            }
            catch
            {
                //ignored
            }
            sslStream.Dispose();
        }

        /*private static void OnAuthenticateAsServer(IAsyncResult result)
        {
            var asyncState = result.AsyncState as object[];
            if (asyncState == null || asyncState.Length != 3) return;
            var tcpSslConnectionListener = asyncState[0] as TcpSslConnectionListener;
            var sslStream = asyncState[1] as SslStream;
            var scsTcpEndPoint = asyncState[2] as ScsTcpEndPoint;
            if (tcpSslConnectionListener == null || sslStream == null || scsTcpEndPoint == null) return;
            try
            {
                sslStream.EndAuthenticateAsServer(result);
                tcpSslConnectionListener.OnCommunicationChannelConnected(new TcpSslCommunicationChannel(scsTcpEndPoint, sslStream));
            }
            catch
            {
                sslStream.Dispose();
            }
        }*/
    }
}