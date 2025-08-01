<template>
   <div>
       <vue-office-docx
           :src="docx"
           style="height: 100vh;"
           @rendered="renderedHandler"
           @error="errorHandler"
       />
   </div>
</template>

<script>
//引入VueOfficeDocx组件
import VueOfficeDocx from '@vue-office/docx'
//引入相关样式
import '@vue-office/docx/lib/index.css'
export default {
    name: "VueOfficeDocxDemo",
    components: {
        VueOfficeDocx
    },
    data() {
        return {
            loading: true,
            docx: ''
        }
    },
    created() {
        this.updateDocxUrl();
    },
    // watch: {
    //     '$route.query.fileUrl': {
    //         handler() {
    //             this.updateDocxUrl();
    //         },
    //         immediate: true
    //     }
    // },
    methods: {
        updateDocxUrl() {
            const fileUrl = this.$route.query.fileUrl;
            if (fileUrl) {
                this.docx = decodeURIComponent(fileUrl);
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