using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RVToolsWeb.Configuration;
using RVToolsWeb.Data.Repositories;
using RVToolsWeb.Models.DTOs;
using RVToolsWeb.Services.Interfaces;

namespace RVToolsWeb.Services;

/// <summary>
/// Service for retrieving filter dropdown options with in-memory caching.
/// Uses sliding expiration to keep frequently-accessed values warm.
/// </summary>
public class FilterService : IFilterService
{
    private readonly FilterRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration;

    public FilterService(
        FilterRepository repository,
        IMemoryCache cache,
        IOptions<AppSettings> settings)
    {
        _repository = repository;
        _cache = cache;
        _cacheDuration = TimeSpan.FromMinutes(settings.Value.Caching.FilterCacheMinutes);
    }

    public async Task<IEnumerable<FilterOptionDto>> GetDatacentersAsync(string? viSdkServer = null)
    {
        var cacheKey = $"Filter_Datacenters_{viSdkServer ?? "all"}";

        var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = _cacheDuration;
            return await _repository.GetDatacentersAsync(viSdkServer);
        });

        return result ?? Enumerable.Empty<FilterOptionDto>();
    }

    public async Task<IEnumerable<FilterOptionDto>> GetClustersAsync(string? datacenter = null, string? viSdkServer = null)
    {
        var cacheKey = $"Filter_Clusters_{datacenter ?? "all"}_{viSdkServer ?? "all"}";

        var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = _cacheDuration;
            return await _repository.GetClustersAsync(datacenter, viSdkServer);
        });

        return result ?? Enumerable.Empty<FilterOptionDto>();
    }

    public async Task<IEnumerable<FilterOptionDto>> GetVISdkServersAsync()
    {
        const string cacheKey = "Filter_VISdkServers";

        var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = _cacheDuration;
            return await _repository.GetVISdkServersAsync();
        });

        return result ?? Enumerable.Empty<FilterOptionDto>();
    }

    public async Task<IEnumerable<FilterOptionDto>> GetPowerstatesAsync()
    {
        const string cacheKey = "Filter_Powerstates";

        var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = _cacheDuration;
            return await _repository.GetPowerstatesAsync();
        });

        return result ?? Enumerable.Empty<FilterOptionDto>();
    }
}
