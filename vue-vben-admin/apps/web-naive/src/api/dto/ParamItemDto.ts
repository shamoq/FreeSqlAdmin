import type { OptionObjectDto } from './OptionObjectDto';

export interface ParamItemDto {
  paramName: string  | null | undefined ;    //  参数名称
  paramCode: string  | null | undefined ;    //  参数编码
  value: any  | null | undefined ;    //  参数值
  isHide: boolean  | null | undefined ;    //  是否隐藏
  functionCode: string  | null | undefined ;    //  权限信息
  paramTypeText: string  | null | undefined ;    //  参数类型
  paramType: number  | null | undefined ;    //  参数类型
  options: OptionObjectDto[] ;    //  选项数据源
}
