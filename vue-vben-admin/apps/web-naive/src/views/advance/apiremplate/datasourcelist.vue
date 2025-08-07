<script lang="ts" setup>
import type { VxeGridProps } from '#/adapter/vxe-table';
import { Page } from '@vben/common-ui';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { requestClient } from "#/api/request";
import { NButton, NCard, NSpace, useMessage, useDialog, NPopconfirm } from 'naive-ui';
import { ref, onMounted } from 'vue';
import type { CustomDataSource } from '#/api/dto/CustomDataSource';
import DataSourceForm from './datasourceform.vue';
import { DataSourceTypeEnum, getDataSourceTypeLabel } from '#/api/dto/DataSourceTypeEnum';

const loading = ref(false);
const message = useMessage();
const dialog = useDialog();
const dataSourceFormRef = ref();

async function fetchList(params) {
  try {
    loading.value = true;
    const { currentPage, pageSize } = params.page;
    const data = await requestClient.post('/CustomDataSource/page', {
      page: currentPage,
      pageSize: pageSize,
    });
    return {
      data: data.data,
      total: data.total,
    };
  } finally {
    loading.value = false;
  }
}

async function testConnection(row: CustomDataSource) {
  try {
    loading.value = true;
    await requestClient.post('/CustomDataSource/test', {
      id: row.id,
    });
    message.success('连接测试成功');
  } catch (error) {
    message.error('连接测试失败');
  } finally {
    loading.value = false;
  }
}

const gridOptions: VxeGridProps = {
  columns: [
    { type: 'seq', width: 70, align: 'center', title: '序号' },
    {
      field: 'name',
      title: '数据源名称',
      align: 'left',
    },
    {
      field: 'dataSourceType',
      title: '数据源类型',
      align: 'left',
      formatter: ({ cellValue }) => getDataSourceTypeLabel(cellValue),
    },
    {
      field: 'databaseType',
      title: '数据库类型',
      align: 'left',
      visible: false,
    },
    {
      field: 'remark',
      title: '备注',
      align: 'left',
    },
    {
      field: 'creator',
      title: '创建人',
      align: 'left',
      width: 120,
    },
    {
      field: 'createdTime',
      title: '创建时间',
      align: 'center',
      width: 160,
    },
    {
      title: '操作',
      width: 200,
      align: 'center',
      slots: { default: 'action' },
    },
  ],
  exportConfig: {},
  height: 'auto',
  keepSource: true,
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
    defaultSort: { field: 'createdTime', order: 'desc' },
    remote: true,
  },
  toolbarConfig: {
    slots: {
      tools: 'toolbar_tools',
    },
  },
};

const [Grid, gridApi] = useVbenVxeGrid({
  gridOptions,
});

function handleCreate() {
  dataSourceFormRef.value?.show();
}

function handleEdit(row: CustomDataSource) {
  dataSourceFormRef.value?.show({
    record: row,
    isUpdate: true,
  });
}

async function handleDelete(row: CustomDataSource) {
    await requestClient.delete('/CustomDataSource/delete/' + row.id);
    message.success('删除成功');
    gridApi.reload();
}

function handleSuccess() {
  gridApi.reload();
}

onMounted(() => {
  gridApi.reload();
});
</script>

<template>
  <Page auto-content-height>
    <Grid>
      <template #toolbar_tools>
        <NSpace>
          <NButton type="primary" @click="handleCreate">新增数据源</NButton>
        </NSpace>
      </template>
      <template #action="{ row }">
        <NSpace>
          <NButton size="small" @click="handleEdit(row)">编辑</NButton>
          <NPopconfirm @positive-click="handleDelete(row)">
            <template #trigger>
              <NButton size="small" type="error">删除</NButton>
            </template>
            确认删除？
          </NPopconfirm>
          <NButton
            v-if="row.dataSourceType === DataSourceTypeEnum.Sql"
            size="small"
            type="info"
            @click="testConnection(row)"
          >
            测试连接
          </NButton>
        </NSpace>
      </template>
    </Grid>

    <DataSourceForm ref="dataSourceFormRef" @success="handleSuccess" />
  </Page>
</template>

<style scoped>
.n-button-group .n-button {
  padding: 0 12px;
}
</style>


