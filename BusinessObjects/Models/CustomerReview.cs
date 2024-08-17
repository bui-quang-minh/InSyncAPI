using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class CustomerReview
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string JobTitle { get; set; } = null!;
        public string Review { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public bool IsShow { get; set; }
    }
}
