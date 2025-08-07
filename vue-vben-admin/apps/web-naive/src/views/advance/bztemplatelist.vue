<script setup lang="ts">
import { ref, onMounted, computed, defineExpose, h } from 'vue';
import { NCard, NSpace, NTabs, NTabPane, NModal, NButton, NForm, NFormItem, NInput, NSelect, NDropdown } from 'naive-ui';
import { requestClient } from "#/api/request";
import { Page } from "@vben/common-ui";
import { useMessage, useDialog } from 'naive-ui';
import { IconifyIcon } from "@vben/icons";
import FileUpload from '#/components/Common/fileupload.vue';
import type { BzTemplateDto } from '#/api/dto/BzTemplateDto';
import type { BzTemplateItemDto } from '#/api/dto/BzTemplateItemDto';

const emit = defineEmits(['success']);

// 定义响应式数据
const templateData = ref<BzTemplateDto>({ types: [], files: [] });
const currentType = ref('all');
const showModal = ref(false);
let requestParams = null;
const message = useMessage();
const showTypeModal = ref(false);
const typeModalTitle = ref('');
const typeFormRef = ref();
const typeForm = ref({
  name: '',
  oldName: ''
});

const typeRules = {
  name: {
    required: true,
    message: '请输入分类名称',
    trigger: 'blur'
  }
};

const dialog = useDialog();
const showDocModal = ref(false);
const docModalTitle = ref('');
const docFormRef = ref();
const docForm = ref<BzTemplateItemDto>({
  id: '',
  name: '',
  type: null,
  remark: '',
  files: []
});

const docRules = {
  id: {
    required: true,
    message: '请上传文件',
    trigger: 'blur',
    validator: (rule, value) => {
      if (docForm.value.id) {
        return true;
      }
      return false;
    }
  },
  name: {
    required: true,
    message: '请输入文档名称',
    trigger: 'blur'
  },
  type: {
    required: true,
    message: '请选择分类',
    trigger: 'change'
  }
};

// 添加选中状态
const selectedDoc = ref<string | null>(null);

// 获取模板数据
async function fetchTemplateData() {
  try {
    const data = await requestClient.post<BzTemplateDto>('/bzTemplate/GetList', {});
    templateData.value = data;
  } catch (error) {
    console.error('获取模板数据失败:', error);
  }
}

// 根据分类筛选文件
const filteredFiles = computed(() => {
  if (currentType.value === 'all') {
    return templateData.value.files;
  }
  return templateData.value.files.filter(file => file.type === currentType.value);
});

// 计算每个分类的数量
const typeCount = computed(() => {
  const counts = {};
  templateData.value.files.forEach(file => {
    counts[file.type] = (counts[file.type] || 0) + 1;
  });
  return counts;
});

// 计算总数量
const totalCount = computed(() => templateData.value.files.length);

// 显示弹窗方法
function show(params?) {
  requestParams = params;
  showModal.value = true;
  fetchTemplateData();
}

// 添加分类
const handleAddType = () => {
  typeModalTitle.value = '新增分类';
  typeForm.value = {
    name: '',
    oldName: ''
  };
  showTypeModal.value = true;
};

// 编辑分类
const handleEditType = (type: string) => {
  typeModalTitle.value = '编辑分类';
  typeForm.value = {
    name: type,
    oldName: type
  };
  showTypeModal.value = true;
};

// 删除分类
const handleDeleteType = async (type: string) => {
  try {
    await dialog.warning({
      title: '确认删除',
      content: `确定要删除分类"${type}"吗？删除后该分类下的模板将变为未分类状态。`,
      positiveText: '确定',
      negativeText: '取消',
      onPositiveClick: async () => {
        // 更新模板数据，将删除的分类下的模板type设为null
        templateData.value.files = templateData.value.files.map(item => {
          if (item.type === type) {
            return { ...item, type: null };
          }
          return item;
        });

        // 更新分类列表
        templateData.value.types = templateData.value.types.filter(t => t !== type);

        // 如果当前选中的是被删除的分类，则切换到全部
        if (currentType.value === type) {
          currentType.value = 'all';
        }

        await handleSave();
        message.success('删除成功');
      }
    });
  } catch (error) {
    console.error('删除分类失败:', error);
  }
};

// 确认分类对话框
const handleTypeModalConfirm = async () => {
  try {
    await typeFormRef.value?.validate();
    
    if (typeForm.value.oldName) {
      // 编辑分类
      // 更新本地数据
      templateData.value.types = templateData.value.types.map(t =>
        t === typeForm.value.oldName ? typeForm.value.name : t
      );

      // 更新模板数据
      templateData.value.files = templateData.value.files.map(item => {
        if (item.type === typeForm.value.oldName) {
          return { ...item, type: typeForm.value.name };
        }
        return item;
      });

      await handleSave();
      message.success('编辑成功');
    } else {
      // 新增分类
      // 更新本地数据
      templateData.value.types.push(typeForm.value.name);

      await handleSave();
      message.success('添加成功');
    }

    showTypeModal.value = false;
  } catch (error) {
    // 验证失败时不关闭对话框
    return false;
  }
};

// 添加文档
const handleAddDoc = () => {
  docModalTitle.value = '新增文档';
  docForm.value = {
    id: '',
    name: '',
    type: null,
    remark: ''
  };
  showDocModal.value = true;
};

// 编辑文档
const handleEditDoc = (doc: BzTemplateItemDto) => {
  docModalTitle.value = '编辑文档';
  docForm.value = { ...doc };
  showDocModal.value = true;
};

// 删除文档
const handleDeleteDoc = async (doc: BzTemplateItemDto) => {
  try {
    await dialog.warning({
      title: '确认删除',
      content: `确定要删除文档"${doc.name}"吗？`,
      positiveText: '确定',
      negativeText: '取消',
      onPositiveClick: async () => {
        // 更新本地数据
        templateData.value.files = templateData.value.files.filter(item => item.id !== doc.id);
        await handleSave();
        message.success('删除成功');
      }
    });
  } catch (error) {
    console.error('删除文档失败:', error);
  }
};

// 确认文档对话框
const handleDocModalConfirm = async () => {
  try {
    await docFormRef.value?.validate();
    
    const existingIndex = templateData.value.files.findIndex(item => item.id === docForm.value.id);
    
    if (existingIndex > -1) {
      // 更新已存在的文档
      templateData.value.files[existingIndex] = { ...docForm.value };
    } else {
      // 插入新文档
      templateData.value.files.push({ ...docForm.value });
    }
    
    await handleSave();
    message.success(existingIndex > -1 ? '编辑成功' : '添加成功');
    showDocModal.value = false;
  } catch (error) {
    // 验证失败时不关闭对话框
    return false;
  }
};

// 分类下拉菜单选项
const getTypeDropdownOptions = (type: string) => [
  {
    label: '编辑分类',
    key: 'edit',
    icon: () => h(IconifyIcon, { icon: 'material-symbols:edit', class: 'text-14' })
  },
  {
    label: '删除分类',
    key: 'delete',
    icon: () => h(IconifyIcon, { icon: 'material-symbols:delete', class: 'text-14' })
  }
];

// 处理分类下拉菜单选择
const handleTypeDropdownSelect = (key: string, type: string) => {
  if (key === 'edit') {
    handleEditType(type);
  } else if (key === 'delete') {
    handleDeleteType(type);
  }
};

// 处理文档选中
const handleDocSelect = (docId: string) => {
  selectedDoc.value = selectedDoc.value === docId ? null : docId;
};

const handleUploadSuccess = async (fileInfoList: any) => {
  if (!fileInfoList.length) {
    return
  }
  var fileInfo = fileInfoList[0];

  if (!fileInfo || !fileInfo.documentId) {
    message.error('文件上传失败，未获取到文件ID');
    return;
  }

  docForm.value.name = fileInfo.name;
  docForm.value.id = fileInfo.documentId

}

const handleSave = async () => {
  await requestClient.post('/BzTemplate/Save', templateData.value);
}

// 初始化数据
onMounted(() => {
  fetchTemplateData();
});

defineExpose({
  show
});
</script>

<template>
  <Page title="范本合同管理" auto-content-height>

    <NCard class="flex-1 overflow-hidden">

      <div class="flex items-center justify-between mb-4">
        <div class="flex-1">
          <NTabs v-model:value="currentType" type="line" animated>
            <NTabPane name="all" :tab="`全部 (${totalCount})`" />
            <NTabPane v-for="type in templateData.types" :key="type" :name="type">
              <template #tab>
                <div class="flex items-center gap-2">
                  <span>{{ type }} ({{ typeCount[type] || 0 }})</span>
                  <NDropdown :options="getTypeDropdownOptions(type)"
                    @select="(key) => handleTypeDropdownSelect(key, type)" trigger="click">
                    <NButton quaternary circle size="tiny">
                      <template #icon>
                        <IconifyIcon icon="material-symbols:more-horiz" class="text-14" />
                      </template>
                    </NButton>
                  </NDropdown>
                </div>
              </template>
            </NTabPane>
            <template #suffix>
              <div class="flex items-center gap-2">
                <NSpace>
                  <NButton type="primary" @click="handleAddType">
                    <template #icon>
                      <IconifyIcon icon="material-symbols:add" class="text-16" />
                    </template>
                    新增分类
                  </NButton>
                  <NButton type="primary" @click="handleAddDoc">
                    <template #icon>
                      <IconifyIcon icon="material-symbols:add" class="text-16" />
                    </template>
                    新增文档
                  </NButton>
                </NSpace>
              </div>
            </template>
          </NTabs>
        </div>
      </div>

      <!-- 分类管理对话框 -->
      <NModal v-model:show="showTypeModal" :title="typeModalTitle" preset="dialog" positive-text="确定" negative-text="取消"
        @positive-click="handleTypeModalConfirm" :mask-closable="false">
        <NForm ref="typeFormRef" :model="typeForm" :rules="typeRules">
          <NFormItem label="分类名称" path="name">
            <NInput v-model:value="typeForm.name" placeholder="请输入分类名称" />
          </NFormItem>
        </NForm>
      </NModal>

      <!-- 文档管理对话框 -->
      <NModal v-model:show="showDocModal" :title="docModalTitle" preset="dialog" positive-text="确定" negative-text="取消"
        @positive-click="handleDocModalConfirm" :mask-closable="false">
        <NForm ref="docFormRef" :model="docForm" :rules="docRules">
          <NFormItem label="" path="id">
            <FileUpload v-model:value="docForm.files" :accept="['docx']" :maxCount="1" bucketName="bztemplate"
              :uploadSuccess="handleUploadSuccess" />
          </NFormItem>
          <NFormItem label="文档名称" path="name">
            <NInput v-model:value="docForm.name" placeholder="请输入文档名称" />
          </NFormItem>
          <NFormItem label="所属分类" path="type">
            <NSelect v-model:value="docForm.type"
              :options="templateData.types.map(type => ({ label: type, value: type }))" placeholder="请选择分类" />
          </NFormItem>
          <NFormItem label="备注说明" path="remark">
            <NInput v-model:value="docForm.remark" type="textarea" placeholder="请输入备注说明" />
          </NFormItem>
        </NForm>
      </NModal>

      <div class="mt-4 overflow-y-auto">
        <div class="grid grid-cols-2 gap-4" style="padding: 5px;">
          <NCard v-for="file in filteredFiles" :key="file.id"
            class="relative group overflow-hidden cursor-pointer transition-all duration-200 custom-card" :class="{
              'is-selected': selectedDoc === file.id,
              'hover:border-primary/50': selectedDoc !== file.id
            }" embedded @click="handleDocSelect(file.id)">
            <div class="flex items-center justify-between">
              <div class="flex-1 overflow-hidden">
                <div class="flex flex-col overflow-hidden">
                  <span class="text-16 font-medium truncate" :class="{ 'text-primary': selectedDoc === file.id }">{{
                    file.name
                    }}</span>
                  <span class="text-14 text-gray-500 truncate">{{ file.remark }}</span>
                </div>
              </div>
              <div
                class="absolute right-2 top-2 opacity-0 group-hover:opacity-100 transition-opacity flex items-center gap-2">
                <NButton quaternary circle size="tiny" @click.stop="handleEditDoc(file)">
                  <template #icon>
                    <IconifyIcon icon="material-symbols:edit" class="text-14" />
                  </template>
                </NButton>
                <NButton quaternary circle size="tiny" @click.stop="handleDeleteDoc(file)">
                  <template #icon>
                    <IconifyIcon icon="material-symbols:delete" class="text-14" />
                  </template>
                </NButton>
              </div>
            </div>
          </NCard>
        </div>
      </div>
    </NCard>
  </Page>
</template>

<style scoped>
.custom-card {
  transition: all 0.2s ease-in-out;

  /* border: 1px solid transparent !important; */
}

.custom-card:hover {
  box-shadow: 0 4px 12px rgb(0 0 0 / 10%);
  transform: translateY(-2px);
}

.custom-card.is-selected {
  border: 2px solid var(--primary-color) !important;
  box-shadow: 0 4px 12px rgb(64 158 255 / 20%);
}

:deep(.n-card-header) {
  padding: 12px 16px;
}

:deep(.n-card__content) {
  padding: 16px;
}
</style>
