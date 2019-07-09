using AutoMapper;
using BlogDemo.Core.Entities;
using BlogDemo.Core.Interfaces;
using BlogDemo.Infrastructure.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlogDemo.Api.Controllers
{
    [Route("api/posts")]
    public class PostController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PostController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public PostController(IPostRepository postRepository, IUnitOfWork unitOfWork, ILogger<PostController> logger, IConfiguration configuration, IMapper mapper)
        {
            _postRepository = postRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> Get(PostParameters queryParameters)
        {
            var postList = await _postRepository.GetAllPostsAsync(queryParameters);
            var postResource = _mapper.Map<IEnumerable<Post>, IEnumerable<PostResource>>(postList);
            var meta = new
            {
                PageIndex = postList.PageIndex,
                PageSize = postList.PageSize,
                TotalItemsCount = postList.TotalItemsCount,
                PageCount = postList.Count
            };
            Response.Headers.Add("X-Pagination",JsonConvert.SerializeObject(meta,new JsonSerializerSettings
            {
                ContractResolver=new CamelCasePropertyNamesContractResolver()
            }));
            return Ok(postResource);
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
            var newPost = new Post
            {
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
