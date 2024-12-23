﻿using System.ComponentModel.DataAnnotations;

namespace InSyncAPI.Dtos
{
    public class DocumentDto
    {
        public class ViewDocumentDto
        {
            public Guid Id { get; set; }
            public string Slug { get; set; } = null!;
            public string Title { get; set; } = null!;
            public string? Content { get; set; }
            public string? Note { get; set; }
            public int Order { get; set; }
            public DateTime DateCreated { get; set; }
            public DateTime? DateUpdated { get; set; }
            public Guid CategoryId { get; set; }
            public string CategoryName { get; set; }
        }
        public class ViewDocumentOfCategoryDto
        {
            public Guid Id { get; set; }
            public string Slug { get; set; } = null!;
            public string Title { get; set; } = null!;
            public int Order { get; set; }
        }
        public class UpdateDocumentDto
        {
            public Guid Id { get; set; }
            [Required]
            [StringLength(600)]
            public string Slug { get; set; } = null!;
            [Required]
            [StringLength(500)]
            public string Title { get; set; } = null!;
            public string? Content { get; set; }
            public string? Note { get; set; }
            [Required]
            [Range(0, int.MaxValue)]
            public int Order { get; set; }
            public Guid CategoryId { get; set; }
        }
        public class AddDocumentDto
        {
            [Required]
            [StringLength(600)]
            public string Slug { get; set; } = null!;
            [Required]
            [StringLength(500)]
            public string Title { get; set; } = null!;
            public string? Content { get; set; }
            public string? Note { get; set; }
            [Required]
            [Range(0, int.MaxValue)]
            public int Order { get; set; }
            public Guid CategoryId { get; set; }
        }
        public class ActionDocumentResponse
        {
            public string Message { get; set; }
            public Guid Id { get; set; }
        }
    }
}
