<script setup lang="ts">
import { ref, watch } from 'vue'
import { NForm, NFormItem, NInput, NInputGroup, NDatePicker, NTimePicker, NSelect, NIcon, NTooltip, NButton, NSpace, useMessage } from 'naive-ui'
import { IconifyIcon } from "@vben/icons"
import { requestClient } from "#/api/request";
import { useTabs } from '@vben/hooks';
import { ParamTypeEnum } from '#/api/enum/ParamTypeEnum'
import type { ParamInfoDto } from '#/api/dto/ParamInfoDto'

interface ParamGroup {
  name: string
  description: string
  items: ParamInfoDto[]
}

const props = defineProps<{
  paramList: ParamGroup[],
  paramId: String,
}>()

const { closeCurrentTab } = useTabs();
const message = useMessage()
const loading = ref(false);

const paramGroups = ref<ParamGroup[]>(props.paramList);

// 监听 paramList 的变化
watch(() => props.paramList, (newValue) => {
  paramGroups.value = newValue;
}, { deep: true });

// 根据参数类型返回对应的输入组件类型
const getInputComponent = (paramType: number) => {
  switch (paramType) {
    case ParamTypeEnum.String: // 文本
      return NInput
    case ParamTypeEnum.Number: // 数值
      return NInputGroup
    case ParamTypeEnum.Date: // 日期
      return NDatePicker
    case ParamTypeEnum.DateTime: // 日期时间
      return NDatePicker
    case ParamTypeEnum.Time: // 时间
      return NTimePicker
    case ParamTypeEnum.Password: // 密码
      return NInput
    case ParamTypeEnum.Option: // 选项
      return NSelect
    default:
      return NInput
  }
}

// 获取组件的props
const getComponentProps = (paramInfo: ParamInfoDto) => {
  var paramType = paramInfo.paramType;
  const baseProps = {
    placeholder: `请输入${paramInfo.paramName}`,
  }

  switch (paramType) {
    case ParamTypeEnum.Number: // 数值
      return { ...baseProps, type: 'number' }
    case ParamTypeEnum.DateTime: // 日期时间
      return { ...baseProps, type: 'datetime' }
    case ParamTypeEnum.Password: // 密码
      return { ...baseProps, type: 'password' }
    case ParamTypeEnum.Option: // 选项类型
      return { ...baseProps, options: paramInfo.options }
    default:
      return baseProps
  }
}

// 保存参数配置
const handleSave = async () => {
  try {
    loading.value = true;
    const params = paramGroups.value.reduce((acc, group) => {
      group.items.forEach(param => {
        if (!param.isHide && param.value !== undefined) {
          acc[param.paramCode] = param.value
        }
      })
      return acc
    }, {} as Record<string, string>)

    await requestClient.post('/param/SaveParamConfig', {
      ...params,
      id: props.paramId,
    })
    message.success('保存成功');
  } finally {
    loading.value = false;
  }
}

</script>

<template>
  <div class="param-config">
    <div v-for="group in paramGroups" :key="group.name" class="param-group">
      <div class="group-header">
        <h3 class="group-title">{{ group.name }}</h3>
        <NTooltip v-if="group.description" trigger="hover">
          <template #trigger>
            <NIcon size="16" class="question-icon">
              <IconifyIcon icon="ant-design:question-circle-outlined" />
            </NIcon>
          </template>
          {{ group.description }}
        </NTooltip>
      </div>
      <NForm label-placement="left" label-width="120" size="small">
        <NFormItem v-for="param in group.items" :key="param.paramCode" :label="param.paramName" v-show="!param.isHide">
          <component :is="getInputComponent(param.paramType)" v-model:value="param.value"
            v-bind="getComponentProps(param)" />
        </NFormItem>
      </NForm>
    </div>
    <div class="action-buttons">
      <NSpace>
        <NButton type="primary" :loading="loading" @click="handleSave" v-access:code="['paramlist:save']">保存</NButton>
        <NButton @click="closeCurrentTab()">关闭</NButton>
      </NSpace>
    </div>
  </div>
</template>

<style scoped>
.param-config {
  padding: 12px;
}

.param-group {
  margin-bottom: 16px;
}

.group-header {
  display: flex;
  align-items: center;
  margin-bottom: 12px;
}

.group-title {
  margin: 0;
  margin-right: 6px;
  font-size: 14px;
  font-weight: 600;
}

.question-icon {
  color: #999;
  cursor: pointer;
}

.action-buttons {
  display: flex;
  justify-content: center;
  margin-top: 24px;
}

:deep(.n-form-item-label) {
  font-size: 13px;
}
</style>
