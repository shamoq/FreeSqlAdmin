<template>
  <div class="preview-not-supported">
    <div class="content">
      <div class="icon">
        <i class="fas fa-file"></i>
      </div>
      <div class="file-name" :title="fileName">{{ fileName }}</div>
      <div class="message">暂不支持该类型文件的预览</div>
      <div class="suggestion">请下载后查看</div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'PreviewNotSupported',
  props: {
    fileName: {
      type: String,
      required: true
    }
  }
}
</script>

<style scoped>
.preview-not-supported {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100%;
  background-color: #f5f6f7;
}

.content {
  text-align: center;
  padding: 2rem;
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

.icon {
  font-size: 4rem;
  color: #909399;
  margin-bottom: 1rem;
}

.file-name {
  font-size: 1.1rem;
  color: #303133;
  margin-bottom: 1rem;
  max-width: 300px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.message {
  font-size: 1rem;
  color: #606266;
  margin-bottom: 0.5rem;
}

.suggestion {
  font-size: 0.9rem;
  color: #909399;
}
</style>