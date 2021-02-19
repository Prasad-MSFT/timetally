// <copyright file="UserTimesheet.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the timesheet of a particular user for particular date.
    /// </summary>
    public class UserTimesheet
    {
        /// <summary>
        /// Gets or sets the calendar date.
        /// </summary>
        public DateTime TimesheetDate { get; set; }

        /// <summary>
        /// Gets or sets the project details.
        /// </summary>
#pragma warning disable CA2227 // Need to add values in list
        public List<ProjectDetails> ProjectDetails { get; set; }
#pragma warning restore CA2227 // Need to add values in list
    }
}
