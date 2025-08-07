<template>
    <div class="iframe-viewer">
        <iframe
            :src="src"
            frameborder="0"
            width="100%"
            height="100%"
            @load="handleLoad"
        ></iframe>
    </div>
</template>

<script>
export default {
    name: "PreviewIframe",
    props: {
        src: {
            type: String,
            default: ''
        }
    },
    data() {
        return {
            loading: true
        }
    },
    methods: {
        handleLoad() {
            this.loading = false;
        }
    }
}
</script>

<style scoped>
.iframe-viewer {
    width: 100%;
    height: 100%;
    min-height: 500px;
    background-color: #fff;
    border-radius: 4px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    overflow: hidden;
}

iframe {
    border: none;
}
</style>