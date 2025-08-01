/**
 * 步骤结果枚举
 */
export enum StepResult {
  /** 发起 */
  Lanch = 1,
  /** 同意 */
  Pass = 2,
  /** 驳回 */
  Reject = 3,
  /** 归档 */
  Finish = 4,
  /** 作废 */
  Abort = 5,
}

/**
 * 步骤结果描述映射
 */
export const StepResultDescription: Record<StepResult, string> = {
  [StepResult.Lanch]: '发起',
  [StepResult.Pass]: '同意',
  [StepResult.Reject]: '驳回',
  [StepResult.Finish]: '归档',
  [StepResult.Abort]: '作废',
};