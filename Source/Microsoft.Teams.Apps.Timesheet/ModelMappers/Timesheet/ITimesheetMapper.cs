// <copyright file="ITimesheetMapper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.ModelMappers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Exposes methods that manages model mappings related to timesheet entity.
    /// </summary>
    public interface ITimesheetMapper
    {
        /// <summary>
        /// Gets the timesheet model to be inserted in database.
        /// </summary>
        /// <param name="timesheetDate">The timesheet date to be save.</param>
        /// <param name="timesheetViewModel">The timesheet view model.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>The timesheet entity model.</returns>
        TimesheetEntity MapForCreateModel(DateTime timesheetDate, TimesheetDetails timesheetViewModel, Guid userObjectId);

        /// <summary>
        /// Maps timesheet view model details to timesheet entity model that to be updated in database.
        /// </summary>
        /// <param name="timesheetViewModel">The timesheet entity view model.</param>
        /// <param name="timesheetModel">The timesheet entity model.</param>
        void MapForUpdateModel(TimesheetDetails timesheetViewModel, TimesheetEntity timesheetModel);

        /// <summary>
        /// Maps timesheet database entity to view model.
        /// </summary>
        /// <param name="timesheet">The timesheet details.</param>
        /// <returns>Returns timesheet view model.</returns>
        TimesheetDTO MapForViewModel(TimesheetEntity timesheet);

        /// <summary>
        /// Gets request approval view model to be sent as API response.
        /// </summary>
        /// <param name="timesheetRequests">List of timesheet entity model.</param>
        /// <returns>Returns a request approval view entity model.</returns>
        IEnumerable<SubmittedRequestDTO> MapToViewModel(IEnumerable<TimesheetEntity> timesheetRequests);
    }
}
