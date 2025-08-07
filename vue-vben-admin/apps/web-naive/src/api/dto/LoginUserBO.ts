export interface LoginUserBO {
  id: string  | null | undefined ;
  userName: string  | null | undefined ;
  userCode: string  | null | undefined ;
  orgId: string  | null | undefined ;
  isAdmin: number  | null | undefined ;
  tenantId: string  | null | undefined ;    //  租户Id
  sessionId: string  | null | undefined ;    //  会话Id
}
