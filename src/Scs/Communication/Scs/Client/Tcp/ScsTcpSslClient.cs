using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.Channels.Tcp;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;

namespace Hik.Communication.Scs.Client.Tcp
{
    /// <summary>
    ///     This class is used to communicate with server over TCP/IP protocol.
    /// </summary>
    internal class ScsTcpSslClient : ScsClientBase
    {
        private readonly string _nombreServerCert;

        /// <summary>
        ///     The endpoint address of the server.
        /// </summary>
        private readonly ScsTcpEndPoint _serverEndPoint;

        /// <summary>
        /// </summary>
        /// <param name="serverEndPoint"></param>
        /// <param name="nombreServerCert"></param>
        public ScsTcpSslClient(ScsTcpEndPoint serverEndPoint, string nombreServerCert)
        {
            _serverEndPoint = serverEndPoint;
            _nombreServerCert = nombreServerCert;
        }

        /// <summary>
        ///     Creates a communication channel using ServerIpAddress and ServerPort.
        /// </summary>
        /// <returns>Ready communication channel to communicate</returns>
        protected override ICommunicationChannel CreateCommunicationChannel()
        {
            var client = new TcpClient();

            try
            {
                client.Client = TcpHelper.ConnectToServer(_serverEndPoint, ConnectTimeout);
                //client.Connect(new IPEndPoint(IPAddress.Parse(_serverEndPoint.IpAddress), _serverEndPoint.TcpPort));

                var sslStream = new SslStream(client.GetStream(), false, ValidateCertificate, null);
                sslStream.AuthenticateAsClient(_nombreServerCert);

                return new TcpSslCommunicationChannel(_serverEndPoint, client, sslStream);
            }
            catch (AuthenticationException)
            {
                client.Close();
                throw;
            }
        }

        public bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }
    }
}