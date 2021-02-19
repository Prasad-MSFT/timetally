// <copyright file="ProjectUtilizationDTO.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System;

    /// <summary>
    /// Custom error response model for APIs.
    /// </summary>
    public class ProjectUtilizationDTO
    {
        /// <summary>
        /// Gets or sets Id of project.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets title of project.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets billable hours of project.
        /// </summary>
        public int BillableUtilizedHours { get; set; }

        /// <summary>
        /// Gets or sets non billable hours of project.
        /// </summary>
        public int NonBillableUtilizedHours { get; set; }

        /// <summary>
        /// Gets or sets underutilized billable hours of project.
        /// </summary>
        public int UnderutilizedBillableHours { get; set; }

        /// <summary>
        /// Gets or sets underutilized non-billable hours of project.
        /// </summary>
        public int UnderutilizedNonBillableHours { get; set; }

        /// <summary>
        /// Gets or sets total hours of project.
        /// </summary>
        public int TotalHours { get; set; }
    }
}
