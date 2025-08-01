import type { Router } from 'vue-router';

import { LOGIN_PATH } from '@vben/constants';
import { preferences } from '@vben/preferences';
import { useAccessStore, useUserStore } from '@vben/stores';
import { startProgress, stopProgress } from '@vben/utils';

import { accessRoutes, coreRouteNames } from '#/router/routes';
import { useAuthStore } from '#/store';

import { generateAccess } from './access';

/**
 * 通用守卫配置
 * @param router
 */
function setupCommonGuard(router: Router) {
  // 记录已经加载的页面
  const loadedPaths = new Set<string>();

  router.beforeEach((to) => {
    to.meta.loaded = loadedPaths.has(to.path);

    // 页面加载进度条
    if (!to.meta.loaded && preferences.transition.progress) {
      startProgress();
    }
    return true;
  });

  router.afterEach((to) => {
    // 记录页面是否加载,如果已经加载，后续的页面切换动画等效果不在重复执行

    loadedPaths.add(to.path);

    // 关闭页面加载进度条
    if (preferences.transition.progress) {
      stopProgress();
    }
  });
}

/**
 * 权限访问守卫配置
 * @param router
 */
function setupAccessGuard(router: Router) {
  router.beforeEach(async (to, from) => {
    const accessStore = useAccessStore();
    const userStore = useUserStore();
    const authStore = useAuthStore();

    // 基本路由，这些路由不需要进入权限拦截
    if (to.name !== 'Root' && coreRouteNames.includes(to.name as string)) {
      if (to.path === LOGIN_PATH && accessStore.accessToken) {
        return decodeURIComponent(
          (to.query?.redirect as string) ||
          userStore.userInfo?.homePath ||
          preferences.app.defaultHomePath,
        );
      }
      return true;
    }

    // accessToken 检查
    if (!accessStore.accessToken) {
      // 明确声明忽略权限访问权限，则可以访问
      if (to.meta.ignoreAccess) {
        return true;
      }

      // 没有访问权限，跳转登录页面
      if (to.fullPath !== LOGIN_PATH) {
        return {
          path: LOGIN_PATH,
          // 如不需要，直接删除 query
          query:
            to.fullPath === preferences.app.defaultHomePath
              ? {}
              : { redirect: encodeURIComponent(to.fullPath) },
          // 携带当前跳转的页面，登录后重新跳转该页面
          replace: true,
        };
      }
      return to;
    }

    // 是否已经生成过动态路由
    if (accessStore.isAccessChecked) {
      return true;
    }

    // 生成路由表
    // 当前登录用户拥有的角色标识列表
    const userInfo = userStore.userInfo || (await authStore.fetchUserInfo());
    const userRoles = userInfo.roles ?? [];
    // const userRoles = userInfo.roles ?? [];
    // 获取权限码
    const [actionCodes, menuCodes] = await authStore.fetchAccessCodes();
    // 保存权限码
    accessStore.setAccessCodes(actionCodes);

    // 生成菜单和路由
    const { accessibleMenus, accessibleRoutes } = await generateAccess({
      roles: menuCodes,  // 使用菜单权限码替代角色权限码
      router,
      // 则会在菜单中显示，但是访问会被重定向到403
      routes: accessRoutes,
    });

    // 如果菜单children为空，则移除菜单
    const menus = accessibleMenus.filter((item) => item.children?.length);

    // 保存菜单信息和路由信息
    accessStore.setAccessMenus(menus);
    accessStore.setAccessRoutes(accessibleRoutes);
    accessStore.setIsAccessChecked(true);

    // 计算目标路由
    let redirectPath = from.query.redirect as string;
    if (!redirectPath) { // 没有指定路径的情况下，根目录则获取默认路径
      if (to.name === 'Root') {
        const menus = accessStore.accessMenus;
        const firstMenu = menus[0]?.children?.[0] || menus[0];
        redirectPath = firstMenu?.path as string;
      } else {
        redirectPath = to.path;
      }

      // 无法计算出来时，直接403
      if (!redirectPath) {
        redirectPath = "/403";
      }
    } else {
      redirectPath = decodeURIComponent(redirectPath);
      if (redirectPath == '/') {
        const menus = accessStore.accessMenus;
        const firstMenu = menus[0]?.children?.[0] || menus[0];
        redirectPath = firstMenu?.path as string;
      }
      // 读取完毕后删掉这个参数，避免死循环
      // delete from.query.redirect;
    }

    return {
      ...router.resolve(decodeURIComponent(redirectPath as string)),
      replace: true,
    };
  });
}

/**
 * 项目守卫配置
 * @param router
 */
function createRouterGuard(router: Router) {
  /** 通用 */
  setupCommonGuard(router);
  /** 权限访问 */
  setupAccessGuard(router);
}

export { createRouterGuard };
