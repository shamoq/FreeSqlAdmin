export interface TenantDto {
  name: string  | null | undefined ;    //  租户名称
  code: string  | null | undefined ;    //  租户编码
  type: string  | null | undefined ;    //  租户类型（正式，测试）
  description: string  | null | undefined ;    //  租户描述
  isEnable: number  | null | undefined ;    //  租户状态
  logo: string  | null | undefined ;    //  租户Logo
  expirationTime: string  | null | undefined ;    //  过期时间
  tenantPackageId: string  | null | undefined ;    //  租户套餐Id
  dataSourceId: string  | null | undefined ;    //  数据库链接
  id: string  | null | undefined ;
}
