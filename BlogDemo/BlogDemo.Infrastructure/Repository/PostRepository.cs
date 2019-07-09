using BlogDemo.Core.Entities;
using BlogDemo.Core.Interfaces;
using BlogDemo.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogDemo.Infrastructure.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly MyContext _myContext;

        public PostRepository(MyContext myContext)
        {
            this._myContext = myContext;
        }
        public async Task<PaginatedList<Post>> GetAllPostsAsync(PostParameters postParameters)
        {
            var query = _myContext.Posts.OrderBy(x => x.Id);
            var Count = await query.CountAsync();
            var data=await query
                .Skip(postParameters.PageIndex*postParameters.PageSize)
                .Take(postParameters.PageSize)
                .ToListAsync();
            return new PaginatedList<Post>(postParameters.PageIndex,postParameters.PageSize,Count,data);
        }

        public async Task<Post> GetPostByIdAsync(int id)
        {
            return await _myContext.Posts.FindAsync(id);
        }

        public void SavePost(Post post)
        {
            _myContext.Posts.Add(post);
        }
    }
}
