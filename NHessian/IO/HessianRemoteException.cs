using System;
using System.Collections.Generic;

namespace NHessian.IO
{
    /// <summary>
    /// <see cref="HessianInput.ReadReply(Type)"/> can return
    /// this <see cref="HessianRemoteException"/> if the server responded with
    /// a fault.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///  <item><a href="http://hessian.caucho.com/doc/hessian-1.0-spec.xtp#Faults">Hessian 1.0.2 spec</a></item>
    ///  <item><a href="http://hessian.caucho.com/doc/hessian-ws.html#anchor16">Hessian 2.0 spec</a></item>
    /// </list>
    /// </remarks>
    public class HessianRemoteException : Exception
    {
        internal HessianRemoteException(
            string originalMessage,
            FaultCode code,
            Exception innerException,
            IReadOnlyDictionary<object, object> rawMap = null)
            : base($"The hessian server reponded with '{originalMessage}' (code: '{code}')", innerException)
        {
            OriginalMessage = originalMessage;
            Code = code;
            RawMap = rawMap;
        }

        /// <summary>
        /// Gets the fault code as returned by the server.
        /// </summary>
        public FaultCode Code { get; }

        /// <summary>
        /// Gets the original fault message returned by the server.
        /// </summary>
        public string OriginalMessage { get; }

        /// <summary>
        /// Gets the raw deserialized fault reply as returned by the server.
        /// </summary>
        public IReadOnlyDictionary<object, object> RawMap { get; }

        internal static HessianRemoteException FromRawMap(IDictionary<object, object> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (!map.ContainsKey("code"))
                throw new ArgumentException("A Hessian fault must define a code.", nameof(map));

            // code, message and detail are specified by the spec
            map.TryGetValue("message", out var message);
            map.TryGetValue("detail", out var detail);

            var code = (FaultCode)Enum.Parse(typeof(FaultCode), (string)map["code"]);

            return new HessianRemoteException(
                message as string,
                code,
                detail as Exception,
                new Dictionary<object, object>(map));
        }
    }
}