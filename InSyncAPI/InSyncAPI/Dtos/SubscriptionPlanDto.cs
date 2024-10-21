using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace InSyncAPI.Dtos
{
    public class ViewSubscriptionPlanDto
    {
        public Guid Id { get; set; }
        public string SubscriptionsName { get; set; } = null!;
        public bool Status { get; set; }
        public decimal Price { get; set; }
        public string UserId { get; set; }
        public string UserIdGuid { get; set; }
        public string DisplayName { get; set; }
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
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Content { get; set; } = null!;

        [Range(0, int.MaxValue)]
        public int? MaxProjects { get; set; }

        [Range(0, int.MaxValue)]
        public long? MaxAssets { get; set; }

        [Range(0, int.MaxValue)]
        public int? MaxScenarios { get; set; }

        [Range(0, int.MaxValue)]
        public int? MaxUsersAccess { get; set; }

        [Range(0, long.MaxValue)]
        public long? StorageLimit { get; set; }

        public string? SupportLevel { get; set; }

        public string? CustomFeaturesDescription { get; set; }

        [Required]
        [Range(0, long.MaxValue)]
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
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        public String UserIdClerk { get; set; }

        [Required]
        public string Content { get; set; } = null!;

        [Range(0, int.MaxValue)]
        public int? MaxProjects { get; set; }

        [Range(0, long.MaxValue)]
        public long? MaxAssets { get; set; }

        [Range(0, int.MaxValue)]
        public int? MaxScenarios { get; set; }

        [Range(0, int.MaxValue)]
        public int? MaxUsersAccess { get; set; }

        [Range(0, long.MaxValue)]
        public long? StorageLimit { get; set; }

        public string? SupportLevel { get; set; }

        public string? CustomFeaturesDescription { get; set; }

        [Required]
        [Range(0, long.MaxValue)]
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
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        public string Content { get; set; } = null!;

        [Range(0, int.MaxValue)]
        public int? MaxProjects { get; set; }

        [Range(0, int.MaxValue)]
        public long? MaxAssets { get; set; }

        [Range(0, int.MaxValue)]
        public int? MaxScenarios { get; set; }

        [Range(0, int.MaxValue)]
        public int? MaxUsersAccess { get; set; }

        [Range(0, long.MaxValue)]
        public long? StorageLimit { get; set; }

        public string? SupportLevel { get; set; }

        public string? CustomFeaturesDescription { get; set; }

        [Range(0, long.MaxValue)]
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
