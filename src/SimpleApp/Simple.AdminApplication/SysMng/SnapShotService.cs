using Mapster;
using Newtonsoft.Json;
using Simple.AdminApplication.Common;
using Simple.AdminApplication.SysMng.Entities;
using Simple.Interfaces;
using Simple.Interfaces.Dtos;

namespace Simple.AdminApplication.SysMng
{
    /// <summary>
    /// 系统快照服务
    /// </summary>
    [Scoped]
    public class SnapShotService : BaseCurdService<SysSnapShot>, ISnapShotService
    {
        public override async Task<SysSnapShot> Save(SysSnapShot entity, bool isForceAdd = false)
        {
            var result = await SaveByBusinessId(entity.BusinessId, entity.Type, entity.Content);
            return result;
        }

        public async Task DeleteSnapByBusinessId(Guid businessId, string type)
        {
            await table.DeleteAsync(x => x.Type == type && x.BusinessId == businessId);
        }

        /// <summary>
        /// 快照信息先删后插
        /// </summary>
        /// <returns></returns>
        public async Task<SysSnapShot> SaveByBusinessId(Guid? businessId, string type, object value)
        {
            var json = string.Empty;
            if (value != null)
            {
                json = value is string s ? s : JsonConvert.SerializeObject(value, Formatting.None);
            }

            var hash = EncryptHelper.MD5Encrypt(json);
            var snaps = await table.Where(x => x.Type == type && x.BusinessId == businessId).ToListAsync();
            if (snaps.Count > 0)
            {
                await table.DeleteAsync(x => x.Type == type && x.BusinessId == businessId);
            }

            // 保存新快照，并更新版本号
            var compressedJson = await StringCompressHelper.CompressData(json);
            var snap = new SysSnapShot()
            {
                Type = type,
                Content = compressedJson,
                Hash = hash,
                BusinessId = businessId,
                Version = snaps.Count == 0 ? 1 : snaps.Max(t => t.Version) + 1
            };
            await table.InsertAsync(snap);

            return snap;
        }

        public async Task<Guid> SaveSnapByBusinessId(Guid? businessId, string type, object value)
        {
            var entity = await SaveByBusinessId(businessId, type, value);
            return entity.Id;
        }

        /// <summary>
        /// 按业务主键,追加快照信息
        /// </summary>
        /// <returns></returns>
        public async Task<SnapShotEntityDto> AppendSnapByBusinessId(Guid businessId, string type, object value)
        {
            var json = string.Empty;
            if (value != null)
            {
                json = value is string s ? s : JsonConvert.SerializeObject(value, Formatting.None);
            }

            var hash = EncryptHelper.MD5Encrypt(json);

            // 根据业务主键或者hash查询数据
            var snaps = await table.Where(x => x.Type == type && x.BusinessId == businessId).ToListAsync();

            // 如果hash存在，则不添加
            if (snaps.Any(x => x.Hash == hash))
            {
                var entity = snaps.First(x => x.Hash == hash);
                return entity.Adapt<SnapShotEntityDto>();
            }

            // 保存新快照，并更新版本号
            var compressedJson = await StringCompressHelper.CompressData(json);
            var snap = new SysSnapShot()
            {
                Type = type,
                Content = compressedJson,
                Hash = hash,
                BusinessId = businessId,
                Version = snaps.Count == 0 ? 1 : snaps.Max(t => t.Version) + 1
            };
            await table.InsertAsync(snap);

            return snap.Adapt<SnapShotEntityDto>();
        }

        /// <summary>
        /// 获取快照数据
        /// </summary>
        /// <returns></returns>
        public async Task<T> GetSnapByBusinessId<T>(Guid businessId, string type)
        {
            var snap = await table.Where(x => x.BusinessId == businessId && x.Type == type).FirstAsync();
            return await SnapByBusinessId<T>(snap);
        }

        /// <summary>
        /// 获取快照数据
        /// </summary>
        /// <returns></returns>
        public async Task<T> GetSnapById<T>(Guid snapId)
        {
            var snap = await table.Where(x => x.Id == snapId).FirstAsync();
            return await SnapByBusinessId<T>(snap);
        }

        private async Task<T> SnapByBusinessId<T>(SysSnapShot snap)
        {
            if (snap == null)
            {
                return default;
            }

            var result = await StringCompressHelper.DecompressData(snap.Content);
            if (typeof(T) == typeof(string))
            {
                return (T)(object)result;
            }

            var data = JsonConvert.DeserializeObject<T>(result);
            return data;
        }
    }
}