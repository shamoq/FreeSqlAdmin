export interface SysFileDocument {
  name: string  | null | undefined ;    //  文件名
  path: string  | null | undefined ;    //  文件路径
  ext: string  | null | undefined ;    //  后缀
  storeType: string  | null | undefined ;    //  存储类型
  bucketName: string  | null | undefined ;    //  存储桶
  tenantId: string  | null | undefined ;    //  租户GUID
  id: string  | null | undefined ;    //  主键
  createdId: string  | null | undefined ;    //  创建人GUID
  createdTime: string  | null | undefined ;    //  创建时间
  creator: string  | null | undefined ;    //  创建人
  updatedTime: string  | null | undefined ;    //  更新时间
  updatedId: string  | null | undefined ;    //  更新人GUID
  updator: string  | null | undefined ;    //  更新人
}
