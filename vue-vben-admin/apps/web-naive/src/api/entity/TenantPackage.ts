export interface TenantPackage {
  name: string  | null | undefined ;    //  套餐名称
  description: string  | null | undefined ;    //  描述
  userCount: number  | null | undefined ;    //  用户数量
  id: string  | null | undefined ;    //  主键
  createdId: string  | null | undefined ;    //  创建人GUID
  createdTime: string  | null | undefined ;    //  创建时间
  creator: string  | null | undefined ;    //  创建人
  updatedTime: string  | null | undefined ;    //  更新时间
  updatedId: string  | null | undefined ;    //  更新人GUID
  updator: string  | null | undefined ;    //  更新人
}
