import { useTabs } from '@vben/hooks';

export function useHandleBack(router: any) {
    const { closeCurrentTab } = useTabs();
    
    function handleBack() {
        // router.back();
        // closeCurrentTab();
    }

    return {
        handleBack,
    };
}