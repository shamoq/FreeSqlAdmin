<template>
    <div>
        <img id="image" :src="src" alt="Picture" style="display:none;">
    </div>
</template>

<script>
import Viewer from 'viewerjs';

export default {
    name: "PreviewImage",
    props: {
        src: {
            type: String,
            default: ''
        }
    },
    data() {
        return {
            viewer: null
        }
    },
    watch: {
        src: {
            handler() {
                this.$nextTick(() => {
                    if (this.viewer) {
                        this.viewer.destroy();
                    }
                    this.initViewer();
                });
            }
        }
    },
    mounted() {
        this.initViewer();
    },
    beforeDestroy() {
        if (this.viewer) {
            this.viewer.destroy();
        }
    },
    methods: {
        initViewer() {
            this.viewer = new Viewer(document.getElementById('image'), {
                inline: true,
                toolbar: {
                    zoomIn: 4,
                    zoomOut: 4,
                    oneToOne: 4,
                    reset: 4,
                    prev: false,
                    play: {
                        show: 4,
                        size: 'large',
                    },
                    next: false,
                    rotateLeft: 4,
                    rotateRight: 4,
                    flipHorizontal: 4,
                    flipVertical: 4,
                },
                viewed() {
                    this.viewer.zoomTo(1);
                },
            });
        }
    }
};
</script>
