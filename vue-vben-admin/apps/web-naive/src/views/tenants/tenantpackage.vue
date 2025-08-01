<script lang="ts" setup>
import type { VxeGridListeners, VxeGridProps } from "#/adapter/vxe-table";
import { NButton, NCard, NSpace, useMessage, useDialog, NPopconfirm, NForm, NFormItem, NInput, NModal, NInputNumber } from "naive-ui";
import { useVbenVxeGrid } from "#/adapter/vxe-table";
import type { VbenFormProps } from "#/adapter/form";
import { ref, reactive, onMounted, h } from "vue";
import { Page } from "@vben/common-ui";
import { requestClient } from "#/api/request";
import type { TenantPackage } from "#/api/entity/TenantPackage";
import { useRouter, useRoute } from 'vue-router';
import type { FormInst, FormRules } from 'naive-ui';

const loading = ref(false);
const message = useMessage();
const currentRow = ref<TenantPackage | null>(null);
const router = useRouter();
const route = useRoute();

// 表单相关
const showModal = ref(false);
const formRef = ref<FormInst | null>(null);
const formData = reactive<Partial<TenantPackage>>({
  name: '',
  description: '',
});

const rules: FormRules = {
  name: {
    required: true,
    message: '请输入套餐名称',
    trigger: 'blur',
  },
  description: {
    required: true,
    message: '请输入套餐描述',
    trigger: 'blur',
  },
};

// 获取列表数据
async function fetchList(params: any) {
  try {
    loading.value = true;

    const { currentPage, pageSize } = params.page;

    const searchParams = {
      page: currentPage,
      pageSize: pageSize,
    };

    const data = await requestClient.post(
      "/tenantpackage/page",
      searchParams,
    );
    return data;
  } catch (error) {
    message.error("获取套餐列表失败");
  } finally {
    loading.value = false;
  }
}

const gridOptions = reactive<VxeGridProps<TenantPackage>>({
  border: true,
  stripe: true,
  loading: loading.value,
  columnConfig: {
    resizable: true,
  },
  rowConfig: {
    isHover: true,
  },
  columns: [
    { type: "seq", width: 70, align: "left" },
    {
      field: "name",
      title: "套餐名称",
      align: "left",
      sortable: true,
      slots: { default: "name" },
    },
    {
      field: "userCount",
      title: "用户数",
      align: "right",
      width: 100,
    },
    {
      field: "description",
      title: "描述",
      align: "left",
    },
    {
      field: "tenantCount",
      title: "租户数量",
      width: 80,
      align: "right",
    },
    {
      field: "creator",
      title: "创建人",
      align: "left",
      width: 150,
    },
    {
      field: "createdTime",
      title: "创建时间",
      align: "center",
      width: 200,
    },
    {
      field: "action",
      fixed: "right",
      align: "left",
      slots: { default: "action" },
      title: "操作",
      width: 320,
    },
  ],
  data: [],
  pagerConfig: {
    enabled: true,
    pageSize: 10,
  },
  proxyConfig: {
    autoload: false,
    response: {
      result: 'data',
      total: 'total',
      list: 'data',
    },
    ajax: {
      query: fetchList
    },
    sort: true,
  },
  sortConfig: {
    multiple: true,
  },
});

const gridEvents: VxeGridListeners<TenantPackage> = {
};

const [Grid, gridApi] = useVbenVxeGrid({ gridEvents, gridOptions });

// 按钮操作
function handleAdd() {
  showModal.value = true;
  formData.id = null;
  formData.name = '';
  formData.userCount = 20;
  formData.description = '';
}

async function handleDelete(row: TenantPackage) {
  await requestClient.post('/tenantpackage/delete', {
    id: row.id,
  });
  message.success('删除成功');
  await gridApi.reload({});
}

function handleEdit(row: TenantPackage) {
  showModal.value = true;
  formData.id = row.id;
  formData.name = row.name;
  formData.description = row.description;
}

function handleView(row: TenantPackage) {
  router.push({ name: 'tenantpackagegrant', params: { id: row.id, type: 'view' } });
}

function handleGrant(row: TenantPackage) {
  router.push({ name: 'tenantpackagegrant', params: { id: row.id, type: 'edit' } });
}

// 表单提交
async function handleSubmit() {
  if (!formRef.value) return;
  
  try {
    await formRef.value.validate();
    loading.value = true;

    await requestClient.post('/tenantpackage/save', formData);
    if (formData.id) {
      message.success('修改成功');
    } else {
      message.success('新增成功');
    }
    
    showModal.value = false;
    await gridApi.reload({});
  } catch (error) {
    // 表单验证失败
  } finally {
    loading.value = false;
  }
}

// 取消
function handleCancel() {
  showModal.value = false;
}

onMounted(() => {
  gridApi.reload({});
});
</script>

<template>
  <Page>
    <NCard>
      <Grid ref="gridRef">
        <template #toolbar-tools>
          <NButton class="mr-2" type="primary" @click="handleAdd" v-access:code="['tenantpackage:add']">新增</NButton>
        </template>
        <template #action="{ row }">
          <NButton class="mr-2" type="primary" @click="handleEdit(row)" v-access:code="['tenantpackage:edit']">编辑</NButton>
          <NButton class="mr-2" type="primary" @click="handleGrant(row)" v-access:code="['tenantpackage:grant']">授权</NButton>
          <NPopconfirm @positiveClick="handleDelete(row)">
            <template #trigger>
              <NButton class="mr-2" type="error" v-access:code="['tenantpackage:del']">删除</NButton>
            </template>
            删除后无法恢复，是否继续
          </NPopconfirm>
        </template>
        <template #name="{ row }">
          <a class="text-link" @click="handleView(row)">{{ row.name }}</a>
        </template>
      </Grid>
    </NCard>

    <!-- 表单弹窗 -->
    <NModal
      v-model:show="showModal"
      :title="formData.id ? '编辑套餐' : '新增套餐'"
      preset="card"
      style="width: 600px"
    >
      <NForm
        ref="formRef"
        :model="formData"
        :rules="rules"
        label-placement="left"
        label-width="auto"
        require-mark-placement="right-hanging"
      >
        <NFormItem label="套餐名称" path="name">
          <NInput
            v-model:value="formData.name"
            placeholder="请输入套餐名称"
            :disabled="route.params.type === 'view'"
          />
        </NFormItem>

        <NFormItem label="用户数" path="userCount">
          <NInputNumber
            v-model:value="formData.userCount"
            placeholder="请输入用户数"
            :show-button="false" :min="20"
                :max="65535"
            :disabled="route.params.type === 'view'"
          />
        </NFormItem>

        <NFormItem label="套餐描述" path="description">
          <NInput
            v-model:value="formData.description"
            type="textarea"
            placeholder="请输入套餐描述"
            :autosize="{ minRows: 3, maxRows: 5 }"
            :disabled="route.params.type === 'view'"
          />
        </NFormItem>

        <div class="flex justify-end space-x-4">
          <NButton @click="handleCancel">取消</NButton>
          <NButton
            v-if="route.params.type !== 'view'"
            type="primary"
            :loading="loading"
            @click="handleSubmit"
          >
            确定
          </NButton>
        </div>
      </NForm>
    </NModal>
  </Page>
</template>

<style scoped>
.flex {
  display: flex;
}

.justify-end {
  justify-content: flex-end;
}

.space-x-4 > * + * {
  margin-left: 1rem;
}
</style>
