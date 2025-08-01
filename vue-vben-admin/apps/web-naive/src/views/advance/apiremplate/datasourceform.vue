<script lang="ts" setup>
import { ref, defineExpose, computed } from 'vue';
import { NForm, NFormItem, NInput, NModal, useMessage, NSelect } from 'naive-ui';
import type { FormInst, FormRules } from 'naive-ui';
import { requestClient } from "#/api/request";
import type { CustomDataSource } from '#/api/dto/CustomDataSource';
import { DataSourceTypeEnum, DataSourceTypeEnumOption } from '#/api/dto/DataSourceTypeEnum';

const emit = defineEmits(['success']);
const message = useMessage();
const showDialog = ref(false);
const dialogTitle = ref('');
const loading = ref(false);
const formRef = ref<FormInst>();
const formData = ref<Partial<CustomDataSource>>({});
const isUpdate = ref(false);

const isSqlType = computed(() => formData.value.dataSourceType === DataSourceTypeEnum.Sql);

const rules: FormRules = {
  name: [
    { required: true, message: '请输入数据源名称', trigger: 'blur' },
  ],
  dataSourceType: [
    { required: true, message: '请选择数据源类型', trigger: 'blur' },
  ],
  databaseType: [
    { required: true, message: '请输入数据库类型', trigger: 'blur' },
  ],
  databaseConnectionString: [
    { required: true, message: '请输入数据库连接字符串', trigger: 'blur' },
  ],
};

function show(data?: { record?: CustomDataSource; isUpdate?: boolean }) {
  showDialog.value = true;
  isUpdate.value = !!data?.isUpdate;
  dialogTitle.value = isUpdate.value ? '编辑数据源' : '新增数据源';
  
  if (data?.record) {
    formData.value = { ...data.record };
  } else {
    formData.value = {
      dataSourceType: DataSourceTypeEnum.Api, // 默认选择 Api 类型
    };
  }
}

function handleClose() {
  formRef.value?.restoreValidation();
  showDialog.value = false;
}

async function handleSubmit() {
  try {
    loading.value = true;
    await formRef.value?.validate();

    if (isUpdate.value && formData.value.id) {
      await requestClient.put('/CustomDataSource/update/' + formData.value.id, formData.value);
      message.success('更新成功');
    } else {
      await requestClient.post('/CustomDataSource/create', formData.value);
      message.success('创建成功');
    }
    
    handleClose();
    emit('success');
  } catch (error) {
    message.error('操作失败');
  } finally {
    loading.value = false;
  }
}

defineExpose({
  show
});
</script> 


<template>
  <NModal
    v-model:show="showDialog"
    preset="dialog"
    :title="dialogTitle"
    :loading="loading"
    positive-text="确认"
    negative-text="取消"
    @positive-click="handleSubmit"
    @negative-click="handleClose"
    @close="handleClose"
  >
    <NForm
      ref="formRef"
      :model="formData"
      :rules="rules"
      label-placement="left"
      label-width="120"
      require-mark-placement="right-hanging"
    >
      <NFormItem label="数据源名称" path="name">
        <NInput v-model:value="formData.name" placeholder="请输入数据源名称" />
      </NFormItem>
      <NFormItem label="数据源类型" path="dataSourceType">
        <NSelect
          v-model:value="formData.dataSourceType"
          :options="DataSourceTypeEnumOption"
          placeholder="请选择数据源类型"
        />
      </NFormItem>
      <template v-if="isSqlType">
        <NFormItem label="数据库类型" path="databaseType">
          <NInput v-model:value="formData.databaseType" placeholder="请输入数据库类型" />
        </NFormItem>
        <NFormItem label="数据库连接字符串" path="databaseConnectionString">
          <NInput
            v-model:value="formData.databaseConnectionString"
            type="textarea"
            placeholder="请输入数据库连接字符串"
            :rows="3"
          />
        </NFormItem>
      </template>
      <NFormItem label="备注" path="remark">
        <NInput
          v-model:value="formData.remark"
          type="textarea"
          placeholder="请输入备注"
          :rows="3"
        />
      </NFormItem>
    </NForm>
  </NModal>
</template>
