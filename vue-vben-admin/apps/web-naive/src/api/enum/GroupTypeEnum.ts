export enum GroupTypeEnum {
  Field = 'field',    // 字段
  Form = 'form',    // 表单
  Table = 'table',    // 表格
}

export const GroupTypeEnumOption = [
  { label: '字段', value: 'field' },
  { label: '表单', value: 'form' },
  { label: '表格', value: 'table' },
];

export function getGroupTypeLabel(value: string): string | undefined {
  const item = GroupTypeEnumOption.find(x => x.value === value);
  return item?.label;
}
