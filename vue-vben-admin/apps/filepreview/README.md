# filepreview
纯前端，通用文件预览服务

# 支持以下文件类型

| 类型 | 支持 | 渲染组件 | 备注 |
| --- | --- | --- | --- |
| docx | ✅ | VueOfficeDocx  |  | 
| xlsx | ✅ | VueOfficeExcel  |  |     
| pdf | ✅ | VueOfficePdf  |  |    
| pptx | ✅ | VueOfficePptx  |  |         
| xml | ✅ | 纯文本预览  |  |   
| txt | ✅ | 纯文本预览  |  |   
| css | ✅ | 纯文本预览  |  |                
| 图片 | ✅ | viewerjs  | jpg,jpeg,bmp,gif,png,tif |    
| ofd | ✅ | ofdviewer  |  |    

# 安装
```bash
npm install 
```

# 启动
```bash
npm start
```

# 使用方法

## 方法1：iframe嵌入
``` js
<script setup lang="ts">

const loadUrl = async (fileName, url, type) => {
    let iframe: any = document.getElementById('iframeFilePreview');
    if (!iframe) {
        await nextTick();
        iframe = document.getElementById('iframeFilePreview');
    }
    if (!iframe || !iframe.contentWindow) {
        return;
    }
    if (iframe.contentWindow.showFiles) {
        iframe.contentWindow.showFiles([{
            name: fileName,
            type,
            url: url,
        }]);
    } else {
        iframe.onload = async () => {
            iframe.contentWindow.showFiles([{
                name: fileName,
                type,
                url: url,
            }]);
        };
    }
}

</script>
<template>
    <div class="editor-container h-full">
        <!-- Word文档预览 -->
        <iframe src="/static/filepreview/index.html" id="iframeFilePreview" class="w-full h-full" />
    </div>
</template>

```

## 方法2：新页面打开
``` js
<script setup lang="ts">

// files: {name,url,type?}
const handlePreview = (files: [], index) => {
    // 获取当前文件的索引
    var search = '?_t=' + Date.now();
    const previewPage = '/static/filepreview/index.html' + search;
    sessionStorage.setItem(search, JSON.stringify({ files, index }))
    window.open(previewPage, '_blank');
};

```

## 方法3：新页面打开，直接通过url传递单个文件信息
``` js
<script setup lang="ts">

// files: {name,url,type?}
  let previewUrl = ''; //文件预览地址
  previewUrl =  previewUrl += '?url=/api/file/download?id='+documentId+'&'name='+fileName;
  window.open(previewUrl, '_blank');

```

## 方法说明

``` js
showFiles(files:[], index:number)

files: {name,url,type?}, type为空时会尝试从name中截取
index: 当前文件的索引，默认0

```

页面初始化时，以url的search部分为key，会从sessionStorage中获取文件列表和当前文件的索引，然后调用showFiles方法显示文件列表和当前文件的预览，适用于打开页面后刷新的情况。


# 其他说明

依赖的外部预览组件，仅当使用时才会加载。


# 演示地址
[https://shamoq.github.io/filepreview/](https://shamoq.github.io/filepreview/)
