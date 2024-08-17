using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class Project
    {
        public Project()
        {
            Assets = new HashSet<Asset>();
            Scenarios = new HashSet<Scenario>();
        }

        public Guid Id { get; set; }
        public string ProjectName { get; set; } = null!;
        public Guid UserId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool IsPublish { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<Asset> Assets { get; set; }
        public virtual ICollection<Scenario> Scenarios { get; set; }
    }
}
