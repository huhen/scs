
using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.Channels.Tcp;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Hik.Communication.Scs.Client.Tcp
{
    /// <summary>
    /// This class is used to communicate with server over TCP/IP protocol.
    /// </summary>
    internal class ScsTcpSslClient : ScsClientBase
    {
        /// <summary>
        /// The endpoint address of the server.
        /// </summary>
        private readonly ScsTcpEndPoint _serverEndPoint;

        private readonly X509Certificate2 _serverCert;
        private readonly X509Certificate2 _clientCert;
        private readonly string _nombreServerCert;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverEndPoint"></param>
        /// <param name="serverCert"></param>
        /// <param name="clientCert"></param>
        /// <param name="nombreServerCert"></param>
        public ScsTcpSslClient(ScsTcpEndPoint serverEndPoint, X509Certificate2 serverCert, X509Certificate2 clientCert, string nombreServerCert)
        {
            _serverEndPoint = serverEndPoint;
            _serverCert = serverCert;
            _clientCert = clientCert;
            _nombreServerCert = nombreServerCert;
        }

        /// <summary>
        /// Creates a communication channel using ServerIpAddress and ServerPort.
        /// </summary>
        /// <returns>Ready communication channel to communicate</returns>
        protected override ICommunicationChannel CreateCommunicationChannel()
        {
            var client = new TcpClient();

            try
            {
                client.Connect(new IPEndPoint(IPAddress.Parse(_serverEndPoint.IpAddress), _serverEndPoint.TcpPort));

                var sslStream = new SslStream(client.GetStream(), false, ValidateCertificate, SelectLocalCertificate);

                X509Certificate2Collection clientCertificates = new X509Certificate2Collection();
                if (_clientCert != null)
                {
                    clientCertificates.Add(_clientCert);
                }

                sslStream.AuthenticateAsClient(_nombreServerCert, clientCertificates, SslProtocols.Default, false);


                return new TcpSslCommunicationChannel(_serverEndPoint, client, sslStream);
            }
            catch (AuthenticationException)
            {
                client.Close();
                throw;
            }
        }

        public bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
            {
                return _serverCert.GetCertHashString().Equals(certificate.GetCertHashString());
            }
            else
            {
                return sslPolicyErrors == SslPolicyErrors.None;
            }
        }

        public X509Certificate SelectLocalCertificate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return _clientCert;
        }
    }
}
