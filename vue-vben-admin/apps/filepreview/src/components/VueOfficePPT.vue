<template>
    <div>
        <vue-office-pptx
        :src="pdf"
        style="height: 100vh"
        @rendered="renderedHandler"
        @error="errorHandler"
        />
    </div>
</template>

<script>
import VueOfficePptx from '@vue-office/pptx'

export default {
    components: {
        VueOfficePptx
    },
    data() {
        return {
            // pdf: '',
            pdf: 'https://501351981.github.io/vue-office/examples/dist/static/test-files/test.pptx' //设置文档地址
        }
    },
    created() {
       // this.updatePPTUrl();
    },
    methods: {
        updatePPTUrl(){
            const fileUrl = this.$route.query.fileUrl;
            if (fileUrl) {
                this.pdf = decodeURIComponent(fileUrl);
            }
        },
        renderedHandler() {
            console.log("渲染完成")
        },
        errorHandler() {
            console.log("渲染失败")
        }
    }
}
</script>

<style scoped>

</style>