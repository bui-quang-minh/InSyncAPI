using System.ComponentModel.DataAnnotations;

namespace InSyncAPI.Dtos
{
    public class ViewTutorialDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Content { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool IsShow { get; set; }
        public long Order { get; set; }
    }

    public class AddTutorialDto
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = null!;
        public string? Content { get; set; }
        public bool IsShow { get; set; } = false;
        public long Order { get; set; } = 0;
    }
    public class UpdateTutorialDto
    { 
        public Guid Id { get; set; }
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = null!;
        public string? Content { get; set; }
        public bool IsShow { get; set; }
        public long Order { get; set; }
    }
    public class ActionTutorialResponse
    {
        public string Message { get; set; }
        public Guid Id { get; set; }
    }
}
