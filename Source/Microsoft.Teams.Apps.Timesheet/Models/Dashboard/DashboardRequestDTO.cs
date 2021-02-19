// <copyright file="DashboardRequestDTO.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the dashboard timesheet requests.
    /// </summary>
    public class DashboardRequestDTO
    {
        /// <summary>
        /// Gets or sets the user Id.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the number of days request submitted for.
        /// </summary>
        public int NumberOfDays { get; set; }

        /// <summary>
        /// Gets or sets the total hours.
        /// </summary>
        public int TotalHours { get; set; }

        /// <summary>
        /// Gets or sets the status of dashboard request which belongs to <see cref="TimesheetStatus"/>.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the list of submitted timesheet request Ids by reportee.
        /// </summary>
        public IEnumerable<Guid> SubmittedTimesheetRequestIds { get; set; }

        /// <summary>
        /// Gets or sets the timesheet dates of dashboard request.
        /// </summary>
#pragma warning disable CA2227
        public List<List<DateTime>> RequestedForDates { get; set; }
#pragma warning restore CA2227
    }
}
