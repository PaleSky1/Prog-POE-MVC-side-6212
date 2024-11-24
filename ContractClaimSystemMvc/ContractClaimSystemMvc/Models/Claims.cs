using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ContractClaimSystemMvc.Models
{
    public class Claims : ITableEntity
    {
        [Key]
        public int LectureID { get; set; }

        public string? LectureName { get; set; }

        public int HoursWorked { get; set; }

        public decimal HourlyRate { get; set; }

        public string? AdditionalNotes { get; set; }

        public string? ClaimDescription { get; set; }

        public string? ClaimReference { get; set; }

        public string? FileName { get; set; }

        public string? FileLink { get; set; }

        public string? PartitionKey { get; set; }

        public string? RowKey { get; set; }

        public ETag ETag { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
    }
}
