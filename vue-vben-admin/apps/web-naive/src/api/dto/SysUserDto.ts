export interface SysUserDto {
  id: string  | null | undefined ;    //  主键
  orgId: string  | null | undefined ;    //  组织机构Id
  userCode: string  | null | undefined ;    //  账号
  userName: string  | null | undefined ;    //  名称
  avatar: string  | null | undefined ;    //  头像
  phone: string  | null | undefined ;    //  联系电话
  email: string  | null | undefined ;    //  电子邮箱
  isAdmin: number  | null | undefined ;    //  是否是超级管理员
  isEnable: number  | null | undefined ;    //  是否启用
  roleId: string  | null | undefined ;    //  账户角色
  lastLoginTime: string  | null | undefined ;    //  上次登录时间
  lastIpAdress: string  | null | undefined ;    //  上次登录IP
  orgnizationName: string  | null | undefined ;    //  组织名称
  orgnizationFullName: string  | null | undefined ;    //  组织名称
  roleName: string  | null | undefined ;    //  角色名称
}
