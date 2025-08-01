<script lang="ts" setup>
import { ref, onMounted, defineExpose } from 'vue';
import { NForm, NFormItem, NInput, NSelect, NTreeSelect, NModal, useMessage } from 'naive-ui';
import type { FormInst, FormRules } from 'naive-ui';
import { requestClient } from "#/api/request";
import { useRouter } from 'vue-router';

const emit = defineEmits(['success']);
const router = useRouter();
const message = useMessage();
const showDialog = ref(false);
const dialogTitle = ref('');
const loading = ref(false);
const formRef = ref();
const formData = ref({});

function show() {
  showDialog.value = true;
  formData.value = {};
}

function handleClose() {
  formRef.value?.restoreValidation();
  showDialog.value = false;
}

async function handleSubmit() {
  try {
    loading.value = true;
    await formRef.value?.validate();

    const data = await requestClient.post('/customtemplate/save', {
      ...formData.value,
    });
    handleClose();
    router.push({
      name: 'customtemplateform',
      params: { id: data.id, type: 'edit' }
    });

  } finally {
    loading.value = false;
  }
}

defineExpose({
  show
});

const templateTypeOptions = ref([]);

async function loadTemplateTypeOptions() {
  try {
    const data = await requestClient.post('/option/GetEnums', { type: 'TemplateTypeEnum' });
    templateTypeOptions.value = data;
  } catch (error) {
    console.error('获取模板类型选项失败', error);
  }
}

onMounted(() => {
  loadTemplateTypeOptions();
});

const rules: FormRules = {
  name: [
    { required: true, message: '请选择模板分类', trigger: 'blur' },
  ],
  contractTypeId: [
    { required: true, message: '请选择模板分类', trigger: 'blur' },
  ],
  templateType: [
    { required: true, message: '请选择模板类型', trigger: 'blur' },
  ],
};
async function getData() {
  try {
    await formRef.value?.validate();
    return [true, formData.value];
  } catch (error) {
    return [false, null];
  }
}
</script>

<template>
  <NModal :show="showDialog" preset="dialog" title="新增批量模板" :loading="loading" positive-text="确认" negative-text="取消"
    @positive-click="handleSubmit" @negative-click="handleClose" @close="handleClose">
    <NForm ref="formRef" :model="formData" :rules="rules" label-placement="left" label-width="100px"
      require-mark-placement="right-hanging">
      <NFormItem label="模板名称" path="name">
        <NInput v-model:value="formData.name" placeholder="请输入模板名称" :rows="3" />
      </NFormItem>
      <NFormItem label="模板类型" path="templateType">
        <NSelect v-model:value="formData.templateType" :options="templateTypeOptions" placeholder="请选择模板类型" />
      </NFormItem>
      <NFormItem label="说明" path="remark">
        <NInput v-model:value="formData.remark" type="textarea" placeholder="请输入说明" :rows="3" />
      </NFormItem>
    </NForm>
  </NModal>
</template>