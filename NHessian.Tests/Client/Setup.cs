using NHessian.Client;
using NHessian.IO;
using System;
using System.Net.Http;

namespace NHessian.Tests.Client
{
    internal static class Setup
    {
        public static TService CreateService<TService>(string endpoint, ProtocolVersion protocolVersion)
            where TService : class
        {
            var serverUrl = new Uri(Environment.GetEnvironmentVariable("HESSIAN_SERVER_URL") ?? "http://localhost:8080");

            return new HttpClient()
                    .HessianService<TService>(
                        new Uri(serverUrl, endpoint),
                        new ClientOptions()
                        {
                            TypeBindings = TypeBindings.Java,
                            ProtocolVersion = protocolVersion
                        });
        }
    }
}
