using System.ComponentModel.DataAnnotations;

namespace PostService.Models.PostValidation
{
    public class PostRequest
    {
        [Required(ErrorMessage = "Title is required.")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        public required string Content { get; set; }



    }
}
