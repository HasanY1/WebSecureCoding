// Models/Post.cs
using System.ComponentModel.DataAnnotations;

namespace PostService.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        public required string Content { get; set; }
    }
}
