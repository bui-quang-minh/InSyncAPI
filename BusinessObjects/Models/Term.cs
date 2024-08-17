using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class Term
    {
        public Guid Id { get; set; }
        public string Question { get; set; } = null!;
        public string Answer { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}
