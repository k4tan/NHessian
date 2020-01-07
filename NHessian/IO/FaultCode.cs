namespace NHessian.IO
{
    /// <summary>
    /// Enumerates exception kinds that can be returned as the result
    /// of an RPC call. The values are defined as part of the spec.
    /// </summary>
    /// <remarks>
    /// http://hessian.caucho.com/doc/hessian-1.0-spec.xtp#Faults
    /// </remarks>
    public enum FaultCode
    {
        /// <summary>
        /// The Hessian request has some sort of syntactic error.
        /// </summary>
        ProtocolException,

        /// <summary>
        /// The requested object does not exist.
        /// </summary>
        NoSuchObjectException,

        /// <summary>
        /// The requested method does not exist.
        /// </summary>
        NoSuchMethodException,

        /// <summary>
        /// A required header was not understood by the server.
        /// </summary>
        RequireHeaderException,

        /// <summary>
        /// The called method threw an exception.
        /// </summary>
        ServiceException
    }
}
