using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ExternalConfiguration
{
    /// <summary>
    /// Provides methods to get service settings from external configuration storage.
    /// </summary>
    public interface IExternalConfigurationProvider
    {
        /// <summary>
        /// Gets service settings from external storage and converts them to the specified .NET type.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the service is not specified.</exception>
        /// <example>
        /// <code>
        /// var settings = GetServiceSettingsAsync&lt;RedisSettings&gt;("Redis");
        /// </code>
        /// </example>
        Task<T> GetServiceSettingsAsync<T>(string service, CancellationToken cancellationToken = default(CancellationToken)) where T : new();

        /// <summary>
        /// Gets service settings from external store and converts them to the specified .NET type.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="hosting">The hosting where the service is hosted. Optional.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the service is not specified.</exception>
        /// <example>
        /// <code>
        /// var settings = GetServiceSettingsAsync&lt;RedisSettings&gt;("Redis", "Azure");
        /// </code>
        /// </example>
        Task<T> GetServiceSettingsAsync<T>(string service, string hosting, CancellationToken cancellationToken = default(CancellationToken)) where T : new();

        /// <summary>
        /// Gets service settings from external store.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the service is not specified.</exception>
        /// <example>
        /// <code>
        /// var settings = GetServiceSettingsAsync&lt;RedisSettings&gt;("Redis");
        /// </code>
        /// </example>
        Task<Dictionary<string, string>> GetServiceSettingsAsync(string service, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Gets service settings from external store.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="hosting">The hosting where the service is hosted. Optional.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the service is not specified.</exception>
        /// <example>
        /// <code>
        /// var settings = GetServiceSettingsAsync&lt;RedisSettings&gt;("Redis", "Azure");
        /// </code>
        /// </example>
        Task<Dictionary<string, string>> GetServiceSettingsAsync(string service, string hosting, CancellationToken cancellationToken = default(CancellationToken));
    }
}