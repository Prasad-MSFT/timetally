// <copyright file="IManagerDashboardHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Interface exposes methods used for managing operation on managers dashboard.
    /// </summary>
    public interface IManagerDashboardHelper
    {
        /// <summary>
        /// Get approved and active project details for dashboard between date range.
        /// </summary>
        /// <param name="managerUserObjectId">The manager user object Id who created a project.</param>
        /// <param name="startDate">Start date of the date range.</param>
        /// <param name="endDate">End date of the date range.</param>
        /// <returns>Returns list of dashboard projects.</returns>
        IEnumerable<DashboardProjectDTO> GetDashboardProjects(Guid managerUserObjectId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets timesheet requests which are pending for manager approval.
        /// </summary>
        /// <param name="managerObjectId">The manager Id for which request has been raised.</param>
        /// <param name="timesheetStatus">The status of requests to fetch.</param>
        /// <returns>Return list of submitted timesheet request.</returns>
        Task<IEnumerable<DashboardRequestDTO>> GetDashboardRequestsAsync(Guid managerObjectId, TimesheetStatus timesheetStatus);
    }
}
