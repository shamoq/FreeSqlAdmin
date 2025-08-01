<script setup lang="ts">
import type { UploadFileInfo } from 'naive-ui'
import { NUpload, NButton, NText, useMessage, NModal, NSpace } from 'naive-ui';
import { ref, computed, watch, defineExpose } from 'vue';
import { useAccessStore } from '@vben/stores';
import { getFileUrl } from '#/utils/frame'

interface Props {
    title?: string;
    accept?: string[];  // 接受的文件类型
    maxCount?: number;  // 最大文件数量
    uploadUrl?: string; // 上传地址
    mode?: string; // 打开模式
    headers?: Record<string, string>; // 添加自定义请求头属性
    uploadSuccess?: Function, // dialog模式下下上传成功回调
    isReadOnly?: boolean; // 是否只读模式
    bucketName?: string; // 存储桶
}

const props = withDefaults(defineProps<Props & {
    value?: string //{ documentId: string; name: string; type: string | null | undefined }[];
}>(), {
    title: '文件上传', //
    accept: () => ['doc', 'docx', 'pdf', 'txt', 'jpg', 'jpeg', 'png', 'gif', 'xls', 'xlsx', 'zip', 'rar'],
    maxCount: 20,
    uploadUrl: '/api/file/upload',
    headers: () => ({}), // 默认为空对象
    value: () => null,
    mode: () => null,
    isReadOnly: false, // 默认非只读
});

const emit = defineEmits<{
    'update:value': [value: { documentId: string; name: string; type: string | null | undefined }[]];
    'on-success': [result: Array<{ documentId: string; name: string; type: string | null | undefined } | null> | null | undefined];
}>();

const message = useMessage();
const loading = ref(false);
const fileList = ref<UploadFileInfo[]>([]);
const accessStore = useAccessStore();
const dialogMode = ref(props.mode === 'dialog');
const modalShow = ref(false);

// 存储文件ID的映射
const fileIdMap = ref(new Map<string, string>());

// 监听value变化并更新文件列表
watch(() => props.value, (newValue) => {
    let arr = newValue ? JSON.parse(newValue) : [];
    // 清空现有数据
    fileList.value = [];
    fileIdMap.value.clear();

    // 添加新数据
    arr.forEach((item) => {
        if (item && item.documentId && item.name) {
            const fileInfo: UploadFileInfo = {
                id: item.documentId,
                name: item.name,
                status: 'finished',
                type: item.type || undefined
            };
            fileList.value.push(fileInfo);
            fileIdMap.value.set(fileInfo.id, item.documentId);
        }
    });
}, { immediate: true });

const acceptString = computed(() => {
    return props.accept.map(type => `.${type}`).join(',');
});

// 合并默认请求头和自定义请求头
const requestHeaders = computed(() => {
    // 获取认证token
    const token = accessStore.accessToken;
    const authHeaders = token ? {
        'Authorization': token
    } : {};

    // 合并自定义请求头
    return {
        ...authHeaders,
        ...props.headers
    };
});

// 处理上传完成
const handleFinish = ({ file, event }: { file: UploadFileInfo, event: ProgressEvent }) => {
    try {
        // 从 event.target 中获取响应
        const xhr = event.target as XMLHttpRequest;
        let fileId = null;

        if (xhr && xhr.responseText) {
            // 尝试解析 JSON 响应
            const response = JSON.parse(xhr.responseText);
            if (response.data) {
                fileId = response.data;
                fileIdMap.value.set(file.id, fileId);
                // 更新父组件的value
                const result = updateValue();
                triggerFormUploadSuccess(result);

            }

        }
    } catch (err) {
        console.error('Error handling finish:', err);
        message.error('处理响应失败');
    }
};

// 处理文件状态变化
const handleChange = ({ file }: { file: UploadFileInfo }) => {
    if (file.status === 'removed') {
        fileIdMap.value.delete(file.id);
        var result = updateValue();
        triggerFormUploadSuccess(result);
    }
};

const triggerFormUploadSuccess = (result) => {
    // dialog模式，点击确定后触发回调
    // 表单模式，上传则触发回调
    if (props.mode != 'dialog' && props.uploadSuccess) {
        props.uploadSuccess(result)
    }
}

const updateValue = () => {
    // 更新v-model绑定的值
    const fileInfos = fileList.value
        .map(f => {
            const fileId = fileIdMap.value.get(f.id);
            if (!fileId) return null;
            return {
                documentId: fileId,
                name: f.name,
            };
        }).filter(Boolean);

    var value = JSON.stringify(fileInfos)
    emit('update:value', value);
    return fileInfos;
}

const handlePreview = (file: UploadFileInfo) => {
    const fileId = fileIdMap.value.get(file.id);
    const token = accessStore.accessToken as String;
    const files = fileList.value.map(f => ({
        fileId: fileIdMap.value.get(f.id),
        name: f.name,
        url: '/api/file/download?id=' + f.id
    })).filter(f => f.fileId);
    // 获取当前文件的索引
    var _t = Date.now();
    const index = files.findIndex(f => f.fileId === fileId);
    var search = '?_t=' + _t + '&token=' + encodeURIComponent(token);
    const previewPage = getFileUrl() + search;
    sessionStorage.setItem(_t, JSON.stringify({ files, index }))
    window.open(previewPage, '_blank');
};

// 处理下载
const handleDownload = (file: UploadFileInfo) => {
    const fileId = fileIdMap.value.get(file.id);
    http://localhost:5888/api/file/download?id=40beca46-c8fe-412e-bb14-872c2d9ed792&token=Bearer%20eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJhcHAiLCJpc3MiOiJhcHAiLCJleHAiOjE3NDIwMjI3MDksInVzZXJDb2RlIjoiYWRtaW4iLCJ1c2VyTmFtZSI6ImFkbWluIiwiaWQiOiJlYWJjYWMwOS1mMDMzLTExZWYtYWQwOS0wMjQyYWMxNjAwMDIiLCJpYXQiOjE3NDE5NTA3MDksIm5iZiI6MTc0MTk1MDcwOSwicm9sZUlkIjoiIiwiT3Jnbml6YXRpb25JZCI6IjExMTExMTExLTExMTEtMTExMS0xMTExLTExMTExMTExMTExMSIsIklzQWRtaW4iOiIxIn0.5aEMEHfn4HuMp8BubtiNjBGNW9IvPZXvcH9xaJKq79A
    if (!fileId) {
        message.error('文件标识不存在');
        return;
    }

    // 获取认证token
    const token = accessStore.accessToken;

    // 创建下载链接
    const link = document.createElement('a');
    link.href = `/api/file/download?id=${fileId}&token=${token || ''}`;
    link.download = file.name;
    document.body.appendChild(link);
    link.click();

    // 清理DOM
    document.body.removeChild(link);
};

const handleClose = () => {
    fileList.value = [];
    fileIdMap.value.clear();
    modalShow.value = false;
};

// 确认上传
const handleOK = async () => {
    if (fileList.value.length === 0) {
        message.warning('请选择文件');
        return;
    }

    const unfinishedFile = fileList.value.find(file => file.status !== 'finished');
    if (unfinishedFile) {
        message.warning('请等待文件上传完成');
        return;
    }
    try {
        loading.value = true;
        if (props.uploadSuccess) {
            await props.uploadSuccess(updateValue())
        }
        modalShow.value = false;
    } catch {

    } finally {
        loading.value = false;
    }
}

const getData = ({ file }: { file: UploadFileInfo }) => {
    return {
        fileName: file.name,
        type: file.type,
        bucketName: props.bucketName || null,
    }
}

const show = () => {
    modalShow.value = true;
    loading.value = false;
    fileList.value = [];
    fileIdMap.value.clear();
}

defineExpose({
    show
});
</script>

<template>
    <div class="upload-container w-full">
        <NUpload v-if="!dialogMode" v-model:file-list="fileList" :max="maxCount" :accept="acceptString"
            :action="props.uploadUrl" :data="getData" :headers="requestHeaders" @finish="handleFinish"
            @change="handleChange" :show-file-list="true" show-download-button @download="handleDownload"
            @preview="handlePreview" :auto-upload="true" :show-remove-button="!props.isReadOnly"
            class="upload-component" :class="{ 'readonly-mode': props.isReadOnly }">
            <NButton v-if="!props.isReadOnly">选择文件</NButton>
            <NText depth="3" class="upload-tip" v-if="!props.isReadOnly">
                支持扩展名：{{ props.accept.join(', ') }}
            </NText>
        </NUpload>
        <NModal v-else v-model:show="modalShow" :title="title" preset="dialog" @close="handleClose"
            style="width: 600px">
            <NUpload v-model:file-list="fileList" :max="maxCount" :accept="acceptString" :action="props.uploadUrl"
                :data="getData" :headers="requestHeaders" @finish="handleFinish" @change="handleChange"
                :show-file-list="true" show-download-button @download="handleDownload" :auto-upload="true"
                @preview="handlePreview" :show-remove-button="!props.isReadOnly" class="upload-component"
                :class="{ 'readonly-mode': props.isReadOnly }">
                <NButton v-if="!props.isReadOnly">选择文件</NButton>
                <NText depth="3" class="upload-tip" v-if="!props.isReadOnly">
                    支持扩展名：{{ props.accept.join(', ') }}
                </NText>
            </NUpload>
            <template #action>
                <NSpace justify="end">
                    <NButton @click="handleClose">取消</NButton>
                    <NButton type="primary" @click="handleOK" :loading="loading">确定</NButton>
                </NSpace>
            </template>
        </NModal>
    </div>
</template>

<style scoped>
.upload-container {}

.upload-tip {
    margin-left: 12px;
    color: var(--n-text-color-3);
}

.upload-component.readonly-mode {
    margin-top: -12px;
}

.upload-component.readonly-mode :deep(.n-upload-trigger) {
    display: none;
}

/* 添加文件列表项的悬停样式 */
.upload-component :deep(.n-upload-file-info__name) {
    cursor: pointer;
    transition: all 0.3s ease;
}

.upload-component :deep(.n-upload-file-info__name:hover) {
    text-decoration: underline;
}
</style>
