using System.Security.Cryptography.X509Certificates;
using Hik.Communication.Scs.Communication.EndPoints;
using Hik.Communication.Scs.Server;

namespace Hik.Communication.ScsServices.Service
{
    /// <summary>
    ///     This class is used to build ScsService applications.
    /// </summary>
    public static class ScsServiceBuilder
    {
        /// <summary>
        ///     Creates a new SCS Service application using an EndPoint.
        /// </summary>
        /// <param name="endPoint">EndPoint that represents address of the service</param>
        /// <returns>Created SCS service application</returns>
        public static IScsServiceApplication CreateService(ScsEndPoint endPoint)
        {
            return new ScsServiceApplication(ScsServerFactory.CreateServer(endPoint));
        }

        /// <summary>
        ///     Creates a new secure SCS Service application using an EndPoint.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="serverCert"></param>
        /// <returns></returns>
        public static IScsServiceApplication CreateSecureService(ScsEndPoint endPoint, X509Certificate2 serverCert)
        {
            return new ScsServiceApplication(ScsServerFactory.CreateSecureServer(endPoint, serverCert));
        }
    }
}