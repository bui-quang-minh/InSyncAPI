﻿using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class Document
    {
        public Guid Id { get; set; }
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Content { get; set; }
        public string? Note { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public Guid CategoryId { get; set; }
        public int Order { get; set; }

        public virtual CategoryDocument Category { get; set; } = null!;
    }
}
