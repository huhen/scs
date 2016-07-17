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

        private readonly byte[] _privateKey;


        /// <summary>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="privateKey"></param>
        public ScsTcpSslServer(ScsTcpEndPoint endPoint, byte[] privateKey)
        {
            _endPoint = endPoint;
            _privateKey = privateKey;
        }


        /// <summary>
        ///     Creates a TCP connection listener.
        /// </summary>
        /// <returns>Created listener object</returns>
        protected override IConnectionListener CreateConnectionListener()
        {
            return new TcpSslConnectionListener(_endPoint, _privateKey);
        }
    }
}