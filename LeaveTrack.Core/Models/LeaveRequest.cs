namespace LeaveTrack.Core.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int LeaveRequestStatusId { get; set; }
        public string? Comment { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }

        public Employee Employee { get; set; } = null!;
        public LeaveType LeaveType { get; set; } = null!;
        public LeaveRequestStatus LeaveRequestStatus { get; set; } = null!;

    }
}
