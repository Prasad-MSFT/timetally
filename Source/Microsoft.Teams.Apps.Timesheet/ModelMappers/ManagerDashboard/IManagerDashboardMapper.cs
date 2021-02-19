// <copyright file="IManagerDashboardMapper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.ModelMappers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Interface exposes methods used for dashboard models mapping.
    /// </summary>
    public interface IManagerDashboardMapper
    {
        /// <summary>
        /// Gets dashboard project view model to be sent as API response.
        /// </summary>
        /// <param name="project">The project entity model.</param>
        /// <param name="timesheets">List of timesheet entity model.</param>
        /// <returns>Returns a dashboard project view entity model.</returns>
        DashboardProjectDTO MapForDashboardProjectViewModel(Project project, IEnumerable<TimesheetEntity> timesheets);

        /// <summary>
        /// Gets dashboard request view model to be sent as API response.
        /// </summary>
        /// <param name="timesheetRequestsCollection">Collection of list of timesheet entity model.</param>
        /// <returns>Returns a dashboard request view entity model.</returns>
        IEnumerable<DashboardRequestDTO> MapForViewModel(Dictionary<Guid, List<TimesheetEntity>>.ValueCollection timesheetRequestsCollection);
    }
}