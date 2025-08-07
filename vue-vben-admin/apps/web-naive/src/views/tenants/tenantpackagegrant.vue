<script lang="ts" setup>
import {  NCard, NSpace, NButton, useMessage, NCheckbox, NDivider, NButtonGroup } from "naive-ui";
import { ref, onMounted, computed } from "vue";
import { Page } from "@vben/common-ui";
import { requestClient } from "#/api/request";
import { useRoute, useRouter } from 'vue-router';
import type { TenantAppRightDto } from '#/api/dto/TenantAppRightDto';
 
const route = useRoute();
const router = useRouter();
const message = useMessage();
const loading = ref(false);
const checkedKeys = ref<string[]>([]);
const id = route.params.id;
const packageName = ref<string>();
const tenantApps = ref<TenantAppRightDto[]>([]);

// 当前选中的应用
const currentApp = ref<TenantAppRightDto>({
    name: '',
    code: '',
    order: 0,
    children: [],
    actions: []
});
 
// 根据路由参数判断是否为只读模式
const isReadOnly = computed(() => route.params.type === 'view');
 
// 处理权限选中状态变化
function handleCheckChange(nav: TenantAppRightDto, menu: TenantAppRightDto | null, action: TenantAppRightDto | null, checked: boolean) {
    // 只读模式下不允许操作
    if (isReadOnly.value) {
        return;
    }
    
    const keys = [...checkedKeys.value];

    if (action) {
        // 处理操作权限
        if (checked) {
            !keys.includes(action.code) && keys.push(action.code);
        } else {
            const index = keys.indexOf(action.code);
            index > -1 && keys.splice(index, 1);
        }

        // 联动菜单选中状态
        const menuChecked = menu?.actions.some(a => keys.includes(a.code));
        if (menuChecked) {
            !keys.includes(menu!.code) && keys.push(menu!.code);
        } else {
            const index = keys.indexOf(menu!.code);
            index > -1 && keys.splice(index, 1);
        }

        // 联动导航选中状态
        const navChecked = nav.children.some(m => keys.includes(m.code));
        if (navChecked) {
            !keys.includes(nav.code) && keys.push(nav.code);
        } else {
            const index = keys.indexOf(nav.code);
            index > -1 && keys.splice(index, 1);
        }
    } else if (menu) {
        // 处理菜单权限
        if (checked) {
            !keys.includes(menu.code) && keys.push(menu.code);
            // 选中所有子操作
            menu.actions.forEach(action => {
                !keys.includes(action.code) && keys.push(action.code);
            });
        } else {
            const index = keys.indexOf(menu.code);
            index > -1 && keys.splice(index, 1);
            // 取消所有子操作
            menu.actions.forEach(action => {
                const actionIndex = keys.indexOf(action.code);
                actionIndex > -1 && keys.splice(actionIndex, 1);
            });
        }

        // 联动导航选中状态
        const navChecked = nav.children.some(m => keys.includes(m.code));
        if (navChecked) {
            !keys.includes(nav.code) && keys.push(nav.code);
        } else {
            const index = keys.indexOf(nav.code);
            index > -1 && keys.splice(index, 1);
        }
    } else if (nav) {
        // 处理导航权限
        if (checked) {
            !keys.includes(nav.code) && keys.push(nav.code);
            // 选中所有子菜单和操作
            nav.children.forEach(menu => {
                !keys.includes(menu.code) && keys.push(menu.code);
                menu.actions.forEach(action => {
                    !keys.includes(action.code) && keys.push(action.code);
                });
            });
        } else {
            const index = keys.indexOf(nav.code);
            index > -1 && keys.splice(index, 1);
            // 取消所有子菜单和操作
            nav.children.forEach(menu => {
                const menuIndex = keys.indexOf(menu.code);
                menuIndex > -1 && keys.splice(menuIndex, 1);
                menu.actions.forEach(action => {
                    const actionIndex = keys.indexOf(action.code);
                    actionIndex > -1 && keys.splice(actionIndex, 1);
                });
            });
        }
    }

    checkedKeys.value = keys;
}

// 切换应用
function handleAppChange(app: TenantAppRightDto) {
  currentApp.value = app;
}

// 加载套餐权限数据
async function fetchPackageGrants() {
    try {
        loading.value = true;
        const data = await requestClient.post('/tenantPackage/getGrants', { id });
        tenantApps.value = data.tenantRights;
        checkedKeys.value = data.actionCodes;
        packageName.value = data.name;
        currentApp.value = data.tenantRights[0];
    }finally {
        loading.value = false;
    }
}

// 保存权限
async function handleSave() {
    if (!id) {
        message.warning('请先选择套餐');
        return;
    }

    try {
        loading.value = true;
       
        await requestClient.post('/tenantPackage/grant', {
            tenantPackageId: id,
            actionCodes: checkedKeys.value
        });
        message.success('保存成功');
    } catch (error) {
        message.error('保存失败');
    } finally {
        loading.value = false;
    }
}

onMounted(() => {
    fetchPackageGrants();
});
</script>

<template>
    <Page>
        <NCard :title="packageName">
            <template #header-extra>
                <NSpace>
                    <NButton v-if="!isReadOnly" type="primary" :loading="loading" @click="handleSave" v-access:code="['tenantpackage:grant']">保存</NButton>
                    <NButton @click="router.back()">返回</NButton>
                </NSpace>
            </template>

            <NButtonGroup class="mb-4">
                <NButton 
                    v-for="app in tenantApps" 
                    :key="app.code"
                    size="small"
                    :type="currentApp.code === app.code ? 'primary' : 'default'"
                    @click="handleAppChange(app)"
                >
                    {{ app.name }}
                </NButton>
            </NButtonGroup>

            <div class="permission-list">
                <div 
                    v-for="nav in currentApp.children" 
                    :key="nav.code" 
                    class="nav-section"
                >
                    <h3>
                        <NCheckbox
                            :checked="checkedKeys.includes(nav.code)"
                            :label="nav.name"
                            @update:checked="(checked) => handleCheckChange(nav, null, null, checked)"
                            :disabled="isReadOnly"
                        />
                    </h3>
                    <div 
                        v-for="menu in nav.children" 
                        :key="menu.code" 
                        class="menu-section"
                    >
                        <div class="menu-header">
                            <span class="menu-name">
                                <NCheckbox
                                    :checked="checkedKeys.includes(menu.code)"
                                    :label="menu.name"
                                    @update:checked="(checked) => handleCheckChange(nav, menu, null, checked)"
                                    :disabled="isReadOnly"
                                />
                            </span>
                            <NDivider vertical style="margin: 0 8px" />
                            <div class="action-list">
                                <NCheckbox
                                    v-for="action in menu.actions"
                                    :key="action.code"
                                    :checked="checkedKeys.includes(action.code)"
                                    :label="action.name"
                                    @update:checked="(checked) => handleCheckChange(nav, menu, action, checked)"
                                    :disabled="isReadOnly"
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </NCard>
    </Page>
</template>

<style scoped>
.permission-list {
    padding: 16px;
}

.mb-4 {
    margin-bottom: 16px;
}

.nav-section {
    margin-bottom: 16px;
}

.nav-section h3 {
    margin-bottom: 8px;
    font-size: 20px;
    font-weight: bold;
    color: #333;
}

.menu-section {
    margin-bottom: 4px;
    margin-left: 24px;
    border-bottom: 1px solid #ddd;
}

.menu-header {
    display: flex;
    gap: 24px;
    align-items: center;
    padding: 8px 0;
}

.menu-name {
    flex-shrink: 0;
    min-width: 160px;
    font-size: 14px;
    font-weight: 500;
}

.action-list {
    display: flex;
    flex: 1;
    flex-wrap: wrap;
    gap: 16px;
}
</style>
