using BlogDemo.Core.Interfaces;
using System.Threading.Tasks;

namespace BlogDemo.Infrastructure.Database
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MyContext _myContext;

        public UnitOfWork(MyContext myContext)
        {
            this._myContext = myContext;
        }
        public async Task<bool> SaveAsync()
        {
            return await _myContext.SaveChangesAsync() > 0;
        }
    }
}
