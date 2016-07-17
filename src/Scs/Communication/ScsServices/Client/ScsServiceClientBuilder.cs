using Hik.Communication.Scs.Communication.EndPoints;

namespace Hik.Communication.ScsServices.Client
{
    /// <summary>
    ///     This class is used to build service clients to remotely invoke methods of a SCS service.
    /// </summary>
    public class ScsServiceClientBuilder
    {
        /// <summary>
        ///     SSL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publicKey"></param>
        /// <param name="endpoint"></param>
        /// <param name="clientObject"></param>
        /// <returns></returns>
        public static IScsServiceClient<T> CreateSecureClient<T>(byte[] publicKey, ScsEndPoint endpoint, object clientObject = null)
            where T : class
        {
            return new ScsServiceClient<T>(endpoint.CreateSecureClient(publicKey), clientObject);
        }

        /// <summary>
        ///     SSL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publicKey"></param>
        /// <param name="endpointAddress"></param>
        /// <param name="clientObject"></param>
        /// <returns></returns>
        public static IScsServiceClient<T> CreateSecureClient<T>(byte[] publicKey, string endpointAddress, object clientObject = null)
            where T : class
        {
            return CreateSecureClient<T>(publicKey, ScsEndPoint.CreateEndPoint(endpointAddress), clientObject);
        }
    }
}