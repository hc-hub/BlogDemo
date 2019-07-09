using BlogDemo.Core.Interfaces;
using System.ComponentModel;

namespace BlogDemo.Core.Entities
{
    public abstract class QueryParameters : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private const int DefaultPageSize = 10;
        private const int DefaultMaxPageSize = 100;
        private int _pageIndex;
        public int PageIndex
        {
            get => _pageIndex;
            set => _pageIndex = value >= 0 ? value : 0;
        }
        private int _pageSize;
        public virtual int PageSize
        {
            get;
            set;
        }
        private string _orderBy;
        public string OrderBy
        {
            get => _orderBy;
            set => _orderBy = value ?? nameof(IEntity.Id);
        }
        private int _maxPageSize = DefaultMaxPageSize;
        public int MaxPageSize
        {
            get=>_maxPageSize;
            set=>MaxPageSize=_maxPageSize;
        }
    }
}
