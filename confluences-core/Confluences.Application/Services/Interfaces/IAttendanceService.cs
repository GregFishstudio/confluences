using Confluences.Domain.Dto; // à la place de Api.Models.Dto.Attendance
using Confluences.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Confluences.Application.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task SaveAttendanceEntriesAsync(List<AttendanceEntryDto> entries);
        Task<List<Presence>> GetPresencesByPeriodAsync(DateTime startDate, DateTime endDate);
    }
}
