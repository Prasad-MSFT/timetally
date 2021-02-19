// <copyright file="Task.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Holds the details of a task entity.
    /// </summary>
    public partial class Task
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Task"/> class.
        /// </summary>
        public Task()
        {
            this.Timesheets = new HashSet<TimesheetEntity>();
        }

        /// <summary>
        /// Gets or sets task Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets task title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a task was deleted.
        /// </summary>
        public bool IsRemoved { get; set; }

        /// <summary>
        /// Gets or sets project Id.
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Gets or sets project details.
        /// </summary>
        public virtual Project Project { get; set; }

        /// <summary>
        /// Gets or sets timesheet details.
        /// </summary>
#pragma warning disable CA2227 // Need to add/remove timesheet details for a task
        public virtual ICollection<TimesheetEntity> Timesheets { get; set; }
#pragma warning restore CA2227 // Need to add/remove timesheet details for a task
    }
}