<script lang="ts" setup>
import type { VxeGridListeners, VxeGridProps } from "#/adapter/vxe-table";
import { NInput, NTree, NDropdown, NButton, NCard, NSpace, useMessage, NDivider, NPopconfirm } from "naive-ui";
import { ref, reactive, onMounted, h } from "vue";
import { requestClient } from "#/api/request";
import { ColPage } from "@vben/common-ui";
import { IconifyIcon } from "@vben/icons";
import { convertToTree } from '#/utils/tree';
import { useRouter } from 'vue-router';
import ParamConfig from './paramconfig.vue';
 
const loading = ref(false);
const message = useMessage();
const router = useRouter();
const currenParamGroupId = ref(null);
const paramData = ref([]); // 参数列表
const paramConfigList = ref([]); // 配置类参数
const paramId = ref();

async function fetchParams() {
  try {
    const { navs, paramConfig, paramConfigId  }  = await requestClient.post('/param/GetParams',{ });
    // 将扁平数据转换为树形结构
    paramData.value = convertToTree(navs);
    paramConfigList.value = paramConfig;
    paramId.value = paramConfigId;

    // 如果有数据，自动选中第一行
    if (paramData.value.length > 0) {
      currenParamGroupId.value = paramData.value[0].id;
    }
  } catch (error) {
  }
}
const handleTreeSelect = (keys: number[]) => {
  if (keys.length > 0) {
    currenParamGroupId.value = keys[0];
  }
};

onMounted(() => {
  fetchParams();
  // fetchUsers();
});

const props = reactive({
  leftCollapsedWidth: 3,
  leftCollapsible: true,
  leftMaxWidth: 30,
  leftMinWidth: 15,
  leftWidth: 20,
  resizable: true,
  rightWidth: 70,
  splitHandle: false,
  splitLine: false,
});
</script>

<template>
  <ColPage description="系统参数类配置" title="参数设置" auto-content-height v-bind="props">
    <template #left="{ isCollapsed }">
      <NCard title="">
        <NTree :data="paramData" block-line key-field="id" label-field="name" parent-field="parentId" selectable :cancelable="false" 
        :selected-keys="[currenParamGroupId]" @update:selected-keys="handleTreeSelect" default-expand-all/>
      </NCard>
    </template>
    <NCard class="ml-2" title="">
      <!-- <Grid ref="gridRef">
        <template #toolbar-tools>
          <NButton type="info" @click="handleAdd">新增</NButton>
        </template>
        <template #action="{ row }">
          <NButton class="mr-2" type="primary" @click="handleEdit(row)" v-if="row.isAdmin == 0">编辑</NButton>
          <NPopconfirm @positiveClick="handleDelete(row)" v-if="row.isAdmin == 0">
            <template #trigger>
              <NButton class="mr-2" type="error">删除</NButton>
            </template>
            删除后无法恢复，是否继续
          </NPopconfirm>
        </template>
      </Grid> -->
      <ParamConfig :paramList="paramConfigList" :paramId="paramId"/>
    </NCard>
 
  </ColPage>
</template>
