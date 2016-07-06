using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.ScsServices.Communication.Messages;

namespace Hik.Communication.ScsServices.Communication
{
    /// <summary>
    ///     This class is used to generate a dynamic proxy to invoke remote methods.
    ///     It translates method invocations to messaging.
    /// </summary>
    /// <typeparam name="TProxy">Type of the proxy class/interface</typeparam>
    /// <typeparam name="TMessenger">Type of the messenger object that is used to send/receive messages</typeparam>
    internal class RemoteInvokeProxy<TProxy, TMessenger> : RealProxy where TMessenger : IMessenger
    {
        /// <summary>
        ///     Messenger object that is used to send/receive messages.
        /// </summary>
        private readonly RequestReplyMessenger<TMessenger> _clientMessenger;

        /// <summary>
        ///     Creates a new RemoteInvokeProxy object.
        /// </summary>
        /// <param name="clientMessenger">Messenger object that is used to send/receive messages</param>
        public RemoteInvokeProxy(RequestReplyMessenger<TMessenger> clientMessenger)
            : base(typeof (TProxy))
        {
            _clientMessenger = clientMessenger;
        }

        /// <summary>
        ///     Overrides message calls and translates them to messages to remote application.
        /// </summary>
        /// <param name="msg">Method invoke message (from RealProxy base class)</param>
        /// <returns>Method invoke return message (to RealProxy base class)</returns>
        public override IMessage Invoke(IMessage msg)
        {
            var message = msg as IMethodCallMessage;
            if (message == null)
            {
                return null;
            }

            var requestMessage = new ScsRemoteInvokeMessage
            {
                ServiceClassName = typeof (TProxy).Name,
                MethodName = message.MethodName,
                //Parameters = message.InArgs
                Parameters = message.Args
            };

            var responseMessage =
                _clientMessenger.SendMessageAndWaitForResponse(requestMessage) as ScsRemoteInvokeReturnMessage;
            if (responseMessage == null)
            {
                return null;
            }

            /*
             * http://stackoverflow.com/questions/3081341/ref-argument-through-a-realproxy
             * The second parameter of ReturnMessage needs to contain the values of the ref and out parameters to pass back. You can get them by saving a reference to the array you pass in:

public override System.Runtime.Remoting.Messaging.IMessage Invoke(System.Runtime.Remoting.Messaging.IMessage msg)
{
    IMethodCallMessage call = msg as IMethodCallMessage;
    var args = call.Args;
    object returnValue = typeof(HelloClass).InvokeMember(call.MethodName, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, _hello, args);
    return new ReturnMessage(returnValue, args, args.Length, call.LogicalCallContext, call);
}
             */
            object[] args = null;
            var largo = 0;

            if (responseMessage.Parameters != null)
            {
                args = responseMessage.Parameters;
                largo = args.Length;
            }
            return responseMessage.RemoteException != null
                ? new ReturnMessage(responseMessage.RemoteException, message)
                : new ReturnMessage(responseMessage.ReturnValue, args, largo, message.LogicalCallContext, message);
        }
    }
}