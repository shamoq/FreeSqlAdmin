<script setup lang="ts">
import { ref } from 'vue';
import { NCard, NTabPane, NTabs } from 'naive-ui';
import { Page } from "@vben/common-ui";
import DataSourceList from './datasourcelist.vue';

const activeTab = ref('datasource');
</script>

<template>
  <Page title="Api模板配置" description="" >
    <NCard>
      <NTabs v-model:value="activeTab" type="line" animated>
        <NTabPane name="datasource" tab="数据源配置">
          <DataSourceList />
        </NTabPane>
        <NTabPane name="template" tab="模板配置">
          <!-- <CustomField /> -->
        </NTabPane>
      </NTabs>
    </NCard>
  </Page>
</template>

<style scoped>
:deep(.n-tabs-nav) {
  padding: 0 16px;
}
</style>