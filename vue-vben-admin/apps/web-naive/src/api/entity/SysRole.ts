export interface SysRole {
  name: string  | null | undefined ;    //  角色名称
  fullName: string  | null | undefined ;    //  角色全名称
  orderCode: string  | null | undefined ;    //  排序编码
  orderFullCode: string  | null | undefined ;    //  排序编码
  parentId: string  | null | undefined ;    //  父级Id
  remark: string  | null | undefined ;    //  备注
  tenantId: string  | null | undefined ;    //  租户GUID
  id: string  | null | undefined ;    //  主键
  createdId: string  | null | undefined ;    //  创建人GUID
  createdTime: string  | null | undefined ;    //  创建时间
  creator: string  | null | undefined ;    //  创建人
  updatedTime: string  | null | undefined ;    //  更新时间
  updatedId: string  | null | undefined ;    //  更新人GUID
  updator: string  | null | undefined ;    //  更新人
}
