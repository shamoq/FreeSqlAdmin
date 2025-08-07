export interface SysParamValue {
  scopeId: string  | null | undefined ;    //  作用域Id，组织
  paramCode: string  | null | undefined ;    //  参数编码
  value: string  | null | undefined ;    //  参数值
  paramType: number  | null | undefined ;    //  参数类型
  paramTypeText: string  | null | undefined ;    //  参数类型
  tenantId: string  | null | undefined ;    //  租户GUID
  id: string  | null | undefined ;    //  主键
  createdId: string  | null | undefined ;    //  创建人GUID
  createdTime: string  | null | undefined ;    //  创建时间
  creator: string  | null | undefined ;    //  创建人
  updatedTime: string  | null | undefined ;    //  更新时间
  updatedId: string  | null | undefined ;    //  更新人GUID
  updator: string  | null | undefined ;    //  更新人
}
