using NHessian.IO;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace NHessian.Client
{
    internal class HessianProxy : DispatchProxy
    {
        private Uri _endpoint;
        private HttpClient _httpClient;
        private ClientOptions _options;

        public void Initialize(
            HttpClient httpClient,
            Uri endpoint,
            ClientOptions options = default)
        {
            /*
             * Initialize is required because this instance is created
             * by `DispatchProxy.Create` which requries an empty constructor.
             */
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _options = options;
        }

        /// <inheritdoc/>
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var returnType = targetMethod.ReturnType;

            // async with return value
            if (returnType.IsGenericType && typeof(Task<>) == returnType.GetGenericTypeDefinition())
                return InvokeAsync(targetMethod.Name, returnType.GetGenericArguments()[0], args);
            // async no return value
            else if (typeof(Task).IsAssignableFrom(returnType))
                return InvokeAsync(targetMethod.Name, null, args);
            // sync
            else
                return HttpInvoke(targetMethod.Name, returnType, args).GetAwaiter().GetResult();
        }

        private static async Task<T> CastAsyncResponse<T>(Task<object> t)
        {
            object result = null;
            try
            {
                return (T)(result = await t);
            }
            catch (InvalidCastException e)
            {
                var message = $"The server responded with a value of unexpected type. Expected type: '{typeof(T)}'; Value: '{result}'";
                throw new InvalidOperationException(message, e);
            }
        }

        private HessianOutput CreateOutput(HessianStreamWriter writer)
        {
            return _options.ProtocolVersion == ProtocolVersion.V1
                        ? (HessianOutput)new HessianOutputV1(writer, _options.TypeBindings)
                        : new HessianOutputV2(writer, _options.TypeBindings);
        }

        private async Task<object> HandleResponse(HttpResponseMessage responseMessage, Type returnType)
        {
            var responseStream = await responseMessage.Content.ReadAsStreamAsync();
            using (var reader = new HessianStreamReader(responseStream))
            {
                // we assume the server responds using the same protocol version
                var input = _options.ProtocolVersion == ProtocolVersion.V1
                    ? (HessianInput)new HessianInputV1(reader, _options.TypeBindings)
                    : new HessianInputV2(reader, _options.TypeBindings);

                var reply = input.ReadReply(returnType);

                if (reply is HessianRemoteException ex)
                {
                    if (_options.UnwrapServiceExceptions
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

        private async Task<object> HttpInvoke(string methodName, Type returnType, object[] args)
        {
            var responseMessage = await SendRequest(methodName, args);
            return await HandleResponse(responseMessage, returnType);
        }

        private Task InvokeAsync(string methodName, Type returnType, object[] args)
        {
            var requestTask = HttpInvoke(methodName, returnType, args);

            if (returnType != null)
            {
                /*
                 * Cast result Task to the expected type.
                 * Because Task is a class, the return type must match perfectly
                 * (for example, Task<object[]> can not be converted to Task<IEnumerable> => CastException).
                 *
                 * Therefore, we must construct a Task type that matches the C# expected type exactly.
                 *
                 * This code creates a generic method that does just that (casting actual 'Task<object[]>' to
                 * expected 'Task<IEnumerable>' for example).
                 *
                 * NOTE This is not a great solution might need to be improved later on.
                 */
                return (Task)typeof(HessianProxy)
                    .GetMethod(nameof(HessianProxy.CastAsyncResponse), BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(returnType)
                    .Invoke(null, new object[] { requestTask });
            }

            return requestTask;
        }

        private async Task<HttpResponseMessage> SendRequest(string methodName, object[] args)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _endpoint)
            {
                Content = new HessianContent(CreateOutput, methodName, args)
            };
            var responseMessage = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            responseMessage.EnsureSuccessStatusCode();
            return responseMessage;
        }
    }
}