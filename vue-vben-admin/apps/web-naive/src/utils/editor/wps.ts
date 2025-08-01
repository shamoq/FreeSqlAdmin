import { requestClient } from "#/api/request";

// http://27.17.30.140:32334
var init = true;
export function useWpsEditor(editorRef) {
  const password = '111222333666777888';

  /**
   * 获取光标定位位置
   */
  async function getPos() {
    const app = editorRef.value.Application;
    var range = await app.ActiveDocument.ActiveWindow.Selection.Range;
    var start = await range.Start;
    var end = await range.End;
    return {
      Start: start,
      End: end
    }
  }

  /**
   * 插入文本域
   */
  async function addDocumentField(field, range) {
    const app = editorRef.value.Application;
    if (!range) {
      var pos = await getPos();
      range = { Start: pos.Start, End: pos.End }
    }
    var name = field.fullCode + '@' + new Date().getTime()
    await app.ActiveDocument.DocumentFields.Add({
      Name: name,
      Range: range,
      Hidden: false, // 是否隐藏，默认 false
      PrintOut: true, // 是否可打印，默认 true
      ReadOnly: false // 是否只读，默认 false
    });
    var documentField = await app.ActiveDocument.DocumentFields.Item({
      Name: name
    });
    documentField.Value = '{' + field.name + '}';
    if (!range) {
      await documentField.GotoEnd();
      // 将光标向右移动
      await app.ActiveDocument.ActiveWindow.Selection.MoveRight()
    }
  }

  /**
   * 
  * 插入带标记的表格
  * @param {Object} options - 表格配置选项
  * @param {string} options.tableId - 表格唯一标识
  * @param {Array} options.headers - 表头配置 [{name: '显示名称', code: '字段编码'}, ...]
  * @param {Array} options.data - 表格数据 [{field1: value1, field2: value2, ...}, ...]
   */
  async function addDocumentTable(options = {}) {
    const { tableId, headers } = options;

    if (!headers.length) {
      return;
    }

    const app = editorRef.value.Application;

    // 获取文档中的表格集合
    const tables = await app.ActiveDocument.Tables;
    // 插入表格
    const range = app.ActiveDocument.ActiveWindow.Selection.Range;
    const newTable = await tables.Add(range,
      3,  // 新增表格的行数
      headers.length, // 新增表格的列数
      1,  // 启用自动调整功能
      1 // 根据表格中包含的内容自动调整表格的大小
    );
    // 获取第一行单元格集合
    const firstRowCells = await newTable.Rows.Item(1).Cells;
    const secondRowCells = await newTable.Rows.Item(2).Cells;

    // 设置第一行单元格文字
    for (let i = 0; i < headers.length; i++) {
      const cell = await firstRowCells.Item(i + 1);
      cell.Range.Text = headers[i].name;

      // 获取第二行单元格的Range
      const dataCell = await secondRowCells.Item(i + 1);
      const dataCellRange = dataCell.Range;
      const start = await dataCellRange.Start;
      const end = await dataCellRange.End;

      // 使用单元格的Range添加域
      await addDocumentField({
        name: headers[i].name,
        fullCode: headers[i].code,
      }, { Start: start, End: end });
    }

  }

  // 保存wps文件
  async function saveWpsDoc() {
    var result = await editorRef.value.save();
    // 保存完wps后，重新刷一下wps，否则多次保存时，后端会报错
    // this.loadWpsInfo();
    return result
  }

  async function setReadOnly(isReadOnly: Boolean) {
    const app = editorRef.value.Application;
    // 设置为只读
    await app.ActiveDocument.SetReadOnly({
      Value: isReadOnly
    })
  }

  /**
   * 限制文档不能编辑
   * @param isProtect 是否保护
   * @param password 保护密码
   */
  async function setProtect(isProtect: boolean) {
    const app = editorRef.value.Application;
    if (isProtect) {
      await app.ActiveDocument.Protect(password);
    } else {
      await app.ActiveDocument.Unprotect(password);
    }
  }

  /**
   * 滚动到对应的公文域
   * @param documentFieldName 
   */
  async function scrollToDocumentField(documentFieldName) {
    const app = editorRef.value.Application;
    if (documentFieldName) {
      var documentField = await app.ActiveDocument.DocumentFields.Item({
        Name: documentFieldName
      })
      if (documentField) {
        var range = await documentField.Range
        await app.ActiveDocument.ActiveWindow.ScrollIntoView(range)
      }
    }
  }

  /**
   * 滚动到对应的内容控件
   * @param app 
   * @param index 
   */
  async function scrollToContentControl(index) {
    const app = editorRef.value.Application;
    var contentControl = await app.ActiveDocument.ContentControls.Item(index)
    if (contentControl) {
      var contentControlRange = await contentControl.Range
      var start = await contentControlRange.Start
      var end = await contentControlRange.End
      var range = await app.ActiveDocument.Range(start, end)
      await app.ActiveDocument.ActiveWindow.ScrollIntoView(range)
    }
  }

  /**
   * wps获取内容控件填写的content
   * @param callback 
   */
  async function getWpsNotFillContentControl(callback) {
    const app = editorRef.value.Application;
    // 内容控件对象
    var contentControls = await app.ActiveDocument.ContentControls
    // 内容控件数量
    var count = await contentControls.Count
    for (var i = 0; i < count; i++) {
      var contentControl = await app.ActiveDocument.ContentControls.Item(i + 1)
      var content = await contentControl.Content
      callback && callback(i + 1, content)
    }
  }

  /**
   * wps翻页
   * @param pageIndex 
   */
  async function wpsPageTurn(pageIndex) {
    const app = editorRef.value.Application;
    app.ActiveDocument.ActiveWindow.Selection.GoTo(
      app.Enum.WdGoToItem.wdGoToPage,
      app.Enum.WdGoToDirection.wdGoToAbsolute,
      pageIndex
    )
  }

  async function toggleEditMode(mode: String) {
    const app = editorRef.value.Application;
    await app.ToggleEditMode(mode && mode.toLowerCase() == "edit" ? 'EDIT' : 'READ');
  }

  return {
    // wpsInstance, //wps对象
    // initWps, // 初始化wps
    // setProtect, // 保护文档

    // 必须提供的方法
    addDocumentField, // 增加公文域
    addDocumentTable, // 增加表格公文域
    saveWpsDoc, // 保存文档
    setReadOnly, // 设置文档只读

    toggleEditMode,
  }
}
