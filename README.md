# ExternalConfigurationProvider

[![AppVeyor](https://img.shields.io/appveyor/ci/aidmsu/ExternalConfigurationProvider/master.svg?label=appveyor)](https://ci.appveyor.com/project/aidmsu/ExternalConfigurationProvider/branch/master)

ExternalConfigurationProvider is a .NET library for getting services configs from external centralized configuration store.

## Features

* Get specified service settings from Consul
* Cache service settings

## Supported configuration stores:

* Consul

## Use case

TODO

## Usage

### .NET Core

So, you have external configuration store (e.g. Consul) which stores configs of all services.

Keys must be in the formats:

```
{environment}/{hosting}/{service}/{settings}
or
{environment}/{service}/{settings}

Example:
staging/azure/redis/url
```

Next, you need to get service settings in apps:

```csharp
using Sdl.Configuration;

// Register Consul configuration provider service in DI.
serviceCollection.AddConsulConfigurationProvider(config =>
{
    config.Url = "http://localhost:8500";
    config.Environment = "Debug";
});	

// Getting settings.
var redisConfig = consulConfigurationProvider.GetServiceConfigAsync("Redis");
var redisClient = new RedisClient(redisConfig["url"]);
```
