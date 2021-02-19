// <copyright file="RequestApprovalDTO.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents the requests approval.
    /// </summary>
    public class RequestApprovalDTO
    {
        /// <summary>
        /// Gets or sets the reportee object Id.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the timesheet request Id.
        /// </summary>
        public Guid TimesheetId { get; set; }

        /// <summary>
        /// Gets or sets the manager comments.
        /// </summary>
        [MaxLength(100)]
        public string ManagerComments { get; set; }

        /// <summary>
        /// Gets or sets the date of timesheets.
        /// </summary>
#pragma warning disable CA2227
        public List<DateTime> TimesheetDate { get; set; }
#pragma warning restore CA2227
    }
}
