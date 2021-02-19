// <copyright file="ITaskHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Helpers.Task
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using ProjectTask = Microsoft.Teams.Apps.Timesheet.Models.TaskEntity;

    /// <summary>
    /// Exposes helper methods required for managing tasks.
    /// </summary>
    public interface ITaskHelper
    {
        /// <summary>
        /// Adds new member task.
        /// </summary>
        /// <param name="taskDetails">The task details to be added.</param>
        /// <param name="projectId">The project Id.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>Returns new task details if task created successfully. Else return null.</returns>
        Task<ResultResponse> AddMemberTaskAsync(ProjectTask taskDetails, Guid projectId, Guid userObjectId);

        /// <summary>
        /// Deletes a task created by project member.
        /// </summary>
        /// <param name="taskId">The task Id to be deleted.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <param name="projectId">The project Id.</param>
        /// <returns>Returns true if task deleted successfully. Else return false.</returns>
        Task<ResultResponse> DeleteMemberTaskAsync(Guid taskId, Guid userObjectId, Guid projectId);
    }
}
