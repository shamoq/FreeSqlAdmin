<script lang="ts" setup>
import type { VbenFormSchema } from '@vben/common-ui';

import { computed, markRaw, onMounted, ref } from 'vue';

import { AuthenticationLogin, SliderCaptcha, z } from '@vben/common-ui';
import { $t } from '@vben/locales';
import CryptoJS from 'crypto-js';

import { useAuthStore } from '#/store';

defineOptions({ name: 'Login' });

const authStore = useAuthStore();

const handleSubmit = (formData: any) => {
  if (formData.password) {
    formData.password = CryptoJS.MD5(formData.password).toString();
  }
  // 登录成功后保存租户代码和用户名
  localStorage.setItem('lastTenantCode', formData.tenantCode);
  localStorage.setItem('lastUsercode', formData.usercode);
  authStore.authLogin(formData);
};

// const tenantList = ref([]);
const loginRef = ref();

const formSchema = computed((): VbenFormSchema[] => {
  return [
    {
      component: 'VbenInput',
      componentProps: {
        placeholder: $t('authentication.selectAccount'),
      },
      fieldName: 'tenantCode',
      label: $t('authentication.selectAccount'),
      rules: z
        .string()
        .min(1, { message: $t('authentication.selectAccount') })
        .optional()
        .default(''),
    },
    {
      component: 'VbenInput',
      componentProps: {
        placeholder: $t('authentication.usernameTip'),
      },
      dependencies: {
        trigger(values, form) {
          // if (values.selectAccount) {
          //   const findUser = tenantList.find(
          //     (item) => item.value === values.selectAccount,
          //   );
          //   if (findUser) {
          //     form.setValues({
          //       password: '123456',
          //       usercode: findUser.value,
          //     });
          //   }
          // }
        },
        triggerFields: ['selectAccount'],
      },
      fieldName: 'usercode',
      label: $t('authentication.username'),
      rules: z.string().min(1, { message: $t('authentication.usernameTip') }),
    },
    {
      component: 'VbenInputPassword',
      componentProps: {
        placeholder: $t('authentication.password'),
      },
      fieldName: 'password',
      label: $t('authentication.password'),
      rules: z.string().min(1, { message: $t('authentication.passwordTip') }),
    },
    {
      component: markRaw(SliderCaptcha),
      fieldName: 'captcha',
      rules: z.boolean().refine((value) => value, {
        message: $t('authentication.verifyRequiredTip'),
      }),
    },
  ];
});

onMounted(async () => {
  // const data = await requestClient.get('/auth/tenants');
  // tenantList.value = data.map(t=> {
  //   return { label: t.tenantName, value: t.tenantCode};
  // });

  // 从本地存储获取上次登录的租户代码和用户名
  const lastTenantCode = localStorage.getItem('lastTenantCode');
  const lastUsercode = localStorage.getItem('lastUsercode');

  loginRef.value.getFormApi().setValues({
    tenantCode: lastTenantCode,
    usercode: lastUsercode,
  })
});
</script>

<template>
  <AuthenticationLogin
     ref="loginRef"
    :form-schema="formSchema"
    :loading="authStore.loginLoading"
    @submit="handleSubmit"
    title="FreeSqlAdmin"
    :show-code-login="false"
    :show-forget-password="false"
    :show-register="false"
    :show-qrcode-login="false"
    :show-remember-me="false"
    :show-third-party-login="false"
  />
</template>
