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
    public class CreateModel : PageModel
    {
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly AppDbContext _context;

        public CreateModel(ILeaveRequestService leaveRequestService, AppDbContext context)
        {
            _leaveRequestService = leaveRequestService;
            _context = context;
        }

        [BindProperty]
        public LeaveRequest Request { get; set; } = new();

        public SelectList EmployeeList { get; set; } = default!;
        public SelectList LeaveTypeList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Request.StartDate = DateTime.UtcNow.AddDays(1);
            Request.EndDate = DateTime.UtcNow.AddDays(1);
            await PopulateDropdownsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return Page();
            }

            if (Request.EndDate < Request.StartDate)
            {
                ModelState.AddModelError(string.Empty, "End date cannot be before start date.");
                await PopulateDropdownsAsync();
                return Page();
            }

            if (Request.StartDate.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(string.Empty, "Start date cannot be in the past.");
                await PopulateDropdownsAsync();
                return Page();
            }

            Request.LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Pending;
            Request.CreatedAt = DateTime.UtcNow;

            await _leaveRequestService.CreateAsync(Request);

            TempData["Success"] = "Leave request submitted successfully.";
            return RedirectToPage("Index");
        }

        private async Task PopulateDropdownsAsync()
        {
            var employees = await _context.Employees
                .OrderBy(e => e.LastName)
                .ToListAsync();

            var leaveTypes = await _context.LeaveTypes
                .ToListAsync();

            EmployeeList = new SelectList(
                employees, "Id", "LastName");

            LeaveTypeList = new SelectList(
                leaveTypes, "Id", "PrettyName");
        }
    }
}