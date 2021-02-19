// <copyright file="ManagerDashboardMapper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Microsoft.Teams.Apps.Timesheet.Test")]

namespace Microsoft.Teams.Apps.Timesheet.ModelMappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// A model class that contains methods related to dashboard models mapping.
    /// </summary>
    public class ManagerDashboardMapper : IManagerDashboardMapper
    {
        /// <summary>
        /// Gets dashboard project view model to be sent as API response.
        /// </summary>
        /// <param name="project">The project entity model.</param>
        /// <param name="timesheets">List of timesheet entity model.</param>
        /// <returns>Returns a dashboard project view entity model.</returns>
        public DashboardProjectDTO MapForDashboardProjectViewModel(Project project, IEnumerable<TimesheetEntity> timesheets)
        {
            project = project ?? throw new ArgumentNullException(nameof(project));
            timesheets = timesheets ?? throw new ArgumentNullException(nameof(timesheets));

            var dashboardProject = new DashboardProjectDTO
            {
                Id = project.Id,
                Title = project.Title,
                TotalHours = project.BillableHours + project.NonBillableHours,
                UtilizedHours = timesheets.Sum(timesheet => timesheet.Hours),
            };

            return dashboardProject;
        }

        /// <summary>
        /// Gets timesheet view model to be sent as API response.
        /// </summary>
        /// <param name="timesheetRequestsCollection">Collection of list of timesheet entity model.</param>
        /// <returns>Returns a timesheet view entity model.</returns>
        public IEnumerable<DashboardRequestDTO> MapForViewModel(Dictionary<Guid, List<TimesheetEntity>>.ValueCollection timesheetRequestsCollection)
        {
            timesheetRequestsCollection = timesheetRequestsCollection ?? throw new ArgumentNullException(nameof(timesheetRequestsCollection));
            var dashboardRequests = timesheetRequestsCollection.Select(timesheetRequests => new DashboardRequestDTO
            {
                NumberOfDays = timesheetRequests.GroupBy(timesheetRequest => timesheetRequest.TimesheetDate).Count(),
                TotalHours = timesheetRequests.Sum(timesheet => timesheet.Hours),
                UserId = timesheetRequests.First().UserId,
                Status = (int)timesheetRequests.First().Status,
                UserName = string.Empty,
                RequestedForDates = this.GetDistinctDates(timesheetRequests),
                SubmittedTimesheetRequestIds = timesheetRequests.Select(timesheet => timesheet.Id),
            });

            return dashboardRequests;
        }

        /// <summary>
        /// Get distinct dates.
        /// </summary>
        /// <param name="timesheetRequests">Timesheets.</param>
        /// <returns>List of dates.</returns>
        internal List<List<DateTime>> GetDistinctDates(List<TimesheetEntity> timesheetRequests)
        {
            var distinctDates = timesheetRequests.Select(timesheet => timesheet.TimesheetDate.Date).Distinct().ToList();
            var orderedDates = distinctDates.OrderBy(date => date.Date).ToList();
            var dateRange = new List<List<DateTime>>();
            int currentItemIndex = 0;

            // Filter date as range.
            // If ordered dates are suppose : [1 JAN, 2 JAN, 3 JAN, 5 JAN] then output will be [[1 JAN, 2 JAN, 3 JAN],[5 JAN]].
            for (int i = 0; i < orderedDates.Count; i++)
            {
                // If i = 0, add date on the start index.
                if (i == 0)
                {
                    dateRange.Add(new List<DateTime>());
                    dateRange[currentItemIndex].Add(orderedDates[i]);
                }
                else
                {
                    // If date is continuous (example 1 JAN, 2 JAN, 3 JAN), add them at same index.
                    if (orderedDates[i].Date.AddDays(-1) == dateRange[currentItemIndex].Last().Date)
                    {
                        dateRange[currentItemIndex].Add(orderedDates[i]);
                    }

                    // Else, add them at next index.
                    else
                    {
                        currentItemIndex += 1;
                        dateRange.Add(new List<DateTime>());
                        dateRange[currentItemIndex].Add(orderedDates[i]);
                    }
                }
            }

            return dateRange;
        }
    }
}