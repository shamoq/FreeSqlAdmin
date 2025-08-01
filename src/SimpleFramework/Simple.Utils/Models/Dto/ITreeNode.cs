using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Utils.Models.Dto
{
    // 树节点类
    public class TreeNode<T> where T : ITreeDto
    {
        public T Data { get; set; }
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        
        public string Name { get; set; }
        
        public bool IsLeaf { get; set; }

        public List<TreeNode<T>> Children { get; set; }
    }

    public class TreeNode : ITreeDto
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        
        public string Name { get; set; }

        public List<TreeNode> Children { get; set; }
    }
}