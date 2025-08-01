export class TaskQueue {
    private queue: Array<() => Promise<any>> = [];
    private isProcessing = false;
    private concurrent: number;
    private activeCount = 0;
    private timeout: number;

    constructor(concurrent: number = 1, timeout: number = 10000) {
        this.concurrent = concurrent;
        this.timeout = timeout;
    }

    /**
     * 添加任务到队列
     * @param task 异步任务函数
     */
    async add(task: () => Promise<any>): Promise<any> {
        return new Promise((resolve, reject) => {
            this.queue.push(async () => {
                try {
                    console.log(`[TaskQueue] 开始执行任务，当前队列长度: ${this.queue.length}`)
                    const result = await Promise.race([
                        task(),
                        new Promise((_, reject) => 
                            setTimeout(() => reject(new Error('任务执行超时')), this.timeout)
                        )
                    ]);
                    console.log(`[TaskQueue] 任务执行完成`)
                    resolve(result);
                } catch (error) {
                    console.error(`[TaskQueue] 任务执行失败:`, error);
                    reject(error);
                }
            });
            console.log(`[TaskQueue] 添加新任务到队列，当前队列长度: ${this.queue.length}, 活动任务数: ${this.activeCount}`)
            this.process();
        });
    }

    /**
     * 处理队列中的任务
     */
    private async process(): Promise<void> {
        console.log(`[TaskQueue] 尝试处理队列任务 - 队列长度: ${this.queue.length}, 活动任务数: ${this.activeCount}, 是否处理中: ${this.isProcessing}`)
        if (this.isProcessing || this.queue.length === 0 || this.activeCount >= this.concurrent) {
            console.log(`[TaskQueue] 跳过处理 - isProcessing: ${this.isProcessing}, queueLength: ${this.queue.length}, activeCount: ${this.activeCount}, concurrent: ${this.concurrent}`)
            return;
        }

        const resetState = () => {
            this.activeCount--;
            this.isProcessing = false;
        };

        this.isProcessing = true;
        this.activeCount++;
        console.log(`[TaskQueue] 开始处理任务 - 活动任务数: ${this.activeCount}`)

        try {
            const task = this.queue.shift();
            if (task) {
                await task();
            }
        } catch (error) {
            console.error('[TaskQueue] 任务执行失败:', error);
        } finally {
            resetState();
            console.log(`[TaskQueue] 任务处理完成 - 队列长度: ${this.queue.length}, 活动任务数: ${this.activeCount}`)
            // 确保继续处理队列中的其他任务
            if (this.queue.length > 0) {
                await this.process();
            }
        }
    }

    /**
     * 清空队列
     */
    clear(): void {
        this.queue = [];
    }

    /**
     * 获取队列中待处理的任务数量
     */
    get size(): number {
        return this.queue.length;
    }

    /**
     * 判断队列是否为空
     */
    get isEmpty(): boolean {
        return this.queue.length === 0;
    }
}

// 创建任务队列实例的工厂函数
export function useTaskQueue(concurrent: number = 1, timeout: number = 10000) {
    return new TaskQueue(concurrent, timeout);
}