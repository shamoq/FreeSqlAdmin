<script lang="ts" setup>
import { NForm, NFormItem, NInput, NSelect, NSpace, NButton, NModal, useMessage } from 'naive-ui';
import { ref } from 'vue';
import { requestClient } from "#/api/request";

const emit = defineEmits(['update:formData', 'success']);
const showModal = ref(false);
const formRef = ref();
const formData = ref();
const loading = ref(false);
const message = useMessage();

const rules = {
    name: {
        required: true,
        message: '请输入组织名称',
        trigger: 'blur',
    }
};

async function handleOk() {
    try {
        await formRef.value?.validate();
        loading.value = true;
        await requestClient.post('/org/save', {
            ...formData.value,
        });
        message.success('保存成功');
        emit('success', formData.value);
        showModal.value = false;
    }finally {
        loading.value = false;
    }
}

function handleCancel() {
    showModal.value = false;
}

function show(data?: any) {
    if (data) {
        formData.value = { ...data };
    } else {
        formData.value = {};
    }
    showModal.value = true;
}

defineExpose({
    show
});
</script>

<template>
    <NModal v-model:show="showModal" preset="dialog" title="组织信息" :loading="loading"
        negative-text="取消" @negative-click="handleCancel">
        <NForm ref="formRef" :model="formData" :rules="rules" label-placement="left" label-width="100"
            require-mark-placement="right-hanging">
            <NFormItem label="父级组织">
                <NInput v-model:value="formData.parentName" disabled placeholder="" />
            </NFormItem>
            <NFormItem label="组织名称" path="name">
                <NInput v-model:value="formData.name" placeholder="请输入组织名称" />
            </NFormItem>
        </NForm>
        <template #action>
            <NButton v-access:code="['org:edit','org:add']" type="primary" :loading="loading" @click="handleOk">确认</NButton>
            <NButton @click="handleCancel">取消</NButton>
        </template>
    </NModal>
</template>
