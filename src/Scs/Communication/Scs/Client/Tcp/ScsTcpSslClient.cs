using System;
using System.Net.Sockets;
using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.Channels.Tcp;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using System.Security.Cryptography;

namespace Hik.Communication.Scs.Client.Tcp
{
    /// <summary>
    ///     This class is used to communicate with server over TCP/IP protocol.
    /// </summary>
    internal class ScsTcpSslClient : ScsClientBase
    {
        //private readonly string _nombreServerCert;
        //private readonly byte[] _hash;
        private readonly byte[] _publicKey;

        /// <summary>
        ///     The endpoint address of the server.
        /// </summary>
        private readonly ScsTcpEndPoint _serverEndPoint;

        /// <summary>
        /// </summary>
        /// <param name="serverEndPoint"></param>
        /// <param name="publicKey"></param>
        public ScsTcpSslClient(ScsTcpEndPoint serverEndPoint, byte[] publicKey)
        {
            _serverEndPoint = serverEndPoint;
            //_nombreServerCert = nombreServerCert;
            //_hash = hash;
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
                client = new TcpClient { Client = TcpHelper.ConnectToServer(_serverEndPoint, ConnectTimeout) };
                var sslStream = client.GetStream();
                var aes = Aes.Create(); 
                //var aes = new RijndaelManaged();
                //SslStream sslStream = null;
                try
                {
                    if (aes == null) throw new Exception("Fail to init AES");
                    byte[] buff1;
                    byte[] buff2;
                    using (var rsa = new RSACryptoServiceProvider())
                    {
                        rsa.ImportCspBlob(_publicKey);
                        buff1 = rsa.Encrypt(aes.Key, false);
                        buff2 = rsa.Encrypt(aes.IV, false);
                    }
                    var buff = new byte[buff1.Length + buff2.Length];
                    Array.Copy(buff1, buff, buff1.Length);
                    Array.Copy(buff2, 0, buff, buff1.Length, buff2.Length);
                    sslStream.Write(buff, 0, buff.Length);
                    return new TcpSslCommunicationChannel(_serverEndPoint, sslStream, aes);
                    //sslStream = new SslStream(client.GetStream(), false, ValidateCertificate);
                    //sslStream.AuthenticateAsClient(_nombreServerCert);
                    //return new TcpSslCommunicationChannel(_serverEndPoint, sslStream);
                }
                catch
                {
                    aes?.Dispose();
                    sslStream.Dispose();
                    throw;
                }
                //client.Client = TcpHelper.ConnectToServer(_serverEndPoint, ConnectTimeout);
            }
            catch
            {
                client?.Close();
                throw;
            }
        }


        /*public bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (_hash != null)
            {
                if (certificate == null || sslPolicyErrors != SslPolicyErrors.RemoteCertificateChainErrors || _publicKey == null) return false;
                return _hash.SequenceEqual(certificate.GetCertHash()) && _publicKey.SequenceEqual(certificate.GetPublicKey());
            }
            return sslPolicyErrors == SslPolicyErrors.None;
        }*/
    }
}