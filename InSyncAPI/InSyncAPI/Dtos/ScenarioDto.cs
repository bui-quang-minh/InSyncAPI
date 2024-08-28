using System.ComponentModel.DataAnnotations;

namespace InSyncAPI.Dtos
{
    public class ViewScenarioDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ScenarioName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? StepsWeb { get; set; }
        public string? StepsAndroid { get; set; }
        public bool? IsFavorites { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? CreatedBy { get; set; }
        public string UserName { get; set; }
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

}
