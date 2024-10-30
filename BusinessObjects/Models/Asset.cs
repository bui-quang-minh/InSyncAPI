using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class Asset
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string AssetName { get; set; } = null!;
        public string? Type { get; set; }
        public string FilePath { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public virtual Project Project { get; set; } = null!;
    }
}
