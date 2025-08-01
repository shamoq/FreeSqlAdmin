import type { DataSetFieldProp } from './DataSetFieldProp';

export interface CustomFieldDto {
  code: string  | null | undefined ;
  fullCode: string  | null | undefined ;
  name: string  | null | undefined ;
  remark: string  | null | undefined ;
  parentGroupType: string  | null | undefined ;
  fieldTypeText: string  | null | undefined ;
  fieldType: number  | null | undefined ;
  orderId: number  | null | undefined ;
  required: boolean  | null | undefined ;
  parentId: string  | null | undefined ;
  groupType: string  | null | undefined ;
  isSystem: boolean  | null | undefined ;
  businessId: string  | null | undefined ;
  prop: DataSetFieldProp  | null | undefined ;
  id: string  | null | undefined ;
}
