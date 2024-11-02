// Controllers/PostsController.cs
using System.Web;
using Ganss.Xss;
using Microsoft.AspNetCore.Mvc;
using PostService.Models;
using PostService.Models.PostValidation;
using PostService.Repositories;
using PostService.Utilities;

namespace PostService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly PostRepository _repository;

        public PostsController()
        {
            _repository = new PostRepository();
        }

        [HttpGet]
        public ActionResult<IEnumerable<Post>> GetPosts()
        {
            return Ok(_repository.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<Post> GetPost(int id)
        {
            var post = _repository.GetById(id);
            if (post == null)
            {
                return NotFound();
            }
            return Ok(post);
        }

        [HttpPost]
        public ActionResult<Post> CreatePost([FromBody] PostRequest post)
        {



            SanitizationHelper.SanitizeRequest(post);

            if (!ModelState.IsValid) // Check if the model state is valid
            {
                return BadRequest(ModelState); // Return validation errors
            }
            // Initialize the sanitizer



            var createdPost = _repository.Add(post);
            return CreatedAtAction(nameof(GetPost), new { id = createdPost.Id }, createdPost);
        }

    }
}
