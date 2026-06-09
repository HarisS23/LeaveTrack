using LeaveTrack.Core.Enums;
using LeaveTrack.Core.Interfaces;
using LeaveTrack.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LeaveTrack.Web.Pages.LeaveRequests
{
    public class DetailModel : PageModel
    {
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly IApprovalService _approvalService;
        private readonly IEmployeeService _employeeService;

        public DetailModel(
            ILeaveRequestService leaveRequestService,
            IApprovalService approvalService,
            IEmployeeService employeeService)
        {
            _leaveRequestService = leaveRequestService;
            _approvalService = approvalService;
            _employeeService = employeeService;
        }

        public LeaveRequest LeaveRequest { get; set; } = default!;
        public bool IsPending { get; set; }
        public string StatusBadge { get; set; } = string.Empty;
        public int RemainingBalance { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var request = await _leaveRequestService.GetByIdAsync(id);
            if (request == null) return NotFound();

            LeaveRequest = request;
            IsPending = request.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Pending;
            RemainingBalance = await _employeeService.GetRemainingLeaveBalanceAsync(request.EmployeeId);
            StatusBadge = GetStatusBadge(request.LeaveRequestStatusId);

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var (success, error) = await _approvalService.ApproveAsync(id);

            if (success)
                TempData["Success"] = "Request approved successfully.";
            else
                TempData["Error"] = error;

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(int id, string rejectionReason)
        {
            var (success, error) = await _approvalService.RejectAsync(id, rejectionReason);

            if (success)
                TempData["Success"] = "Request rejected.";
            else
                TempData["Error"] = error;

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            await _leaveRequestService.CancelAsync(id);
            TempData["Success"] = "Request cancelled.";
            return RedirectToPage(new { id });
        }

        private string GetStatusBadge(int statusId) => statusId switch
        {
            (int)LeaveRequestStatusEnum.Pending => "bg-warning text-dark",
            (int)LeaveRequestStatusEnum.Approved => "bg-success",
            (int)LeaveRequestStatusEnum.Rejected => "bg-danger",
            (int)LeaveRequestStatusEnum.Cancelled => "bg-secondary",
            _ => "bg-light text-dark"
        };
    }
}