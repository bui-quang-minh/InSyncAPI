using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public partial class Scenario
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ScenarioName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? StepsWeb { get; set; }
        public string? StepsAndroid { get; set; }

        public virtual Project Project { get; set; } = null!;
    }
}
