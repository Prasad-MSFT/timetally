// <copyright file="TaskRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Models = Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// This class manages all database operations related to timesheet entity.
    /// </summary>
    public class TaskRepository : BaseRepository<Models.TaskEntity>, ITaskRepository
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
        public ICollection<Models.TaskEntity> GetProjectTasks(Guid projectId, DateTime startDate, DateTime endDate)
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
        public ICollection<Models.TaskEntity> GetTasksByProjectId(Guid projectId)
        {
            return this.Context.Tasks
                .Where(task => task.ProjectId == projectId && !task.IsRemoved)
                .ToList();
        }

        /// <summary>
        /// Creates a new task entry in task entity.
        /// </summary>
        /// <param name="tasks">The tasks to save.</param>
        /// <returns>Returns whether operation is successful or not</returns>
        public async Task CreateTasksAsync(IEnumerable<Models.TaskEntity> tasks)
        {
            await this.Context.AddRangeAsync(tasks);
        }

        /// <summary>
        /// Updates task entries in task entity.
        /// </summary>
        /// <param name="tasks">The tasks to update.</param>
        public void UpdateTasks(IEnumerable<Models.TaskEntity> tasks)
        {
            this.Context.Tasks.UpdateRange(tasks);
        }

        /// <summary>
        /// Gets tasks.
        /// </summary>
        /// <param name="taskIds">The task Ids of which tasks to fetch.</param>
        /// <returns>Return list of tasks entity model.</returns>
        public List<Models.TaskEntity> GetTasksByIds(List<Guid> taskIds)
        {
            var tasks = this.Context.Tasks.
                Where(task => taskIds.Contains(task.Id)).ToList();
            return tasks;
        }

        /// <summary>
        /// Gets task details including member.
        /// </summary>
        /// <param name="taskId">The task Id to get.</param>
        /// <returns>Returns the task details.</returns>
        public Models.TaskEntity GetTask(Guid taskId)
        {
            return this.Context.Tasks
                .Where(task => task.Id == taskId)
                .Include(task => task.MemberMapping)
                .FirstOrDefault();
        }
    }
}