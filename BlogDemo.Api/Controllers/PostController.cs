using AutoMapper;
using BlogDemo.Api.Helpers;
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
using Microsoft.AspNetCore.JsonPatch;

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
        [RequestHeaderMatchingMediaType("Accept", new[] { "application/json" })]
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
        [RequestHeaderMatchingMediaType("Accept", new[] { "application/hateoas+json" })]
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
            var shapedWithLinks = shapedPostResource.Select(x =>
            {
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
                postList.PageIndex,
                postList.PageSize,
                postList.TotalItemsCount,
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
            var links = CreateLinksForPost(id, Fields);
            var result2 = result as IDictionary<string, object>;
            result2.Add("links", links);
            return Ok(result2);
        }
        [HttpPost(Name = "CreatePost")]
        [RequestHeaderMatchingMediaType("Content-Type", new[] { "application/post.create+json" })]
        [RequestHeaderMatchingMediaType("Accept", new[] { "application/hateoas+json" })]
        public async Task<IActionResult> Post([FromBody] PostAddResource postAddResource)
        {
            if (postAddResource == null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return new MyUnprocessableEntityObjectResult(ModelState);
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
            var linksResource = CreateLinksForPost(newPost.Id);
            var linkedPostResource=resultResource.ToDynamic() as IDictionary<string,object>;
            linkedPostResource.Add("links",linksResource);
            return CreatedAtRoute("GetPost", new { id = linkedPostResource["Id"] }, linkedPostResource);
        }
        [HttpDelete("{id}",Name = "DeletePost")]
        public async Task<IActionResult> DeletePost(int id)
        {
            Post post =await _postRepository.GetPostByIdAsync(id);
            if (post==null)
            {
                return NotFound();
            }
            _postRepository.DeletePost(post);
            if (!await _unitOfWork.SaveAsync())
            {
                throw new Exception($"删除Post{id}时失败！");
            }

            return NoContent();
        }
        [HttpPut("{id}",Name ="UpdatePost")]
        [RequestHeaderMatchingMediaType("Content-Type", new[] { "application/post.update+json" })]
        public async Task<IActionResult> UpdatePost(int id,[FromBody] PostUpdateResource postUpdateResource)
        {
            if (postUpdateResource==null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new MyUnprocessableEntityObjectResult(ModelState);
            }

            var post = await _postRepository.GetPostByIdAsync(id);
            if (post==null)
            {
                return NotFound();
            }

            post.LastModified = DateTime.Now;
            _mapper.Map(postUpdateResource, post);
            if (!await _unitOfWork.SaveAsync())
            {
                throw new Exception($"修改Post{id}失败！");
            }

            return NoContent();
        }
        [HttpPatch("{id}",Name = "PartiallyUpdatePost")]
        [RequestHeaderMatchingMediaType("Content-Type",new []{"application/json-patch+json"})]
        public async Task<IActionResult> PartiallyUpdatePost(int id,[FromBody] JsonPatchDocument<PostUpdateResource> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var post = await _postRepository.GetPostByIdAsync(id);
            if (post==null)
            {
                return NotFound();
            }

            var postToPatch = _mapper.Map<PostUpdateResource>(post);//执行从源对象到新目标对象的映射。源类型是从源对象推断出来的。
            patchDocument.ApplyTo(postToPatch,ModelState);
            TryValidateModel(postToPatch);
            if (!ModelState.IsValid)
            {
                return new MyUnprocessableEntityObjectResult(ModelState);
            }
            _mapper.Map(postToPatch, post);
            post.LastModified=DateTime.Now;
            _postRepository.Update(post);

            if (! await _unitOfWork.SaveAsync())
            {
                throw new Exception($"修改Post{id}时出错！");
            }

            return NoContent();
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
        public IEnumerable<LinkResource> CreateLinksForPosts(PostParameters postParameters, bool hasPrevious, bool hasNext)
        {
            var links = new List<LinkResource>();
            links.Add(
                new LinkResource(
                    CreatePostUri(postParameters, PaginationResourceUriType.CurrentPage), "self", "GET"
                    )
                );
            if (hasPrevious)
            {
                links.Add(
                    new LinkResource(
                    CreatePostUri(postParameters, PaginationResourceUriType.PreviousPage), "previous", "GET"
                ));
            }
            if (hasNext)
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


