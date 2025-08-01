<script lang="ts" setup>
import { NForm, NFormItem, NInput, NSpace, NButton, NModal, useMessage } from "naive-ui";
import { ref } from "vue";
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
        message: "请输入角色名称",
        trigger: "blur",
    },
};

async function handleOk() {
    await formRef.value?.validate();
    await requestClient.post('/role/save', {
        ...formData.value,
    });
    message.success('保存成功');
    emit('success', formData.value);
    showModal.value = false;
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
    <NModal v-model:show="showModal" preset="dialog" title="角色信息" :loading="loading"
        negative-text="取消" @negative-click="handleCancel">
        <NForm ref="formRef" :model="formData" :rules="rules" label-placement="left" label-width="100"
            require-mark-placement="right-hanging">
            <NFormItem label="角色名称" path="name">
                <NInput v-model:value="formData.name" placeholder="请输入角色名称" />
            </NFormItem>
            <NFormItem label="说明" path="remark">
                <NInput type="textarea" v-model:value="formData.remark" placeholder="请输入说明" />
            </NFormItem>
        </NForm>
        <template #action>
            <NButton v-access:code="['role:edit','role:add']" type="primary" :loading="loading" @click="handleOk">确认</NButton>
            <NButton @click="handleCancel">取消</NButton>
        </template>
    </NModal>
</template>
