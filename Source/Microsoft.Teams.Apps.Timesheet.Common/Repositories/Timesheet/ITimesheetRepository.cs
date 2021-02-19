// <copyright file="ITimesheetRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;

    /// <summary>
    /// Exposes methods which will be used to perform database operations on timesheet entity.
    /// </summary>
    public interface ITimesheetRepository : IBaseRepository<TimesheetEntity>
    {
        /// <summary>
        /// Gets the active timesheet requests.
        /// </summary>
        /// <param name="userObjectIds">The user Ids of which requests to get.</param>
        /// <returns>Returns the list of timesheet requests.</returns>
        Task<Dictionary<Guid, List<TimesheetEntity>>> GetTimesheetRequestsAsync(List<Guid> userObjectIds);

        /// <summary>
        /// Gets the timesheet requests.
        /// </summary>
        /// <param name="managerId">The manager Id of which requests to get.</param>
        /// <param name="timesheetStatus">The status of requests to get.</param>
        /// <returns>Returns the list of timesheet requests.</returns>
        Task<Dictionary<Guid, List<TimesheetEntity>>> GetTimesheetRequestsByManagerAsync(Guid managerId, TimesheetStatus timesheetStatus);

        /// <summary>
        /// Gets the timesheet requests.
        /// </summary>
        /// <param name="userIds">The user Ids of which requests to get.</param>
        /// <returns>Returns the list of timesheet requests.</returns>
        List<TimesheetEntity> GetTimesheetRequestsByUserIds(List<Guid> userIds);

        /// <summary>
        /// Updates timesheet entries.
        /// </summary>
        /// <param name="timesheets">The list of timesheet entries to be updated.</param>
        void Update(IEnumerable<TimesheetEntity> timesheets);

        /// <summary>
        /// Gets the timesheet requests.
        /// </summary>
        /// <param name="userId">The user Id of which requests to get.</param>
        /// <param name="timesheetDates">The dates of requests to get.</param>
        /// <returns>Returns the list of timesheet requests.</returns>
        Task<List<TimesheetEntity>> GetTimesheetsAsync(Guid userId, List<DateTime> timesheetDates);

        /// <summary>
        /// Gets the timesheet requests using project id
        /// </summary>
        /// <param name="projectId">The project id of which requests to get.</param>
        /// <param name="timesheetStatus">Indicates the status of timesheet.</param>
        /// /// <param name="startDate">Start date of the the month.</param>
        /// <param name="endDate">Last date the of the the month</param>
        /// <returns>Returns the collection of timesheet requests.</returns>
        IEnumerable<TimesheetEntity> GetTimesheetRequestsByProjectId(Guid projectId, TimesheetStatus timesheetStatus, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the timesheet requests using project id
        /// </summary>
        /// <param name="taskId">The task id of which requests to get.</param>
        /// <param name="timesheetStatus">Indicates the status of timesheet.</param>
        /// /// <param name="startDate">Start date of the the month.</param>
        /// <param name="endDate">Last date the of the the month</param>
        /// <returns>Returns the collection of timesheet requests.</returns>
        public IEnumerable<TimesheetEntity> GetTimesheetRequestsByTaskId(Guid taskId, TimesheetStatus timesheetStatus, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the timesheets filled by logged-in user.
        /// </summary>
        /// <param name="userObjectId">The user object Id of which timesheets to get.</param>
        /// <returns>List of timesheet.</returns>
        IEnumerable<TimesheetEntity> GetTimesheetsOfUser(Guid userObjectId);

        /// <summary>
        /// Gets the timesheets of logged-in user for specified dates.
        /// </summary>
        /// <param name="timesheetDates">The dates of which timesheet needs to be retrieved.</param>
        /// <param name="userObjectId">The user object Id of which timesheets to get.</param>
        /// <param name="projectIds">The projects Ids of which timesheets to get.</param>
        /// <returns>Returns the collection of timesheet.</returns>
        IEnumerable<TimesheetEntity> GetTimesheetsOfUser(IEnumerable<DateTime> timesheetDates, Guid userObjectId, IEnumerable<Guid> projectIds = null);
    }
}
