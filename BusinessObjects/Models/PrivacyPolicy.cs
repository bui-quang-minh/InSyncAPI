using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class PrivacyPolicy
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string Title { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}
