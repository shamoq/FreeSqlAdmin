<script lang="ts" setup>
import type { VxeGridListeners, VxeGridProps } from "#/adapter/vxe-table";
import { NButton, NCard, useMessage } from "naive-ui";
import { useVbenVxeGrid } from "#/adapter/vxe-table";
import type { VbenFormProps } from "#/adapter/form";
import { ref, reactive, onMounted } from "vue";
import { Page } from "@vben/common-ui";
import { requestClient } from "#/api/request";
import type { SysLoginLog } from "#/api/entity/SysLoginLog";

const loading = ref(false);
const message = useMessage();

async function fetchList(params: Record<string, any>): Promise<any> {
  try {
    loading.value = true;
    // 获取表单值
    const formValues: Record<string, any> = await gridApi.formApi.getValues();
    const filters: any[] = [];
    if (formValues?.userName) {
      filters.push({ field: 'userName', op: 'like', value: formValues.userName });
    }
    if (formValues?.userCode) {
      filters.push({ field: 'userCode', op: 'like', value: formValues.userCode });
    }
    if (formValues?.loginIp) {
      filters.push({ field: 'loginIp', op: 'like', value: formValues.loginIp });
    }
    if (formValues?.device) {
      filters.push({ field: 'device', op: 'like', value: formValues.device });
    }
    if (formValues?.browser) {
      filters.push({ field: 'browser', op: 'like', value: formValues.browser });
    }
    if (formValues?.loginTimeRange) {
      if (formValues.loginTimeRange[0]) {
        filters.push({ field: 'loginTime', op: '>=', value: formValues.loginTimeRange[0] });
      }
      if (formValues.loginTimeRange[1]) {
        filters.push({ field: 'loginTime', op: '<=', value: formValues.loginTimeRange[1] });
      }
    }
    const { currentPage, pageSize } = params.page;
    const searchParams: Record<string, any> = {
      page: currentPage,
      pageSize: pageSize,
      filters,
    };
    const data: any = await requestClient.post(
      "/sysloginlog/page",
      searchParams,
    );
    return data;
  } finally {
    loading.value = false;
  }
}

const formOptions: VbenFormProps = ({
  collapsed: true,
  schema: [
    {
      component: "Input",
      componentProps: {
        placeholder: "请输入用户名",
      },
      fieldName: "userName",
      label: "用户名",
    },
    {
      component: "Input",
      componentProps: {
        placeholder: "请输入用户编码",
      },
      fieldName: "userCode",
      label: "用户编码",
    },
    {
      component: "Input",
      componentProps: {
        placeholder: "请输入登录IP",
      },
      fieldName: "loginIp",
      label: "登录IP",
    },
    {
      component: "Input",
      componentProps: {
        placeholder: "请输入设备信息",
      },
      fieldName: "device",
      label: "设备",
    },
    {
      component: "Input",
      componentProps: {
        placeholder: "请输入浏览器信息",
      },
      fieldName: "browser",
      label: "浏览器",
    },
    {
      component: "DatePicker",
      componentProps: {
        type: "daterange",
        clearable: true,
        placeholder: "开始时间,结束时间",
      },
      fieldName: "loginTimeRange",
      label: "登录时间",
    },
  ],
  showCollapseButton: true,
  submitButtonOptions: {
    content: "查询"
  },
  resetButtonOptions: {
    content: "重置",
    show: true,
  },
  submitOnEnter: true,
});

const gridOptions: VxeGridProps<SysLoginLog> = reactive({
  border: true,
  stripe: true,
  loading: loading.value,
  columnConfig: {
    resizable: true,
  },
  rowConfig: {
    isHover: true,
  },
  columns: [
    { type: "seq", width: 70, align: "left" },
    { field: "userName", title: "用户名", align: "left" },
    { field: "userCode", title: "用户编码", align: "left" },
    { field: "loginTime", title: "登录时间", align: "center", width: 200 },
    { field: "loginIp", title: "登录IP", align: "left", width: 140 },
    { field: "device", title: "设备", align: "left" },
    { field: "browser", title: "浏览器", align: "left" },
    { field: "screenResolution", title: "分辨率", align: "left" },
  ],
  data: [],
  pagerConfig: {
    enabled: true,
    pageSize: 10,
  },
  proxyConfig: {
    autoload: false,
    response: {
      result: 'data',
      total: 'total',
      list: 'data',
    },
    ajax: {
      query: fetchList
    },
    sort: true,
  },
  sortConfig: {
    multiple: true,
  },
});

const gridEvents: VxeGridListeners<SysLoginLog> = {};

const [Grid, gridApi] = useVbenVxeGrid({ gridEvents, gridOptions, formOptions });

onMounted(() => {
  gridApi.reload({});
});
</script>

<template>
  <Page>
    <NCard>
      <Grid ref="gridRef" />
    </NCard>
  </Page>
</template>
