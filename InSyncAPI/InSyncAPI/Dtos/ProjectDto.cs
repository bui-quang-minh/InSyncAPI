using System.ComponentModel.DataAnnotations;

namespace InSyncAPI.Dtos
{
    public class ViewProjectDto
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; } = null!;
        public string? Description { get; set; }
        public Guid UserId { get; set; }
        public string displayName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool IsPublish { get; set; }
    }
    public class AddProjectDto
    {
        [Required]
        [StringLength(255, MinimumLength = 5)]
        public string ProjectName { get; set; } = null!;
        public string? Description { get; set; }
        public Guid UserId { get; set; }
        public bool IsPublish { get; set; }
    }
    public class UpdateProjectDto
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsPublish { get; set; }
    }
    public class ActionProjectResponse
    {
        public string Message { get; set; }
        public Guid Id { get; set; }
    }
}
