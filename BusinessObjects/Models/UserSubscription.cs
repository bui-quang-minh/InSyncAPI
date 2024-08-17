using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class UserSubscription
    {
        public Guid Id { get; set; }
        public Guid SubscriptionPlanId { get; set; }
        public Guid UserId { get; set; }
        public DateTime StripeCurrentPeriodEnd { get; set; }
        public string StripeCustomerId { get; set; } = null!;
        public string StripeSubscriptionId { get; set; } = null!;
        public string StripePriceId { get; set; } = null!;
        public DateTime DateCreated { get; set; }

        public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
