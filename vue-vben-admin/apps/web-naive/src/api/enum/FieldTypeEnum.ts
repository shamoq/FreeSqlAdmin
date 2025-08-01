export enum FieldTypeEnum {
  Text = 1,    // 文本
  Number = 2,    // 数字
  Date = 3,    // 日期
  Bool = 4,    // 布尔
  Enum = 5,    // 枚举
  Image = 6,    // 图片
}

export const FieldTypeEnumOption = [
  { label: '文本', value: 1 },
  { label: '数字', value: 2 },
  { label: '日期', value: 3 },
  { label: '布尔', value: 4 },
  { label: '枚举', value: 5 },
  { label: '图片', value: 6 },
];

export function getFieldTypeLabel(value: number): string | undefined {
  const item = FieldTypeEnumOption.find(x => x.value === value);
  return item?.label;
}
