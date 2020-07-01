using System;
using System.Collections.Generic;
using System.Text;

namespace BlogDemo.Infrastructure.Services
{
    public class MappedProperty
    {
        public string Name { get; set; }
        /// <summary>
        /// 排序是否是相反的 true:是
        /// </summary>
        public bool Revert { get; set; }
    }
}
