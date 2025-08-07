import { requestClient } from "#/api/request";

export function useRichEditor(editorRef) {

    let extParam: any = {};

    const option = {
        height: 'calc(100vh)',
        toolbar: {
            defaultMode: 'ribbon',
            menus: ['base', 'table', 'page', 'export'],// ['base', 'insert', 'table', 'tools', 'page', 'export'],
            disableMenuItems: [],
            importWord: {
                enabled: true,
                options: {},
                useCustomMethod: false,
            },
        },
        page: {
            showBreakMarks: false,
        },
        document:{
            enableMarkdown: false,  
        },
        shareUrl: '',
        async handleSave(content, page, document) {
            await requestClient.post(`/TemplateVersion/SaveUmo`, Object.assign(content, extParam.saveParam));
            return true;
        },
    };

    function getOptions(){
        return option;
    }

    async function loadContent(loadParam) {
        // 加载数据
        let data = await requestClient.post(`/TemplateVersion/GetUmoContent`, loadParam);
        if (data) {
            editorRef.value.setContent(data)
        }
        setReadOnly(option.isReadOnly)
    }

    function addDocumentField(field) {
        const editor = editorRef.value.getEditor().value;
        if (!editor) return;
        editor.chain()
            .focus()
            .setHtmlField({ id: field.fullCode, display: 'input', label: field.name })
            .insertContent('{'+field.name+'}')
            .unsetHtmlField()
            .insertContent(' ') // 插入一个空格，保持光标
            .run();
    };

    function fillFieldData(data) {
        const editor = editorRef.value.getEditor().value;
        if (!editor) return;
        editor.state.doc.descendants((node, pos) => {
            node.marks.forEach(mark => {
                var fieldCode = mark.attrs.id;
                if (mark.type.name === 'htmlField' && data[fieldCode] != null) {
                    editor.chain()
                        .focus()
                        .setTextSelection({ from: pos, to: pos + node.nodeSize })
                        .setMark('htmlField', { id: fieldCode, display: 'span' })
                        .insertContent(String(data[fieldCode]))
                        .run();
                }
            });
        });
    };

    /**
 * 插入带标记的表格
 * @param {Object} options - 表格配置选项
 * @param {string} options.tableId - 表格唯一标识
 * @param {Array} options.headers - 表头配置 [{name: '显示名称', code: '字段编码'}, ...]
 * @param {Array} options.data - 表格数据 [{field1: value1, field2: value2, ...}, ...]
 */
    function addDocumentTable(options = {}) {
        const editor = editorRef.value.getEditor().value;
        if (!editor) return;

        const { tableId, headers } = options;

        if(!headers.length){
            return ;
        }

        // 插入表格基本结构
        editor
            .chain()
            .focus()
            .insertTable({
                rows: 3,
                cols: headers.length,
            })
            .run();

        // 插入带标记的表头
        headers.forEach((header, index) => {
            if (index > 0) {
                editor.chain().focus().goToNextCell().run();
            }

            editor
                .chain()
                .focus()
                .setHtmlField({ id: header.code, display: 'span', table: tableId, label: header.name })
                // .setMark("htmlField", { id: header.code, display: 'input', table: tableId })
                .insertContent(header.name)
                .unsetHtmlField()
                .run();
        });

        // 插入数据行
        // data.forEach((row) => {
        //   headers.forEach((header, colIndex) => {
        //     // 移动到下一个单元格
        //     if (colIndex === 0) {
        //       // 如果是新行的第一个单元格
        //       editor.chain().focus().goToNextCell().run();
        //     } else {
        //       // 如果是行内的后续单元格
        //       editor.chain().focus().goToNextCell().run();
        //     }

        //     // 获取单元格内容，确保转换为字符串
        //     const cellContent = (row[header.code] !== undefined && row[header.code] !== null)
        //       ? row[header.code].toString()
        //       : "";

        //     editor
        //       .chain()
        //       .focus()
        //       .insertContent(cellContent)
        //       .run();
        //   });
        // });

        return tableId;
    }

    function setContent(htmlContent) {
        editorRef.value.setContent(htmlContent) 
    };

    function getContent() {
        const content = editorRef.value.getContent();
        return content;
    };

    async function saveContent(saveParams) {
        if(saveParams){
            option.saveParam = saveParams;
        }
        var isReadOnly = editorRef.value.getOptions().document.readOnly;
        if(isReadOnly){
            var html = editorRef.value.getContent();
            var json = editorRef.value.getJSON();
            var text = editorRef.value.getText();
            await option.onSave({html,json: json.content,text});
        }else{
            await editorRef.value.saveContent();
        }
    };

    function setReadOnly(isReadOnly) {
        // editorRef.value.setReadOnly(isReadOnly)
    }

    function setExtParam(param) {
        Object.assign(extParam, param)
    }

    return {
        option,
        getContent, // 获取文档内容
        setContent, // 设置文件内容
        fillFieldData, // 填充公文域数据

        setExtParam, // 设置其他参数

        // 必须提供的方法
        getOptions, // 获取配置
        addDocumentField, // 增加公文域
        addDocumentTable, // 增加表格公文域
        saveContent, // 保存文档
        loadContent, // 加载文档
        setReadOnly, // 是否只读
    }
}
