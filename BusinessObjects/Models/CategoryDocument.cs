using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class CategoryDocument
    {
        public CategoryDocument()
        {
            Documents = new HashSet<Document>();
        }

        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public int Order { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<Document> Documents { get; set; }
    }
}
