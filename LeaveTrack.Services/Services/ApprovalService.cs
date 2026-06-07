using LeaveTrack.Core.Interfaces;
using LeaveTrack.Data.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveTrack.Services.Services
{
    public class ApprovalService : IApprovalService
    {
        private readonly AppDbContext _dbContext;
        public ApprovalService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
