export enum TemplateTypeEnum {
  Richtext = 'richtext',    // 富文本
  Word = 'word',    // word
  Wps = 'wps',    // Wps
}

export const TemplateTypeEnumOption = [
  { label: '富文本', value: 'richtext' },
  { label: 'word', value: 'word' },
  { label: 'Wps', value: 'wps' },
];

export function getTemplateTypeLabel(value: string): string | undefined {
  const item = TemplateTypeEnumOption.find(x => x.value === value);
  return item?.label;
}
