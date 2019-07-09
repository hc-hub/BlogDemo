﻿using System.Collections.Generic;

namespace BlogDemo.Core.Entities
{
    public class PaginatedList<T> : List<T> where T : class
    {
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        private int _totalItemsCount;
        public int TotalItemsCount
        {
            get => _totalItemsCount;
            set => _totalItemsCount = value > 0 ? value : 0;
        }
        public int pageCount => TotalItemsCount / PageSize + (TotalItemsCount % PageSize > 0 ? 1 : 0);
        public PaginatedList(int pageIdex,int pageSize,int totalItemsCount,IEnumerable<T> data)
        {
            PageIndex = pageIdex;
            PageSize = pageSize;
            TotalItemsCount = totalItemsCount;
            AddRange(data);
        }
    }
}
