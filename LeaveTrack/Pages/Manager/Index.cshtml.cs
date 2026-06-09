using LeaveTrack.Core.Enums;
using LeaveTrack.Core.Interfaces;
using LeaveTrack.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LeaveTrack.Web.Pages.Manager
{
    public class IndexModel : PageModel
    {
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly IApprovalService _approvalService;

        public IndexModel(
            ILeaveRequestService leaveRequestService,
            IApprovalService approvalService)
        {
            _leaveRequestService = leaveRequestService;
            _approvalService = approvalService;
        }

        public IEnumerable<LeaveRequest> PendingRequests { get; set; } = new List<LeaveRequest>();

        public async Task OnGetAsync()
        {
            PendingRequests = await _leaveRequestService.FilterAsync(
                null,
                (int)LeaveRequestStatusEnum.Pending,
                null,
                null,
                null);
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var (success, error) = await _approvalService.ApproveAsync(id);

            if (success)
                TempData["Success"] = "Request approved successfully.";
            else
                TempData["Error"] = error;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id, string rejectionReason)
        {
            var (success, error) = await _approvalService.RejectAsync(id, rejectionReason);

            if (success)
                TempData["Success"] = "Request rejected successfully.";
            else
                TempData["Error"] = error;

            return RedirectToPage();
        }
    }
}