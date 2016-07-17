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
        ///     Creates a new secure SCS Service application using an EndPoint.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static IScsServiceApplication CreateSecureService(ScsEndPoint endPoint, byte[] privateKey)
        {
            return new ScsServiceApplication(ScsServerFactory.CreateSecureServer(endPoint, privateKey));
        }
    }
}