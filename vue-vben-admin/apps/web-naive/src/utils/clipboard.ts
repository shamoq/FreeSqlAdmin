/**
 * 复制文本到剪贴板
 * @param {string} text - 要复制的文本内容
 * @param {Object} [options] - 可选配置项
 * @param {function} [options.onSuccess] - 复制成功时的回调函数
 * @param {function} [options.onError] - 复制失败时的回调函数
 * @returns {Promise<boolean>} - 返回一个Promise，成功时resolve为true，失败时reject
 */
export async function copyToClipboard(text, options = {}) {
  try {
      // 优先使用现代的 Clipboard API
      if (navigator.clipboard) {
          await navigator.clipboard.writeText(text);
          options.onSuccess?.();
          return true;
      }

      // 传统的 document.execCommand 方法作为备选方案
      const textarea = document.createElement('textarea');
      textarea.value = text;
      textarea.style.position = 'fixed';
      textarea.style.opacity = '0';
      document.body.appendChild(textarea);
      textarea.select();
      textarea.setSelectionRange(0, textarea.value.length);

      const successful = document.execCommand('copy');
      document.body.removeChild(textarea);

      if (successful) {
          options.onSuccess?.();
          return true;
      } else {
          throw new Error('无法执行复制命令');
      }
  } catch (error) {
      console.error('复制失败:', error);
      options.onError?.();
      return false;
  }
}
