using LeaveTrack.Core.Enums;
using LeaveTrack.Core.Interfaces;
using LeaveTrack.Core.Models;
using LeaveTrack.Data;
using LeaveTrack.Data.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LeaveTrack.Web.Pages.LeaveRequests
{
    public class EditModel : PageModel
    {
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly AppDbContext _context;

        public EditModel(ILeaveRequestService leaveRequestService, AppDbContext context)
        {
            _leaveRequestService = leaveRequestService;
            _context = context;
        }

        [BindProperty]
        public LeaveRequest Request { get; set; } = default!;
        public SelectList LeaveTypeList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var request = await _leaveRequestService.GetByIdAsync(id);
            if (request == null) return NotFound();

            if (request.LeaveRequestStatusId != (int)LeaveRequestStatusEnum.Pending)
            {
                TempData["Error"] = "Only pending requests can be edited.";
                return RedirectToPage("Detail", new { id });
            }

            Request = request;
            await PopulateDropdownsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Request.EndDate < Request.StartDate)
            {
                ModelState.AddModelError(string.Empty, "End date cannot be before start date.");
                await RefreshRequestNavigationAsync();
                await PopulateDropdownsAsync();
                return Page();
            }

            if (Request.StartDate.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(string.Empty, "Start date cannot be in the past.");
                await RefreshRequestNavigationAsync();
                await PopulateDropdownsAsync();
                return Page();
            }

            var existing = await _leaveRequestService.GetByIdAsync(Request.Id);
            if (existing == null) return NotFound();

            if (existing.LeaveRequestStatusId != (int)LeaveRequestStatusEnum.Pending)
            {
                TempData["Error"] = "This request can no longer be edited.";
                return RedirectToPage("Detail", new { id = Request.Id });
            }

            existing.LeaveTypeId = Request.LeaveTypeId;
            existing.StartDate = Request.StartDate;
            existing.EndDate = Request.EndDate;
            existing.Comment = Request.Comment;

            await _leaveRequestService.UpdateAsync(existing);

            TempData["Success"] = "Request updated successfully.";
            return RedirectToPage("Detail", new { id = Request.Id });
        }

        private async Task RefreshRequestNavigationAsync()
        {
            var full = await _leaveRequestService.GetByIdAsync(Request.Id);
            if (full != null)
                Request.Employee = full.Employee;
        }

        private async Task PopulateDropdownsAsync()
        {
            var leaveTypes = await _context.LeaveTypes.ToListAsync();
            LeaveTypeList = new SelectList(leaveTypes, "Id", "PrettyName");
        }
    }
}