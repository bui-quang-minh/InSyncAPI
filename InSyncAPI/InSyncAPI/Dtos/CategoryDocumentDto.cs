using System.ComponentModel.DataAnnotations;
using static InSyncAPI.Dtos.DocumentDto;

namespace InSyncAPI.Dtos
{
    public class CategoryDocumentDto
    {
        public class ViewCategoryDocumentDto
        {
            public Guid Id { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public int Order { get; set; }
            public DateTime DateCreated { get; set; }
            public DateTime? DateUpdated { get; set; }

            public virtual IEnumerable<ViewDocumentDto> Documents { get; set; }
        }
        public class UpdateCategoryDocumentDto
        {
            public Guid Id { get; set; }
            [Required]
            [StringLength(500)]
            public string Title { get; set; } = null!;
            [Required]
            [Range(0, int.MaxValue)]
            public int Order { get; set; }
            public string? Description { get; set; }
        }
        public class AddCategoryDocumentDto
        {
           
            [Required]
            [StringLength(500)]
            public string Title { get; set; } = null!;
            [Required]
            [Range(0, int.MaxValue)]
            public int Order { get; set; }
            public string? Description { get; set; }
        }
        public class ActionCategoryDocumentResponse
        {
            public string Message { get; set; }
            public Guid Id { get; set; }
        }
    }
}
