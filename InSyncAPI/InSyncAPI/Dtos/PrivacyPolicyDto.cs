using System.ComponentModel.DataAnnotations;

namespace InSyncAPI.Dtos
{
    public class ViewPrivacyPolicyDto
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string Title { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
    public class AddPrivacyPolicyDto
    {
        public string? Description { get; set; }
        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = null!;
    }
    public class UpdatePrivacyPolicyDto
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = null!;
    }

}
