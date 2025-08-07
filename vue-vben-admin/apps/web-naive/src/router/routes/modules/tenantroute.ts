import type { RouteRecordRaw } from 'vue-router';

import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'ic:baseline-view-in-ar',
      keepAlive: true,
      order: 200,
      title: '租户管理' ,  
    },
    name: 'tenant',
    path: '/tenant',
    children: [
      {
        meta: {
          title: '数据库',
          authority:['tenantdatasource'],
        },
        name: 'datasource',
        path: '/tenant/datasource',
        component: () => import('#/views/tenants/datasource.vue'),
      },
      {
        meta: {
          title: '数据源表单',
          hideInMenu: true,
          authority:['tenantdatasource'],
          activePath: '/tenant/datasource',
        },
        name: 'datasourceform',
        path: '/tenant/datasourceform/:type/:id?',
        component: () => import('#/views/tenants/datasourceform.vue'),
      },
      {
        meta: {
          title: '租户套餐',
          maxNumOfOpenTab: 1,
          authority:['tenantpackage'],
        },
        name: 'tenantpackage',
        path: '/tenant/package',
        component: () => import('#/views/tenants/tenantpackage.vue'),
      },
      {
        meta: {
          title: '租户授权',
          maxNumOfOpenTab: 1,
          hideInMenu: true,
          authority:['tenantpackage'],
          activePath: '/tenant/package', // 新增
        },
        name: 'tenantpackagegrant',
        path: '/tenant/package/grant/:type/:id',
        component: () => import('#/views/tenants/tenantpackagegrant.vue'),
      },
      {
        meta: {
          title: '租户管理',
          maxNumOfOpenTab: 1,
          authority:['tenantlist'],
        },
        name: 'tenantlist',
        path: '/tenant/list',
        component: () => import('#/views/tenants/tenantlist.vue'),
      },
      {
        meta: {
          title: '租户表单',
          maxNumOfOpenTab: 1,
          authority:['tenantlist'],
          hideInMenu: true,
          activePath: '/tenant/list',
        },
        name: 'tenantform',
        path: '/tenant/:type/:id?',
        component: () => import('#/views/tenants/tenantform.vue'),
      },
    ],
  },
];

export default routes;
