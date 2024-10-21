using System.ComponentModel.DataAnnotations;

namespace InSyncAPI.Dtos
{
    public class ViewProjectDto
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; } = null!;
        public string? Description { get; set; }
        public string UserId { get; set; }
        public Guid UserIdGuid { get; set; }
        public string DisplayName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool IsPublish { get; set; }
    }
    public class AddProjectDto
    {
        [Required]
        [StringLength(255)]
        public string ProjectName { get; set; } = null!;
        public string? Description { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public bool IsPublish { get; set; }
    }
    public class AddProjectClerkDto
    {
        [Required]
        [StringLength(255)]
        public string ProjectName { get; set; } = null!;
        public string? Description { get; set; }
        [Required]
        public string  UserIdClerk { get; set; }
        [Required]
        public bool IsPublish { get; set; }
    }
    public class UpdateProjectDto
    {
        public Guid Id { get; set; }
        [Required]
        [StringLength(255)]
        public string ProjectName { get; set; } = null!;
        public string? Description { get; set; }
        [Required]
        public bool IsPublish { get; set; }
    }
    public class ActionProjectResponse
    {
        public string Message { get; set; }
        public Guid Id { get; set; }
    }
}
