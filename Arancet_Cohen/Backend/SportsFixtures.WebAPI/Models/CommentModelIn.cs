using System.ComponentModel.DataAnnotations;


namespace ObligatorioDA2.WebAPI.Models
{
    public class CommentModelIn
    {
        [Required(AllowEmptyStrings = false)]
        public string Text { get; set; }
    }
}
