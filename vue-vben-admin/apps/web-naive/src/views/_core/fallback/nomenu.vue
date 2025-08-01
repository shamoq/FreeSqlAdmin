<script setup lang="ts">
import { computed } from 'vue';
import { useRouter } from 'vue-router';

import { LogOut } from '@vben/icons';
import { $t } from '@vben/locales';
import { NButton } from 'naive-ui';

import { useAuthStore } from '#/store';

const router = useRouter();
const authStore = useAuthStore();

const titleText = computed(() => $t('ui.fallback.forbidden'));
const descText = computed(() => $t('ui.fallback.forbiddenDesc'));
const logout = computed(() => $t('common.logout'));

// 退出登录
async function handleLogout() {
  await authStore.logout();
}
</script>

<template>
  <div class="flex size-full flex-col items-center justify-center duration-300">
    <div class="flex-col-center">
      <p class="text-foreground mt-8 text-2xl md:text-3xl lg:text-4xl">
        {{ titleText }}
      </p>
      <p class="text-muted-foreground md:text-md my-4 lg:text-lg">
        {{ descText }}
      </p>
      <NButton size="large" @click="handleLogout">
        <template #icon>
          <LogOut class="mr-2 size-4" />
        </template>
        {{ logout }}
      </NButton>
    </div>
  </div>
</template>
