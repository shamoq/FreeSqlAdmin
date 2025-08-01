<script lang="ts" setup>
import type { VxeGridProps } from '#/adapter/vxe-table';
import { Page } from '@vben/common-ui';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { requestClient } from "#/api/request";
import { NButton, NCard, NSpace, useMessage, useDialog, NPopconfirm, NButtonGroup, NModal, NTag, NTooltip } from "naive-ui";
import { ref, onMounted } from "vue";
import { useRouter } from 'vue-router';
import CustomBaseForm from './custombaseform.vue'
import BzTemplateList from './bztemplateselect.vue'

const loading = ref(false);
const router = useRouter();
const customBaseFormRef = ref();
const bzTemplateListRef = ref();

async function fetchList(params) {
    try {
        loading.value = true;

        const { currentPage, pageSize } = params.page;

        var filters = [];
        const formValues = await gridApi.formApi.getValues();
        if (formValues?.name) {
            filters.push({ field: 'name', value: formValues.name });
        }
        if (formValues?.remark) {
            filters.push({ field: 'remark', value: formValues.remark });
        }

        const searchParams = {
            page: currentPage,
            pageSize: pageSize,
            filters,
        };



        const data = await requestClient.post(
            "/customtemplate/page",
            searchParams,
        );
        return data;
    } finally {
        loading.value = false;
    }
}

const formOptions = ({
    collapsed: true,
    schema: [
        {
            component: "Input",
            componentProps: {
                placeholder: "请输入模板名称",
            },
            fieldName: "name",
            label: "模板名称",
        },
        {
            component: "Input",
            componentProps: {
                placeholder: "请输入模板说明",
            },
            fieldName: "remark",
            label: "模板说明",
        },
    ],
    showCollapseButton: true,
    submitButtonOptions: {
        content: "查询",
        size: 'small',
    },
    resetButtonOptions: {
        content: "重置",
        size: 'small',
        show: true,
    },
    submitOnEnter: true,
});


const gridOptions: VxeGridProps = {
    checkboxConfig: {
        highlight: true,
        labelField: 'name',
    },
    columns: [
        { type: "seq", width: 70, align: "center", title: "序号" },
        {
            field: "name",
            title: "模版名称",
            align: "left",
            slots: { default: 'name' },
        },
        {
            field: "templateTypeText",
            title: "模版类型",
            align: "left",
        },
        {
            field: "remark",
            title: "说明",
            align: "left",
        },
        {
            field: "creator",
            title: "创建人",
            align: "left",
            width: 180,
        },
        {
            field: "createdTime",
            title: "创建时间",
            align: "center",
            width: 160,
        },
    ],
    exportConfig: {},
    height: 'auto',
    keepSource: true,
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
        defaultSort: { field: 'category', order: 'desc' },
        remote: true,
    },
    toolbarConfig: {
        slots: {
            tools: 'toolbar_tools',
        },
    },
};

const [Grid, gridApi] = useVbenVxeGrid({
    gridOptions,
    formOptions,
    // showSearchForm: false
});

function handleView(row) {
    router.push({
        name: 'customtemplateform',
        params: { id: row.id, type: 'edit' }
    });
}

function handleIntroduceSuccess(){
    gridApi.reload();
}

onMounted(() => {

});

</script>

<template>
    <Page auto-content-height>
        <Grid>
            <template #toolbar_tools>
                <NSpace>
                    <NButton type="primary" @click="customBaseFormRef.show()" v-access:code="['custom:add']">新建模板</NButton>
                    <NButton type="primary" @click="bzTemplateListRef.show({ type:'custom'})" v-access:code="['custom:introduce']">引入范本合同</NButton>
                </NSpace>
            </template>
            <template #name="{ row }">
                <a class="contract-name-link" @click="handleView(row)"  v-access:code="['custom:edit']">{{ row.name }}</a>
            </template>
        </Grid>

        <CustomBaseForm ref="customBaseFormRef" />
        <BzTemplateList ref="bzTemplateListRef" @success="handleIntroduceSuccess()"/>
    </Page>
</template>



<!-- 添加样式 -->
<style scoped>
.n-button-group .n-button {
    padding: 0 12px;
}

.contract-name-link {
    color: #2080f0;
    cursor: pointer;
    text-decoration: none;
}

.contract-name-link:hover {
    text-decoration: underline;
}
</style>