import type { RouteRecordRaw } from 'vue-router';

import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'ic:baseline-view-in-ar',
      keepAlive: true,
      order: 9999,
      title: '系统管理' ,  
    },
    name: 'system',
    path: '/system',
    children: [

      {
        meta: {
          icon: 'ic:baseline-view-in-ar',
          keepAlive: true,
          title: '组织架构' ,  
        },
        name: 'usercenter',
        path: '/usercenter',
        children: [
          {
            meta: {
              title: '角色管理',
              authority:['role'],
            },
            name: 'systemrole',
            path: '/system/role',
            component: () => import('#/views/system/role.vue'),
          },
          {
            meta: {
              title: '用户管理' ,
              authority:['user'],
            },
            name: 'systemuser',
            path: '/system/user',
            component: () => import('#/views/system/user.vue'),
          },
          {
            meta: {
              title: '角色权限配置',
              hideInMenu: true,
              authority:['role'],
              fullPathKey: true,
              activePath: '/system/role',
            },
            name: 'rolegrant',
            path: '/system/rolegrant/:roleId',
            component: () => import('#/views/system/rolegrant.vue'),
          },
          {
            meta: {
              title: '用户信息',
              hideInMenu: true,
              maxNumOfOpenTab: 1,
              authority:['user'],
            },
            name: 'userform',
            path: '/system/userform/:type/:id?',
            component: () => import('#/views/system/userform.vue'),
          }
        ],
      },
      {
        meta: {
          icon: 'ic:baseline-view-in-ar',
          keepAlive: true,
          title: '日志管理' ,  
        },
        name: 'syslog',
        path: '/syslog',
        children:[
          {
            meta: {
              title: '登录日志',
              authority:['loginlog'],
            },
            name: 'loginlog',
            path: '/syslog/loginlog',
            component: () => import('#/views/system/loginlog.vue'),
          },
        ]
      },
      {
        meta: {
          title: '系统设置',
        },
        name: 'systemsetting',
        path: '/systemsetting',
        children:[
          {
            meta: {
              title: '参数设置',
              authority:['paramlist'],
            },
            name: 'paramlist',
            path: '/system/paramlist',
            component: () => import('#/views/system/paramlist.vue'),
          },
        ]
      },
      {
        meta: {
          title: '修改密码',
          hideInMenu: true,
        },
        name: 'resetpassword',
        path: '/user/resetpassword',
        component: () => import('#/views/system/resetpassword.vue'),
      },
    ],
  },
];

export default routes;
