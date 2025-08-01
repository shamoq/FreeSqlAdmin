<script setup lang="ts">
import { useMessage, NCard, NSpace, NButton, NButtonGroup, NSpin, NTree, NForm, NFormItem, NInput, NSelect, useDialog, NDropdown } from 'naive-ui';
import { ref, computed, onMounted, h } from 'vue';
import { IconifyIcon } from "@vben/icons";
import { Page } from "@vben/common-ui";
import { useRoute, useRouter } from 'vue-router';
import { requestClient } from "#/api/request";
import { convertToArray } from '#/utils/tree';
import { useUniverEditor } from '#/utils/editor/univer'
import TemplateEditor from '#/components/BaseEditor/index.vue';
import TemplatePreview from '#/components/BaseEditor/preview.vue';
import { downloadFile } from '#/utils/file'
import { useAccessStore } from '@vben/stores';
import { useAccess } from '@vben/access';
import { getUniverUrl } from '#/utils/frame';

const route = useRoute();
const router = useRouter();
const message = useMessage();
const { hasAccessByCodes } = useAccess();
const dialog = useDialog();
const accessStore = useAccessStore();
const activeTab = ref('template');
const loading = ref(true);
const univerEditor = useUniverEditor();
const templatePreviewRef = ref();
let firstFields = null; // 首次加载时的业务字段

const isReadOnly = computed(() => route.params.type == 'view');

const handleIframeLoad = (evt) => {
    loading.value = false;
    univerEditor.init(evt.target, { businessId: route.params.id, type: 'excel' })
        .then((isNew) => {
            if (isNew && firstFields) {
                univerEditor.createTable('合同信息', firstFields);
            }
        });
}

const handleSave = async (showMessage) => {
    // 验证模板基本信息
    await formRef.value?.validate();

    // 保存Excel数据
    await univerEditor.save({
        businessId: route.params.id,
        type: 'excel',
    });

    // 保存模板编辑器信息
    await editorRef.value.saveContent();

    // 自定义字段
    const fullData = univerEditor.getTableFullData();

    // 保存模板基本信息
    await requestClient.post(`/customtemplate/saveDetail`, {
        ...formData.value,
        fields: convertToArray(fullData.fields),
        data: JSON.stringify(fullData.sheetData),
    });

    if (showMessage) {
        message.success('保存成功');
    }

}

const checkData = () => {
    const firstColumnNames = univerEditor.getTableColumnData(0, 0);

    // 检查数据是否为空
    if (!firstColumnNames || firstColumnNames.length === 0) {
        message.warning('请先添加数据');
        return;
    }

    // 检查空行
    const emptyRowIndex = firstColumnNames.findIndex(name => !name);
    if (emptyRowIndex !== -1) {
        message.warning(`第${emptyRowIndex + 2}行数据为空`);
        return;
    }

    // 检查重复行
    const duplicateMap = new Map();
    for (let i = 0; i < firstColumnNames.length; i++) {
        const name = firstColumnNames[i];
        if (duplicateMap.has(name)) {
            message.warning(`第${duplicateMap.get(name) + 2}行与第${i + 2}行数据重复，${name}`);
            return;
        }
        duplicateMap.set(name, i);
    }

    return firstColumnNames;
}

const handleDelete = async () => {
    await requestClient.post(`/customtemplate/delete`, { id: route.params.id });
    message.success('删除成功');
    router.back();
}

const handleTabChange = (tab: string) => {
    if (tab == 'preview') {
        const firstColumnNames = checkData();
        if (!firstColumnNames) {
            return;
        }

        handleSave().then(() => {

            activeTab.value = tab;
            const files = firstColumnNames.map((item, index) => {
                return {
                    type: 'docx',
                    name: item + ".docx",
                    url: '/api/CustomTemplate/PreviewDocx?id=' + route.params.id + "&index=" + index + "&_t=" + new Date().getTime(),
                }
            });
            templatePreviewRef.value.showFiles(files);
        });
    } else if (tab == 'template') {
        activeTab.value = tab;
        treeData.value = univerEditor.getHeaderFields();
    } else {
        activeTab.value = tab;
    }
}

// 模板tab页面
const formRef = ref();
const treeData = ref([]);
const editorRef = ref();

const handleAddDocumentTable = (option) => {
    var tableOption = {
        tableId: option.fullCode,
        headers: option.children.map(t => {
            return { code: t.fullCode, name: t.name };
        })
    }
    editorRef.value.addDocumentTable(tableOption);
}

const renderSuffix = ({ option }: { option: any }) => {
    if (isReadOnly.value) {
        return
    }
    if (option.groupType == 'field' && option.parentGroupType == 'form') {
        return h(
            NButton,
            {
                text: true,
                onClick: (e) => {
                    if (e.type === 'click') {
                        editorRef.value.addDocumentField(option);
                    }
                }
            },
            { default: () => [h(IconifyIcon, { icon: 'material-symbols:add-circle-outline' }), '插入字段'] }
        );
    } else if (option.groupType == 'table') {
        return h(
            NButton,
            {
                text: true,
                onClick: (e) => {
                    if (e.type === 'click') {
                        handleAddDocumentTable(option);
                    }
                }
            },
            { default: () => [h(IconifyIcon, { icon: 'material-symbols:add-circle-outline' }), '插入表格'] }
        );
    }
}

const formData = ref({
    id: null,
    name: '模板名称',
    remark: null,
    templateType: null,
})

const fetchTemplateDetail = async () => {
    const id = route.params.id;
    const data = await requestClient.post(`/customtemplate/getDetail`, { id });
    formData.value.id = data.id;
    formData.value.name = data.name;
    formData.value.remark = data.remark;
    formData.value.templateType = data.templateType;

    const tableFields = data.fields.filter(t => t.parentGroupType === 'form').map(t => { return t.name });
    if (!univerEditor.createTable('合同信息', [tableFields])) {
        firstFields = [tableFields];
    }
    return data;
}

const templateTypeOptions = ref([]);

async function fetchTemplateTypeOptions() {
    try {
        const data = await requestClient.post('/option/GetEnums', { type: 'TemplateTypeEnum' });
        templateTypeOptions.value = data;
    } catch (error) {
        message.error('获取模板类型选项失败');
    }
}

const rules = {
    name: {
        required: true,
        message: '请输入模板名称',
        trigger: 'blur',
    },
};


onMounted(async () => {
    fetchTemplateTypeOptions();
    const id = route.params.id;
    await fetchTemplateDetail();
});

async function handleDownloadTemplate() {
    const { id, fileId, fileName } = await requestClient.post('/TemplateVersion/GetOfficeTemplate', { id: route.params.id });
    if (!id || !fileId) {
        message.error('未上传模版文件');
        return;
    }
    downloadFile('/api/file/download?id=' + fileId, formData.value.name + '.docx');
}

const renderLabel = ({ option }: { option: any }) => {
    return h(
        'div',
        { class: 'flex items-center' },
        [
            h(IconifyIcon, { icon: getFieldTypeIcon(option), class: 'mr-1' }),
            option.label,// + ' (' + option.key + ')',
            option.required && h('span', { class: 'text-red-500 ml-1' }, '*'),
        ]
    );
};

const getFieldTypeIcon = (option) => {
    if (option.groupType == 'form') {
        return 'material-symbols:table-chart';
    } else if (option.groupType == 'table') {
        return 'material-symbols:table-rows';
    }
    // 根据字段类型返回对应的图标
    switch (option.fieldTypeEnum) {
        case 1: // 文本类型
            return 'material-symbols:text-fields';
        case 2: // 数值类型
            return 'material-symbols:numbers';
        case 3: // 金额类型
            return 'material-symbols:payments';
        case 4: // 选项类型
            return 'material-symbols:list';
        case 5: // 日期类型
            return 'material-symbols:calendar-month';
        case 6: // 时间类型
            return 'material-symbols:schedule';
        default:
            return 'material-symbols:text-fields';
    }
};

const moreOptions = computed(() => [
    {
        label: '导出所有文件(合并)',
        key: 'downloadMerege',
        show: hasAccessByCodes(['custom:export']),
    },
    {
        label: '导出所有文件(不合并)',
        key: 'downloadNoMerege',
        show: hasAccessByCodes(['custom:export']),
    },
    {
        label: '下载模板',
        key: 'download',
        show: hasAccessByCodes(['custom:download']),
    },
    {
        label: '删除',
        key: 'delete',
        show: hasAccessByCodes(['custom:del']),
    },
    {
        label: '返回',
        key: 'back',
        show: true,
    },
].filter(t=>t.show));

const handleMoreSelect = async (key) => {
    if (key == 'back') {
        router.go(-1);
    } else if (key == 'download') {
        await handleDownloadTemplate();
    } else if (key == 'delete') {
        dialog.warning({
            title: '警告',
            content: '确定要删除该合同吗？',
            positiveText: '确定',
            negativeText: '取消',
            onPositiveClick: async () => {
                await handleDelete();
            }
        });
    } else if (key == 'downloadMerege') {
        if (checkData()) {
            await handleSave(false);
            downloadFile('/api/customtemplate/DownloadAll?isMerege=1&id=' + route.params.id, formData.value.name);
        }
    } else if (key == 'downloadNoMerege') {
        if (checkData()) {
            await handleSave(false);
            downloadFile('/api/customtemplate/DownloadAll?isMerege=0&id=' + route.params.id, formData.value.name);
        }
    }
};
</script>

<template>
    <Page>
        <NCard>
            <template #header>
                <div class="grid grid-cols-3 items-center">
                    <div class="text-lg font-medium"> {{ formData.name }} </div>
                    <div class="flex justify-center">
                        <NButtonGroup>
                            <NButton :type="activeTab == 'template' ? 'primary' : 'default'"
                                @click="handleTabChange('template')">模板
                            </NButton>
                            <NButton :type="activeTab == 'data' ? 'primary' : 'default'"
                                @click="handleTabChange('data')">数据
                            </NButton>
                            <NButton :type="activeTab == 'preview' ? 'primary' : 'default'"
                                @click="handleTabChange('preview')">
                                预览
                            </NButton>
                        </NButtonGroup>
                    </div>
                    <div class="flex justify-end">
                        <NSpace>
                            <NButton type="primary" @click="handleSave" v-access:code="['custom:add','custom:edit']">保存</NButton>
                            <NDropdown trigger="hover" :options="moreOptions" @select="handleMoreSelect">
                                <NButton>更多操作
                                    <IconifyIcon icon="material-symbols:keyboard-arrow-down" />
                                </NButton>
                            </NDropdown>
                        </NSpace>
                    </div>
                </div>
            </template>

            <div class="flex relative" style="min-height: calc(100vh - 200px)">
                <NSpin :show="loading && activeTab === 'data'" description="正在加载编辑器..." class="absolute inset-0 z-0">
                    <iframe v-show="activeTab == 'data'" :src="getUniverUrl()" class="w-full h-full"
                        style="min-height: calc(100vh - 200px)" @load="handleIframeLoad" />
                </NSpin>
                <div v-show="activeTab == 'template'" class="flex w-full z-10" style="min-height: calc(100vh - 200px)">
                    <div class="w-[70%] bg-gray-100">
                        <TemplateEditor ref="editorRef" :type="formData.templateType" category="Custom"/>
                    </div>
                    <div class="w-[30%] pl-4">
                        <NForm ref="formRef" :model="formData" :rules="rules" label-placement="left" label-width="100"
                            require-mark-placement="right-hanging">
                            <NFormItem label="模板名称" path="name">
                                <template v-if="isReadOnly">
                                    <div class="form-text">{{ formData.name || '-' }}</div>
                                </template>
                                <NInput v-else v-model:value="formData.name" placeholder="请输入模板名称" />
                            </NFormItem>
                            <NFormItem label="模板类型" path="templateType">
                                <template v-if="isReadOnly">
                                    <div class="form-text">{{ templateTypeOptions.find(t => t.value ===
                        formData.templateType)?.label || '-' }}</div>
                                </template>
                                <NSelect v-else v-model:value="formData.templateType" :options="templateTypeOptions"
                                    placeholder="请选择模板类型" />
                            </NFormItem>
                            <NFormItem label="可用字段" class="w-full">
                                <div class="h-[calc(100vh-400px)] overflow-auto w-full" style="margin-top:4px;">
                                    <NTree :data="treeData" :accordion="true" block-line :selectable="false"
                                        :render-suffix="renderSuffix" :render-label="renderLabel" />
                                </div>
                            </NFormItem>
                        </NForm>
                    </div>
                </div>
                <div v-show="activeTab == 'preview'" class="h-full w-full  z-10"
                    style="min-height: calc(100vh - 200px)">
                    <TemplatePreview ref="templatePreviewRef" class="h-full w-full"
                        style="min-height: calc(100vh - 200px)" />
                </div>
            </div>
        </NCard>
    </Page>
</template>

<style scoped>
.form-text {
    line-height: 34px;
    color: var(--n-text-color);
}
</style>
