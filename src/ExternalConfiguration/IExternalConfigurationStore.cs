using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ExternalConfiguration
{
    /// <summary>
    /// Provides methods to get service settings from external storage.
    /// </summary>
    public interface IExternalConfigurationStore
    {
        /// <summary>
        /// Gets service settings from storage.
        /// </summary>
        /// <param name="environment">The environment where app runs.</param>
        /// <param name="hosting">The hosting where the service is hosted. Optional.</param>
        /// <param name="service">The service name.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the service is not specified.</exception>
        /// <example>
        /// <code>
        /// var settings = GetServiceSettingsAsync&lt;RedisSettings&gt;("Redis", "Azure");
        /// </code>
        /// </example>
        Task<Dictionary<string, string>> GetServiceConfigAsync(string environment, string hosting, string service, CancellationToken cancellationToken = default(CancellationToken));
    }
}