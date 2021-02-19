// <copyright file="TimesheetRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Teams.Apps.Timesheet.Common.Extensions;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;

    /// <summary>
    /// This class manages all database operations related to timesheet entity.
    /// </summary>
    public class TimesheetRepository : BaseRepository<TimesheetEntity>, ITimesheetRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimesheetRepository"/> class.
        /// </summary>
        /// <param name="context">The timesheet context.</param>
        public TimesheetRepository(TimesheetContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Gets the active timesheet requests.
        /// </summary>
        /// <param name="userObjectIds">The user Ids of which requests to get.</param>
        /// <returns>Returns the list of timesheet requests.</returns>
        public async Task<Dictionary<Guid, List<TimesheetEntity>>> GetTimesheetRequestsAsync(List<Guid> userObjectIds)
        {
            var data = await this.Context.Timesheets
                .Where(x => x.Status == (int)TimesheetStatus.Submitted && userObjectIds.Contains(x.UserId))
                .Include(x => x.Task)
                .Include(x => x.Task.Project)
                .ToListAsync();

            return data
                .AsEnumerable()
                .GroupBy(x => x.UserId)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        /// <summary>
        /// Gets the timesheet requests.
        /// </summary>
        /// <param name="managerId">The manager Id of which requests to get.</param>
        /// <param name="timesheetStatus">The status of requests to get.</param>
        /// <returns>Returns the list of timesheet requests.</returns>
        public async Task<Dictionary<Guid, List<TimesheetEntity>>> GetTimesheetRequestsByManagerAsync(Guid managerId, TimesheetStatus timesheetStatus)
        {
            var data = await this.Context.Timesheets
                .Where(x => x.Status == (int)timesheetStatus && x.Task.Project.CreatedBy == managerId)
                .ToListAsync();

            return data
                .AsEnumerable()
                .GroupBy(x => x.UserId)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        /// <summary>
        /// Gets the timesheet requests.
        /// </summary>
        /// <param name="userId">The user Id of which requests to get.</param>
        /// <param name="timesheetDates">The dates of requests to get.</param>
        /// <returns>Returns the list of timesheet requests.</returns>
        public async Task<List<TimesheetEntity>> GetTimesheetsAsync(Guid userId, List<DateTime> timesheetDates)
        {
            var data = await this.Context.Timesheets
                .Where(x => timesheetDates.Contains(x.TimesheetDate.Date) && x.UserId == userId)
                .Include(x => x.Task)
                .Include(x => x.Task.Project)
                .ToListAsync();

            return data;
        }

        /// <summary>
        /// Gets the timesheet requests.
        /// </summary>
        /// <param name="userIds">The user Id of which requests to get.</param>
        /// <returns>Returns the list of timesheet requests.</returns>
        public List<TimesheetEntity> GetTimesheetRequestsByUserIds(List<Guid> userIds)
        {
            var data = this.Context.Timesheets
                .Where(x => userIds.Contains(x.UserId) && x.Status == (int)TimesheetStatus.Submitted)
                .Include(x => x.Task)
                .Include(x => x.Task.Project)
                .ToList();

            return data;
        }

        /// <summary>
        /// Updates timesheet entries.
        /// </summary>
        /// <param name="timesheets">The list of timesheet entries to be updated.</param>
        public void Update(IEnumerable<TimesheetEntity> timesheets)
        {
            this.Context.Timesheets.UpdateRange(timesheets);
        }

        /// <summary>
        /// Gets the timesheet requests using project id
        /// </summary>
        /// <param name="projectId">The project id of which requests to get.</param>
        /// <param name="timesheetStatus">Indicates the status of timesheet.</param>
        /// <param name="startDate">Start date of the month.</param>
        /// <param name="endDate">Last date the of the month</param>
        /// <returns>Returns the collection of timesheet requests.</returns>
        public IEnumerable<TimesheetEntity> GetTimesheetRequestsByProjectId(Guid projectId, TimesheetStatus timesheetStatus, DateTime startDate, DateTime endDate)
        {
            var status = (short)timesheetStatus;
            var data = this.Context.Timesheets
                .Where(timesheet => timesheet.Task.ProjectId == projectId && timesheet.Status == status && timesheet.TimesheetDate.Date >= startDate.Date && timesheet.TimesheetDate.Date <= endDate.Date)
                .Include(timesheet => timesheet.Task);

            return data;
        }

        /// <summary>
        /// Gets the  timesheet requests using task id
        /// </summary>
        /// <param name="taskId">The task id of which requests to get.</param>
        /// <param name="timesheetStatus">Indicates the status of timesheet.</param>
        /// <param name="startDate">Start date of the month.</param>
        /// <param name="endDate">Last date the of the month</param>
        /// <returns>Returns the collection of timesheet requests.</returns>
        public IEnumerable<TimesheetEntity> GetTimesheetRequestsByTaskId(Guid taskId, TimesheetStatus timesheetStatus, DateTime startDate, DateTime endDate)
        {
            return this.Context.Timesheets.Where(timesheet => timesheet.TaskId == taskId && timesheet.Status == (int)timesheetStatus && timesheet.TimesheetDate.Date >= startDate.Date && timesheet.TimesheetDate.Date <= endDate.Date);
        }

        /// <summary>
        /// Gets the timesheets filled by logged-in user.
        /// </summary>
        /// <param name="userObjectId">The user object Id of which timesheets to get.</param>
        /// <returns>List of timesheet.</returns>
        public IEnumerable<TimesheetEntity> GetTimesheetsOfUser(Guid userObjectId)
        {
            return this.Context.Timesheets.Where(timesheet => timesheet.UserId.Equals(userObjectId));
        }

        /// <summary>
        /// Gets the timesheets of logged-in user for specified dates.
        /// </summary>
        /// <param name="timesheetDates">The dates of which timesheet needs to be retrieved.</param>
        /// <param name="userObjectId">The user object Id of which timesheets to get.</param>
        /// <param name="projectIds">The projects Ids of which timesheets to get.</param>
        /// <returns>Returns the collection of timesheet.</returns>
        public IEnumerable<TimesheetEntity> GetTimesheetsOfUser(IEnumerable<DateTime> timesheetDates, Guid userObjectId, IEnumerable<Guid> projectIds = null)
        {
            var timesheets = this.Context.Timesheets
                .Where(timesheet => timesheet.UserId.Equals(userObjectId) && timesheetDates.Contains(timesheet.TimesheetDate.Date))
                .AsEnumerable();

            if (!projectIds.IsNullOrEmpty())
            {
                timesheets = timesheets
                    .Where(timesheet => projectIds.Contains(timesheet.Task.ProjectId)) ?? new List<TimesheetEntity>();
            }

            return timesheets;
        }
    }
}
