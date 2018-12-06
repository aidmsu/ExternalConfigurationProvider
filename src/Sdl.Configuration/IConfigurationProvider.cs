using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sdl.Configuration
{
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Gets service settings from configuration storage and converts them to the specified .NET type.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="hosting">The hosting where the service is hosted. Optional.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the service is not specified.</exception>
        /// <example>
        /// <code>
        /// var settings = GetServiceConfigAsync&lt;ServiceSettings&gt;("Mango");
        /// // or
        /// var settings = GetServiceConfigAsync&lt;ServiceSettings&gt;("Telephony", "azure");
        /// </code>
        /// </example>
        Task<T> GetServiceConfigAsync<T>(string service, string hosting = null) where T : new();

        /// <summary>
        /// Gets service settings from configuration storage.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="hosting">The hosting where the service is hosted. Optional.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the service is not specified.</exception>
        /// <example>
        /// <code>
        /// var settings = GetServiceConfigAsync&lt;ServiceSettings&gt;("Mango");
        /// // or
        /// var settings = GetServiceConfigAsync&lt;ServiceSettings&gt;("Telephony", "azure");
        /// </code>
        /// </example>
        Task<Dictionary<string, string>> GetServiceConfigAsync(string service, string hosting = null);
    }
}