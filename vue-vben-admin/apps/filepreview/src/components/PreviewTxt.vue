<template>
    <div>
        <div class="txt-viewer" v-if="content">
            <pre>{{ content }}</pre>
        </div>
    </div>
</template>

<script>
export default {
    name: "PreviewTxt",
    props: {
        src: {
            type: String,
            default: ''
        }
    },
    data() {
        return {
            loading: true,
            content: null,
            txtUrl: ''
        }
    },
    watch: {
        src: {
            handler(newVal) {
                if (newVal) {
                    this.txtUrl = newVal;
                    this.loadTxtContent();
                }
            },
            immediate: true
        },
    },
    methods: {
        async loadTxtContent() {
            try {
                const response = await fetch(this.txtUrl);
                if (!response.ok) {
                    throw new Error('Failed to load TXT file');
                }
                this.content = await response.text();
                this.loading = false;
            } catch (error) {
                console.error('Error loading TXT file:', error);
                this.loading = false;
            }
        }
    }
};
</script>

<style scoped>
.txt-viewer {
    padding: 20px;
    background-color: #fff;
    border-radius: 4px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

pre {
    white-space: pre-wrap;
    word-wrap: break-word;
    font-family: monospace;
    margin: 0;
    line-height: 1.5;
}
</style>