using System.Security.Cryptography.X509Certificates;
using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.Channels.Tcp;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;

namespace Hik.Communication.Scs.Server.Tcp
{
    /// <summary>
    ///     This class is used to create a SSL TCP server.
    /// </summary>
    internal class ScsTcpSslServer : ScsServerBase
    {
        /// <summary>
        ///     The endpoint address of the server to listen incoming connections.
        /// </summary>
        private readonly ScsTcpEndPoint _endPoint;

        private readonly X509Certificate _serverCert;


        /// <summary>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="serverCert"></param>
        public ScsTcpSslServer(ScsTcpEndPoint endPoint, X509Certificate serverCert)
        {
            _endPoint = endPoint;
            _serverCert = serverCert;
        }


        /// <summary>
        ///     Creates a TCP connection listener.
        /// </summary>
        /// <returns>Created listener object</returns>
        protected override IConnectionListener CreateConnectionListener()
        {
            return new TcpSslConnectionListener(_endPoint, _serverCert);
        }
    }
}