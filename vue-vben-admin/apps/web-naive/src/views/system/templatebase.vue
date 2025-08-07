<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import { NForm, NFormItem, NInput, NSelect, NTreeSelect } from 'naive-ui';
import type { FormInst, FormRules } from 'naive-ui';
import { convertToTree } from '#/utils/tree';
import { requestClient } from "#/api/request";

const props = withDefaults(defineProps<{
    formData?: {
      name: string;
      contractTypeId: string;
      remark: string,
    };
}>(), {
    formData: () => ({})
});

const formRef = ref();
const formData = ref({ ...props.formData });

const emit = defineEmits(['update:formData']);

const contractTypes = ref([]);
async function loadContractTypes() {
    try {
        const data = await requestClient.post('/contracttype/list', {});
        contractTypes.value = convertToTree(data);
    } catch (error) {
        console.error('获取合同分类失败', error);
    }
}

onMounted(() => {
    loadContractTypes();
});

const rules: FormRules = {
  name: [
    { required: true, message: '请选择模板分类', trigger: 'blur' },
  ],
  contractTypeId: [
    { required: true, message: '请选择模板分类', trigger: 'blur' },
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

defineExpose({
  getData,
});
</script>

<template>
  <NForm ref="formRef" :model="formData" :rules="rules" label-placement="left" label-width="100px" require-mark-placement="right-hanging">
    <NFormItem label="模板名称" path="name">
      <NInput v-model:value="formData.name" placeholder="请输入模板名称" :rows="3" />
    </NFormItem>
    <NFormItem label="模板分类" path="contractTypeId">
      <NTreeSelect v-model:value="formData.contractTypeId" :options="contractTypes" placeholder="请选择模板分类" key-field="id" label-field="name"
      filterable class="w-full" default-expand-all  />
    </NFormItem>
    <NFormItem label="说明" path="remark">
      <NInput v-model:value="formData.remark" type="textarea" placeholder="请输入说明" :rows="3" />
    </NFormItem>
  </NForm>
</template>