# ExternalConfigurationProvider

[![NuGet](https://img.shields.io/nuget/v/ExternalConfigurationProvider.svg)](https://www.nuget.org/packages/ExternalConfigurationProvider)
[![AppVeyor](https://img.shields.io/appveyor/ci/aidmsu/ExternalConfigurationProvider/master.svg?label=appveyor)](https://ci.appveyor.com/project/aidmsu/ExternalConfigurationProvider/branch/master)
[![Codecov branch](https://img.shields.io/codecov/c/github/aidmsu/ExternalConfigurationProvider/master.svg)](https://codecov.io/gh/aidmsu/ExternalConfigurationProvider)

ExternalConfigurationProvider is a .NET library for getting services configs from external centralized configuration store.

## Features

* Get specified service settings from Consul
* Cache service settings

## Supported configuration stores:

* Consul

## Use cases
 
* Store sensitive data (passwords, connection strings) in external store instead of VCS. 
* Store all settings in one centralized store. When you change settings of a service you don't need to update config files of all applications which use the service. You just update config in external configuration store.

## Usage

### Consul with .NET Core

Install and run Consul following [guide](https://www.consul.io/docs/install/index.html).

Add to Consul KV values:

```
dev/redis/url http://localhost:6379
dev/redis/password StrongPassword

staging/azure/redis/url http://azure.com/redis
staging/azure/redis/password SuperStrongPassword
```

Register Consul configuration provider service in DI:

```csharp
using ExternalConfiguration;

// Getting current environment.
// In our exmaple: dev or staging.
var env = ... ;

serviceCollection.AddConsulConfigurationProvider(config =>
{
    config.Url = "http://localhost:8500";
    config.Environment = env;
});	
```

Use `IExternalConfigurationProvider` to get settings in `dev` environment:

```csharp
public class Repository
{
	private IRedisClient _redisClient;

	public Repository(IExternalConfigurationProvider externalConfigurationProvider)
	{
		var redisConfig = externalConfigurationProvider.GetServiceConfigAsync("redis");
		_redisClient = new RedisClient(redisConfig["url"], redisConfig["password"]); 
		// http://localhost:6379 StrongPassword
	}
}
```

In case of `hosting` is specified:

```csharp
var redisConfig = externalConfigurationProvider.GetServiceConfigAsync("redis", "azure");
var redisClient = new RedisClient(redisConfig["url"], redisConfig["password"]); 
// http://azure.com/redis SuperStrongPassword
```

