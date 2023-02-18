using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.VisualBasic.CompilerServices;

namespace intStripsServer.Helpers;

public static class DistributedCacheExtensions
{
    public static Task SetAsync<T>(this IDistributedCache cache, string key, T value)
    {
        return SetAsync(cache, key, value, new DistributedCacheEntryOptions());
    }

    public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
    {
        var json = JsonSerializer.Serialize(value);
        var bytes = Encoding.UTF8.GetBytes(json);

        return cache.SetAsync(key, bytes, options);
    }

    public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key)
    {
        var bytes = await cache.GetAsync(key);
        var result = default(T);

        if (bytes == null) return result;

        var json = Encoding.UTF8.GetString(bytes);
        result = JsonSerializer.Deserialize<T>(json);

        return result;
    }

    public static bool TryGet<T>(this IDistributedCache cache, string key, out T? value)
    {
        var bytes = cache.Get(key);
        value = default;
        
        if (bytes == null) return false;

        var json = Encoding.UTF8.GetString(bytes);
        value = JsonSerializer.Deserialize<T>(json);

        return value != null;
    }
}