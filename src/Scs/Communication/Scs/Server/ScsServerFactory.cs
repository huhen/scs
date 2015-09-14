using Hik.Communication.Scs.Communication.EndPoints;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
namespace Hik.Communication.Scs.Server
{
    /// <summary>
    /// This class is used to create SCS servers.
    /// </summary>
    public static class ScsServerFactory
    {
        /// <summary>
        /// Creates a new SCS Server using an EndPoint.
        /// </summary>
        /// <param name="endPoint">Endpoint that represents address of the server</param>
        /// <returns>Created TCP server</returns>
        public static IScsServer CreateServer(ScsEndPoint endPoint)
        {
            return endPoint.CreateServer();
        }
        
       /// <summary>
       /// SSL
       /// </summary>
       /// <param name="endPoint"></param>
       /// <param name="cert"></param>
       /// <returns></returns>
        public static IScsServer CreateSecureServer(ScsEndPoint endPoint, X509Certificate2 serverCert, List<X509Certificate2> clientCerts)
        {
            return endPoint.CreateSecureServer(serverCert, clientCerts);
        }
    }
}
