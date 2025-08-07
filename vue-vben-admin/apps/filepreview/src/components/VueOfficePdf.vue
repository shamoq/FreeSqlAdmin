<template>
    <div>
        <vue-office-pdf
            :src="pdf"
            @rendered="renderedHandler"
            @error="errorHandler"
        />
    </div>
</template>

<script>
//引入VueOfficePdf组件
import VueOfficePdf from '@vue-office/pdf'
export default {
    name: "VueOfficePdfDemo",
    components: {
        VueOfficePdf
    },
    data() {
        return {
            loading: true,
            pdf: ''
        }
    },
    created() {
        this.updatePdfUrl();
    },
    // watch: {
    //     '$route.query.fileUrl': {
    //         handler() {
    //             this.updatePdfUrl();
    //         },
    //         immediate: true
    //     }
    // },
    methods: {
        updatePdfUrl() {
            const fileUrl = this.$route.query.fileUrl;
            if (fileUrl) {
                this.pdf = decodeURIComponent(fileUrl);
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