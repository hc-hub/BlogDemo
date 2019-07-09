using AutoMapper;
using BlogDemo.Core.Entities;
using BlogDemo.Core.Interfaces;
using BlogDemo.Infrastructure.Database;
using BlogDemo.Infrastructure.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogDemo.Api.Controllers
{
    [Route("api/posts")]
    public class PostController:Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PostController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public PostController(IPostRepository postRepository,IUnitOfWork unitOfWork,ILogger<PostController> logger,IConfiguration configuration,IMapper mapper)
        {
            _postRepository = postRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> Get(int pageIndex,int pageSize)
        {
            var posts = await _postRepository.GetAllPostsAsync();
            var postResource=_mapper.Map<IEnumerable<Post>,IEnumerable<PostResource>>(posts);
            return Ok(postResource.Skip(pageIndex*pageSize).Take(pageSize).ToList());
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            return Ok(post);
        }
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var newPost = new Post {
                Title = "Title A",
                Author = "admin",
                Body = "fasdfasdff",
                LastModified = DateTime.Now
            };
            _postRepository.SavePost(newPost);
            await _unitOfWork.SaveAsync();
            return Ok();
        }
    }
}
