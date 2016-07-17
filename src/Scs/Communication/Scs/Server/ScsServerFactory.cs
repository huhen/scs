using Hik.Communication.Scs.Communication.EndPoints;

namespace Hik.Communication.Scs.Server
{
    /// <summary>
    ///     This class is used to create SCS servers.
    /// </summary>
    public static class ScsServerFactory
    {
        /// <summary>
        ///     SSL
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static IScsServer CreateSecureServer(ScsEndPoint endPoint, byte[] privateKey)
        {
            return endPoint.CreateSecureServer(privateKey);
        }
    }
}