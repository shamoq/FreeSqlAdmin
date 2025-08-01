<script lang="ts" setup>
import { NTree, NCard, NSpace, NButton, useMessage } from "naive-ui";
import { ref, onMounted, h } from "vue";
import { Page } from "@vben/common-ui";
import { requestClient } from "#/api/request";
import { useRoute, useRouter } from 'vue-router';
import contractRights from '#/resources/contractrights.json';

const route = useRoute();
const router = useRouter();

interface TreeNode {
  key: string;
  label: string;
  children?: TreeNode[];
  actions?: TreeNode[];
  parentKey?: string;
  checked?: boolean;
}

const message = useMessage();
const loading = ref(false);
const treeData = ref<TreeNode[]>([]);
const checkedKeys = ref<string[]>([]);
const roleId = route.params.roleId;
const roleName = ref<string>();

// 将权限数据转换为树形结构
function convertToTreeData(rights: any[]): TreeNode[] {
  // 先找出所有的导航节点
  const navNodes = rights.filter(item => item.type === 'nav');

  return navNodes.map(nav => {
    // 找出当前导航下的所有菜单
    const menuNodes = rights.filter(item => item.type === 'menu' && item.parent === nav.code);

    return {
      key: nav.code,
      label: nav.name,
      children: menuNodes.map(menu => {
        // 找出当前菜单下的所有操作
        const actionNodes = rights.filter(item => item.type === 'action' && item.parent === menu.code);

        return {
          key: menu.code,
          label: menu.name,
          children: actionNodes.map(action => ({
            key: action.code,
            label: action.name
          }))
        };
      })
    };
  });
}

// 初始化树形数据
onMounted(() => {
  treeData.value = convertToTreeData(contractRights);
  fetchRoleGrants(roleId);
});

// 加载角色权限数据
async function fetchRoleGrants(roleId: any) {
  try {
    loading.value = true;
    // 这里应该调用后端API获取角色权限数据
    const data = await requestClient.post('/role/getGrants', { id: roleId });
    const { rights, name } = data;
    // 将rights数组中的menuCode和actionCode组合成key
    checkedKeys.value = rights.map((item: any) => {
      if (item.actionCode) {
        return `${item.menuCode}:${item.actionCode}`;
      }
      return item.menuCode;
    }) || [];
    roleName.value = name;
  } catch (error) {
  } finally {
    loading.value = false;
  }
}

// 保存角色权限
async function handleSave() {
  if (!roleId) {
    message.warning('请先选择角色');
    return;
  }

  try {
    loading.value = true;
    // 处理选中的权限数据
    const grants = checkedKeys.value.map(key => {
      const [menuCode, actionCode] = key.split(':');
      // 从树形数据中查找对应的节点名称
      const findNodeName = (code: string) => {
        const findNode = (nodes: TreeNode[]): string | undefined => {
          for (const node of nodes) {
            if (node.key === code) return node.label;
            if (node.children) {
              const found = findNode(node.children);
              if (found) return found;
            }
          }
          return undefined;
        };
        return findNode(treeData.value);
      };

      const menuName = findNodeName(menuCode);
      const actionName = actionCode ? findNodeName(actionCode) : undefined;

      return {
        menuCode,
        menuName,
        actionCode: key,
        actionName,
        application: 'appContract'
      };
    });

    await requestClient.post('/role/grant', {
      roleId,
      rights: grants
    });
    message.success('保存成功');
  } catch (error) {
  } finally {
    loading.value = false;
  }
}

</script>

<template>
  <Page title="角色授权" description="配置角色的菜单和按钮权限">
    <NCard :title="roleName">
      <template #header-extra>
        <NSpace>
          <NButton type="primary" :loading="loading" @click="handleSave">保存</NButton>
          <NButton @click="router.back()">返回</NButton>
        </NSpace>
      </template>
      <NTree
        :data="treeData"
        checkable
        cascade
        :checked-keys="checkedKeys"
        :default-expand-all="true"
        @update:checked-keys="(keys) => checkedKeys = keys"
      />
    </NCard>
  </Page>
</template>
