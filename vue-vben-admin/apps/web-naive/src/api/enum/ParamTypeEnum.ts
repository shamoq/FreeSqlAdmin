export enum ParamTypeEnum {
  String = 1,    // 文本
  Number = 2,    // 数值
  Date = 3,    // 日期
  DateTime = 4,    // 日期时间
  Time = 5,    // 时间
  Password = 6,    // 密码
  Option = 7,    // 选项
  Customer = 8,    // 自定义
}

export const ParamTypeEnumOption = [
  { label: '文本', value: 1 },
  { label: '数值', value: 2 },
  { label: '日期', value: 3 },
  { label: '日期时间', value: 4 },
  { label: '时间', value: 5 },
  { label: '密码', value: 6 },
  { label: '选项', value: 7 },
  { label: '自定义', value: 8 },
];

export function getParamTypeLabel(value: number): string | undefined {
  const item = ParamTypeEnumOption.find(x => x.value === value);
  return item?.label;
}
