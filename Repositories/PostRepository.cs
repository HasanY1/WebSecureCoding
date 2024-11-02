// Repositories/PostRepository.cs
using PostService.Models;
using PostService.Models.PostValidation;
using System.Collections.Generic;
using System.Linq;

namespace PostService.Repositories
{
    public class PostRepository
    {
        private static List<Post> posts = new List<Post>();
        private static int nextId = 1;

        public IEnumerable<Post> GetAll() => posts;

        public Post ? GetById(int id) => posts.FirstOrDefault(p => p.Id == id);

        public Post Add(PostRequest post)
        {
            Post createdPost = new()
            {
                Content = post.Content,
                Title = post.Title,
                Id = nextId++
            };
            posts.Add(createdPost);
            return createdPost;
        }
    }
}
