// <copyright file="ProjectDetails.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the projects details and related tasks.
    /// </summary>
    public class ProjectDetails
    {
        /// <summary>
        /// Gets or sets project Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets project title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets project start date in UTC time zone.
        /// </summary>
        public DateTime StartDateInUtc { get; set; }

        /// <summary>
        /// Gets or sets project end date in UTC time zone.
        /// </summary>
        public DateTime EndDateInUtc { get; set; }

        /// <summary>
        /// Gets or sets the timesheet details.
        /// </summary>
#pragma warning disable CA2227 // Need to add timesheet details in list.
        public List<TimesheetDetails> TimesheetDetails { get; set; }
#pragma warning restore CA2227 // Need to add timesheet details in list.
    }
}
