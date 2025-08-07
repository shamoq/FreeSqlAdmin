import { generateUUID } from './uuid'

/**
 * 将扁平数据转换为树形结构
 * @param items 扁平数据数组
 * @param options 配置选项
 */
export interface TreeConvertOptions {
  idField?: string;
  parentField?: string;
  childrenField?: string;
}

export function convertToTree<T extends Record<string, any>>(
  items: T[],
  options: TreeConvertOptions = {}
): T[] {
  const {
    idField = 'id',
    parentField = 'parentId',
    childrenField = 'children'
  } = options;

  const result: T[] = [];
  const itemMap = new Map();

  // 首先创建一个映射表
  items.forEach(item => {
    itemMap.set(item[idField], { ...item });
  });

  // 构建树形结构
  items.forEach(item => {
    const node = itemMap.get(item[idField]);
    const parentId = item[parentField];
    const parent = itemMap.get(parentId);
    
    if (parent) {
      if(!parent[childrenField])
        parent[childrenField] = [];
      parent[childrenField].push(node);
    } else {
      result.push(node);
    }
  });

  // 移除末级节点的children属性

  return result;
}

/**
 * 将树形字段数据转换为平级结构
 * @param treeData 树形数据
 * @returns 平级的字段数组
 */
export function convertToArray(treeData: any[], options: TreeConvertOptions = {}) {
  const result = [];
  const {
    idField = 'id',
    parentField = 'parentId',
    childrenField = 'children'
  } = options;
  
  function traverse(node: any, parentKey?: string) {
    // 如果没有id，生成随机Id
      if (!node[idField]) {
          node[idField] = generateUUID();
      }

      var item = {
        ...node,
        [parentField]: parentKey,
      };
      delete item[childrenField];
      result.push(item);
    
    if (node[childrenField]) {
      node[childrenField].forEach((child: any) => {
        traverse(child, node[idField]);
      });
    }
  }

  treeData.forEach(node => traverse(node));
  return result;
}

// 新增方法，在treeData结构中，递归查询元素
export function findInTree(treeData: any[], callback: (node: any) => boolean, options: TreeConvertOptions = {}): any {
  const {
    childrenField = 'children'
  } = options;
  for (let node of treeData) {
    if (callback(node)) {
      return node;
    }
    if (node[childrenField] && node[childrenField].length > 0) {
      const foundNode = findInTree(node[childrenField], callback, options);
      if (foundNode) {
        return foundNode;
      }
    }
  }
  return null;
}