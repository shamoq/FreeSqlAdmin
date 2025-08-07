<script lang="ts" setup>
import type { VxeGridListeners, VxeGridProps } from "#/adapter/vxe-table";
import { NButton, NCard, NSpace, useMessage, useDialog, NPopconfirm } from "naive-ui";
import { useVbenVxeGrid } from "#/adapter/vxe-table";
import type { VbenFormProps } from "#/adapter/form";
import { ref, reactive, onMounted, h } from "vue";
import { Page } from "@vben/common-ui";
import { requestClient } from "#/api/request";
import RoleForm from './roleform.vue'
import { useRouter } from 'vue-router';

interface RowType {
  id: number;
  parentId: number | null;
  name: string;
  status?: number;
}

const loading = ref(false);
const message = useMessage();
const roleFormRef = ref();
const router = useRouter();

async function fetchRoleList(params: any) {
  try {
    loading.value = true;

    const data = await requestClient.post(
      "/role/list",
      {
        ...params,
      },
    );
    gridApi.setGridOptions({
      data: data,
    });
  } catch (error) {
    message.error("获取角色列表失败");
  } finally {
    loading.value = false;
  }
}

const formOptions: VbenFormProps = ref({
  // 默认展开
  collapsed: false,
  schema: [
    {
      component: "Input",
      componentProps: {
        placeholder: "Please enter category",
      },
      defaultValue: "1",
      fieldName: "category",
      label: "快速搜索",
    },
    {
      component: "Input",
      componentProps: {
        placeholder: "Please enter productName",
      },
      fieldName: "productName",
      label: "ProductName",
    },
    {
      component: "Input",
      componentProps: {
        placeholder: "Please enter price",
      },
      fieldName: "price",
      label: "Price",
    },
    {
      component: "Select",
      componentProps: {
        allowClear: true,
        options: [
          {
            label: "Color1",
            value: "1",
          },
          {
            label: "Color2",
            value: "2",
          },
        ],
        placeholder: "请选择",
      },
      fieldName: "color",
      label: "Color",
    },
    {
      component: "DatePicker",
      fieldName: "datePicker",
      label: "Date",
    },
  ],
  // 控制表单是否显示折叠按钮
  showCollapseButton: true,
  submitButtonOptions: {
    content: "查询",
  },
  // 是否在字段值改变时提交表单
  submitOnChange: false,
  // 按下回车时是否提交表单
  submitOnEnter: false,
});

const gridOptions = reactive<VxeGridProps<RowType>>({
  border: true,
  stripe: true,
  loading: loading.value,
  columnConfig: {
    resizable: true,
  },
  rowConfig: {
    isHover: true,
  },
  checkboxConfig: {
    labelField: "name",
    highlight: true,
    // range: true
  },
  columns: [
    { type: "seq", width: 70, align: "left" },
    // { type: 'checkbox', title: 'ID', width: 140 },
    {
      field: "name",
      title: "角色名称",
      treeNode: true,
      align: "left",
      sortable: true,
    },
    {
      field: "remark",
      title: "说明",
      align: "left",
      sortable: true,
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
    enabled: false,
  },
  treeConfig: {
    parentField: "parentId",
    rowField: "id",
    transform: true,
    expandAll: true,
  },
  sortConfig: {
    multiple: true,
  },
});

const gridEvents: VxeGridListeners<RowType> = {
};

function handleGrant(row: RowType) {
  const url = `/system/rolegrant?roleId=${row.id}`;
  router.push({ name: 'rolegrant', params: { roleId: row.id }, query: {pageKey: 'rolegrant'} });
}

const [Grid, gridApi] = useVbenVxeGrid({ gridEvents, gridOptions });


// 按钮操作

function handleAdd(row?: RowType) {
  roleFormRef.value.show({ parentId: row?.id })
}

function handleAddRoot() {
  roleFormRef.value.show({ })
}

async function handleDelete(row: RowType) {
  await requestClient.post('/role/delete', {
    ...row,
  });
  message.success('删除成功');
  await fetchRoleList({
  });
}

function handleEdit(row: RowType) {
  roleFormRef.value.show(row)
}

//#redregion


onMounted(() => {
  fetchRoleList({
  }).then(() => {
    gridApi.grid?.setAllTreeExpand(true);
  });
});
</script>

<template>
  <Page>
    <NCard>
      <Grid ref="gridRef">
        <template #toolbar-tools>
          <NButton lass="mr-2" type="primary" @click="handleAddRoot" v-access:code="['role:add']">新增</NButton>
        </template>
        <template #action="{ row }">
          <NButton class="mr-2" type="primary" @click="handleAdd(row)" v-access:code="['role:add']">新增</NButton>
          <NButton class="mr-2" type="primary" @click="handleEdit(row)" v-access:code="['role:edit']">编辑</NButton>
          <NButton class="mr-2" type="primary" @click="handleGrant(row)"  v-access:code="['role:grant']">授权</NButton>
          <NPopconfirm v-if="row.isLeaf" @positiveClick="handleDelete(row)">
            <template #trigger>
              <NButton class="mr-2" type="error"  v-access:code="['role:del']">删除</NButton>
            </template>
            删除后无法恢复，是否继续
          </NPopconfirm>
        </template>
      </Grid>
    </NCard>
      <RoleForm ref="roleFormRef" @success="fetchRoleList()"/>
  </Page>
</template>
