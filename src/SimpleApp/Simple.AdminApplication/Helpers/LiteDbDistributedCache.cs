using LiteDB;
using Microsoft.Extensions.Caching.Distributed;

namespace Simple.Utils.Helper;

/// <summary>
/// 基于 LiteDB 实现的 IDistributedCache，完全兼容标准接口
/// </summary>
public class LiteDbDistributedCache : IDistributedCache
{
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);
    private AppProfileDataTable _appProfileDataTable;

    public LiteDbDistributedCache(string dbPath)
    {
        _appProfileDataTable = HostServiceExtension.AppProfileData["cache"];
        // 启动定时清理过期缓存的后台任务
        // StartExpiredItemsCleaner();
    }

    // 实现 IDistributedCache 接口方法
    public byte[]? Get(string key)
    {
        return GetAsync(key).GetAwaiter().GetResult();
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var item = _appProfileDataTable.Get<CacheItem>(key);
        if (item == null) return null;

        // 检查是否过期
        if (IsExpired(item))
        {
            await RemoveAsync(key, token);
            return null;
        }

        // 滑动过期：更新最后访问时间
        if (item.SlidingExpiration.HasValue)
        {
            item.LastAccessed = DateTimeOffset.UtcNow;
            _appProfileDataTable.Set(key, item);
        }

        return item.Value;
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        SetAsync(key, value, options).GetAwaiter().GetResult();
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var item = new CacheItem
        {
            Key = key,
            Value = value,
            AbsoluteExpiration = options.AbsoluteExpiration,
            SlidingExpiration = options.SlidingExpiration,
            LastAccessed = DateTimeOffset.UtcNow
        };

        // 处理相对过期时间（转换为绝对时间）
        if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            item.AbsoluteExpiration = DateTimeOffset.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);
        }

        _appProfileDataTable.Set(key, item);
        await Task.CompletedTask;
    }

    public void Remove(string key)
    {
        RemoveAsync(key).GetAwaiter().GetResult();
    }

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        _appProfileDataTable.Remove(key);
        await Task.CompletedTask;
    }

    public void Refresh(string key)
    {
        RefreshAsync(key).GetAwaiter().GetResult();
    }

    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
        // 刷新即更新最后访问时间（延长滑动过期）
        token.ThrowIfCancellationRequested();
        var item = _appProfileDataTable.Get<CacheItem>(key);
        if (item != null)
        {
            item.LastAccessed = DateTimeOffset.UtcNow;
            _appProfileDataTable.Set(key, item);
        }
        await Task.CompletedTask;
    }

    // 检查缓存项是否过期
    private bool IsExpired(CacheItem item)
    {
        // 绝对过期检查
        if (item.AbsoluteExpiration.HasValue && item.AbsoluteExpiration < DateTimeOffset.UtcNow)
        {
            return true;
        }

        // 滑动过期检查（最后访问时间 + 滑动过期时间 < 当前时间）
        if (item.SlidingExpiration.HasValue 
            && (item.LastAccessed.Add(item.SlidingExpiration.Value) < DateTimeOffset.UtcNow))
        {
            return true;
        }

        return false;
    }

    // 定时清理过期缓存项
    private void StartExpiredItemsCleaner()
    {
        // _ = Task.Run(async () =>
        // {
        //     while (true)
        //     {
        //         await Task.Delay(TimeSpan.FromMinutes(5)); // 每5分钟清理一次
        //         try
        //         {
        //             // 删除绝对过期或滑动过期的项
        //             var expiredAbsolute = Query<CacheItem>.Where(x => x.AbsoluteExpiration < DateTimeOffset.UtcNow);
        //             var expiredSliding = Query<CacheItem>.Where(x => 
        //                 x.SlidingExpiration.HasValue && 
        //                 x.LastAccessed.Add(x.SlidingExpiration.Value) < DateTimeOffset.UtcNow);
        //             
        //             _cacheCollection.Delete(expiredAbsolute.Or(expiredSliding));
        //         }
        //         catch (Exception ex)
        //         {
        //             // 记录清理失败日志，不中断任务
        //             Console.WriteLine($"清理过期缓存失败：{ex.Message}");
        //         }
        //     }
        // });
    }
    
    
    
    // 缓存项实体（对应 LiteDB 集合）
    private class CacheItem
    {
        public string Key { get; set; } = string.Empty;
        public byte[] Value { get; set; } = Array.Empty<byte>();
        public DateTimeOffset? AbsoluteExpiration { get; set; } // 绝对过期时间
        public TimeSpan? SlidingExpiration { get; set; } // 滑动过期时间
        public DateTimeOffset LastAccessed { get; set; } = DateTimeOffset.UtcNow; // 最后访问时间（用于滑动过期）
    }
}