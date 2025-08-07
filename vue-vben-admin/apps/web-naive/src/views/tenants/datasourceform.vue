<script lang="ts" setup>
import { ref, reactive, watch, onMounted } from 'vue';
import { Page } from "@vben/common-ui";
import { useMessage } from 'naive-ui';
import type { FormInst, FormRules } from 'naive-ui';
import { NForm, NFormItem, NInput, NSelect, NInputNumber, NButton, NSpace, NCard, NDescriptions, NDescriptionsItem } from 'naive-ui';
import type { TenantDataSource } from '#/api/entity/TenantDataSource';
import { requestClient } from '#/api/request';
import { useRouter, useRoute } from 'vue-router';

const message = useMessage();
const formRef = ref<FormInst | null>(null);
const loading = ref(false);
const router = useRouter();
const route = useRoute();
const isViewMode = ref(route.params.type === 'view');

const dbTypes = [
  { label: 'MySQL', value: 'mysql' },
  { label: 'PostgreSQL', value: 'postgresql' },
  { label: 'SQL Server', value: 'sqlserver' },
  { label: 'Oracle', value: 'oracle' },
  { label: '达梦', value: 'dameng' },
  { label: '神州通用', value: 'shentong' },
  { label: '人大金仓', value: 'kingbase' },
  { label: '南大通用', value: 'gbase' },
  { label: '虚谷', value: 'xugu' },
  { label: '翰高', value: 'highgo' },
  { label: '华为', value: 'huawei' },
];

const defaultPorts = {
  mysql: 3306,
  postgresql: 5432,
  sqlserver: 1433,
  oracle: 1521,
  dameng: 5236,
  shentong: 5236,
  kingbase: 54321,
  gbase: 5258,
  xugu: 5138,
  highgo: 5866,
  huawei: 5432,
};

const connectionParams = ref({
  host: '',
  port: defaultPorts.mysql,
  database: '',
  username: '',
  password: '',
  additionalParams: '',
});

const rules: FormRules = {
  dbType: {
    required: true,
    message: '请选择数据库类型',
    trigger: 'change',
  },
  'connectionParams.host': {
    required: true,
    message: '请输入主机地址',
    trigger: 'blur',
  },
  'connectionParams.port': {
    required: true,
    message: '请输入端口号',
    trigger: ['blur', 'change'],
    type: 'number',
    validator: (rule, value) => {
      if (value === null || value === undefined || value === '') {
        return new Error('请输入端口号');
      }
      if (value < 1 || value > 65535) {
        return new Error('端口号必须在1-65535之间');
      }
      return true;
    }
  },
  'connectionParams.database': {
    required: true,
    message: '请输入数据库名',
    trigger: 'blur',
  },
  'connectionParams.username': {
    required: true,
    message: '请输入用户名',
    trigger: 'blur',
  },
  'connectionParams.password': {
    required: true,
    message: '请输入密码',
    trigger: 'blur',
  },
  name: {
    required: true,
    message: '请输入说明',
    trigger: 'blur',
  },
};

// 修改表单模型，添加connectionParams
const formModel = reactive<Partial<TenantDataSource> & { connectionParams: any }>({
  dbType: 'mysql',
  connectionString: '',
  remark: '',
  connectionParams: connectionParams.value,
  name: '',
});

// 监听connectionParams变化，同步到formModel
watch(connectionParams, (newVal) => {
  formModel.connectionParams = newVal;
}, { deep: true });

// 监听数据库类型变化，更新默认端口
watch(() => formModel.dbType, (newType) => {
  if (newType && defaultPorts[newType as keyof typeof defaultPorts]) {
    connectionParams.value.port = defaultPorts[newType as keyof typeof defaultPorts];
  }
});

// 生成连接字符串
function generateConnectionString() {
  const { host, port, database, username, password, additionalParams } = connectionParams.value;
  let connStr = '';

  switch (formModel.dbType) {
    case 'mysql':
      connStr = `Server=${host};Port=${port};Database=${database};User Id=${username};Password=${password};`;
      break;
    case 'postgresql':
      connStr = `Host=${host};Port=${port};Database=${database};Username=${username};Password=${password};`;
      break;
    case 'sqlserver':
      connStr = `Server=${host},${port};Database=${database};User Id=${username};Password=${password};`;
      break;
    case 'oracle':
      connStr = `Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=${host})(PORT=${port}))(CONNECT_DATA=(SERVICE_NAME=${database})));User Id=${username};Password=${password};`;
      break;
    case 'dameng':
      connStr = `Server=${host};Port=${port};Database=${database};User Id=${username};Password=${password};`;
      break;
    case 'shentong':
      connStr = `Server=${host};Port=${port};Database=${database};User Id=${username};Password=${password};`;
      break;
    case 'kingbase':
      connStr = `Host=${host};Port=${port};Database=${database};Username=${username};Password=${password};`;
      break;
    case 'gbase':
      connStr = `Server=${host};Port=${port};Database=${database};User Id=${username};Password=${password};`;
      break;
    case 'xugu':
      connStr = `Server=${host};Port=${port};Database=${database};User Id=${username};Password=${password};`;
      break;
    case 'highgo':
      connStr = `Host=${host};Port=${port};Database=${database};Username=${username};Password=${password};`;
      break;
    case 'huawei':
      connStr = `Host=${host};Port=${port};Database=${database};Username=${username};Password=${password};`;
      break;
  }

  if (additionalParams) {
    connStr += additionalParams;
  }

  formModel.connectionString = connStr;
}

// 测试连接
async function handleTest() {
  try {
    loading.value = true;
    generateConnectionString();
    const data = await requestClient.post('/tenantdatasource/Connect', {
      connectionString: formModel.connectionString,
      dbType: formModel.dbType,
    });
    if (data) {
      message.success('连接测试成功');
    } else {
      message.error('连接测试失败');
    }
  } catch (error) {
    message.error('连接测试失败');
  } finally {
    loading.value = false;
  }
}

// 修改提交表单函数
async function handleSubmit() {
  if (!formRef.value) return;

  try {
    await formRef.value.validate();
    loading.value = true;
    generateConnectionString();

    await requestClient.post('/tenantdatasource/save', {
      ...formModel,
      connectionParams: JSON.stringify(connectionParams.value),
      id: route.params.id,
    });
    message.success(route.params.id ? '修改成功' : '新增成功');
    router.back();
  } finally {
    loading.value = false;
  }
}

// 取消
function handleCancel() {
  router.back();
}

// 修改获取数据函数
async function fetchData() {
  if (!route.params.id) return;
  try {
    loading.value = true;
    const data = await requestClient.post('/tenantdatasource/get', { id: route.params.id });
    Object.assign(formModel, data);

    if (data.connectionParams && data.connectionParams.length > 2) {
      const params = JSON.parse(data.connectionParams);
      connectionParams.value = params;
      formModel.connectionParams = params;
    }
  } finally {
    loading.value = false;
  }
}
onMounted(() => {
  // 初始化
  fetchData();
});

</script>

<template>
  <Page>
    <NCard :title="route.params.id ? (isViewMode ? '查看数据库' : '编辑数据库') : '新增数据库'" size="huge">
      <template v-if="isViewMode">
        <NDescriptions bordered>
          <NDescriptionsItem label="连接名称">
            {{ formModel.name }}
          </NDescriptionsItem>
          <NDescriptionsItem label="数据库类型">
            {{dbTypes.find(t => t.value === formModel.dbType)?.label}}
          </NDescriptionsItem>
          <NDescriptionsItem label="主机地址">
            {{ connectionParams.host }}
          </NDescriptionsItem>
          <NDescriptionsItem label="端口">
            {{ connectionParams.port }}
          </NDescriptionsItem>
          <NDescriptionsItem label="数据库名">
            {{ connectionParams.database }}
          </NDescriptionsItem>
          <NDescriptionsItem label="用户名">
            {{ connectionParams.username }}
          </NDescriptionsItem>
          <NDescriptionsItem label="附加参数">
            {{ connectionParams.additionalParams }}
          </NDescriptionsItem>
          <NDescriptionsItem label="说明">
            {{ formModel.remark }}
          </NDescriptionsItem>
        </NDescriptions>
        <div class="mt-4 flex justify-end">
          <NButton @click="handleCancel">返回</NButton>
        </div>
      </template>

      <template v-else>
        <NForm ref="formRef" :model="formModel" :rules="rules" label-placement="left" label-width="auto"
          require-mark-placement="right-hanging">
          <div class="mt-4">
            <NFormItem label="连接名称" path="name">
              <NInput v-model:value="formModel.name" placeholder="请输入连接名称" />
            </NFormItem>
          </div>
          <div class="grid grid-cols-2 gap-4">

            <NFormItem label="数据库类型" path="dbType">
              <NSelect v-model:value="formModel.dbType" :options="dbTypes" placeholder="请选择数据库类型" :filterable="true"/>
            </NFormItem>

            <NFormItem label="主机地址" path="connectionParams.host">
              <NInput v-model:value="connectionParams.host" placeholder="请输入主机地址" />
            </NFormItem>

            <NFormItem label="端口" path="connectionParams.port">
              <NInputNumber v-model:value="connectionParams.port" placeholder="请输入端口号" :show-button="false" :min="1"
                :max="65535" class="w-full" @update:value="(val) => formModel.connectionParams.port = val" />
            </NFormItem>

            <NFormItem label="数据库名" path="connectionParams.database">
              <NInput v-model:value="connectionParams.database" placeholder="请输入数据库名" />
            </NFormItem>

            <NFormItem label="用户名" path="connectionParams.username">
              <NInput v-model:value="connectionParams.username" placeholder="请输入用户名" />
            </NFormItem>

            <NFormItem label="密码" path="connectionParams.password">
              <NInput v-model:value="connectionParams.password" type="password" placeholder="请输入密码"
                show-password-on="click" />
            </NFormItem>
          </div>

          <div class="mt-4">
            <NFormItem label="附加参数" path="connectionParams.additionalParams">
              <NInput v-model:value="connectionParams.additionalParams" type="textarea"
                placeholder="请输入附加连接参数，例如：charset=utf8;" :autosize="{ minRows: 2, maxRows: 5 }" />
            </NFormItem>

            <NFormItem label="说明" path="remark">
              <NInput v-model:value="formModel.remark" type="textarea" placeholder="请输入说明"
                :autosize="{ minRows: 2, maxRows: 5 }" />
            </NFormItem>
          </div>

          <div class="mt-6 flex justify-end space-x-4">
            <NButton @click="handleCancel">取消</NButton>
            <NButton type="info" :loading="loading" @click="handleTest" v-access:code="['tenantdatasource:edit']">测试连接
            </NButton>
            <NButton type="primary" :loading="loading" @click="handleSubmit"
              v-access:code="['tenantdatasource:add', 'tenantdatasource:edit']">保存</NButton>
          </div>
        </NForm>
      </template>
    </NCard>
  </Page>
</template>
