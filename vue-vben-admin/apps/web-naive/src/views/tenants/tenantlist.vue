<script lang="ts" setup>
import type { VxeGridListeners, VxeGridProps } from "#/adapter/vxe-table";
import { NButton, NCard, NSpace, useMessage, useDialog, NPopconfirm } from "naive-ui";
import { useVbenVxeGrid } from "#/adapter/vxe-table";
import type { VbenFormProps } from "#/adapter/form";
import { ref, reactive, onMounted, h } from "vue";
import { Page } from "@vben/common-ui";
import { requestClient } from "#/api/request";
import type { Tenant } from "#/api/entity/Tenant";
import { useRouter } from 'vue-router';
import { TenantEnvTypeEnumOption, getTenantEnvTypeLabel } from '#/api/enum/TenantEnvTypeEnum';

const loading = ref(false);
const message = useMessage();
const router = useRouter();

async function fetchList(params: any) {
  try {
    loading.value = true;

    // 获取表单值
    const formValues = await gridApi.formApi.getValues();
    var filters = [];
    if (formValues.name) {
      filters.push({ field: 'name', op: 'like', value: formValues.name });
    }
    if (formValues.code) {
      filters.push({ field: 'code', op: 'like', value: formValues.code });
    }
    if (formValues.type) {
      filters.push({ field: 'type', op: '=', value: formValues.type });
    }

    const { currentPage, pageSize } = params.page;

    const searchParams = {
      page: currentPage,
      pageSize: pageSize,
      filters,
    };

    const data = await requestClient.post(
      "/tenant/page",
      searchParams,
    );
    return data;

  } finally {
    loading.value = false;
  }
}

const formOptions: VbenFormProps = ref({
  collapsed: false,
  schema: [
    {
      component: "Input",
      componentProps: {
        placeholder: "请输入租户名称",
      },
      fieldName: "name",
      label: "租户名称",
    },
    {
      component: "Input",
      componentProps: {
        placeholder: "请输入租户编码",
      },
      fieldName: "code",
      label: "租户编码",
    },
    {
      component: "Select",
      componentProps: {
        allowClear: true,
        options: TenantEnvTypeEnumOption,
        placeholder: "请选择租户类型",
      },
      fieldName: "type",
      label: "租户类型",
    },
  ],
  showCollapseButton: true,
  submitButtonOptions: {
    content: "查询",
  },
  submitOnEnter: true,
  submitOnChange: false,
});

const gridOptions = reactive<VxeGridProps<Tenant>>({
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
      title: "租户名称",
      align: "left",
      sortable: true,
      slots: { default: "name" },
    },
    {
      field: "code",
      title: "租户编码",
      align: "left",
      sortable: true,
    },
    {
      field: "tenantPackageName",
      title: "套餐",
      align: "left",
      sortable: true,
    },
    {
      field: "dataSourceName",
      title: "数据库",
      align: "left",
      sortable: true,
    },
    {
      field: "type",
      title: "租户类型",
      align: "left",
      slots: { default: "type" },
    },
    {
      field: "isEnable",
      title: "状态",
      align: "left",
      slots: { default: "status" },
      width: 80,
    },
    {
      field: "expirationTime",
      title: "过期时间",
      align: "center",
    },
    {
      field: "createdTime",
      title: "创建时间",
      align: "center",
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

const gridEvents: VxeGridListeners<Tenant> = {
};

const [Grid, gridApi] = useVbenVxeGrid({ gridEvents, gridOptions, formOptions });

// 按钮操作
function handleAdd() {
  router.push({ name: 'tenantform', params: { type: "add" } });
}

async function handleDelete(row: Tenant) {
  await requestClient.post('/tenant/delete', {
    id: row.id,
  });
  message.success('删除成功');
  gridApi.reload();
}

function handleEdit(row: Tenant) {
  router.push({ name: 'tenantform', params: { id: row.id, type: 'edit' } });
}

function handleView(row: Tenant) {
  router.push({ name: 'tenantform', params: { id: row.id, type: 'view' } });
}

</script>

<template>
  <Page>
    <NCard>
      <Grid ref="gridRef">
        <template #toolbar-tools>
          <NButton class="mr-2" type="primary" @click="handleAdd" v-access:code="['tenantlist:add']">新增</NButton>
        </template>
        <template #status="{ row }">
          <span :style="{ color: row.isEnable ? '#18a058' : '#d03050' }">
            {{ row.isEnable ? '启用' : '禁用' }}
          </span>
        </template>
        <template #type="{ row }">
          {{getTenantEnvTypeLabel(row.type)}}
        </template>
        <template #action="{ row }">
          <NButton class="mr-2" type="primary" @click="handleEdit(row)"  v-access:code="['tenantlist:edit']">编辑</NButton>
          <NPopconfirm @positiveClick="handleDelete(row)">
            <template #trigger>
              <NButton class="mr-2" type="error" v-access:code="['tenantlist:del']">删除</NButton>
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
