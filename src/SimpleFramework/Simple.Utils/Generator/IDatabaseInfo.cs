using Simple.Utils.Generator.Dto;

namespace Simple.Utils.Generator
{
    /// <summary>
    /// 获取数据库信息
    /// </summary>
    public interface IDatabaseInfo
    {
        /// <summary>
        /// 获取数据库表信息
        /// </summary>
        /// <returns></returns>
        DatabaseTableInfoDto GetTableInfoByName(string schema, string tableName);

        /// <summary>
        /// 创建数据库表
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <returns></returns>
        GenerateDatabaseSqlResult CreateTable(DatabaseTableInfoDto tableInfo);

        /// <summary>
        /// 判断字段是否发生变化
        /// </summary>
        /// <returns></returns>
        bool IsFieldChange(DatabaseFieldInfoDto typeField, DatabaseFieldInfoDto dbField);

        /// <summary>
        /// 添加表注释
        /// </summary>
        /// <returns></returns>
        string CreateTableDescription(string schema, string tableName, string description);

        /// <summary>
        /// 创建字段
        /// </summary>
        /// <returns></returns>
        string CreateTableField(string schema, string tableName, DatabaseFieldInfoDto fieldInfo);

        /// <summary>
        /// 更新字段
        /// </summary>
        /// <returns></returns>
        string UpdateTableField(string schema, string tableName, DatabaseFieldInfoDto fieldInfo);

        /// <summary>
        /// 删除字段
        /// </summary>
        /// <returns></returns>
        string DropTableField(string schema, string tableName, DatabaseFieldInfoDto fieldInfo);

        /// <summary>
        /// 创建主键
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="tableName"></param>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        string CreatePrimaryKey(string schema, string tableName, DatabaseFieldInfoDto fieldInfo);

        /// <summary>
        /// 获取命名空间加表名
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        string GetSchemaTableName(string schema, string tableName);

        /// <summary>
        /// 获取默认用户
        /// </summary>
        /// <returns></returns>
        string GetDefaultSchema(string schema);

        List<string> GetAllTableNames();

        /// <summary>
        /// 类型转换 DB = C#
        /// </summary>
        /// <returns></returns>
        string ConvertDatabaseTypeToCsharpType(DatabaseFieldInfoDto databaseFieldInfo);

        /// <summary>
        /// 类型转换 C# => DB
        /// </summary>
        /// <returns></returns>
        string ConvertCsharpTypeToDatabaseType(string csharpTypeList);
    }
}