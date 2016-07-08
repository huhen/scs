using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;

namespace Hik.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    ///     This class is used to listen and accept incoming TCP
    ///     connection requests on a TCP port.
    /// </summary>
    internal class TcpSslConnectionListener : ConnectionListenerBase
    {
        private readonly List<X509Certificate2> _clientCerts;

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

        /// <summary>
        ///     The thread to listen socket
        /// </summary>
        private Thread _thread;


        /// <summary>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="serverCert"></param>
        /// <param name="clientCerts"></param>
        public TcpSslConnectionListener(ScsTcpEndPoint endPoint, X509Certificate2 serverCert,
            List<X509Certificate2> clientCerts)
        {
            _endPoint = endPoint;
            _serverCert = serverCert;
            _clientCerts = clientCerts;
        }

        /// <summary>
        ///     Starts listening incoming connections.
        /// </summary>
        public override void Start()
        {
            StartSocket();
            _running = true;
            _thread = new Thread(DoListenAsThread);
            _thread.Start();
        }

        /// <summary>
        ///     Stops listening incoming connections.
        /// </summary>
        public override void Stop()
        {
            _running = false;
            StopSocket();
        }

        /// <summary>
        ///     Starts listening socket.
        /// </summary>
        private void StartSocket()
        {
            _listenerSocket = new TcpListener(IPAddress.Any, _endPoint.TcpPort);
            _listenerSocket.Start();
        }

        /// <summary>
        ///     Stops listening socket.
        /// </summary>
        private void StopSocket()
        {
            try
            {
                _listenerSocket.Stop();
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        ///     Entrance point of the thread.
        ///     This method is used by the thread to listen incoming requests.
        /// </summary>
        private void DoListenAsThread()
        {
            while (_running)
            {
                try
                {
                    var client = _listenerSocket.AcceptTcpClient();
                    if (client.Connected)
                    {
                        SslStream sslStream;
                        if (_clientCerts != null)
                        {
                            sslStream = new SslStream(client.GetStream(), false, ValidateCertificate);
                            sslStream.AuthenticateAsServer(_serverCert, true, SslProtocols.Default, true);
                        }
                        else
                        {
                            sslStream = new SslStream(client.GetStream(), false);
                            sslStream.AuthenticateAsServer(_serverCert, false, SslProtocols.Default, true);
                        }
                        OnCommunicationChannelConnected(new TcpSslCommunicationChannel(_endPoint, client, sslStream));
                    }
                }
                catch
                {
                    //Disconnect, wait for a while and connect again.
                    StopSocket();
                    Thread.Sleep(1000);
                    if (!_running)
                    {
                        return;
                    }

                    try
                    {
                        StartSocket();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        /// <summary>
        ///     Validacion de certificado
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        public bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (_clientCerts == null)
            {
                return false;
            }

            if ((sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors) && (_clientCerts != null))
            {
                return
                    _clientCerts.Select(clientCert => clientCert.GetCertHashString())
                        .Any(hashString => hashString != null && hashString.Equals(certificate.GetCertHashString()));
            }

            return sslPolicyErrors == SslPolicyErrors.None;
        }
    }
}