import { useAccessStore } from '@vben/stores';
import { getFileUrl } from './frame';

const accessStore = useAccessStore();

// 封装一个下载文件的方法
export function downloadFile(url: string, fileName: string) {
  //   const link = document.createElement('a');
  //   link.style.display = 'none';
  //   link.href = url;
  //   link.setAttribute('download', fileName);
  //   document.body.appendChild(link);
  //   link.click();
  //   document.body.removeChild(link);
  const newUrl = parseFileUrl(url, {
    fileName
  });
  window.open(newUrl, '_blank');
}

export function parseFileUrl(url: string, urlParams?: any) {
  var token = accessStore.accessToken?.replace("Bearer ", "");
  var separate = url.indexOf('?') > -1 ? '&' : '?';

  // 没有参数，直接拼接token
  if (!urlParams) {
    url = `${url}${separate}token=${token}&_t=` + new Date().getTime();
  } else {
    // 自动拼接所有参数，最后在拼接token
    for (const key in urlParams) {
      if (Object.prototype.hasOwnProperty.call(urlParams, key)) {
        const element = encodeURIComponent(urlParams[key]);
        url = `${url}${separate}${key}=${element}`;
        separate = '&';
      }
    }
    url = `${url}${separate}token=${token}&_t=` + new Date().getTime();
  }
  return url;

}

export function previewByDocumentId(documentId: string, fileName: string) {
  let previewUrl = getFileUrl();
  previewUrl = parseFileUrl(previewUrl, {url: '/api/file/download?id='+documentId, name: fileName});
  window.open(previewUrl, '_blank');
}

