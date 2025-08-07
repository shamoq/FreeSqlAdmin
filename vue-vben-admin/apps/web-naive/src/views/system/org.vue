<script lang="ts" setup>
import type { VxeGridListeners, VxeGridProps } from '#/adapter/vxe-table';
import type { TreeOption } from 'naive-ui';
import { NButton, NCard, NSpace, useMessage, NDrawer, NDrawerContent, NPopconfirm, NDropdown, NTree, NInput, NDivider } from 'naive-ui';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { h, ref, onMounted } from 'vue';
import { Page } from '@vben/common-ui';
import OrgnizationForm from './orgform.vue';
import { requestClient } from "#/api/request";
import { IconifyIcon } from "@vben/icons";

const orgData = ref([]);
const pattern = ref('');

interface RowType {
  id: string;
  parentId: string | null;
  name: string;
  isEnable?: number;
}

const message = useMessage();
const formRef = ref();
const currentRow = ref<RowType | null>(null);
const loading = ref(false);

const loadOrganizations = async () => {
  try {
    loading.value = true;
    const data = await requestClient.post('/org/list', {});
    orgData.value = data;
  } catch (error) {
    message.error('获取组织列表失败');
  } finally {
    loading.value = false;
  }
};

const handleStatusChange = async (row: RowType) => {
  try {
    const newStatus = row.isEnable === 1 ? 0 : 1;
    await requestClient.post('/org/setflag', { id: row.id, isEnable: newStatus });
    row.isEnable = newStatus;
    message.success('状态更新成功');
    await loadOrganizations();
  } catch (error) {
    message.error('状态更新失败');
  }
};

const orgRowButtons = [{
  label: '新增子级',
  key: 'add',
},
{
  label: '编辑',
  key: 'editProfile',
},
{
  label: '删除',
  key: 'delete',
}];

const handleOrgSelect = async (row: RowType, key: string) => {
  switch (key) {
    case 'add':
      formRef.value.show({ parentName: row.name, parentId: row.id })
      break;
    case 'editProfile':
      formRef.value.show(row)
      break;
    case 'delete':
      await requestClient.post('/org/delete', { id: row.id });
      message.success('删除成功');
      await loadOrganizations();
  }
};

function renderSuffix({ option }: { option: TreeOption }) {
  return h(
    NDropdown,
    {
      options: orgRowButtons,
      onSelect: (key) => handleOrgSelect(option as unknown as RowType, key)
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

const handleAddRoot = () => {
  currentRow.value = null;
  showModal.value = true;
};

async function handleFormSuccess() {
  const [success, formData] = await formRef.value?.getData();
  if (!success) return;
  try {
    await requestClient.post('/org/save', {
      ...formData,
    });
    message.success('保存成功');
    showModal.value = false;
    await loadOrganizations();
  } catch (error) {
    message.error('保存失败');
  }
}

onMounted(() => {
  loadOrganizations();
});
</script>

<template>
  <Page description="组织架构管理" title="组织架构">
    <NCard>
      <div class="flex justify-between items-center mb-2">
        <NButton type="info" @click="handleAddRoot" v-access:code="['org:add', 'org:edit']">新增根节点</NButton>
        <NInput v-model:value="pattern" placeholder="搜索" style="width: 200px;" />
      </div>
      <NDivider />
      <NTree :data="orgData" block-line key-field="id" parent-field="parentId" label-field="name" selectable
        :pattern="pattern" :default-expanded-keys="[1]" :render-suffix="renderSuffix">
      </NTree>
    </NCard>

    <OrgnizationForm ref="formRef" :formData="currentRow" @success="handleFormSuccess" />
  </Page>
</template>
