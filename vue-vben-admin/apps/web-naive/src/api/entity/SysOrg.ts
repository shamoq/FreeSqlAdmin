export interface SysOrg {
  name: string  | null | undefined ;    //  名称
  code: string  | null | undefined ;    //  编码
  fullName: string  | null | undefined ;    //  全名称
  orgType: number  | null | undefined ;    //  组织类型，1 公司 2 部门
  parentId: string  | null | undefined ;    //  上级组织
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
