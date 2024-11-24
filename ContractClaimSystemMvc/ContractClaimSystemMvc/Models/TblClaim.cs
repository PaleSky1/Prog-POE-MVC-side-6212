namespace ContractClaimSystemMvc.Models
{
    public class TblClaim
    {
        public int ClaimId { get; set; }
        public int UserId { get; set; }
        public int HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalPayment { get; set; }
        public string Status { get; set; }

        public string UploadedFile { get; set; } 
        public string UploadedFileName { get; set; }
        public virtual TblUser User { get; set; } = null!;
    }
}