using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class User
    {
        public User()
        {
            Projects = new HashSet<Project>();
            Scenarios = new HashSet<Scenario>();
            SubscriptionPlans = new HashSet<SubscriptionPlan>();
            UserSubscriptions = new HashSet<UserSubscription>();
        }

        public Guid Id { get; set; }
        public string? UserIdClerk { get; set; }
        public string UserName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? DisplayName { get; set; }
        public bool? EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public bool? LockoutEnable { get; set; }
        public long AccessFailCount { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? Dob { get; set; }
        public string? Address { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int StatusUser { get; set; }
        public string? Role { get; set; }

        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<Scenario> Scenarios { get; set; }
        public virtual ICollection<SubscriptionPlan> SubscriptionPlans { get; set; }
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; }
    }
}
