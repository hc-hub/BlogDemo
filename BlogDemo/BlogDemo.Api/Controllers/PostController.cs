using AutoMapper;
using BlogDemo.Core.Entities;
using BlogDemo.Core.Interfaces;
using BlogDemo.Infrastructure.Extensions;
using BlogDemo.Infrastructure.Resources;
using BlogDemo.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogDemo.Api.Helpers;

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
        private readonly IUrlHelper _urlHelper;
        private readonly ITypeHelperService _typeHelperService;
        private readonly IPropertyMappingContainer _propertyMappingContainer;

        public PostController(IPostRepository postRepository,
            IUnitOfWork unitOfWork,
            ILogger<PostController> logger,
            IConfiguration configuration,
            IMapper mapper,
            IUrlHelper urlHelper,
            ITypeHelperService typeHelperService,
            IPropertyMappingContainer propertyMappingContainer)
        {
            _postRepository = postRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
            _mapper = mapper;
            _urlHelper = urlHelper;
            _typeHelperService = typeHelperService;
            _propertyMappingContainer = propertyMappingContainer;
        }
        [AllowAnonymous]  //指定应用此属性的类或方法不需要授权。
        [HttpGet(Name = "GetPosts")]
        [RequestHeaderMatchingMediaType("Accept",new[] { "application/json" })]
        public async Task<IActionResult> GetHateoas(PostParameters postParameters)
        {
            if (!_propertyMappingContainer.ValidateMappingExistsFor<PostResource, Post>(postParameters.OrderBy))
            {
                return BadRequest("Can't finds fields for sorting.");
            }

            if (!_typeHelperService.TypeHasProperties<PostResource>(postParameters.Fields))
            {
                return BadRequest("Fields not exist.");
            }
            var postList = await _postRepository.GetAllPostsAsync(postParameters);
            var postResource = _mapper.Map<IEnumerable<Post>, IEnumerable<PostResource>>(postList);
            var shapedPostResource = postResource.ToDynamicIEnumerable(postParameters.Fields);
            var proviousPage = postList.HasPrevious ? CreatePostUri(postParameters, PaginationResourceUriType.PreviousPage) : null;
            var nextPage = postList.HasNext ? CreatePostUri(postParameters, PaginationResourceUriType.NextPage) : null;
            var meta = new
            {
                PageIndex = postList.PageIndex,
                PageSize = postList.PageSize,
                TotalItemsCount = postList.TotalItemsCount,
                PageCount = postList.Count,
                proviousPage,
                nextPage
            };
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(meta, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));
            return Ok(shapedPostResource);
        }
        [AllowAnonymous]
        [HttpGet(Name = "GetPosts")]
        [RequestHeaderMatchingMediaType("Accept", new[] { "application/vnd.cgzl.hateoas+json" })]
        public async Task<IActionResult> Get(PostParameters postParameters)
        {
            if (!_propertyMappingContainer.ValidateMappingExistsFor<PostResource, Post>(postParameters.OrderBy))
            {
                return BadRequest("Can't finds fields for sorting.");
            }

            if (!_typeHelperService.TypeHasProperties<PostResource>(postParameters.Fields))
            {
                return BadRequest("Fields not exist.");
            }
            var postList = await _postRepository.GetAllPostsAsync(postParameters);
            var postResource = _mapper.Map<IEnumerable<Post>, IEnumerable<PostResource>>(postList);
            var shapedPostResource = postResource.ToDynamicIEnumerable(postParameters.Fields);
            var shapedWithLinks = shapedPostResource.Select(x => {
                var dic = x as IDictionary<string, object>;
                var postlinks = CreateLinksForPost((int)dic["Id"], postParameters.Fields);
                dic.Add("links", postlinks);
                return dic;
            });
            var links = CreateLinksForPosts(postParameters, postList.HasPrevious, postList.HasNext);
            var result = new
            {
                shapedWithLinks,
                links
            };
            var meta = new
            {
                PageIndex = postList.PageIndex,
                PageSize = postList.PageSize,
                TotalItemsCount = postList.TotalItemsCount,
                PageCount = postList.Count
            };
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(meta, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));
            return Ok(result);
        }
        [HttpGet("{id}", Name = "GetPost")]
        public async Task<IActionResult> Get(int id, string Fields)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            var postResource = _mapper.Map<Post, PostResource>(post);
            var result = postResource.ToDynamic(Fields);
            var links = CreateLinksForPost(id,Fields);
            var result2 = result as IDictionary<string, object>;
            result2.Add("links",links);
            return Ok(result2);
        }
        [HttpPost(Name ="CreatePost")]
        public async Task<IActionResult> Post([FromBody] PostAddResource postAddResource)
        {
            if (postAddResource==null)
            {
                return BadRequest();
            }
            var newPost = _mapper.Map<PostAddResource, Post>(postAddResource);

            newPost.Author = "admin";
            newPost.LastModified = DateTime.Now;

            _postRepository.AddPost(newPost);
            if (!await _unitOfWork.SaveAsync())
            {
                throw new Exception("Save Failed");
            }
            var resultResource = _mapper.Map<Post, PostResource>(newPost);
            return Ok(resultResource);
        }
        public string CreatePostUri(PostParameters postParameters, PaginationResourceUriType uriType)
        {
            switch (uriType)
            {
                case PaginationResourceUriType.PreviousPage:
                    var previousParameters = new
                    {
                        pageIndex = postParameters.PageIndex - 1,
                        pageSize = postParameters.PageSize,
                        orderBy = postParameters.OrderBy,
                        fields = postParameters.Fields
                    };
                    return _urlHelper.Link("GetPosts", previousParameters);
                case PaginationResourceUriType.NextPage:
                    var nextParameters = new
                    {
                        pageIndex = postParameters.PageIndex + 1,
                        pageSize = postParameters.PageSize,
                        orderBy = postParameters.OrderBy,
                        fields = postParameters.Fields
                    };
                    return _urlHelper.Link("GetPosts", nextParameters);
                default:
                    var currentParameters = new
                    {
                        pageIndex = postParameters.PageIndex,
                        pageSize = postParameters.PageSize,
                        orderBy = postParameters.OrderBy,
                        fields = postParameters.Fields
                    };
                    return _urlHelper.Link("GetPosts", currentParameters);
            }
        }
        public IEnumerable<LinkResource> CreateLinksForPost(int id, string fields = null)
        {
            var links = new List<LinkResource>();
            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                    new LinkResource(
                    _urlHelper.Link("GetPost", new { id }), "self", "GET"
                ));
            }
            else
            {
                links.Add(
                    new LinkResource(
                        _urlHelper.Link("GetPost", new { id, fields }), "self", "GET"
                ));
            }
            links.Add(
                new LinkResource(
                    _urlHelper.Link("DeletePost", new { id }), "delete_post", "DELETE"
                    )
                );
            return links;
        }
        public IEnumerable<LinkResource> CreateLinksForPosts(PostParameters postParameters, bool hasPrevious,bool hasNext)
        {
            var links = new List<LinkResource>();
            links.Add(
                new LinkResource(
                    CreatePostUri(postParameters,PaginationResourceUriType.CurrentPage),"self","GET"
                    )
                );
            if (hasPrevious)
            {
                links.Add(
                    new LinkResource(
                    CreatePostUri(postParameters, PaginationResourceUriType.PreviousPage), "previous", "GET"
                ));
            }
            if(hasNext)
            {
                links.Add(
                    new LinkResource(
                        CreatePostUri(postParameters, PaginationResourceUriType.NextPage), "next", "GET"
                ));
            }
            
            return links;
        }
    }
}


