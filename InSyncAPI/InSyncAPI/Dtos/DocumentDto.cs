using System.ComponentModel.DataAnnotations;

namespace InSyncAPI.Dtos
{
    public class DocumentDto
    {
        public class ViewDocumentDto
        {
            public Guid Id { get; set; }
            public string Slug { get; set; } = null!;
            public string Title { get; set; } = null!;
            public string? Content { get; set; }
            public string? Note { get; set; }
            public DateTime? DateCreated { get; set; }
            public DateTime? DateUpdated { get; set; }
            public string Category { get; set; } = null!;
        }
        public class UpdateDocumentDto
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
            [Required]
            [StringLength(300)]
            public string Category { get; set; } = null!;
        }
        public class AddDocumentDto
        {
            [Required]
            [StringLength(600)]
            public string Slug { get; set; } = null!;
            [Required]
            [StringLength(500)]
            public string Title { get; set; } = null!;
            public string? Content { get; set; }
            public string? Note { get; set; }
            [Required]
            [StringLength(300)]
            public string Category { get; set; } = null!;
        }
        public class ActionDocumentResponse
        {
            public string Message { get; set; }
            public Guid Id { get; set; }
        }
    }
}
