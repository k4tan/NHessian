namespace NHessian.Client
{
    /// <summary>
    /// Hessian protocol version
    /// </summary>
    public enum ProtocolVersion
    {
        /// <summary>
        /// http://hessian.caucho.com/doc/hessian-1.0-spec.xtp
        /// </summary>
        V1 = 1,

        /// <summary>
        /// http://hessian.caucho.com/doc/hessian-ws.html
        /// http://hessian.caucho.com/doc/hessian-serialization.html
        /// </summary>
        V2 = 2
    }
}