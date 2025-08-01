export interface TenantAppRightDto {
  name: string  | null | undefined ;    //  应用名称
  code: string  | null | undefined ;    //  应用编码
  order: number  | null | undefined ;    //  排序
  children: TenantAppRightDto[] ;    //  子级
  actions: TenantAppRightDto[] ;    //  动作点
}
