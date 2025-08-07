<script lang="ts" setup>
import { NSelect } from 'naive-ui';
import { ref, onMounted, watch } from 'vue';
import { requestClient } from '#/api/request';

const props = withDefaults(defineProps<{
  url: string;
  value?: string | number | null;
  initialLabel?: string;
  auto: boolean;
  optionsData:  [],
}>(), {
  auto: true
});

const emit = defineEmits(['update:value', 'change']);

const loading = ref(false);
const options = ref<any[]>([]);

// 首次加载信息
let isFirstLoad = true;
let firstOption: Array<{ label: string; value: string | number }> = [];

// 监听 optionsData 变化
watch(() => props.optionsData,
  (val: []) => {
    options.value = val;
    firstOption = val;
    isFirstLoad = false;
  },
  { immediate: true }
);

// 加载签约方数据
async function fetchOptions(search?: string) {
  if (!props.url) {
    return;
  }
  try {
    loading.value = true;
    const data = await requestClient.post(props.url, {
      page: 1,
      pageSize: 100,
      search: search,
    });
    options.value = data;
    if (isFirstLoad) {
      firstOption = data;
    }
  }finally {
    loading.value = false;
  }
}

// 监听搜索输入
function handleSearch(query: string) {
  fetchOptions(query);
}

function handleUpdateValue(val) {
  emit('update:value', val);
  const selectedOption = options.value?.find(opt => opt.value === val);
  emit('change', selectedOption || null);
}

// 自定义选项显示
function renderLabel(option) {
  if (props.value && props.initialLabel) {
    const values = Array.isArray(props.value) ? props.value : [props.value];
    if (values.includes(option.value) && !options.value.find(opt => opt.value === option.value)) {
      return props.initialLabel;
    }
  }
  return option.label;
}

function handleFocus() {
  console.log(options.value, firstOption);
  if(!options.value.length){
    options.value = firstOption;
  }
}

function handleBlur() {
  options.value = firstOption;
}

onMounted(() => {
  // 手动指定了options
  if (props.auto) {
    fetchOptions();
  }
});
</script>

<template>
  <div class="w-full">
    <NSelect v-bind="$attrs" :value="value" :options="options" :clearable="true" :filterable="true" :remote="true"
      @update:value="handleUpdateValue" @search="handleSearch" @focus="handleFocus" @blur="handleBlur"
      :render-label="renderLabel">
      <template #action>
        <!-- <NButton text type="primary" @click="handleAdd">新增签约方</NButton> -->
        <!-- <NButton class="ml-2" text type="primary" @click="handleAdd">新窗口打开</NButton> -->
      </template>
    </NSelect>
  </div>

</template>

<style scoped></style>
