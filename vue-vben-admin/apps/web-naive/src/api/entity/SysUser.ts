export interface SysUser {
  orgId: string  | null | undefined ;    //  组织机构Id
  userCode: string  | null | undefined ;    //  账号
  password: string  | null | undefined ;    //  密码
  salt: string  | null | undefined ;    //  密码加密因子
  userName: string  | null | undefined ;    //  名称
  avatar: string  | null | undefined ;    //  头像
  phone: string  | null | undefined ;    //  联系电话
  email: string  | null | undefined ;    //  电子邮箱
  isAdmin: number  | null | undefined ;    //  是否是超级管理员
  isEnable: number  | null | undefined ;    //  是否启用
  lastLoginTime: string  | null | undefined ;    //  上次登录时间
  lastIpAdress: string  | null | undefined ;    //  上次登录IP
  tenantId: string  | null | undefined ;    //  租户GUID
  id: string  | null | undefined ;    //  主键
  createdId: string  | null | undefined ;    //  创建人GUID
  createdTime: string  | null | undefined ;    //  创建时间
  creator: string  | null | undefined ;    //  创建人
  updatedTime: string  | null | undefined ;    //  更新时间
  updatedId: string  | null | undefined ;    //  更新人GUID
  updator: string  | null | undefined ;    //  更新人
}
