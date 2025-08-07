<script lang="ts" setup>
import type { VxeGridListeners, VxeGridProps } from "#/adapter/vxe-table";
import { NButton, NCard, NSpace, useMessage, useDialog, NPopconfirm, NTag } from "naive-ui";
import { useVbenVxeGrid } from "#/adapter/vxe-table";
import type { VbenFormProps } from "#/adapter/form";
import { ref, reactive, onMounted, h } from "vue";
import { Page } from "@vben/common-ui";
import { requestClient } from "#/api/request";
import type { TenantDataSource } from "#/api/entity/TenantDataSource";
import { useRouter } from 'vue-router';

const loading = ref(false);
const message = useMessage();
const currentRow = ref<TenantDataSource | null>(null);
const router = useRouter();

async function fetchList(params: any) {
  try {
    loading.value = true;

    const { currentPage, pageSize } = params.page;

    const searchParams = {
      page: currentPage,
      pageSize: pageSize,
    };

    const data = await requestClient.post(
      "/tenantdatasource/page",
      searchParams,
    );
    return data;
  } catch (error) {
    message.error("获取数据源列表失败");
  } finally {
    loading.value = false;
  }
}

const gridOptions = reactive<VxeGridProps<TenantDataSource>>({
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
      title: "名称",
      align: "left",
      sortable: true,
      slots: { default: "name" },
    },
    {
      field: "dbType",
      title: "数据库类型",
      align: "center",
      width: 120,
    },
    {
      field: "connectionString",
      title: "连接字符串",
      align: "left",
      slots: { default: "connection" },
    },
    {
      field: "status",
      title: "状态",
      align: "left",
      width: 150,
      slots: { default: "status" },
    },
    {
      field: "tenantCount",
      title: "租户数量",
      align: "right",
      width: 80,
    },
    {
      field: "remark",
      title: "说明",
      align: "left",
      sortable: true,
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

const gridEvents: VxeGridListeners<TenantDataSource> = {
};

const [Grid, gridApi] = useVbenVxeGrid({ gridEvents, gridOptions });

// 按钮操作
function handleAdd() {
  router.push({ name: 'datasourceform', params: { type: 'add' } });
}

async function handleDelete(row: TenantDataSource) {
  await requestClient.post('/tenantdatasource/delete', {
    id: row.id,
  });
  message.success('删除成功');
  await gridApi.reload({});
}

function handleEdit(row: TenantDataSource) {
  router.push({ name: 'datasourceform', params: { id: row.id, type: 'edit' } });
}

function handleView(row: TenantDataSource) {
  router.push({ name: 'datasourceform', params: { id: row.id, type: 'view' } });
}

// 状态映射方法
function getStatusInfo(status: number | string): { label: string; type: 'default' | 'success' | 'primary' | 'info' | 'warning' | 'error' } {
  switch (status) {
    case 1:
      return { label: "已连接", type: "success" };
    case 0:
      return { label: "未连接", type: "error" };
    case 2:
      return { label: "连接中", type: "info" };
    default:
      return { label: "未知", type: "default" };
  }
}

</script>

<template>
  <Page>
    <NCard>
      <Grid ref="gridRef">
        <template #toolbar-tools>
          <NButton class="mr-2" type="primary" @click="handleAdd" v-access:code="['tenantdatasource:add']">新增</NButton>
        </template>
        <template #connection="{ row }">
          <span style="max-width: 200px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
            {{ row.connectionString }}
          </span>
        </template>
        <template #status="{ row }">
          <NTag :type="getStatusInfo(row.status).type" size="small">
            {{ getStatusInfo(row.status).label }}
          </NTag>
        </template>
        <template #action="{ row }">
          <NButton class="mr-2" type="primary" @click="handleEdit(row)" v-access:code="['tenantdatasource:edit']">编辑</NButton>
          <NPopconfirm @positiveClick="handleDelete(row)">
            <template #trigger>
              <NButton class="mr-2" type="error" v-access:code="['tenantdatasource:del']">删除</NButton>
            </template>
            删除后无法恢复，是否继续
          </NPopconfirm>
        </template>
        <template #name="{ row }">
          <a class="text-link" @click="handleView(row)">{{ row.name }}</a>
        </template>
      </Grid>
    </NCard>
  </Page>
</template>
