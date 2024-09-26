using System.ComponentModel.DataAnnotations;

namespace InSyncAPI.Dtos
{
    public class ViewScenarioDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? StepsWeb { get; set; }
        public string? StepsAndroid { get; set; }
        public bool? IsFavorites { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? AuthorId { get; set; }
        public string AuthorName { get; set; }
    }

    public class AddScenarioDto
    {
        [Required]
        public Guid ProjectId { get; set; }
        [Required]
        [StringLength(255, MinimumLength =5)]
        public string ScenarioName { get; set; } = null!;
        public string? Description { get; set; }
        public string? StepsWeb { get; set; }
        public string? StepsAndroid { get; set; }
        public bool? IsFavorites { get; set; }
        public string? ImageUrl { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public class UpdateScenarioDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        [Required]
        [StringLength(255, MinimumLength = 5)]
        public string ScenarioName { get; set; } = null!;
        public string? Description { get; set; }
        public string? StepsWeb { get; set; }
        public string? StepsAndroid { get; set; }
        public bool? IsFavorites { get; set; }
        public string? ImageUrl { get; set; } 
    }
    public class UpdateRenameScenarioDto
    {
        public Guid Id { get; set; }
        [Required]
        [StringLength(255, MinimumLength = 5)]
        public string ScenarioName { get; set; } = null!;
    }
    public class ActionScenarioResponse
    {
        public string Message { get; set; }
        public Guid Id { get; set; }
    }
}
