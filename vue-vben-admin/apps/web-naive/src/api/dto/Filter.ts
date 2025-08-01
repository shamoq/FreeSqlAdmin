export interface Filter {
  type: string  | null | undefined ;    //  And 连接 默认
  field: string  | null | undefined ;    //  字段名称
  op: string  | null | undefined ;    //  操作符
  value: string  | null | undefined ;    //  值
  filters: Filter[] ;    //  表达式
}
