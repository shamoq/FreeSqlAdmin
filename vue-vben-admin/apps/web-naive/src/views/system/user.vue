<script lang="ts" setup>
import type { VxeGridListeners, VxeGridProps } from "#/adapter/vxe-table";
import type { TreeOption } from 'naive-ui'
import { NInput, NTree, NDropdown, NButton, NCard, NTooltip, useMessage, NDivider, NPopconfirm } from "naive-ui";
import { useVbenVxeGrid } from "#/adapter/vxe-table";
import type { VbenFormProps } from "#/adapter/form";
import { ref, reactive, onMounted, h } from "vue";
import { requestClient } from "#/api/request";
import { ColPage } from "@vben/common-ui";
import { IconifyIcon } from "@vben/icons";
import OrgnizationForm from './orgform.vue';
import { convertToTree } from '#/utils/tree';
import { useRouter } from 'vue-router';
import { useAccess } from '@vben/access';
import type { SysUserDto } from "#/api/dto/SysUserDto";
import { useDialog } from 'naive-ui';

const loading = ref(false);
const message = useMessage();
const dialog = useDialog();
const router = useRouter();
const { hasAccessByCodes } = useAccess();
const pattern = ref('');

const formOptions: VbenFormProps = ref({
  // 默认展开
  collapsed: false,
  schema: [
    {
      component: "Input",
      componentProps: {
        placeholder: "输入用户名或者账号搜索",
      },
      defaultValue: "",
      fieldName: "name",
      label: "快速搜索",
    },
  ],
  showCollapseButton: false,
  // 控制表单是否显示折叠按钮
  resetButtonOptions: {
    show: false,
  },
  submitButtonOptions: {
    show: false,
  },
  submitOnEnter: true,
  submitOnChange: true,
});

const fetchUsers = async (params: any = {}) => {
  try {
    loading.value = true;

    const formValues = await gridApi.formApi.getValues();
    var filters = [];
    filters.push({
      field: 'userName',
      value: formValues.name,
    });
    filters.push({
      field: 'usercode',
      value: formValues.name,
    });

    const { currentPage, pageSize } = params.page;
    const data = await requestClient.post('/user/page', {
      page: currentPage,
      pageSize,
      orgId: currentorgId.value,
      filters,
    });
    return data;
  } catch (error) {
    message.error('获取用户列表失败');
  } finally {
    loading.value = false;
  }
};

const handleStatusChange = async (row: SysUserDto) => {
  return;
    const newStatus = row.isEnable === 1 ? 0 : 1;
    await requestClient.post('/user/setflag', { id: row.id, isEnable: newStatus });
    row.isEnable = newStatus;
    message.success('状态更新成功');
};

const handleEdit = (row: SysUserDto) => {
  router.push({
    name: 'userform',
    params: {
      type: 'edit',
      id: row.id
    }
  });
};

const currentorgId = ref(null);

const handleAdd = () => {
  router.push({
    name: 'userform',
    params: { type: 'add' },
    query: {
      orgId: currentorgId.value
    }
  });
};

const handleViewDetail = (row: SysUserDto) => {
  router.push({
    name: 'userform',
    params: {
      type: 'view',
      id: row.id
    }
  });
};

const gridOptions: VxeGridProps<SysUserDto> = {
  border: true,
  loading: loading.value,
  columnConfig: {
    resizable: true,
  },
  rowConfig: {
    isCurrent: true,
    isHover: true,
  },
  columns: [
    { type: 'seq', width: 70, align: 'left' },
    {
      field: 'userName',
      title: '用户名称',
      align: 'left',
      slots: { default: 'name' },
    },
    {
      field: 'userCode',
      title: '账号',
      align: 'left',
    },
    {
      field: 'roleName',
      title: '角色',
      align: 'left',
    },
    {
      field: 'orgnizationName',
      title: '所属组织',
      align: 'left',
      slots: {
        default: 'orgnization'
      }
    },
    {
      field: 'isEnable',
      title: '状态',
      width: 80,
      slots: {
        default: ({ row }: { row: SysUserDto }) => {
          return h(
            NButton,
            {
              type: row.isEnable === 1 ? 'success' : 'warning',
              size: 'small',
              onClick: () => handleStatusChange(row),
            },
            { default: () => (row.isEnable === 1 ? '启用' : '禁用') },
          );
        },
      },
    },
    {
      field: 'action',
      fixed: 'right',
      align: 'left',
      slots: { default: 'action' },
      title: '操作',
      width: 200,
    },
  ],
  data: [],
  pagerConfig: {
    enabled: true,
    pageSize: 10,
    currentPage: 1,
    total: 0
  },
  proxyConfig: {
    autoLoad: false, // 树节点选中才加载数据
    response: {
      result: 'data',
      total: 'total',
      list: 'data',
    },
    ajax: {
      query: fetchUsers
    },
    sort: true,
  },
};

const [Grid, gridApi] = useVbenVxeGrid({ gridOptions, formOptions });

async function handleDelete(row: SysUserDto) {
  await requestClient.post('/user/delete', {
    ...row,
  });
  message.success('删除成功');
  await gridApi.reload();
}

// 组织列表
const orgData = ref([]);

async function fetchOrgList() {
  try {
    const data = await requestClient.post('/org/list', {
      name: pattern.value
    });
    // 将扁平数据转换为树形结构
    orgData.value = convertToTree(data);
    // 如果有数据，自动选中第一行
    if (orgData.value.length > 0) {
      currentorgId.value = orgData.value[0].id;
      await gridApi.reload();
    }
  } catch (error) {
    message.error('获取组织列表失败');
  }
}
const handleTreeSelect = async (keys: number[]) => {
  if (keys.length > 0) {
    currentorgId.value = keys[0];
    await gridApi.reload();
  }
};

const orgRowButtons = [{
  label: '新增子级',
  key: 'add',
  show: hasAccessByCodes(['org:add']),
},
{
  label: '编辑',
  key: 'editProfile',
  show: hasAccessByCodes(['org:edit']),
},
{
  label: '删除',
  key: 'delete',
  show: hasAccessByCodes(['org:del']),
}];

const orgFormRef = ref();

const handleOrgSelect = async (row: SysUserDto, key: string) => {
  switch (key) {
    case 'add':
      orgFormRef.value.show({ parentName: row.name, parentId: row.id })
      break;
    case 'editProfile':
      orgFormRef.value.show({ ...row, parentName: row.parentName })
      break;
    case 'delete':
      // 删除前弹窗确认
      dialog.warning({
        title: '提示',
        content: '确定要删除该组织吗？删除后无法恢复！',
        positiveText: '确定',
        negativeText: '取消',
        onPositiveClick: async () => {
          await requestClient.post('/org/delete', { id: row.id });
          message.success('删除成功');
          await fetchOrgList();
        }
      });
  }
};

function renderSuffix({ option }: { option: TreeOption }) {
  const rightButton = orgRowButtons.filter(t => t.show);
  if (rightButton.length == 0) return;
  return h(
    NDropdown,
    {
      options: rightButton.map(btn => {
        return btn;
      }),
      onSelect: (key) => handleOrgSelect(option, key)
    },
    {
      default: () => h(
        NButton,
        { text: true },
        { default: () => h(IconifyIcon, { icon: 'carbon:overflow-menu-horizontal' }) }
      )
    }
  )
}

onMounted(() => {
  fetchOrgList();
});

const props = reactive({
  leftCollapsedWidth: 5,
  leftCollapsible: true,
  leftMaxWidth: 30,
  leftMinWidth: 15,
  leftWidth: 20,
  resizable: true,
  rightWidth: 70,
  splitHandle: false,
  splitLine: false,
});
</script>

<template>
  <ColPage description="系统用户管理" title="用户管理" auto-content-height v-bind="props">
    <template #left="{ isCollapsed }">
      <NCard title="">
        <NInput v-model:value="pattern" placeholder="搜索" @keydown.enter="fetchOrgList" style="margin-bottom: 10px;" />
        <NTree :data="orgData" block-line key-field="id" label-field="name" parent-field="parentId" selectable
          :cancelable="false" :renderSuffix="renderSuffix" :selected-keys="[currentorgId]"
          @update:selected-keys="handleTreeSelect" default-expand-all />
      </NCard>
    </template>
    <NCard class="ml-2" title="">
      <Grid ref="gridRef">
        <template #name="{ row }">
          <a class="link-text" @click="handleViewDetail(row)">{{ row.userName }}</a>
        </template>
        <template #orgnization="{ row }">
          <NTooltip v-if="row.orgnizationFullName" placement="top">
            <template #trigger>
              <span>{{ row.orgnizationName }}</span>
            </template>
            {{ row.orgnizationFullName }}
          </NTooltip>
          <span v-else>{{ row.orgnizationName }}</span>
        </template>
        <template #toolbar-tools>
          <NButton type="info" @click="handleAdd" v-access:code="['user:add']">新增</NButton>
        </template>
        <template #action="{ row }">
          <NButton class="mr-2" type="primary" @click="handleEdit(row)" v-if="row.isAdmin == 0"
            v-access:code="['user:edit']">编辑</NButton>
          <NPopconfirm @positiveClick="handleDelete(row)" v-if="row.isAdmin == 0">
            <template #trigger>
              <NButton class="mr-2" type="error" v-access:code="['user:del']">删除</NButton>
            </template>
            删除后无法恢复，是否继续
          </NPopconfirm>
        </template>
      </Grid>
    </NCard>

    <OrgnizationForm ref="orgFormRef" @success="fetchOrgList()" />
  </ColPage>
</template>

<style>
.link-text {
  color: #2080f0;
  text-decoration: none;
  cursor: pointer;
}

.link-text:hover {
  text-decoration: underline;
}
</style>
