<template>
  <div id="app-wrapper">
    <div class="header-area" v-if="showSidebar">
      <div class="current-file-name">{{ currentFileName }}</div>
      <div class="menu-area">
        <div class="menu-wrapper">
          <div class="menu-trigger">文件列表</div>
          <ul class="file-list">
            <li v-for="(file, index) in files" :key="file.url" :class="{ active: currentFileIndex === index }"
              @click="previewFile(index)">
              {{ file.name }}
            </li>
          </ul>
        </div>
        <button class="download-btn" @click="downloadCurrentFile">下载</button>
        <button v-if="!isIframeMode" class="download-btn" @click="closePreview">关闭</button>
      </div>
    </div>
    <div class="main-content" :v-show-sidebar="showSidebar"
      :style="{ height: showSidebar ? 'calc(100vh - 50px)' : '100vh' }">
      <div v-if="isLoading" class="loading-indicator">加载中...</div>
      <div class="nav-buttons" v-if="showSidebar">
        <button class="nav-btn prev viewer-prev" :disabled="!hasPrevFile" @click="previewFile(currentFileIndex - 1)">
          &lt;
        </button>
        <button class="nav-btn next viewer-next" :disabled="!hasNextFile" @click="previewFile(currentFileIndex + 1)">
          &gt;
        </button>
      </div>
      <component :is="currentComponent" v-if="!isLoading" :src="currentFile" style="height: calc(100vh - 50px)"
        @rendered="renderedHandler" @error="errorHandler"
        :options="currentComponent === 'VueOfficeExcel' ? options : {}" />
    </div>
  </div>
</template>

<script>
import { defineAsyncComponent } from "vue";
import PreviewTxt from "./components/PreviewTxt.vue";
import PreviewIframe from "./components/PreviewIframe.vue";
import PreviewNotSupported from "./components/PreviewNotSupported.vue";
import "@vue-office/excel/lib/index.css";
import "viewerjs/dist/viewer.css";

// 定义加载失败提示组件
const ErrorComponent = {
  template: "<div>加载失败</div>",
};

// 定义加载中提示组件
const LoadingComponent = {
  template: "<div>加载中...</div>",
};

const VueOfficeDocx = defineAsyncComponent({
  loader: () => import("@vue-office/docx").then((comp) => comp.default || comp),
  errorComponent: ErrorComponent,
  loadingComponent: LoadingComponent,
});

const VueOfficeExcel = defineAsyncComponent({
  loader: () =>
    import("@vue-office/excel").then((comp) => comp.default || comp),
  errorComponent: ErrorComponent,
  loadingComponent: LoadingComponent,
});

const VueOfficePdf = defineAsyncComponent({
  loader: () => import("@vue-office/pdf").then((comp) => comp.default || comp),
  errorComponent: ErrorComponent,
  loadingComponent: LoadingComponent,
});

const VueOfficePptx = defineAsyncComponent({
  loader: () => import("@vue-office/pptx").then((comp) => comp.default || comp),
  errorComponent: ErrorComponent,
  loadingComponent: LoadingComponent,
});

const PreviewImage = defineAsyncComponent({
  loader: () =>
    import("./components/PreviewImage.vue").then(
      (comp) => comp.default || comp
    ),
  errorComponent: ErrorComponent,
  loadingComponent: LoadingComponent,
});

export default {
  name: "App",
  components: {
    VueOfficeDocx,
    VueOfficeExcel,
    VueOfficePdf,
    VueOfficePptx,
    PreviewTxt,
    PreviewImage,
    PreviewNotSupported,
    PreviewIframe,
  },
  data() {
    return {
      files: [],
      currentFile: "",
      currentComponent: null,
      showSidebar: false,
      unsupportedFile: null,
      isLoading: false,
      currentFileName: "",
      currentFileIndex: 0,
      searchQuery: "",
      options: {
        xls: false,
        minColLength: 0,
        minRowLength: 0,
        widthOffset: 30,
        heightOffset: 30,
        beforeTransformData: (workbookData) => {
          return workbookData;
        },
        transformData: (workbookData) => {
          return workbookData;
        },
      },
      urlParams: {},
      isFrame: false,
    };
  },
  computed: {
    hasPrevFile() {
      return this.currentFileIndex > 0;
    },
    hasNextFile() {
      return this.currentFileIndex < this.files.length - 1;
    },
    filteredFiles() {
      if (!this.searchQuery) return this.files;
      const query = this.searchQuery.toLowerCase();
      return this.files.filter(file =>
        file.name.toLowerCase().includes(query)
      );
    },
    isIframeMode() {
      if (window.frameElement) {
        return true;
      } else {
        return false;
      }
    }
  },
  created() {
    window.showFiles = this.showFiles;
    window.addEventListener("keydown", this.handleKeydown);
    this.urlParams = this.getUrlParams(location.href);
    this.isFrame = this.isInIframe();

    if (this.urlParams["url"]) {
      // 如果地址栏直接就传递了url，直接显示文件
      this.showFiles([{ url: this.urlParams.url, name: this.urlParams.name }], 0, true);
    }
    else if (this.urlParams["_t"]) {
      // 通过缓存读取文件列表
      const json = sessionStorage.getItem(this.urlParams["_t"]);
      if (json) {
        try {
          var data = JSON.parse(json);
          if (data && data.files) {
            this.showFiles(data.files, data.index, true);
          }
        } catch { }
      }
    }
    window.showDemo = this.showDemo;
    // this.showDemo(); // 数据演示，实际可去掉这一行
  },
  beforeDestroy() {
    window.removeEventListener("keydown", this.handleKeydown);
  },
  methods: {
    componentMap(type) {
      switch (type) {
        case "docx":
          return "VueOfficeDocx";
        case "xlsx":
          // case 'xls':
          return "VueOfficeExcel";
        case "pdf":
          return "VueOfficePdf";
        case 'ofd':
          return 'PreviewIframe';
        case "pptx":
          // case 'ppt':
          return "VueOfficePptx";
        case "xml":
        case "txt":
        case "css":
          return "PreviewTxt";
        // 图像文件
        case "jpg":
        case "jpeg":
        case "bmp":
        case "gif":
        case "png":
        case "tif":
          return "PreviewImage";
        case "avi":
        case "mpg":
        case "mpeg":
        case "mov":
        case "mp4":
          return "PreviewVideo";
        case "wav":
        case "ram":
        case "mp3":
          return "PreviewSound";
        default:
          return "PreviewNotSupported";
      }
    },
    getUrlParams(url) {
      const search = new URL(url).search;
      const paramsObj = {};
      if (search.startsWith('?')) {
        search.substring(1).split('&').forEach(pair => {
          const [key, value] = pair.split('=');
          paramsObj[decodeURIComponent(key)] = decodeURIComponent(value || '');
        });
      }
      return paramsObj;
    },
    showDemo() {
      const demoFiles = [
        // 图片 jpg,jpeg,bmp,gif,png,tif
        { name: "花.jpg", url: "./files/花.jpg" },
        { name: "小草.jpeg", url: "./files/小草.jpeg" },
        { name: "风景.bmp", url: "./files/风景.bmp" },
        { name: "小猫.gif", url: "./files/小猫.gif" },
        { name: "server.png", url: "./files/server.png" },
        { name: "特殊图片2.tif", url: "./files/特殊图片2.tif" },
        // Word文档 ( doc,docx )
        { name: "租赁合同.docx", url: "./files/租赁合同.docx" },
        // Excel文档 ( xls,xlsx )
        { name: "表格.xlsx", url: "./files/表格.xlsx" },
        // PowerPoint文档 ( ppt,pptx )
        { name: "演示文稿.pptx", url: "./files/演示文稿.pptx" },
        // 文本文件
        {
          name: "子路、曾皙、冉有、公西华侍坐.txt",
          url: "./files/子路、曾皙、冉有、公西华侍坐.txt",
        },
        { name: "数据协议.xml", url: "./files/数据协议.xml" },
        { name: "index.css", url: "./files/index.css" },
        // pdf文档
        { name: "租赁文件.pdf", url: "./files/租赁文件.pdf" },
        // 发票
        { name: "发票.ofd", url: "./files/发票.ofd" },
        // 不支持的文件格式
        { name: "旧版表格.xls", url: "./files/旧版表格.xls" },
        { name: "旧版文档.doc", url: "./files/旧版文档.doc" },
      ];
      this.showFiles(demoFiles, 0);
    },
    showFiles(files, index, nocache) {
      if (!nocache) {
        sessionStorage.setItem(location.href, JSON.stringify({ files, index }));
      }
      // 参数默认值处理
      if (!Array.isArray(files)) {
        files = [files];
      }
      if (!index || isNaN(index)) {
        index = 0;
      }

      this.files = files.map((file, index) => {
        // 在文件地址栏参数，增加url上的所有参数信息（公共参数，如token）
        let fileUrl = file.url;
        let connector = fileUrl.includes("?") ? "&" : "?";
        for (const paramName in this.urlParams) {
          fileUrl += connector + paramName + '=' + encodeURIComponent(this.urlParams[paramName])
          connector = '&';
        }

        return {
          name: typeof file === "string" ? file : file.name,
          url: fileUrl,
          type: file.type || file.name.split(".").pop().toLowerCase(),
          __fileId: 'file' + index,
        };
      });

      // 如果是iframe模式，只有一个文件，则不显示工具栏
      if (this.isFrame) {
        this.showSidebar = files.length > 1;
      } else {
        // 新页面打开，始终展示工具栏
        this.showSidebar = true;
      }

      if (this.files.length > 0) {
        this.previewFile(index);
      }
    },
    isInIframe() {
      try {
        return window.self !== window.top;
      } catch (e) {
        return true; // 捕获到安全错误，说明在跨域iframe中
      }
    },
    previewFile(index) {
      const file = this.files[index];
      if (!file) {
        return;
      }
      this.currentFileIndex = index;
      const componentName = this.componentMap(file.type);
      if (componentName) {
        this.currentFile = this.getPrefviewUrl(componentName, file.type, file.url);
        this.currentComponent = componentName;
        this.unsupportedFile = null;
        this.currentFileName = file.name;
      } else {
        this.unsupportedFile = file.name;
      }
    },
    getPrefviewUrl(componentName, type, url) {
      if (componentName === "PreviewIframe" && type === 'ofd') {
        if (this.isRelativePath(url)) {
          const baseUrl = window.location.origin;
          const relativePath = this.normalizeRelativePath(url);
          const encodeUrl = encodeURIComponent(baseUrl + relativePath);
          const ofdFrame = this.normalizeRelativePath('ofdview/ofdview.html?file=');
          return ofdFrame + encodeUrl;
        }
      }
      return url;
    },
    isRelativePath(url) {
      return !url.startsWith('http://') && !url.startsWith('https://');
    },
    normalizeRelativePath(url) {
      const currentPath = window.location.pathname;
      let basePath = currentPath.substring(0, currentPath.lastIndexOf('/'));

      // 处理以/开头的绝对路径
      if (url.startsWith('/')) {
        return url;
      }

      // 移除开头的./
      if (url.startsWith('./')) {
        url = url.slice(2);
      }

      // 处理../（上级目录）的情况
      while (url.startsWith('../')) {
        url = url.slice(3);
        basePath = basePath.substring(0, basePath.lastIndexOf('/'));
      }

      // 拼接并规范化路径
      let absoluteUrl = basePath + '/' + url;

      // 规范化路径：移除多余的斜杠和点
      absoluteUrl = absoluteUrl.replace(/\/+/g, '/').replace(/\.\/+/g, '/');

      return absoluteUrl;
    },
    renderedHandler() {
      this.isLoading = false;
      console.log("渲染完成");
    },
    errorHandler() {
      this.isLoading = false;
      console.log("渲染失败");
    },
    async downloadCurrentFile() {
      try {
        const currentFileObj = this.files[this.currentFileIndex];
        if (!currentFileObj) return;

        // 如果是URL，先获取文件内容再下载
        this.downloadFromUrl(currentFileObj.url, currentFileObj.name);
      } catch (error) {
        console.error("下载文件失败:", error);
      }
    },
    downloadFromUrl(url, filename) {
      const link = document.createElement("a");
      link.href = url;
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    },
    closePreview() {
      // this.currentFile = '';
      // this.currentComponent = null;
      // this.currentFileName = '';
      // this.isLoading = false;
      try {
        window.close();
      } catch { }
    },
    handleKeydown(event) {
      if (this.showSidebar) {
        if (event.key === "ArrowLeft") {
          this.previewFile(this.currentFileIndex - 1);
        } else if (event.key === "ArrowRight") {
          this.previewFile(this.currentFileIndex + 1);
        }
      }
    },
  },
};
</script>

<style>
/* html,
body,
#app-wrapper {
  height: 100%;
  padding: 0;
  margin: 0;
} */

#app-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.header-area {
  position: fixed;
  top: 0;
  right: 0;
  left: 0;
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 50px;
  padding: 0 20px;
  background-color: #fff;
  border-bottom: 1px solid #e6e6e6;
}

.current-file-name {
  font-size: 16px;
  font-weight: bold;
  color: #303133;
}

.menu-area {
  display: flex;
  gap: 12px;
  align-items: center;
}

.download-btn {
  padding: 8px 16px;
  color: white;
  cursor: pointer;
  background-color: #409eff;
  border: none;
  border-radius: 4px;
  transition: background-color 0.3s;
}

.download-btn:hover {
  background-color: #66b1ff;
}

.menu-wrapper {
  position: relative;
}

.menu-trigger {
  padding: 8px 16px;
  color: #303133;
  cursor: pointer;
  background-color: #f5f7fa;
  border-radius: 4px;
}

.menu-wrapper:hover .file-list {
  display: block;
}

.file-list {
  position: absolute;
  top: 100%;
  right: 0;
  z-index: 1000;
  display: none;
  width: 250px;
  min-width: 200px;
  max-width: 50vw;
  max-height: 500px;
  padding: 0;
  margin: 0;
  overflow-y: auto;
  list-style: none;
  scrollbar-color: #f5f7fa #f5f7fa;
  scrollbar-width: thin;
  background-color: #fff;
  border: 1px solid #e6e6e6;
  border-radius: 4px;
  box-shadow: 0 2px 12px 0 rgb(0 0 0 / 10%);
  transition: scrollbar-color 0.3s;
}

.search-input {
  box-sizing: border-box;
  width: 100%;
  padding: 6px 8px;
  font-size: 14px;
  color: #606266;
  border: 1px solid #dcdfe6;
  border-radius: 4px;
  transition: border-color 0.3s;
}

.search-input:focus {
  outline: none;
  border-color: #409eff;
}

.search-input::placeholder {
  color: #c0c4cc;
}

.menu-wrapper:hover .file-list {
  scrollbar-color: #409eff #f5f7fa !important;
}

.file-list::-webkit-scrollbar {
  width: 6px;
}

.file-list::-webkit-scrollbar-track {
  background: #f5f7fa;
}

.file-list::-webkit-scrollbar-thumb {
  background-color: transparent;
  border-radius: 3px;
  transition: background-color 0.3s;
}

.file-list:hover::-webkit-scrollbar-thumb {
  background-color: #409eff;
}

.file-list li {
  box-sizing: border-box;
  width: 100%;
  padding: 8px 16px;
  overflow: hidden;
  text-overflow: ellipsis;
  color: #303133;
  white-space: nowrap;
  cursor: pointer;
  transition: all 0.3s;
}

.file-list li:hover {
  background-color: #f5f7fa;
}

.file-list li.active {
  color: #409eff;
  background-color: #ecf5ff;
}

.main-content {
  position: relative;
  flex: 1;
  min-height: 0;
  margin-top: 0;
}

.main-content[v-show-sidebar="true"] {
  margin-top: 50px;
}

.nav-buttons {
  position: fixed;
  top: 50%;
  right: 0;
  left: 0;
  z-index: 100;
  display: flex;
  justify-content: space-between;
  padding: 0 20px;
  pointer-events: none;
  transform: translateY(-50%);

  /* 添加左右内边距 */
}

.nav-btn {
  position: relative;
  top: 0%;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 60px;
  color: white;
  pointer-events: auto;
  cursor: pointer;
  background: rgb(0 0 0 / 30%);
  border: none;
  border-radius: 4px;
  transform: translateY(-50%);
  transition: all 0.3s;
}

.nav-btn:hover {
  background: rgb(0 0 0 / 50%);
}

.nav-btn:disabled {
  cursor: not-allowed;
  opacity: 0.3;
}

.nav-btn .arrow {
  font-size: 24px;
  line-height: 1;
}

.loading-indicator {
  position: absolute;
  top: 50%;
  left: 50%;
  font-size: 18px;
  color: #409eff;
  transform: translate(-50%, -50%);
}

.unsupported-tip {
  padding: 40px;
  font-size: 18px;
  color: #d32f2f;
  text-align: center;
}

.nav-btn {
  position: relative;
}

.nav-btn::before {
  position: absolute;
  top: 50%;
  left: 50%;
  width: 20px;
  height: 20px;
  content: "";
  background-repeat: no-repeat;
  background-position: center;
  background-size: contain;
  transform: translate(-50%, -50%);
}
</style>
