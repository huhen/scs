using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.Channels.Tcp;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using System;
using System.Linq;
using System.Net;

namespace Hik.Communication.Scs.Client.Tcp
{
    /// <summary>
    ///     This class is used to communicate with server over TCP/IP protocol.
    /// </summary>
    internal class ScsTcpSslClient : ScsClientBase
    {
        private readonly string _nombreServerCert;
        private readonly byte[] _hash;
        private readonly byte[] _publicKey;

        /// <summary>
        ///     The endpoint address of the server.
        /// </summary>
        private readonly ScsTcpEndPoint _serverEndPoint;

        /// <summary>
        /// </summary>
        /// <param name="serverEndPoint"></param>
        /// <param name="nombreServerCert"></param>
        /// <param name="hash"></param>
        /// <param name="publicKey"></param>
        public ScsTcpSslClient(ScsTcpEndPoint serverEndPoint, string nombreServerCert, byte[] hash, byte[] publicKey)
        {
            _serverEndPoint = serverEndPoint;
            _nombreServerCert = nombreServerCert;
            _hash = hash;
            _publicKey = publicKey;
        }

        ~ScsTcpSslClient()
        {
            Dispose();
        }

        /// <summary>
        ///     Creates a communication channel using ServerIpAddress and ServerPort.
        /// </summary>
        /// <returns>Ready communication channel to communicate</returns>
        protected override ICommunicationChannel CreateCommunicationChannel()
        {
            TcpClient client = null;

            try
            {
                client = new TcpClient();
                client.Client = TcpHelper.ConnectToServer(_serverEndPoint, ConnectTimeout);
                //client.Connect(_serverEndPoint.IpAddress, _serverEndPoint.TcpPort);
                SslStream sslStream = null;
                try
                {
                    sslStream = new SslStream(client.GetStream(), false, ValidateCertificate);
                    sslStream.AuthenticateAsClient(_nombreServerCert);
                    return new TcpSslCommunicationChannel(_serverEndPoint, sslStream);
                }
                catch
                {
                    if (sslStream != null)
                    {
                        sslStream.Dispose();
                        sslStream = null;
                    }
                    throw;
                }
                //client.Client = TcpHelper.ConnectToServer(_serverEndPoint, ConnectTimeout);
            }
            catch
            {
                if (client != null)
                {
                    client.Close();
                    client = null;
                }
                throw;
            }
        }


        public bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (_hash != null)
            {
                if (certificate == null || sslPolicyErrors != SslPolicyErrors.RemoteCertificateChainErrors || _publicKey == null) return false;
                return _hash.SequenceEqual(certificate.GetCertHash()) && _publicKey.SequenceEqual(certificate.GetPublicKey());
            }
            return sslPolicyErrors == SslPolicyErrors.None;
        }
    }
}