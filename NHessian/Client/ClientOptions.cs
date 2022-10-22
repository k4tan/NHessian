using NHessian.IO;

namespace NHessian.Client
{
    /// <summary>
    /// Contains additional options for clients.
    /// </summary>
    public class ClientOptions
    {
        /// <summary>
        /// Gets or sets custom type bindings to be used during serialization.
        /// </summary>
        public TypeBindings TypeBindings { get; set; } = null;

        /// <summary>
        /// Gets or sets the hessian version that should be used during serialization.
        /// </summary>
        public ProtocolVersion ProtocolVersion { get; set; } = ProtocolVersion.V2;

        /// <summary>
        /// Gets or sets a value indicating whether service exceptions should be thrown rather than
        /// wrapped into <see cref="HessianRemoteException"/>.
        /// </summary>
        public bool UnwrapServiceExceptions { get; set; } = true;
    }
}