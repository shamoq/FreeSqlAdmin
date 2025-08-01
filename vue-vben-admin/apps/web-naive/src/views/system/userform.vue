<script lang="ts" setup>
import { NForm, NFormItem, NInput, NSelect, NTreeSelect, useMessage, NGrid, NGridItem, NSpace, NButton, NCard } from 'naive-ui';
import { ref, onMounted, computed } from 'vue';
import { requestClient } from "#/api/request";
import { Page } from "@vben/common-ui";
import { useRoute, useRouter } from 'vue-router';
import { convertToTree } from '#/utils/tree';
import CryptoJS from 'crypto-js';


const route = useRoute();
const router = useRouter();
const message = useMessage();
const loading = ref(false);

const props = withDefaults(defineProps<{
  formData?: {
    id?: string;
    userName?: string;
    userCode?: string;
    roleName?: string;
    isEnable?: number;
    orgId?: string;
    orgName?: string;
    email?: string;
    phone?: string;
    password?: string;
  };
}>(), {
  formData: () => ({})
});

const formRef = ref();
const formData = ref({ ...props.formData });
const orgOptions = ref([]);
const roleOptions = ref([]);
const defaultPassword = route.params.type === 'add' ? '' : "****************";

const statusOptions = [
  {
    label: '启用',
    value: 1
  },
  {
    label: '禁用',
    value: 0
  }
];

const rules = {
  userName: {
    required: true,
    message: '请输入用户名称',
    trigger: 'blur',
  },
  userCode: {
    required: true,
    message: '请输入账号',
    trigger: 'blur',
  },
  password: {
    required: true,
    message: '请输入密码',
    trigger: 'blur',
  },
  orgId: {
    required: false,
    message: '请选择组织',
    trigger: 'blur',
  },
  roleId: {
    required: false,
    message: '请选择角色',
    trigger: 'blur',
  },
  email: {
    type: 'email',
    message: '请输入正确的邮箱格式',
    trigger: ['blur', 'input']
  },
  phone: {
    pattern: /^1[3-9]\d{9}$/,
    message: '请输入正确的手机号格式',
    trigger: ['blur', 'input']
  }
};

const isReadOnly = computed(() => route.params.type === 'view');
const pageTitle = computed(() => {
  switch (route.params.type) {
    case 'edit':
      return '编辑用户';
    case 'add':
      return '新增用户';
    case 'view':
      return '用户详情';
    default:
      return '用户信息';
  }
});

async function loadOrgList() {
  try {
    const data = await requestClient.post('/org/list', {});
    orgOptions.value = convertToTree(data);
  } catch (error) {
    console.error('获取组织列表失败', error);
  }
}

async function loadRoleList() {
  try {
    const data = await requestClient.post('/role/list', {});
    roleOptions.value = convertToTree(data);
  } catch (error) {
    console.error('获取角色列表失败', error);
  }
}

async function getData() {
  try {
    await formRef.value?.validate();
    return [true, formData.value];
  } catch (error) {
    return [false, null];
  }
}

async function handleSave() {
  const [success, data] = await getData();
  if (!success) return;

  loading.value = true;

  let postData = { ...data, password: undefined };

  if (formData.value.password != defaultPassword) {
    postData.password = CryptoJS.MD5(formData.value.password).toString();
  }
  try{
    await requestClient.post('/user/save', postData);
    formData.value.password = defaultPassword;
    message.success('保存成功');
  }finally{
    loading.value = false;
  }
  
  router.back();
}

onMounted(() => {
  loadOrgList();
  loadRoleList();

  // 如果是编辑模式，加载用户数据
  const type = route.params.type;
  const id = route.params.id;

  if (id) {
    requestClient.post('/user/get', { id }).then(data => {
      formData.value = data;
      formData.value.password = defaultPassword;
    }).catch(() => {
      message.error('获取用户信息失败');
    });
  }

  // 如果是新增用户，设置默认组织
  if (type === 'add') {
    if (route.query.orgId) {
      formData.value.orgId = route.query.orgId as string;
    }
    formData.value.isEnable = 1;
  }
});

defineExpose({
  getData
});

// 添加用于获取显示文本的辅助函数
function findNodeName(tree: any[], id: string | number): string {
  for (const node of tree) {
    if (node.id === id) return node.name;
    if (node.children) {
      const name = findNodeName(node.children, id);
      if (name) return name;
    }
  }
  return '';
}

function getOrgName(id: string | number) {
  return findNodeName(orgOptions.value, id);
}

function getRoleName(id: string | number) {
  return findNodeName(roleOptions.value, id);
}

function getStatusName(value: number) {
  return statusOptions.find(option => option.value === value)?.label || '';
}
</script>

<template>
  <Page>
    <NCard :title="pageTitle">
      <template #header-extra>
        <NSpace>
          <NButton :loading="loading" v-if="!isReadOnly" type="primary" @click="handleSave">保存</NButton>
          <NButton @click="router.back()">返回</NButton>
        </NSpace>
      </template>
      <NForm ref="formRef" :model="formData" :rules="rules" label-placement="left" label-width="100"
        require-mark-placement="right-hanging">
        <NGrid :cols="2" :x-gap="12" :y-gap="8">
          <NGridItem>
            <NFormItem label="用户名称" path="userName">
              <template v-if="isReadOnly">
                <div class="form-text">{{ formData.userName || '-' }}</div>
              </template>
              <NInput v-else v-model:value="formData.userName" placeholder="请输入用户名称" class="w-full" />
            </NFormItem>
          </NGridItem>
          <NGridItem>
            <NFormItem label="状态" path="isEnable">
              <template v-if="isReadOnly">
                <div class="form-text">{{ getStatusName(formData.isEnable) || '-' }}</div>
              </template>
              <NSelect v-else v-model:value="formData.isEnable" :options="statusOptions" placeholder="请选择状态"
                class="w-full" />
            </NFormItem>
          </NGridItem>
          <NGridItem>
            <NFormItem label="账号" path="userCode">
              <template v-if="isReadOnly">
                <div class="form-text">{{ formData.userCode || '-' }}</div>
              </template>
              <NInput v-else v-model:value="formData.userCode" placeholder="请输入账号" class="w-full" />
            </NFormItem>
          </NGridItem>
          <NGridItem>
            <NFormItem label="密码" path="password">
              <template v-if="isReadOnly">
                <div class="form-text">{{ '******' }}</div>
              </template>
              <NInput v-else v-model:value="formData.password" type="password" placeholder="请输入密码" class="w-full" />
            </NFormItem>
          </NGridItem>
          <NGridItem>
            <NFormItem label="所属组织" path="orgId">
              <template v-if="isReadOnly">
                <div class="form-text">{{ getOrgName(formData.orgId) || '-' }}</div>
              </template>
              <NTreeSelect v-else v-model:value="formData.orgId" :options="orgOptions" placeholder="请选择组织"
                key-field="id" label-field="name" filterable class="w-full" default-expand-all />
            </NFormItem>
          </NGridItem>
          <NGridItem>
            <NFormItem label="角色" path="roleIds">
              <template v-if="isReadOnly">
                <div class="form-text">{{ formData.roleNames?.join(';') || '-' }}</div>
              </template>
              <NTreeSelect v-else v-model:value="formData.roleIds" :options="roleOptions" :multiple="true"
                placeholder="请选择角色" key-field="id" label-field="name" filterable clearable class="w-full"
                default-expand-all />
            </NFormItem>
          </NGridItem>
          <NGridItem>
            <NFormItem label="邮箱" path="email">
              <template v-if="isReadOnly">
                <div class="form-text">{{ formData.email || '-' }}</div>
              </template>
              <NInput v-else v-model:value="formData.email" placeholder="请输入邮箱" class="w-full" />
            </NFormItem>
          </NGridItem>
          <NGridItem>
            <NFormItem label="手机号" path="phone">
              <template v-if="isReadOnly">
                <div class="form-text">{{ formData.phone || '-' }}</div>
              </template>
              <NInput v-else v-model:value="formData.phone" placeholder="请输入手机号" class="w-full" />
            </NFormItem>
          </NGridItem>
        </NGrid>
      </NForm>
    </NCard>
  </Page>

</template>

<style scoped>
:deep(.n-form-item-feedback-wrapper) {
  min-height: 18px;
}

.form-text {
  line-height: 34px;
  color: var(--n-text-color);
}
</style>
