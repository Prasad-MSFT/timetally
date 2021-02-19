// <copyright file="TimesheetMapper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.ModelMappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// This class manages model mappings related to timesheet entity.
    /// </summary>
    public class TimesheetMapper : ITimesheetMapper
    {
        /// <summary>
        /// Gets the timesheet model to be inserted in database.
        /// </summary>
        /// <param name="timesheetDate">The timesheet date to be save.</param>
        /// <param name="timesheetViewModel">The timesheet view model.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>The timesheet entity model.</returns>
        public TimesheetEntity MapForCreateModel(DateTime timesheetDate, TimesheetDetails timesheetViewModel, Guid userObjectId)
        {
            timesheetViewModel = timesheetViewModel ?? throw new ArgumentNullException(nameof(timesheetViewModel));

            var timesheet = new TimesheetEntity
            {
                TaskId = timesheetViewModel.TaskId,
                TaskTitle = timesheetViewModel.TaskTitle,
                TimesheetDate = timesheetDate,
                Hours = timesheetViewModel.Hours,
                Status = timesheetViewModel.Status,
                UserId = userObjectId,
            };

            if (timesheetViewModel.Status == (int)TimesheetStatus.Submitted)
            {
                timesheet.SubmittedOn = DateTime.UtcNow;
            }
            else
            {
                timesheet.SubmittedOn = null;
            }

            return timesheet;
        }

        /// <summary>
        /// Maps timesheet view model details to timesheet entity model that to be updated in database.
        /// </summary>
        /// <param name="timesheetViewModel">The timesheet entity view model.</param>
        /// <param name="timesheetModel">The timesheet entity model.</param>
        public void MapForUpdateModel(TimesheetDetails timesheetViewModel, TimesheetEntity timesheetModel)
        {
            timesheetViewModel = timesheetViewModel ?? throw new ArgumentNullException(nameof(timesheetViewModel));
            timesheetModel = timesheetModel ?? throw new ArgumentNullException(nameof(timesheetModel));

            timesheetModel.Status = timesheetViewModel.Status;
            timesheetModel.Hours = timesheetViewModel.Hours;
            timesheetModel.LastModifiedOn = DateTime.UtcNow;
        }

        /// <summary>
        /// Maps timesheet database entity to view model.
        /// </summary>
        /// <param name="timesheet">The timesheet details.</param>
        /// <returns>Returns timesheet view model.</returns>
        public TimesheetDTO MapForViewModel(TimesheetEntity timesheet)
        {
            timesheet = timesheet ?? throw new ArgumentNullException(nameof(timesheet), "Timesheet details should not be null");

            return new TimesheetDTO
            {
                Id = timesheet.Id,
                TaskTitle = timesheet.TaskTitle,
                TimesheetDate = timesheet.TimesheetDate.Date,
                Hours = timesheet.Hours,
                Status = timesheet.Status,
            };
        }

        /// <summary>
        /// Gets request approval view model to be sent as API response.
        /// </summary>
        /// <param name="timesheetRequests">List of submitted timesheet requests.</param>
        /// <returns>Returns a request approval view entity model.</returns>
        public IEnumerable<SubmittedRequestDTO> MapToViewModel(IEnumerable<TimesheetEntity> timesheetRequests)
        {
            timesheetRequests = timesheetRequests ?? throw new ArgumentNullException(nameof(timesheetRequests));

            var userTimesheets = timesheetRequests.GroupBy(timesheetRequest => timesheetRequest.TimesheetDate).Select(timesheetRequestsGroup => new SubmittedRequestDTO
            {
                TotalHours = timesheetRequestsGroup.Sum(timesheetRequest => timesheetRequest.Hours),
                UserId = timesheetRequestsGroup.First().UserId,
                Status = timesheetRequestsGroup.First().Status,
                TimesheetDate = timesheetRequestsGroup.First().TimesheetDate,
                ProjectTitles = timesheetRequestsGroup
                                    .Select(timesheet => timesheet.Task.Project.Title.Trim())
                                    .Distinct(),
                SubmittedTimesheetIds = timesheetRequestsGroup.Select(timesheet => timesheet.Id),
            });

            return userTimesheets;
        }
    }
}
