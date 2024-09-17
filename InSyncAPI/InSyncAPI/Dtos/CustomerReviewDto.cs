using System.ComponentModel.DataAnnotations;

namespace InSyncAPI.Dtos
{
    public class ViewCustomerReviewDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string JobTitle { get; set; } = null!;
        public string Review { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public bool IsShow { get; set; }
    }

    public class AddCustomerReviewDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
        [Required]
        [StringLength(150)]
        public string JobTitle { get; set; } = null!;
        [Required]
        [StringLength(200)]
        public string Review { get; set; } = null!;
    }

    public class UpdateCustomerReviewDto
    {
        public Guid Id { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string JobTitle { get; set; } = null!;
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Review { get; set; } = null!;
        public bool IsShow { get; set; }
    }
    public class ActionCustomerReviewResponse
    {
        public string Message { get; set; }
        public Guid Id { get; set; }
    }
}
