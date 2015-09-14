using System.Net.Sockets;
using System.Threading;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using System.Collections.Generic;
namespace Hik.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// This class is used to listen and accept incoming TCP
    /// connection requests on a TCP port.
    /// </summary>
    internal class TcpSslConnectionListener : ConnectionListenerBase
    {
        /// <summary>
        /// The endpoint address of the server to listen incoming connections.
        /// </summary>
        private readonly ScsTcpEndPoint _endPoint;

        /// <summary>
        /// Server socket to listen incoming connection requests.
        /// </summary>
        private TcpListener _listenerSocket;

        /// <summary>
        /// The thread to listen socket
        /// </summary>
        private Thread _thread;

        /// <summary>
        /// A flag to control thread's running
        /// </summary>
        private volatile bool _running;

        private readonly X509Certificate _serverCert;
        private readonly List<X509Certificate2> _clientCerts;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="serverCert"></param>
        /// <param name="clientCert"></param>
        public TcpSslConnectionListener(ScsTcpEndPoint endPoint, X509Certificate2 serverCert, List<X509Certificate2> clientCerts)
        {
            _endPoint = endPoint;
            _serverCert = serverCert;
            _clientCerts = clientCerts;
        }

        /// <summary>
        /// Starts listening incoming connections.
        /// </summary>
        public override void Start()
        {
            StartSocket();
            _running = true;
            _thread = new Thread(DoListenAsThread);
            _thread.Start();
        }

        /// <summary>
        /// Stops listening incoming connections.
        /// </summary>
        public override void Stop()
        {
            _running = false;
            StopSocket();
        }

        /// <summary>
        /// Starts listening socket.
        /// </summary>
        private void StartSocket()
        {
            _listenerSocket = new TcpListener(System.Net.IPAddress.Any, _endPoint.TcpPort);
            _listenerSocket.Start();
        }

        /// <summary>
        /// Stops listening socket.
        /// </summary>
        private void StopSocket()
        {
            try
            {
                _listenerSocket.Stop();
            }
            catch
            {

            }
        }

        /// <summary>
        /// Entrance point of the thread.
        /// This method is used by the thread to listen incoming requests.
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
                        SslStream sslStream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateCertificate)); 
                        sslStream.AuthenticateAsServer(_serverCert, true, System.Security.Authentication.SslProtocols.Default, true);

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

                    }
                }
            }
        }

        /// <summary>
        /// Validacion de certificado
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        public bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {

            if (_clientCerts == null)
            {
                return false;
            }

            if ((sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors) && (_clientCerts != null))
            {
                foreach (var _clientCert in _clientCerts)
                    if (_clientCert.GetCertHashString().Equals(certificate.GetCertHashString()))
                        return true;
                return false;
            }
            else
            {
                return (sslPolicyErrors == SslPolicyErrors.None);
            }
        }
    }
}