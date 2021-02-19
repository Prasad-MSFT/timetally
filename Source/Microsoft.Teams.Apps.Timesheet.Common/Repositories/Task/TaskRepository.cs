// <copyright file="TaskRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Models = Microsoft.Teams.Apps.Timesheet.Common.Models;

    /// <summary>
    /// This class manages all database operations related to timesheet entity.
    /// </summary>
    public class TaskRepository : BaseRepository<Models.Task>, ITaskRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskRepository"/> class.
        /// </summary>
        /// <param name="context">The timesheet context.</param>
        public TaskRepository(TimesheetContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Get tasks of a project.
        /// </summary>
        /// <param name="projectId">The project id of which tasks needs to be retrieved.</param>
        /// <param name="startDate">Start user id who created a project.</param>
        /// <param name="endDate">EndDate user id who created a project.</param>
        /// <returns>Returns the list of tasks.</returns>
        public ICollection<Models.Task> GetProjectTasks(Guid projectId, DateTime startDate, DateTime endDate)
        {
            return this.Context.Tasks
                .Where(task => task.Timesheets.Where(timesheet => timesheet.TimesheetDate >= startDate && timesheet.TimesheetDate <= endDate).ToList().Count > 0)
                .ToList();
        }

        /// <summary>
        /// Get tasks of a project.
        /// </summary>
        /// <param name="projectId">The project id of which tasks needs to be retrieved.</param>
        /// <returns>Returns the list of tasks.</returns>
        public ICollection<Models.Task> GetTasksByProjectId(Guid projectId)
        {
            return this.Context.Tasks
                .Where(task => task.ProjectId == projectId)
                .ToList();
        }

        /// <summary>
        /// Creates a new task entry in task entity.
        /// </summary>
        /// <param name="tasks">The tasks to save.</param>
        /// <returns>Returns whether operation is successful or not</returns>
        public async Task<bool> CreateTasksAsync(IEnumerable<Models.Task> tasks)
        {
            await this.Context.AddRangeAsync(tasks);
            return this.Context.SaveChanges() > 0;
        }

        /// <summary>
        /// Updates task entries in task entity.
        /// </summary>
        /// <param name="tasks">The tasks to update.</param>
        /// <returns>Returns whether operation is successful or not</returns>
        public bool UpdateTasks(IEnumerable<Models.Task> tasks)
        {
            this.Context.Tasks.UpdateRange(tasks);
            return this.Context.SaveChanges() > 0;
        }

        /// <summary>
        /// Gets tasks.
        /// </summary>
        /// <param name="taskIds">The task Ids of which tasks to fetch.</param>
        /// <returns>Return list of tasks entity model.</returns>
        public List<Models.Task> GetTasksByIds(List<Guid> taskIds)
        {
            var tasks = this.Context.Tasks.
                Where(task => taskIds.Contains(task.Id)).ToList();
            return tasks;
        }
    }
}