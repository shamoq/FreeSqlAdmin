<script setup lang="ts">
import { ref, onMounted, computed, defineExpose } from 'vue';
import { NCard, NSpace, NCheckbox, NCheckboxGroup, NTabs, NTabPane, NModal, NButton } from 'naive-ui';
import { requestClient } from "#/api/request";

// 定义数据结构
interface BzTemplateItemDto {
  id: string;
  name: string;
  type: string;
  remark: string;
}

interface BzTemplateDto {
  types: string[];
  files: BzTemplateItemDto[];
}
const emit = defineEmits(['success']);

// 定义响应式数据
const templateData = ref<BzTemplateDto>({ types: [], files: [] });
const selectedTemplates = ref<string[]>([]);
const currentType = ref('all');
const showModal = ref(false);
let requestParams = null;
const loading = ref(false);

// 获取模板数据
async function fetchTemplateData() {
  try {
    const data = await requestClient.post<BzTemplateDto>('/bzTemplate/GetList', {});
    templateData.value = data;
  } catch (error) {
    console.error('获取模板数据失败:', error);
  }
}

// 获取模板数据
async function fetchIntroduce(ids: string[]) {
    await requestClient.post<BzTemplateDto>('/bzTemplate/Introduce', {
      ...requestParams,
      ids,
    });
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

// 处理确定按钮点击
async function handleConfirm() {
  try {
    loading.value = true;
    await fetchIntroduce(selectedTemplates.value);
    emit('success');
    showModal.value = false;
    selectedTemplates.value = [];
  } finally {
    loading.value = false;
  }
}



// 处理取消按钮点击
function handleCancel() {
  showModal.value = false;
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
  <NModal v-model:show="showModal" preset="card" title="业务模板" style="width: 800px;height:700px;" :on-close="handleCancel">
    <div class="flex flex-col h-full">
      <NCard class="flex-1 overflow-hidden">
        <NTabs v-model:value="currentType" type="line" animated>
          <NTabPane name="all" :tab="`全部 (${totalCount})`" />
          <NTabPane v-for="type in templateData.types" :key="type" :name="type" :tab="`${type} (${typeCount[type] || 0})`" />
        </NTabs>

        <div class="mt-4 overflow-y-auto" style="height: 450px;">
          <NCheckboxGroup v-model:value="selectedTemplates">
            <div class="grid grid-cols-2 gap-4 h-full">
              <NCard v-for="file in filteredFiles" :key="file.id" class="relative group overflow-hidden" embedded>
                <div class="flex items-center justify-between">
                  <div class="flex-1 overflow-hidden">
                    <NCheckbox :value="file.id">
                      <div class="flex flex-col overflow-hidden">
                        <span class="text-16 font-medium truncate">{{ file.name }}</span>
                        <span class="text-14 text-gray-500 truncate">{{ file.remark }}</span>
                      </div>
                    </NCheckbox>
                  </div>
                  <div class="absolute right-2 top-2 opacity-0 group-hover:opacity-100 transition-opacity">
                    <!-- <NButton text type="primary" @click="handlePreview(file)">
                      预览
                    </NButton> -->
                  </div>
                </div>
              </NCard>
            </div>
          </NCheckboxGroup>
        </div>
      </NCard>
      <div class="flex justify-end mt-4 space-x-2">
        <NButton @click="handleCancel">取消</NButton>
        <NButton type="primary" :loading="loading" @click="handleConfirm" v-access:code="['custom:introduce','template:introduce']">确定</NButton>
      </div>
    </div>
  </NModal>
</template>

<style scoped>
.n-card :deep(.n-card__content) {
  padding: 16px;
}
</style>