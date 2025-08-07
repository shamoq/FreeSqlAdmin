namespace Simple.AdminApplication.UserMng.Entities
{
    public class SysOrg : DefaultTenantEntity, ITreeEntity
    {
        /// <summary>名称</summary>
        public string Name { get; set; }

        /// <summary>编码</summary>
        public string Code { get; set; }

        /// <summary>全名称</summary>
        public string FullName { get; set; }

        /// <summary>
        /// 组织类型，1 公司 2 部门
        /// </summary>
        public int OrgType { get; set; }

        /// <summary>
        /// 上级组织
        /// </summary>
        public Guid? ParentId { get; set; }
        
        /// <summary>
        /// 排序编码
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 排序全编码
        /// </summary>
        public string OrderFullCode { get; set; }

        // /// <summary>
        // /// 所属公司
        // /// </summary>
        // public Guid CompanyId { get; set; }

        // /// <summary>备注</summary>
        // public string Remark { get; set; }

        // /// <summary>是否公司</summary>
        // public int IsCompany { get; set; }

        // /// <summary>
        // /// 是否末级公司
        // /// </summary>
        // public int IsEndCompany { get; set; }
        //
        // /// <summary>
        // /// 是否部门
        // /// </summary>
        // public int IsDepartment { get; set; }
        //
        // /// <summary>
        // /// 是否末级部门
        // /// </summary>
        // public int IsEndDepartment { get; set; }

        // /// <summary>
        // /// 是否启用
        // /// </summary>
        // public int IsEnable { get; set; }
        
        // /// <summary>负责人</summary>
        // public string Contract { get; set; }

    }
}