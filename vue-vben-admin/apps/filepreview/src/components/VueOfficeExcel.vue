<template>
   <div>
       <vue-office-excel
           :src="excel"
           :options="options"
           style="height: 100vh;"
           @rendered="renderedHandler"
           @error="errorHandler"
       />
   </div>
</template>

<script>
//引入VueOfficeExcel组件
import VueOfficeExcel from '@vue-office/excel'
//引入相关样式
import '@vue-office/excel/lib/index.css'
export default {
    name: "VueOfficeExcelDemo",
    components: {
        VueOfficeExcel
    },
    data() {
        return {
            loading: true,
            options:{
                xls: false,
                minColLength: 0,
                minRowLength: 0,
                widthOffset: 10,
                heightOffset: 10,
                beforeTransformData: (workbookData) => {return workbookData},
                transformData: (workbookData) => {return workbookData},
            },
            excel: ''
        }
    },
    created() {
        this.updateExcelUrl();
    },
    // watch: {
    //     '$route.query.fileUrl': {
    //         handler() {
    //             this.updateExcelUrl();
    //         },
    //         immediate: true
    //     }
    // },
    methods: {
        updateExcelUrl() {
            const fileUrl = this.$route.query.fileUrl;
            if (fileUrl) {
                this.excel = decodeURIComponent(fileUrl);
            }
        },
        renderedHandler() {
            this.loading = false;
            console.log("渲染完成")
        },
        errorHandler() {
            this.loading = false;
            console.log("渲染失败")
        }
    }
};
</script>

<style scoped>

</style>