using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class SubscriptionPlan
    {
        public SubscriptionPlan()
        {
            UserSubscriptions = new HashSet<UserSubscription>();
        }

        public Guid Id { get; set; }
        public string SubscriptionsName { get; set; } = null!;
        public bool Status { get; set; }
        public decimal Price { get; set; }
        public Guid UserId { get; set; }
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
        public long? DataRetentionPeriod { get; set; }
        public bool? PrioritySupport { get; set; }
        public bool? MonthlyReporting { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; }
    }
}
