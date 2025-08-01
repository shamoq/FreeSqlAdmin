/**
 * 根据合同状态获取对应的颜色类型
 * @param statusEnum 合同状态枚举值
 * @returns 颜色类型
 */
export function getStatusColor(statusEnum: number): 'warning' | 'success' | 'info' | 'error' | 'default' {
    switch (statusEnum) {
        case 1:
            return 'warning';
        case 2:
        case 4:
        case 13:
            return 'success';
        case 12:
            return 'info';
        case 14:
        case 15:
        case 16:
            return 'error';
        default:
            return 'default';
    }
}


/**
 * 
 * @param field 根据字段类型获取对应的图标
 */
export function getFieldTypeIcon (field: any)  {
    if (field.groupType == 'form') {
        return 'material-symbols:table-chart';
    } else if (field.groupType == 'table') {
        return 'material-symbols:table-rows';
    }
    // 根据字段类型返回对应的图标
    switch (field.fieldType) {
        case 1: // 文本类型
            return 'material-symbols:text-fields';
        case 2: // 数值类型
            return 'material-symbols:numbers';
        case 3: // 金额类型
            return 'material-symbols:payments';
        case 4: // 选项类型
            return 'material-symbols:list';
        case 5: // 日期类型
            return 'material-symbols:calendar-month';
        case 6: // 时间类型
            return 'material-symbols:schedule';
        default:
            return 'material-symbols:text-fields';
    }
};


export function fillSystemField (fieldCode, fieldValue, customData) {
    if (fieldCode == 'partA' || fieldCode == 'partB' || fieldCode == 'contract') {
        // 为value对象的每个属性添加system_前缀
        if (fieldValue) {
            for (const [key, val] of Object.entries(fieldValue)) {
                customData[`system_${fieldCode}_${key}`] = val;
            }
        }
    } else {
        let newCode = 'system_contract_' + fieldCode
        customData[newCode] = fieldValue;
    }
}