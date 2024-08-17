using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class Tutorial
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Content { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool IsShow { get; set; }
        public long Order { get; set; }
    }
}
