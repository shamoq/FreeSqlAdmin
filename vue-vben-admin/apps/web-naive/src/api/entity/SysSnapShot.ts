export interface SysSnapShot {
  type: string  | null | undefined ;    //  数据类型
  content: string  | null | undefined ;    //  数据值
  hash: string  | null | undefined ;    //  数据hash值
  businessId: string  | null | undefined ;    //  业务Id
  version: number  | null | undefined ;    //  版本
  tenantId: string  | null | undefined ;    //  租户GUID
  id: string  | null | undefined ;    //  主键
  createdId: string  | null | undefined ;    //  创建人GUID
  createdTime: string  | null | undefined ;    //  创建时间
  creator: string  | null | undefined ;    //  创建人
  updatedTime: string  | null | undefined ;    //  更新时间
  updatedId: string  | null | undefined ;    //  更新人GUID
  updator: string  | null | undefined ;    //  更新人
}
