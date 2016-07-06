using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;

namespace Hik.Communication.Scs.Client.Tcp
{
    /// <summary>
    ///     This class is used to simplify TCP socket operations.
    /// </summary>
    internal static class TcpHelper
    {
        public static Socket ConnectToServer(ScsTcpEndPoint endPoint, int timeoutMs)
        {
            var pc = ProxyConfig.ProxyConfig.GetConfig();
            if (pc.ProxyEnable && !string.IsNullOrEmpty(pc.ProxyType) && pc.ProxyType.Equals("HTTP"))
            {
                return ConnectViaHttpProxy(endPoint.IpAddress, endPoint.TcpPort, pc.ProxyAddress, pc.ProxyPort,
                    pc.ProxyUserName, pc.ProxyPassword);
            }

            return ConnectToServerNoProxy(new IPEndPoint(IPAddress.Parse(endPoint.IpAddress), endPoint.TcpPort),
                timeoutMs);
        }

        /// <summary>
        ///     This code is used to connect to a TCP socket with timeout option.
        /// </summary>
        /// <param name="endPoint">IP endpoint of remote server</param>
        /// <param name="timeoutMs">Timeout to wait until connect</param>
        /// <returns>Socket object connected to server</returns>
        /// <exception cref="SocketException">Throws SocketException if can not connect.</exception>
        /// <exception cref="TimeoutException">Throws TimeoutException if can not connect within specified timeoutMs</exception>
        public static Socket ConnectToServerNoProxy(EndPoint endPoint, int timeoutMs)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Blocking = false;
                socket.Connect(endPoint);
                socket.Blocking = true;
                return socket;
            }
            catch (SocketException socketException)
            {
                if (socketException.ErrorCode != 10035)
                {
                    socket.Close();
                    throw;
                }

                if (!socket.Poll(timeoutMs*1000, SelectMode.SelectWrite))
                {
                    socket.Close();
                    throw new TimeoutException("The host failed to connect. Timeout occured.");
                }

                socket.Blocking = true;
                return socket;
            }
        }

        public static Socket ConnectViaHttpProxy(string targetHost, int targetPort, string httpProxyHost,
            int httpProxyPort, string proxyUserName, string proxyPassword)
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var uriBuilder = new UriBuilder
            {
                Scheme = Uri.UriSchemeHttp,
                Host = httpProxyHost,
                Port = httpProxyPort
            };

            var request = WebRequest.Create("http://" + targetHost + ":" + targetPort);
            var webProxy = new WebProxy(uriBuilder.Uri);

            request.Proxy = webProxy;
            request.Method = "CONNECT";

            webProxy.Credentials = new NetworkCredential(proxyUserName, proxyPassword);

            var response = request.GetResponse();

            var responseStream = response.GetResponseStream();
            if (responseStream == null)
                throw new ArgumentNullException(nameof(responseStream));
            var rsType = responseStream.GetType();
            var connectionProperty = rsType.GetProperty("Connection", flags);

            var connection = connectionProperty.GetValue(responseStream, null);
            var connectionType = connection.GetType();
            var networkStreamProperty = connectionType.GetProperty("NetworkStream", flags);

            var networkStream = networkStreamProperty.GetValue(connection, null);
            var nsType = networkStream.GetType();
            var socketProperty = nsType.GetProperty("Socket", flags);
            return (Socket) socketProperty.GetValue(networkStream, null);
        }
    }
}