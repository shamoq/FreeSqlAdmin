export interface RoleDto {
  id: string  | null | undefined ;    //  主键
  name: string  | null | undefined ;    //  角色名称
  fullName: string  | null | undefined ;    //  角色全名称
  parentId: string  | null | undefined ;    //  父级Id
  remark: string  | null | undefined ;    //  备注
}
