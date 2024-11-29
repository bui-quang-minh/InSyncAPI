using System.ComponentModel.DataAnnotations;

namespace InSyncAPI.Dtos
{
    public class PageDto
    {
        public class ViewPageDto
        {
            public Guid Id { get; set; }
            public string Slug { get; set; } = null!;
            public string Title { get; set; } = null!;
            public string? Content { get; set; }
            public string? Note { get; set; }
            public DateTime DateCreated { get; set; }
            public DateTime? DateUpdated { get; set; }
        }
        public class UpdatePageDto
        {
            public Guid Id { get; set; }
            [Required]
            [StringLength(600)]
            public string Slug { get; set; } = null!;
            [Required]
            [StringLength(500)]
            public string Title { get; set; } = null!;
            public string? Content { get; set; }
            public string? Note { get; set; }
        }
        public class AddPageDto
        {
            [Required]
            [StringLength(600)]
            public string Slug { get; set; } = null!;
            [Required]
            [StringLength(500)]
            public string Title { get; set; } = null!;
            public string? Content { get; set; }
            public string? Note { get; set; }
        }
        public class ActionPageResponse
        {
            public string Message { get; set; }
            public Guid Id { get; set; }
        }
    }
}
