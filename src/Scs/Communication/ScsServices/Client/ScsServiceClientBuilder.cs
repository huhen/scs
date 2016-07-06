using System.Security.Cryptography.X509Certificates;
using Hik.Communication.Scs.Communication.EndPoints;

namespace Hik.Communication.ScsServices.Client
{
    /// <summary>
    ///     This class is used to build service clients to remotely invoke methods of a SCS service.
    /// </summary>
    public class ScsServiceClientBuilder
    {
        /// <summary>
        ///     Creates a client to connect to a SCS service.
        /// </summary>
        /// <typeparam name="T">Type of service interface for remote method call</typeparam>
        /// <param name="endpoint">EndPoint of the server</param>
        /// <param name="clientObject">
        ///     Client-side object that handles remote method calls from server to client.
        ///     May be null if client has no methods to be invoked by server
        /// </param>
        /// <returns>Created client object to connect to the server</returns>
        public static IScsServiceClient<T> CreateClient<T>(ScsEndPoint endpoint, object clientObject = null)
            where T : class
        {
            return new ScsServiceClient<T>(endpoint.CreateClient(), clientObject);
        }

        /// <summary>
        ///     Creates a client to connect to a SCS service.
        /// </summary>
        /// <typeparam name="T">Type of service interface for remote method call</typeparam>
        /// <param name="endpointAddress">EndPoint address of the server</param>
        /// <param name="clientObject">
        ///     Client-side object that handles remote method calls from server to client.
        ///     May be null if client has no methods to be invoked by server
        /// </param>
        /// <returns>Created client object to connect to the server</returns>
        public static IScsServiceClient<T> CreateClient<T>(string endpointAddress, object clientObject = null)
            where T : class
        {
            return CreateClient<T>(ScsEndPoint.CreateEndPoint(endpointAddress), clientObject);
        }

        /// <summary>
        ///     SSL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverCert"></param>
        /// <param name="clientCert"></param>
        /// <param name="nombreServerCert"></param>
        /// <param name="endpoint"></param>
        /// <param name="clientObject"></param>
        /// <returns></returns>
        public static IScsServiceClient<T> CreateSecureClient<T>(X509Certificate2 serverCert,
            X509Certificate2 clientCert, string nombreServerCert, ScsEndPoint endpoint, object clientObject = null)
            where T : class
        {
            return new ScsServiceClient<T>(endpoint.CreateSecureClient(serverCert, clientCert, nombreServerCert),
                clientObject);
        }

        /// <summary>
        ///     SSL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverCert"></param>
        /// <param name="clientCert"></param>
        /// <param name="nombreServerCert"></param>
        /// <param name="endpointAddress"></param>
        /// <param name="clientObject"></param>
        /// <returns></returns>
        public static IScsServiceClient<T> CreateSecureClient<T>(X509Certificate2 serverCert,
            X509Certificate2 clientCert, string nombreServerCert, string endpointAddress, object clientObject = null)
            where T : class
        {
            return CreateSecureClient<T>(serverCert, clientCert, nombreServerCert,
                ScsEndPoint.CreateEndPoint(endpointAddress), clientObject);
        }
    }
}