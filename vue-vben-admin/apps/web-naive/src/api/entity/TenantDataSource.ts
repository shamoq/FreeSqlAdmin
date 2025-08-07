export interface TenantDataSource {
  name: string  | null | undefined ;    //  名称
  dbType: string  | null | undefined ;    //  数据源类型
  connectionString: string  | null | undefined ;    //  数据源连接字符串
  connectionParams: string  | null | undefined ;    //  数据源连接字符串
  remark: string  | null | undefined ;    //  说明
  id: string  | null | undefined ;    //  主键
  createdId: string  | null | undefined ;    //  创建人GUID
  createdTime: string  | null | undefined ;    //  创建时间
  creator: string  | null | undefined ;    //  创建人
  updatedTime: string  | null | undefined ;    //  更新时间
  updatedId: string  | null | undefined ;    //  更新人GUID
  updator: string  | null | undefined ;    //  更新人
}
