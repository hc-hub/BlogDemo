using BlogDemo.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlogDemo.Core.Interfaces
{
    public interface IPostRepository
    {
        Task<PaginatedList<Post>> GetAllPostsAsync(PostParameters queryParameters);
        Task<Post> GetPostByIdAsync(int id);
        void AddPost(Post post);
        void DeletePost(Post post);
        void Update(Post post);
    }
}
