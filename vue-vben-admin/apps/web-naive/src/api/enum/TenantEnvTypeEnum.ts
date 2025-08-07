export enum TenantEnvTypeEnum {
  Prod = 'Prod',    // 正式
  Test = 'Test',    // 测试
}

export const TenantEnvTypeEnumOption = [
  { label: '正式', value: 'Prod' },
  { label: '测试', value: 'Test' },
];

export function getTenantEnvTypeLabel(value: string): string | undefined {
  const item = TenantEnvTypeEnumOption.find(x => x.value === value);
  return item?.label;
}
