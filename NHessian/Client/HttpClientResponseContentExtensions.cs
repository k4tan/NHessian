using NHessian.IO;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NHessian.Client
{
    /// <summary>
    /// Extension methods for <see cref="HttpContent"/> instances tha contain a hessian response.
    /// </summary>
    public static class HttpClientResponseContentExtensions
    {
        /// <summary>
        /// Serialize the HTTP hessian content to an object as an asynchronous operation.
        /// </summary>
        /// <param name="responseContent">
        /// The content that contains a hessian response.
        /// </param>
        /// <param name="expectedResultType">
        /// Expected type of the result object. Null means no result expected (void).
        /// </param>
        /// <param name="options">
        /// Instance containing additional options.
        /// </param>
        /// <returns>
        /// The task object representing the asynchronous operation.
        /// </returns>
        public static async Task<object> ReadAsHessianAsync(
            this HttpContent responseContent,
            Type expectedResultType,
            ClientOptions options = default)
        {
            var responseStream = await responseContent.ReadAsStreamAsync();
            using (var reader = new HessianStreamReader(responseStream))
            {
                var input = options.ProtocolVersion == ProtocolVersion.V1
                    ? (HessianInput)new HessianInputV1(reader, options.TypeBindings)
                    : new HessianInputV2(reader, options.TypeBindings);

                var reply = input.ReadReply(expectedResultType);
                if (reply is HessianRemoteException ex)
                {
                    if (options.UnwrapServiceExceptions
                        && ex.Code == FaultCode.ServiceException
                        && ex.InnerException != null)
                    {
                        // throw the remoted exception if configured
                        throw ex.InnerException;
                    }
                    throw ex;
                }
                return reply;
            }
        }
    }
}