using BlogDemo.Core.Entities;
using BlogDemo.Core.Interfaces;
using BlogDemo.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogDemo.Infrastructure.Services;
using BlogDemo.Infrastructure.Extensions;
using BlogDemo.Infrastructure.Resources;

namespace BlogDemo.Infrastructure.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly MyContext _myContext;
        private readonly IPropertyMappingContainer _propertyMappingContainer;

        public PostRepository(MyContext myContext,IPropertyMappingContainer propertyMappingContainer)
        {
            _myContext = myContext;
            _propertyMappingContainer = propertyMappingContainer;
        }
        public async Task<PaginatedList<Post>> GetAllPostsAsync(PostParameters postParameters)
        {
            var query = _myContext.Posts.AsQueryable();
            if (!string.IsNullOrEmpty(postParameters.Title))
            {
                var title = postParameters.Title.ToLowerInvariant();
                query=query.Where(x=>x.Title.ToLowerInvariant()==title);
            }
            //query = query.OrderBy(x => x.Id);
            //query = query.ApplySort(postParameters.OrderBy,_propertyMappingContainer.Resolve<PostResource,Post>());
            var Count = await query.CountAsync();
            var data=await query
                .Skip(postParameters.PageIndex*postParameters.PageSize)
                .Take(postParameters.PageSize)
                .ApplySort(postParameters.OrderBy, _propertyMappingContainer.Resolve<PostResource, Post>())
                .ToListAsync();
            return new PaginatedList<Post>(postParameters.PageIndex,postParameters.PageSize,Count,data);
        }

        public async Task<Post> GetPostByIdAsync(int id)
        {
            return await _myContext.Posts.FindAsync(id);
        }

        public void AddPost(Post post)
        {
            _myContext.Posts.Add(post);
        }

        public void DeletePost(Post post)
        {
            _myContext.Remove(post);
        }

        public void Update(Post post)
        {
            _myContext.Update(post);
        }
    }
}
