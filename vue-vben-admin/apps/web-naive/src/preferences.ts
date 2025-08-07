import { defineOverridesPreferences } from '@vben/preferences';
import { useAppConfig } from '@vben/hooks';
const { VITE_APP_ENABLE_PERFERENCE } = useAppConfig(import.meta.env, import.meta.env.PROD);

/**
 * @description 项目配置文件
 * 只需要覆盖项目中的一部分配置，不需要的配置不用覆盖，会自动使用默认配置
 * !!! 更改配置后请清空缓存，否则可能不生效
 */
export const overridesPreferences = defineOverridesPreferences({
  // overrides
  app: {
    name: import.meta.env.VITE_APP_TITLE,
    locale: 'zh-CN',
    enablePreferences: VITE_APP_ENABLE_PERFERENCE,
},
  copyright: {
    companyName: '',
    companySiteLink: '',
    date: '2025',
    enable: true,
    icp: '',
    icpLink: '',
  },
  theme: {
    mode: "light"
  },
  widget: {
    languageToggle: false,
    globalSearch: false,
    "notification": false,
    "sidebarToggle": false
  },
  "shortcutKeys": {
    "globalSearch": false
  },
  "tabbar": {
    "enable": false,
  },

});
