using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveTrack.Core.Interfaces
{
    public interface IApprovalService
    {
        Task<(bool Success, string? ErrorMessage)> ApproveAsync(int leaveRequestId);
        Task<(bool Success, string? ErrorMessage)> RejectAsync(int leaveRequestId, string rejectionReason);
    }
}
