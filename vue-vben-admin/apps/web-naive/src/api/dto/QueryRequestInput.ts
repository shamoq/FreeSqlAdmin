import type { Filter } from './Filter';

export interface QueryRequestInput {
  pageSize: number  | null | undefined ;
  page: number  | null | undefined ;
  filters: Filter[] ;
  sortField: string  | null | undefined ;
  sortType: string  | null | undefined ;
}
