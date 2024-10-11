using System.ComponentModel.DataAnnotations;

namespace InSyncAPI.Dtos
{
    public class ViewSubscriptionPlanDto
    {
        public Guid Id { get; set; }
        public string SubscriptionsName { get; set; } = null!;
        public bool Status { get; set; }
        public decimal Price { get; set; }
        public Guid UserId { get; set; }
        public string DisplayName { get;set; }
        public string Content { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int? MaxProjects { get; set; }
        public long? MaxAssets { get; set; }
        public int? MaxScenarios { get; set; }
        public int? MaxUsersAccess { get; set; }
        public long? StorageLimit { get; set; }
        public string? SupportLevel { get; set; }
        public string? CustomFeaturesDescription { get; set; }
        public long DataRetentionPeriod { get; set; }
        public bool? PrioritySupport { get; set; }
        public bool? MonthlyReporting { get; set; }
    }

    public class AddSubscriptionPlanDto
    {

        [Required]
        [MaxLength(255)]
        public string SubscriptionsName { get; set; } = null!;
        [Required]
        public bool Status { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string Content { get; set; } = null!;
        public int? MaxProjects { get; set; }
        public long? MaxAssets { get; set; }
        public int? MaxScenarios { get; set; }
        public int? MaxUsersAccess { get; set; }
        public long? StorageLimit { get; set; }
        public string? SupportLevel { get; set; }
        public string? CustomFeaturesDescription { get; set; }
        [Required]
        public long DataRetentionPeriod { get; set; }
        public bool? PrioritySupport { get; set; }
        public bool? MonthlyReporting { get; set; }
    }
    public class AddSubscriptionPlanUserClerkDto
    {

        [Required]
        [MaxLength(255)]
        public string SubscriptionsName { get; set; } = null!;
        [Required]
        public bool Status { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        [Required]
        public String UserIdClerk { get; set; }
        [Required]
        public string Content { get; set; } = null!;
        public int? MaxProjects { get; set; }
        public long? MaxAssets { get; set; }
        public int? MaxScenarios { get; set; }
        public int? MaxUsersAccess { get; set; }
        public long? StorageLimit { get; set; }
        public string? SupportLevel { get; set; }
        public string? CustomFeaturesDescription { get; set; }
        [Required]
        public long DataRetentionPeriod { get; set; }
        public bool? PrioritySupport { get; set; }
        public bool? MonthlyReporting { get; set; }
    }

    public class UpdateSubscriptionPlanDto
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(255)]
        public string SubscriptionsName { get; set; } = null!;
        [Required]
        public bool Status { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        [Required]
        public string Content { get; set; } = null!;
        public int? MaxProjects { get; set; }
        public long? MaxAssets { get; set; }
        public int? MaxScenarios { get; set; }
        public int? MaxUsersAccess { get; set; }
        public long? StorageLimit { get; set; }
        public string? SupportLevel { get; set; }
        public string? CustomFeaturesDescription { get; set; }
        [Required]
        public long DataRetentionPeriod { get; set; }
        public bool? PrioritySupport { get; set; }
        public bool? MonthlyReporting { get; set; }
    }
    public class ActionSubsciptionPlanResponse
    {
        public string Message { get; set; }
        public Guid Id { get; set; }
    }
}
