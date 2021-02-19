// <copyright file="TaskDTO.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Holds the details of a task entity.
    /// </summary>
    public class TaskDTO
    {
        /// <summary>
        /// Gets or sets task Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets task title.
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a task added by member.
        /// </summary>
        public bool IsAddedByMember { get; set; }

        /// <summary>
        /// Gets or sets start date of a task.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets end date of a task.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets project Id.
        /// </summary>
        public Guid ProjectId { get; set; }
    }
}