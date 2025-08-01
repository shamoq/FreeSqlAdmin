export enum ImageFieldTypeEnum {
  AbsoluteHref = 'absoluteHref',    // 完整链接
  AbsolutePath = 'absolutePath',    // 绝对路径
  BarCode = 'barCode',    // 条形码
  QrCode = 'qrCode',    // 二维码
}

export const ImageFieldTypeEnumOption = [
  { label: '完整链接', value: 'absoluteHref' },
  { label: '绝对路径', value: 'absolutePath' },
  { label: '条形码', value: 'barCode' },
  { label: '二维码', value: 'qrCode' },
];

export function getImageFieldTypeLabel(value: string): string | undefined {
  const item = ImageFieldTypeEnumOption.find(x => x.value === value);
  return item?.label;
}
