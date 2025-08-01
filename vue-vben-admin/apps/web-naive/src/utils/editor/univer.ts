import { requestClient } from "#/api/request";

export function useUniverEditor() {

    let univerAPI = null;
    let univer = null;
    let workbook = null;

    function createTable(sheetName, data) {
        if (!workbook) {
            return false;
        }
        const sheet = workbook.getActiveSheet();
        sheet.setName(sheetName);
        // 根据data的行数和列数计算range
        // const lastCol = String.fromCharCode(65 + data[0].length - 1); // 根据列数计算最后一列的字母
        // const lastRow = data.length; // 获取行数
        // const range = sheet.getRange(`A1:${lastCol}${lastRow}`);
        const range = sheet.getRange(0, 0, 1, data[0].length);

        // 设置表格数据
        // const data = [
        //     ['产品', '价格', '库存', '销量'],
        //     ['笔记本电脑', '6999', '100', '50'],
        //     ['智能手机', '3999', '200', '150'],
        //     ['平板电脑', '2999', '150', '80'],
        //     ['智能手表', '1999', '300', '120']
        // ];

        // const range = sheet.getRange(0, 0, 1, 5);
        range.setValues(data);

        // 设置表格样式
        // range.setBorder({
        //   top: { style: 1 },
        //   bottom: { style: 1 },
        //   left: { style: 1 },
        //   right: { style: 1 },
        //   inside: { style: 1 }
        // });

        // 设置表头样式
        const headerRange = sheet.getRange(0, 0, data.length, data[0].length);
        headerRange.setFontWeight('bold');
        headerRange.setBackgroundColor('#1a56db');
        headerRange.setHorizontalAlignment('center');
        headerRange.setVerticalAlignment('middle');
        headerRange.setFontColor('#ffffff');
        // headerRange.setRowHeight(30); // 设置行高

        // 设置列宽
        // sheet.getRange('A:A').setColumnWidth(120); // 产品列
        // sheet.getRange('B:B').setColumnWidth(80);  // 价格列
        // sheet.getRange('C:C').setColumnWidth(80);  // 库存列
        // sheet.getRange('D:D').setColumnWidth(80);  // 销量列

        // 设置数据列样式
        const dataRange = sheet.getRange('A2:D5');
        dataRange.setHorizontalAlignment('center');

        // 冻结首行
        sheet.setFrozenRows(1);
        return true;
    }

    async function save({ type, businessId, businessName }) {
        if (type) {
            selfInfo.type = type;
        }
        if (businessId) {
            selfInfo.businessId = businessId;
        }
        if (businessName) {
            selfInfo.businessName = businessName;
        }
        const snapshot = workbook.save();
        await requestClient.post('/snapshot/save', {
            ...selfInfo,
            content: JSON.stringify(snapshot),
        })
    }

    var selfInfo: any = {};
    async function load({ type, businessId }) {
        const data = await requestClient.post('/snapshot/getByBusinessId', {
            type,
            businessId,
        });
        selfInfo.type = type;
        selfInfo.businessId = businessId;

        // univerAPI.addEvent(univerAPI.Event.LifeCycleChanged, ({ stage }) => {
        //     if (stage === LifecycleStages.Starting) {
        //         const formula = univerAPI.getFormula();
        //         formula.setInitialFormulaComputing(CalculationMode.FORCED);
        //     }
        // });

        if (data) {
            workbook = univerAPI.createWorkbook(JSON.parse(data));
            return false;
        } else {
            workbook = univerAPI.createWorkbook();
            return true;
            // createTable();
        }
    }


    async function init(iframe, contentParams): Promise<boolean> {
        // 加载Excel数据 
        if (!iframe) return false;

        // 等待iframe加载完成
        if (iframe.contentWindow.init) {
            const instance = iframe.contentWindow.init();
            univerAPI = instance.univerAPI;
            univer = instance.univer;
            // univerRef.value = instance;
            return await load(contentParams);
        } else {
            return new Promise((resolve) => {
                iframe.onload = async () => {
                    const instance = iframe.contentWindow.init();
                    // univerRef.value = instance;
                    univerAPI = instance.univerAPI;
                    univer = instance.univer;
                    let result = await load(contentParams);
                    resolve(result);
                };
            });
        }
    }

    function getHeaderFields() {
        const { fields } = getTableFullData({ nodata: true });
        fields.map(item => {
            item.key = item.name;
            item.label = item.name;

            item.children.map(child => {
                child.key = child.name;
                child.label = child.name;
            });
        });
        return fields;
    }

    function getTableColumnData(sheetIndex, colIndex) {
        const sheets = workbook.getSheets();
        if (sheets.length == 0 || sheets.length <= sheetIndex) {
            return [];
        }
        const sheetData = sheets[sheetIndex].getSheet().getSnapshot();

        const cellData = sheetData.cellData;
        const data = [];

        // 解析数据（第二行开始）
        for (let rowIndexStr in cellData) {
            const rowIndex = Number(rowIndexStr);
            if (!isNaN(rowIndex) && Number(rowIndex) > 0) {
                const cellValues = []
                for (let colIndexStr in cellData[rowIndex]) {
                    const colIndex = Number(colIndexStr);
                    if (!isNaN(colIndex)) {
                        const value = getCellValue(sheets[0], rowIndex, colIndex)
                        cellValues.push(value);
                    }
                }
                if (cellValues.filter(t => t).length > 0) {
                    data.push(cellValues[0]);
                }
            }
        }

        return data;
    }

    function getTableFullData(option?: any = {}) {
        let firstSheetData = [];
        let sheetHeaders = [];
        let keyFieldName = '';

        const sheets = workbook.getSheets();
        let sheetIndex = 0;
        for (const sheet of sheets) {
            const sheetData = sheet.getSheet().getSnapshot();
            const sheetName = sheet.getSheetName();

            const cellData = sheetData.cellData;
            const header = [];
            const data = [];

            // 解析表头（第一行）
            if (cellData[0]) {
                for (let colIndexStr in cellData[0]) {
                    const colIndex = Number(colIndexStr);
                    if (!isNaN(colIndex)) {
                        header.push(getCellValue(sheet, 0, colIndex));
                    }
                }
            }

            var sheetCode = "user_sheet" + sheetIndex;

            const fields = header.map((item, index) => {
                var code = 'field' + index;
                return {
                    name: item,
                    code: code,
                    fullCode: sheetIndex == 0 && index == 0 ? "main" : sheetCode + "_" + code,
                    groupType: 'field',
                    parentGroupType: sheetIndex == 0 ? 'form' : 'table',
                }
            });

            const parentField = {
                name: sheetName,
                code: sheetCode,
                fullCode: sheetCode,
                groupType: sheetIndex == 0 ? 'form' : 'table',
                children: fields
            }
            sheetHeaders.push(parentField);

            // 解析数据（第二行开始）
            if (!option.nodata) {
                for (let rowIndexStr in cellData) {
                    const rowIndex = Number(rowIndexStr);
                    if (!isNaN(rowIndex) && rowIndex > 0) {
                        const rowData = {};
                        var isEmptyRow = true;
                        for (let colIndexStr in cellData[rowIndex]) {
                            const colIndex = Number(colIndexStr);

                            if (!isNaN(colIndex)) {
                                var field = fields[colIndex];
                                if (!field) {
                                    continue;
                                }
                                rowData[field.fullCode] = getCellValue(sheet, rowIndex, colIndex)
                                if (rowData[field.fullCode]) {
                                    isEmptyRow = false;
                                }
                            }
                        }
                        if (!isEmptyRow) {
                            data.push(rowData);
                        }
                    }
                }
            }


            if (sheetIndex == 0) {
                firstSheetData = data;
                keyFieldName = fields[0].fullCode;
            } else {
                var currKeyFieldName = fields[0].fullCode;
                for (let i = 0; i < firstSheetData.length; i++) {
                    var keyCell = firstSheetData[i][keyFieldName];
                    if (keyCell) {
                        var tableData = data.filter(item => item[currKeyFieldName] == keyCell);
                        firstSheetData[i][sheetCode] = tableData;
                    }
                }
            }

            sheetIndex++;
        }
        return {
            fields: sheetHeaders,
            sheetData: firstSheetData,
        };
    }

    function getCellValue(sheet, rowIndex, colIndex) {
        var cell = sheet.getRange(rowIndex, colIndex).getCellData();
        if (cell.p && cell.p.body && cell.p.body.dataStream) {
            // 移除换行符和空白符
            return cell.p.body.dataStream.replace(/[\r\n\s]+/g, '');
        }
        return cell.v;
    }

    return {
        createTable,
        save,
        init,
        getHeaderFields,
        getTableColumnData,
        getTableFullData,
    }
}