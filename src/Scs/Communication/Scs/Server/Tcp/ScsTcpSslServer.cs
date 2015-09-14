using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.Channels.Tcp;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
namespace Hik.Communication.Scs.Server.Tcp
{
    /// <summary>
    /// This class is used to create a SSL TCP server.
    /// </summary>
    internal class ScsTcpSslServer : ScsServerBase
    {
        /// <summary>
        /// The endpoint address of the server to listen incoming connections.
        /// </summary>
        private readonly ScsTcpEndPoint _endPoint;

        private readonly X509Certificate2 _serverCert;
        private readonly List<X509Certificate2> _clientCerts;
             

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="serverCert"></param>
        /// <param name="clientCert"></param>
        public ScsTcpSslServer(ScsTcpEndPoint endPoint, X509Certificate2 serverCert, List<X509Certificate2> clientCerts)
        {
            _endPoint = endPoint;
            _serverCert = serverCert;
            _clientCerts = clientCerts;
        }


        /// <summary>
        /// Creates a TCP connection listener.
        /// </summary>
        /// <returns>Created listener object</returns>
        protected override IConnectionListener CreateConnectionListener()
        {
            return new TcpSslConnectionListener(_endPoint, _serverCert, _clientCerts);
        }
    }
}
