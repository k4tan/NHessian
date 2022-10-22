using NHessian.IO;
using System;
using System.Net.Http;
using System.Reflection;

namespace NHessian.Client
{
    /// <summary>
    /// Extension methods for <see cref="HttpClient"/> instances
    /// that construct hessian services that use said <see cref="HttpClient"/>
    /// for requests.
    /// </summary>
    public static class HessianServiceExtensions
    {
        /// <summary>
        /// Create a hessian service proxy for the provided type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The hessian service type.
        /// </typeparam>
        /// <param name="httpClient">
        /// The client used to communicate with the hessian remote endpoint.
        /// </param>
        /// <param name="endpoint">
        /// The service endpoint.
        /// </param>
        /// <param name="protocolVersion">
        /// The hessian version that should be used to serialize the hessian request.
        /// </param>
        /// <param name="unwrapServiceExceptions">
        /// Indicates whether service exceptions are thrown directly without wrapping them
        /// into <see cref="HessianRemoteException"/>.
        /// </param>
        /// <param name="typeBindings">
        /// Custom type bindings
        /// </param>
        /// <returns>
        /// Returns the created hessian service.
        /// </returns>
        [Obsolete("Use the other overload instead.")]
        public static T HessianService<T>(
            this HttpClient httpClient,
            Uri endpoint,
            TypeBindings typeBindings = null,
            ProtocolVersion protocolVersion = ProtocolVersion.V2,
            bool unwrapServiceExceptions = true)
        {
            var options = new ClientOptions()
            {
                ProtocolVersion = protocolVersion,
                TypeBindings = typeBindings,
                UnwrapServiceExceptions = unwrapServiceExceptions
            };

            return httpClient.HessianService<T>(endpoint, options);
        }

        /// <summary>
        /// Create a hessian service proxy for the provided type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The hessian service type.
        /// </typeparam>
        /// <param name="httpClient">
        /// The client used to communicate with the hessian remote endpoint.
        /// </param>
        /// <param name="endpoint">
        /// The service endpoint.
        /// </param>
        /// <param name="options">
        /// Instance containing additional options.
        /// </param>
        /// <returns>
        /// Returns the created hessian service.
        /// </returns>
        public static T HessianService<T>(
            this HttpClient httpClient,
            Uri endpoint,
            ClientOptions options)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            var proxy = DispatchProxy.Create<T, HessianProxy>();
            ((HessianProxy)(object)proxy).Initialize(httpClient, endpoint, options);
            return proxy;
        }
    }
}