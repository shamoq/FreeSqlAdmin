<script setup lang="ts">
import { ref } from 'vue'
import { useMessage,NCard,NForm,NFormItem,NInput,NButton } from 'naive-ui'
import { Page } from "@vben/common-ui";
import { requestClient } from "#/api/request";
import CryptoJS from 'crypto-js';

const message = useMessage()
const formRef = ref<any>(null)
const loading = ref(false)

const formData = ref({
  oldPassword: '',
  newPassword: '',
  confirmPassword: '',
})

const rules = {
  oldPassword: {
    required: true,
    message: '请输入旧密码',
    trigger: ['blur', 'input'],
  },
  newPassword: [
    {
      required: true,
      message: '请输入新密码',
      trigger: ['blur', 'input'],
    },
    {
      min: 6,
      message: '密码长度不能小于6位',
      trigger: ['blur', 'input'],
    },
  ],
  confirmPassword: [
    {
      required: true,
      message: '请再次输入新密码',
      trigger: ['blur', 'input'],
    },
    {
      validator: (rule: any, value: string) => {
        return value === formData.value.newPassword
      },
      message: '两次输入的密码不一致',
      trigger: ['blur', 'input'],
    },
  ],
}

async function handleSubmit() {
  try {
    await formRef.value?.validate()
    loading.value = true
    await requestClient.post('/user/resetpassword', {
      oldPassword: CryptoJS.MD5(formData.value.oldPassword).toString(),
      newPassword: CryptoJS.MD5(formData.value.newPassword).toString(),
    })
    message.success('密码修改成功')
    resetForm()
  } catch (error: any) {
    if (error?.message) {
      message.error(error.message)
    }
  } finally {
    loading.value = false
  }
}

function resetForm() {
  formRef.value?.restoreValidation()
  formData.value = {
    oldPassword: '',
    newPassword: '',
    confirmPassword: '',
  }
}
</script>

<template>
  <Page title="修改密码">
    <NCard class="max-w-2xl mx-auto">
      <NForm
        ref="formRef"
        :model="formData"
        :rules="rules"
        label-placement="left"
        label-width="100"
        require-mark-placement="right-hanging"
        size="medium"
      >
        <NFormItem label="旧密码" path="oldPassword">
          <NInput
            v-model:value="formData.oldPassword"
            type="password"
            placeholder="请输入旧密码"
            show-password-on="click"
          />
        </NFormItem>
        <NFormItem label="新密码" path="newPassword">
          <NInput
            v-model:value="formData.newPassword"
            type="password"
            placeholder="请输入新密码"
            show-password-on="click"
          />
        </NFormItem>
        <NFormItem label="确认新密码" path="confirmPassword">
          <NInput
            v-model:value="formData.confirmPassword"
            type="password"
            placeholder="请再次输入新密码"
            show-password-on="click"
          />
        </NFormItem>
        <div class="flex justify-center mt-4">
          <NButton type="primary" @click="handleSubmit" :loading="loading">确认修改</NButton>
          <NButton class="ml-4" @click="resetForm">重置</NButton>
        </div>
      </NForm>
    </NCard>
  </Page>
</template>