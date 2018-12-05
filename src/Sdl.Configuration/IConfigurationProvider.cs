using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sdl.Configuration
{
    public interface IConfigurationProvider
    {
        Task<Dictionary<string, string>> GetServiceConfigAsync(string service, string hosting = null);
        Task<T> GetServiceConfigAsync<T>(string service, string hosting = null);
    }
}