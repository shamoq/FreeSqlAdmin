<script lang="ts" setup>
import { ref, reactive, onMounted } from 'vue';
import { Page } from "@vben/common-ui";
import { useMessage } from 'naive-ui';
import type { FormInst, FormRules } from 'naive-ui';
import { NForm, NFormItem, NInput, NSelect, NDatePicker, NButton, NSpace, NCard, NUpload } from 'naive-ui';
import type { Tenant } from '#/api/entity/Tenant';
import type { TenantPackage } from '#/api/entity/TenantPackage';
import { requestClient } from '#/api/request';
import { useRouter, useRoute } from 'vue-router';
import type { TenantDataSource } from '#/api/entity/TenantDataSource';
import { TenantEnvTypeEnumOption } from '#/api/enum/TenantEnvTypeEnum';
import { formatDate } from '@vben/utils';

const message = useMessage();
const formRef = ref<FormInst | null>(null);
const loading = ref(false);
const router = useRouter();
const route = useRoute();
const isViewMode = ref(route.params.type === 'view');

const tenantPackages = ref<{ label: string; value: string }[]>([]);

// 数据库列表
const dataSources = ref<{ label: string; value: string }[]>([]);

// 表单数据
const formData = reactive<Partial<Tenant>>({
    name: '',
    code: '',
    type: 'Prod',
    description: '',
    isEnable: 1,
    logo: '',
    dataSourceId: null,
    expirationTime: null,
    tenantPackageId: ''
});

// 状态选项
const statusOptions = [
    { label: '启用', value: 1 },
    { label: '禁用', value: 0 }
];

// 表单验证规则
const rules: FormRules = {
    name: {
        required: true,
        message: '请输入租户名称',
        trigger: 'blur'
    },
    code: {
        required: true,
        message: '请输入租户编码',
        trigger: 'blur'
    },
    type: {
        required: true,
        message: '请选择租户类型',
        trigger: 'change'
    },
    tenantPackageId: {
        required: true,
        message: '请选择租户套餐',
        trigger: 'change'
    },
    dataSourceId: {
        required: true,
        message: '请选择数据库',
        trigger: 'change'
    },
    expirationTime: {
        required: true,
        message: '请选择过期时间',
        trigger: ['blur', 'change']
    }
};

// 获取租户套餐列表
async function fetchTenantPackages() {
    try {
        const data = await requestClient.post('/tenantpackage/list', {});
        tenantPackages.value = data.map((item: TenantPackage) => ({
            label: item.name || '',
            value: item.id || ''
        }));
    } catch (error) {
        message.error('获取租户套餐列表失败');
    }
}

// 获取数据库列表
async function fetchDataSources() {
    try {
        const data = await requestClient.post('/tenantdatasource/option', {});
        dataSources.value = data;
    } catch (error) {
        message.error('获取数据库列表失败');
    }
}

// 获取数据库名称
function getDataSourceName(dataSourceId: string | null | undefined) {
    if (!dataSourceId) return '';
    const dataSource = dataSources.value.find(item => item.value === dataSourceId);
    return dataSource?.label || '';
}

// 获取租户套餐名称
function getTenantPackageName(tenantPackageId: string | null | undefined) {
    if (!tenantPackageId) return '';
    const tenantPackage = tenantPackages.value.find(item => item.value === tenantPackageId);
    return tenantPackage?.label || '';
}

// 获取租户详情
async function fetchTenantDetail(id: string) {
    try {
        loading.value = true;
        let data = await requestClient.post('/tenant/get', { id });
        // 租户过期时间格式化
        if(data.expirationTime){
          data.expirationTime = formatDate(data.expirationTime, 'YYYY-MM-DD')
        }
        Object.assign(formData, data);
    } catch (error) {
        message.error('获取租户详情失败');
    } finally {
        loading.value = false;
    }
}

// 提交表单
async function handleSubmit() {
    if (!formRef.value) return;

    try {
        await formRef.value.validate();
        loading.value = true;

        await requestClient.post('/tenant/save', formData);
        message.success(formData.id ? '修改成功' : '新增成功');
        router.push({ name: 'tenantlist' });
    } catch (error) {
        // 表单验证失败
    } finally {
        loading.value = false;
    }
}

// 取消
function handleCancel() {
    router.back();
}

onMounted(async () => {
    await Promise.all([
        fetchTenantPackages(),
        fetchDataSources()
    ]);
    const id = route.params.id;
    if (id) {
        await fetchTenantDetail(id as string);
    }
});
</script>

<template>
    <Page>
        <NCard :title="formData.id ? (isViewMode ? '租户详情' : '编辑租户') : '新增租户'">

            <NForm ref="formRef" :model="formData" :rules="rules" label-placement="left" label-width="auto"
                require-mark-placement="right-hanging">
                <NFormItem label="租户类型" path="type">
                    <NSelect v-if="!isViewMode" v-model:value="formData.type" :options="TenantEnvTypeEnumOption" placeholder="请选择租户类型" :filterable="true"  />
                    <span v-else>{{ formData.type }}</span>
                </NFormItem>

                <NFormItem label="租户名称" path="name">
                    <NInput v-if="!isViewMode" v-model:value="formData.name" placeholder="请输入租户名称" />
                    <span v-else>{{ formData.name }}</span>
                </NFormItem>

                <NFormItem label="租户编码" path="code">
                    <NInput v-if="!isViewMode" v-model:value="formData.code" placeholder="请输入租户编码" />
                    <span v-else>{{ formData.code }}</span>
                </NFormItem>

                <NFormItem label="数据库" path="dataSourceId">
                    <NSelect v-if="!isViewMode" :value="formData.dataSourceId" @update:value="(val) => formData.dataSourceId = val"
                        :options="dataSources" placeholder="请选择数据库" :disabled="!!formData.id" :filterable="true" />
                    <span v-else>{{ getDataSourceName(formData.dataSourceId) }}</span>
                </NFormItem>

                <NFormItem label="租户套餐" path="tenantPackageId">
                    <NSelect v-if="!isViewMode" v-model:value="formData.tenantPackageId" :options="tenantPackages" placeholder="请选择租户套餐" :filterable="true"  />
                    <span v-else>{{ getTenantPackageName(formData.tenantPackageId) }}</span>
                </NFormItem>

                <NFormItem label="租户描述" path="description">
                    <NInput v-if="!isViewMode" v-model:value="formData.description" type="textarea" placeholder="请输入租户描述"
                        :autosize="{ minRows: 3, maxRows: 5 }" />
                    <span v-else>{{ formData.description }}</span>
                </NFormItem>

                <NFormItem label="租户状态" path="isEnable">
                    <NSelect v-if="!isViewMode" :value="formData.isEnable" @update:value="(val) => formData.isEnable = val"
                        :options="statusOptions" />
                    <span v-else>{{ formData.isEnable === 1 ? '启用' : '禁用' }}</span>
                </NFormItem>

                <NFormItem label="过期时间" path="expirationTime">
                    <NDatePicker v-if="!isViewMode" v-model:formatted-value="formData.expirationTime" type="date" clearable placeholder="请选择合同开始日期" />
                    <span v-else>{{ formData.expirationTime }}</span>
                </NFormItem>

                <div class="flex justify-end space-x-4">
                    <NButton @click="handleCancel">返回</NButton>
                    <NButton v-if="!isViewMode" type="primary" :loading="loading" @click="handleSubmit"  v-access:code="['tenantlist:add','tenantlist:edit']">
                        确定
                    </NButton>
                </div>
            </NForm>
        </NCard>
    </Page>
</template>

<style scoped>
.flex {
    display: flex;
}

.justify-end {
    justify-content: flex-end;
}

.space-x-4>*+* {
    margin-left: 1rem;
}
</style>
