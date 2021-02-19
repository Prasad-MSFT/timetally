// <copyright file="ITaskRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models = Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Exposes methods which will be used to perform database operations on task entity.
    /// </summary>
    public interface ITaskRepository : IBaseRepository<Models.TaskEntity>
    {
        /// <summary>
        /// Get tasks of a project.
        /// </summary>
        /// <param name="projectId">The project id of which tasks needs to be retrieved.</param>
        /// <param name="startDate">Start user id who created a project.</param>
        /// <param name="endDate">EndDate user id who created a project.</param>
        /// <returns>Returns the list of tasks.</returns>
        ICollection<Models.TaskEntity> GetProjectTasks(Guid projectId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Creates a new task entry in task entity.
        /// </summary>
        /// <param name="tasks">The tasks to save.</param>
        /// <returns>Returns whether operation is successful or not</returns>
        Task CreateTasksAsync(IEnumerable<Models.TaskEntity> tasks);

        /// <summary>
        /// Updates task entries in task entity.
        /// </summary>
        /// <param name="tasks">The tasks to update.</param>
        void UpdateTasks(IEnumerable<Models.TaskEntity> tasks);

        /// <summary>
        /// Get tasks of a project.
        /// </summary>
        /// <param name="projectId">The project id of which tasks needs to be retrieved.</param>
        /// <returns>Returns the list of tasks.</returns>
        ICollection<Models.TaskEntity> GetTasksByProjectId(Guid projectId);

        /// <summary>
        /// Gets tasks.
        /// </summary>
        /// <param name="taskIds">The task Ids of which tasks to fetch.</param>
        /// <returns>Return list of tasks entity model.</returns>
        public List<Models.TaskEntity> GetTasksByIds(List<Guid> taskIds);

        /// <summary>
        /// Gets task details including member.
        /// </summary>
        /// <param name="taskId">The task Id to get.</param>
        /// <returns>Returns the task details.</returns>
        Models.TaskEntity GetTask(Guid taskId);
    }
}