using NHessian.IO;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NHessian.Client
{
    internal class HessianContent : HttpContent
    {
        private readonly object[] _args;
        private readonly string _methodName;
        private readonly Func<HessianStreamWriter, HessianOutput> _outputFactory;

        public HessianContent(Func<HessianStreamWriter, HessianOutput> outputFactory, string methodName, object[] args)
        {
            _methodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            _args = args ?? throw new ArgumentNullException(nameof(args));
            _outputFactory = outputFactory ?? throw new ArgumentNullException(nameof(outputFactory));

            Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-hessian");
        }

        /// <inheritdoc/>
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return Task.Run(() =>
            {
                using (var writer = new HessianStreamWriter(stream, true))
                {
                    _outputFactory(writer).WriteCall(_methodName, _args);
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