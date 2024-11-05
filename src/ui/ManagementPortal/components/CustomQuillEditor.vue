<template>
    <div class="quill-container">
        <QuillEditor
            :content="content"
            contentType="html"
            :toolbar="'#custom-toolbar' + index"
            @update:content="handleContentUpdate"
            :style="{ minHeight: minHeight }"
        >
            <template #toolbar>
                <div :id="'custom-toolbar' + index">
                    <select class="ql-size">
                        <option value="small"></option>
                        <option selected></option>
                        <option value="large"></option>
                        <option value="huge"></option>
                    </select>
                    <button class="ql-bold" aria-label="Bold"></button>
                    <button class="ql-italic" aria-label="Italic"></button>
                    <button class="ql-underline" aria-label="Underline"></button>
                    <button class="ql-strike" aria-label="Strike"></button>
                    <button class="ql-link" aria-label="Link"></button>
                    <button class="ql-image" aria-label="Image"></button>
                    <button class="ql-list" value="ordered" aria-label="Ordered List"></button>
                    <button class="ql-list" value="bullet" aria-label="Unordered List"></button>
                    <button class="ql-clean" aria-label="Remove Styles"></button>
                    <button class="quill-view-html" aria-label="Edit HTML" @click="toggleHtmlDialog">Edit HTML</button>
                </div>
            </template>
        </QuillEditor>
        <Dialog v-model:visible="showHtmlDialog" :modal="true" :closable="false" :style="{ width: '50vw' }">
            <Textarea v-model="rawHtml" style="width: 100%; height: 100%;" autoResize />
            <template #footer>
                <Button label="Save" @click="saveRawHtml" />
                <Button label="Cancel" @click="toggleHtmlDialog" />
            </template>
        </Dialog>
    </div>
</template>

<script lang="ts">
import { QuillEditor } from '@vueup/vue-quill';
import '@vueup/vue-quill/dist/vue-quill.snow.css';

function filterQuillHTML(html: string) {
    return html
        .replace(/(<p><br><\/p>)+/g, match => '<br>'.repeat((match.split('<p><br></p>').length))) // Handle multiple consecutive <p><br></p> tags accurately
        .replace(/<\/p><p>/g, '<br>') // Replace </p><p> with <br> between paragraphs
        .replace(/<\/?p[^>]*>/g, ''); // Remove any remaining <p> tags
}

export default {
    components: { QuillEditor },

    props: {
        initialContent: {
            type: String,
            required: true,
            default: '',
        },
        index: {
            type: Number,
            required: true,
            default: 0,
        },
        minHeight: {
            type: String,
            default: 'auto',
        }
    },
    
    data() {
        return {
            content: this.initialContent,
            rawHtml: '',
            showHtmlDialog: false
        };
    },

    watch: {
        initialContent(newContent) {
            if (newContent !== this.content) {
                this.content = newContent;
            }
        }
    },
    
    methods: {
        toggleHtmlDialog() {
            this.rawHtml = filterQuillHTML(JSON.parse(JSON.stringify(this.content)));
            this.showHtmlDialog = !this.showHtmlDialog;
        },

        saveRawHtml() {
            // this.rawHtml = this.rawHtml.replace(/<br>/g, '<br>');
            this.content = this.rawHtml.split('<br>').map(line => `<p>${line}</p>`).join('');
            this.$emit('contentUpdate', this.content);
            this.showHtmlDialog = false;
        },
        
        handleContentUpdate(newContent) {
            if (newContent !== this.content) {
                this.$emit('contentUpdate', newContent);
            }
        }
    }
};
</script>

<style scoped>
.quill-container {
    max-width: 80ch;
}
.quill-view-html {
    font-size: 1rem;
    color: #4b5563;
    font-weight: 550;
    border-radius: 4px;
    padding: 0.5rem;
    cursor: pointer;
}
</style>
