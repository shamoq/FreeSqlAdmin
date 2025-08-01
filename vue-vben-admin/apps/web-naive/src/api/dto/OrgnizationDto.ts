export interface OrgnizationDto {
  id: string  | null | undefined ;    //  主键
  name: string  | null | undefined ;    //  名称
  code: string  | null | undefined ;    //  编码
  fullName: string  | null | undefined ;    //  全名称
  contract: string  | null | undefined ;    //  负责人
  orgType: number  | null | undefined ;    //  组织类型，1 公司 2 部门
  parentId: string  | null | undefined ;    //  上级组织
  companyId: string  | null | undefined ;    //  所属公司
  remark: string  | null | undefined ;    //  备注
  isCompany: boolean  | null | undefined ;    //  是否公司
  isEndCompany: boolean  | null | undefined ;    //  是否末级公司
  isDepartment: boolean  | null | undefined ;    //  是否部门
  isEndDepartment: boolean  | null | undefined ;    //  是否末级部门
  isEnable: boolean  | null | undefined ;    //  是否启用
}
