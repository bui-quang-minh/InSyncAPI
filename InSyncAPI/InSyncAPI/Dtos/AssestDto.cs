﻿using System.ComponentModel.DataAnnotations;

namespace InSyncAPI.Dtos
{
    public class ViewAssetDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string AssestName { get; set; } = null!;
        public string? Type { get; set; }
        public string FilePath { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public DateTime? DateUdpated { get; set; }
    }
    public class UpdateAssetDto
    {
        public Guid Id { get; set; }
        [Required]
        [StringLength(255)]
        public string AssestName { get; set; } = null!;
        public string? Type { get; set; }
    }
    public class AddAssetDto
    {
        public Guid ProjectId { get; set; }
        [Required]
        [StringLength(255)]
        public string AssestName { get; set; } = null!;
        [StringLength(255)]
        public string? Type { get; set; }
        [Required]
        public string FilePath { get; set; } = null!;
    }
    public class ActionAssetResponse
    {
        public string Message { get; set; }
        public Guid Id { get; set; }
    }

}
