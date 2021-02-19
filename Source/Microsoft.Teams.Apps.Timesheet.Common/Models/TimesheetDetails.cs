// <copyright file="TimesheetDetails.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Models
{
    using System;

    /// <summary>
    /// Represents the timesheet details.
    /// </summary>
    public class TimesheetDetails
    {
        /// <summary>
        /// Gets or sets task Id.
        /// </summary>
        public Guid TaskId { get; set; }

        /// <summary>
        /// Gets or sets task title.
        /// </summary>
        public string TaskTitle { get; set; }

        /// <summary>
        /// Gets or sets utilized efforts.
        /// </summary>
        public int Hours { get; set; }

        /// <summary>
        /// Gets or sets the status of current task which belongs to <see cref="TimesheetStatus"/>
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets manager comments.
        /// </summary>
        public string ManagerComments { get; set; }
    }
}
