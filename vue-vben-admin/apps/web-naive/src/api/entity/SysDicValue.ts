export interface SysDicValue {
  code: string  | null | undefined ;    //  编码
  name: string  | null | undefined ;    //  名称
  value: string  | null | undefined ;
  remark: string  | null | undefined ;    //  备注
  dicTypeId: number  | null | undefined ;
  fullName: string  | null | undefined ;    //  全名称
  isEnable: number  | null | undefined ;    //  是否启用
  parentId: string  | null | undefined ;    //  父级GUID
  orderCode: string  | null | undefined ;    //  排序编码
  orderFullCode: string  | null | undefined ;    //  排序全编码
  tenantId: string  | null | undefined ;    //  租户GUID
  id: string  | null | undefined ;    //  主键
  createdId: string  | null | undefined ;    //  创建人GUID
  createdTime: string  | null | undefined ;    //  创建时间
  creator: string  | null | undefined ;    //  创建人
  updatedTime: string  | null | undefined ;    //  更新时间
  updatedId: string  | null | undefined ;    //  更新人GUID
  updator: string  | null | undefined ;    //  更新人
}
