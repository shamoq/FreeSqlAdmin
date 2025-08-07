export interface Tenant {
  name: string  | null | undefined ;    //  租户名称
  code: string  | null | undefined ;    //  租户编码
  type: string  | null | undefined ;    //  租户类型（正式，测试）
  description: string  | null | undefined ;    //  租户描述
  isEnable: number  | null | undefined ;    //  租户状态
  logo: string  | null | undefined ;    //  租户Logo
  dataSourceId: string  | null | undefined ;    //  数据库链接
  expirationTime: string  | null | undefined ;    //  过期时间
  tenantPackageId: string  | null | undefined ;    //  租户套餐Id
  id: string  | null | undefined ;    //  主键
  createdId: string  | null | undefined ;    //  创建人GUID
  createdTime: string  | null | undefined ;    //  创建时间
  creator: string  | null | undefined ;    //  创建人
  updatedTime: string  | null | undefined ;    //  更新时间
  updatedId: string  | null | undefined ;    //  更新人GUID
  updator: string  | null | undefined ;    //  更新人
}
