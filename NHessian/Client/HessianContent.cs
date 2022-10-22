using NHessian.IO;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NHessian.Client
{
    /// <summary>
    /// Specialized <see cref="HttpContent"/> that contains a hessian serialized payload.
    /// </summary>
    public class HessianContent : HttpContent
    {
        private readonly object[] _args;
        private readonly ClientOptions _options;
        private readonly string _methodName;

        /// <summary>
        /// Initiliazes a new instance of <see cref="HessianContent"/>.
        /// </summary>
        /// <param name="methodName">
        /// The name of the remote hessian method that is being called.
        /// </param>
        /// <param name="args">
        /// The parameters that should be serializes for the invoked method.
        /// </param>
        /// <param name="options">
        /// Additional options for serialization.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        public HessianContent(string methodName, object[] args, ClientOptions options = default)
        {
            _methodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            _args = args ?? throw new ArgumentNullException(nameof(args));
            _options = options;

            Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-hessian");
        }

        /// <inheritdoc/>
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return Task.Run(() =>
            {
                using (var writer = new HessianStreamWriter(stream, true))
                {
                    var hessianOutput = _options.ProtocolVersion == ProtocolVersion.V1
                                ? (HessianOutput)new HessianOutputV1(writer, _options.TypeBindings)
                                 : new HessianOutputV2(writer, _options.TypeBindings);

                    hessianOutput.WriteCall(_methodName, _args);
                }
            });
        }

        /// <inheritdoc/>
        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}