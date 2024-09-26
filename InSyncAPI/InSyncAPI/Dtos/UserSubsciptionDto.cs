using System.ComponentModel.DataAnnotations;

namespace InSyncAPI.Dtos
{
    public class ViewUserSubsciptionDto
    {
        public Guid Id { get; set; }
        public Guid SubscriptionPlanId { get; set; }
        public string SubscriptionPlanName { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public DateTime StripeCurrentPeriodEnd { get; set; }
        public string StripeCustomerId { get; set; } = null!;
        public string StripeSubscriptionId { get; set; } = null!;
        public string StripePriceId { get; set; } = null!;
        public DateTime DateCreated { get; set; }
    }
    public class AddUserSubsciptionDto
    {
        public Guid SubscriptionPlanId { get; set; }
        public Guid UserId { get; set; }
        [Required]
        public DateTime StripeCurrentPeriodEnd { get; set; }
        [Required]
        public string StripeCustomerId { get; set; } = null!;
        [Required]
        public string StripeSubscriptionId { get; set; } = null!;
        [Required]
        public string StripePriceId { get; set; } = null!;
        
    }
    public class UpdateUserSubsciptionDto
    {
        public Guid Id { get; set; }
        public Guid SubscriptionPlanId { get; set; }
        public Guid UserId { get; set; }
        [Required]
        public DateTime StripeCurrentPeriodEnd { get; set; }
        [Required]
        public string StripeCustomerId { get; set; } = null!;
        [Required]
        public string StripeSubscriptionId { get; set; } = null!;
        [Required]
        public string StripePriceId { get; set; } = null!;
    }

    public class ActionUserSubsciptionResponse
    {
        public string Message { get; set; }
        public Guid Id { get; set; }
    }
}
