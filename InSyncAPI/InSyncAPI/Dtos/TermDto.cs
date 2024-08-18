using System.ComponentModel.DataAnnotations;

namespace InSyncAPI.Dtos
{
    public class ViewTermDto
    {
        public Guid Id { get; set; }
        public string Question { get; set; } = null!;
        public string Answer { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
    public class AddTermsDto
    {
        [Required]
        [MaxLength(200)]
        public string Question { get; set; } = null!;
        [Required]
        public string Answer { get; set; } = null!;  
    }
    public class UpdateTermsDto
    {
        public Guid Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Question { get; set; } = null!;
        [Required]
        public string Answer { get; set; } = null!;
       
    }
}
