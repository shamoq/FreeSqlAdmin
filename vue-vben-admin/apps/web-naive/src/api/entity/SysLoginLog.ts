export interface SysLoginLog {
  userId: string  | null | undefined ;    //  用户ID
  userName: string  | null | undefined ;    //  用户名
  userCode: string  | null | undefined ;    //  用户编码
  loginTime: string  | null | undefined ;    //  登录时间
  loginIp: string  | null | undefined ;    //  登录IP地址
  device: string  | null | undefined ;    //  登录设备
  browser: string  | null | undefined ;    //  浏览器信息
  errorMsg: string  | null | undefined ;    //  错误信息
  screenResolution: string  | null | undefined ;    //  登录设备分辨率
  sessionId: string  | null | undefined ;    //  会话ID
  tenantId: string  | null | undefined ;    //  租户GUID
  id: string  | null | undefined ;    //  主键
  createdId: string  | null | undefined ;    //  创建人GUID
  createdTime: string  | null | undefined ;    //  创建时间
  creator: string  | null | undefined ;    //  创建人
  updatedTime: string  | null | undefined ;    //  更新时间
  updatedId: string  | null | undefined ;    //  更新人GUID
  updator: string  | null | undefined ;    //  更新人
}
