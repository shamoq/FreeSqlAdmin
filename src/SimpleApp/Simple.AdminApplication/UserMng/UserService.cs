using Mapster;
using Simple.AdminApplication.Common;
using Simple.AdminApplication.Extensions;
using Simple.AdminApplication.UserMng.Entities;
using Simple.Interfaces;
using Simple.Interfaces.Dtos;
using Simple.Utils.Exceptions;

namespace Simple.AdminApplication.UserMng
{
    [Scoped]
    public class UserService : BaseCurdService<SysUser>, IUserService
    {
        private User2RoleService _user2RoleService;

        public UserService(User2RoleService user2RoleService)
        {
            _user2RoleService = user2RoleService;
        }

        /// <summary>
        /// password 本身是md5加密后的密码，这里跟用户Id和盐进行加密
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string GetMd5Password(SysUser entity, string password)
        {
            var md5Password =
                EncryptHelper.MD5Encrypt(
                    password + entity.Id.ToString().Replace("-", string.Empty) + entity.Salt);
            return md5Password;
        }

        public override async Task<SysUser> Save(SysUser entity, bool isForceAdd = false)
        {
            if (entity.IsAdmin == 1)
                throw new CustomException("超级管理员不能修改");

            var hasData =
                await table.Select.AnyAsync(
                    t => t.UserCode == entity.UserCode && t.Id != entity.Id);
            if (hasData)
                throw new CustomException("账号已存在");

            var isAdd = entity.Id == Guid.Empty;
            if (isAdd)
            {
                entity.Id = Guid.NewGuid();
                entity.Salt = Guid.NewGuid().ToString().Replace("-", "");
                entity.Password = GetMd5Password(entity, entity.Password);
                await table.InsertAsync(entity);
            }
            else
            {
                var oldUser = await table.Where(t => t.Id == entity.Id).FirstAsync();
                if (oldUser == null)
                    throw new CustomException("用户不存在");
                if (oldUser.Password != entity.Password)
                {
                    // 修改时有新密码，则需要加密
                    entity.Salt = Guid.NewGuid().ToString().Replace("-", "");
                    entity.Password = GetMd5Password(entity, entity.Password);
                }

                await table.UpdateAsync(entity);
            }

            return entity;
        }

        public override async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await base.FindAsync(id);
            if (entity.IsAdmin == 1)
                throw new CustomException("超级管理员不能删除");
            var item = await base.DeleteAsync(id);
            await _user2RoleService.DeleteAsync(t => t.UserId == id);
            return true;
        }

        public async Task ResetPassword(Guid userId, string oldPassword, string newPassword)
        {
            var user = await table.Where(t => t.Id == userId).FirstAsync();
            if (user == null)
            {
                throw new CustomException("用户不存在");
            }

            // 校验旧密码
            var dbOldPassword = GetMd5Password(user, oldPassword);
            var dbNewPassword = GetMd5Password(user, newPassword);
            if (dbOldPassword != user.Password)
            {
                throw new CustomException("旧密码错误");
            }

            if (dbNewPassword == user.Password)
            {
                throw new CustomException("新密码不能与旧密码相同");
            }

            // 更新密码
            user.Password = dbNewPassword;
        }

        public async Task<List<UserEntityDto>> GetUserByIds(List<Guid> userIds)
        {
            var users = await table.Where(t => userIds.Contains(t.Id)).ToListAsync();
            return users.Adapt<List<UserEntityDto>>();
        }
    }
}