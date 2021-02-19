// <copyright file="TaskHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Helpers.Task
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Repositories;
    using ProjectTask = Microsoft.Teams.Apps.Timesheet.Models.TaskEntity;

    /// <summary>
    /// Helper class which manages operations on project tasks.
    /// </summary>
    public class TaskHelper : ITaskHelper
    {
        /// <summary>
        /// The instance of repository accessors to access particular repository.
        /// </summary>
        private readonly IRepositoryAccessors repositoryAccessor;

        /// <summary>
        /// Logs errors and information.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskHelper"/> class.
        /// </summary>
        /// <param name="repositoryAccessor">The instance of repository accessors to access repositories.</param>
        /// <param name="logger">Logs errors and information.</param>
        public TaskHelper(IRepositoryAccessors repositoryAccessor, ILogger<TaskHelper> logger)
        {
            this.repositoryAccessor = repositoryAccessor;
            this.logger = logger;
        }

        /// <summary>
        /// Adds new member task.
        /// </summary>
        /// <param name="taskDetails">The task details to be added.</param>
        /// <param name="projectId">The project Id.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>Returns new task details if task created successfully. Else return null.</returns>
        public async Task<ResultResponse> AddMemberTaskAsync(ProjectTask taskDetails, Guid projectId, Guid userObjectId)
        {
            taskDetails = taskDetails ?? throw new ArgumentNullException(nameof(taskDetails), "The task details should not be null.");

            var projectDetails = await this.repositoryAccessor.ProjectRepository.GetAsync(projectId);

            if (projectDetails == null)
            {
                this.logger.LogInformation("Project details not found");
                return new ResultResponse
                {
                    ErrorMessage = "Invalid project",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                };
            }

            var projectMembers = this.repositoryAccessor.MemberRepository.GetMembers(projectId);

            if (!projectMembers.Any())
            {
                this.logger.LogInformation("Project does not contain any member");
                return new ResultResponse
                {
                    ErrorMessage = "Invalid project",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                };
            }

            var memberDetails = projectMembers.Find(member => member.UserId == userObjectId);

            if (memberDetails == null)
            {
                this.logger.LogInformation("User is not member of project");
                return new ResultResponse
                {
                    ErrorMessage = "User is not member of project",
                    StatusCode = System.Net.HttpStatusCode.Unauthorized,
                };
            }

            if (taskDetails.StartDate < projectDetails.StartDate.Date || taskDetails.EndDate > projectDetails.EndDate.Date)
            {
                this.logger.LogInformation("Task start and end date is not within project start and end date");
                return new ResultResponse
                {
                    ErrorMessage = "Invalid start and end date for task",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                };
            }

            taskDetails.MemberMappingId = memberDetails.Id;
            taskDetails.StartDate = taskDetails.StartDate.Date;
            taskDetails.EndDate = taskDetails.EndDate.Date;

            var createdTaskDetails = this.repositoryAccessor.TaskRepository.Add(taskDetails);
            if (await this.repositoryAccessor.SaveChangesAsync() > 0)
            {
                this.logger.LogInformation("Task added successfully");
                return new ResultResponse
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Response = createdTaskDetails,
                };
            }
            else
            {
                this.logger.LogInformation("Error occurred while adding new task");
                return new ResultResponse
                {
                    ErrorMessage = "Unable to create task",
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                };
            }
        }

        /// <summary>
        /// Deletes a task created by project member.
        /// </summary>
        /// <param name="taskId">The task Id to be deleted.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <param name="projectId">The project Id.</param>
        /// <returns>Returns true if task deleted successfully. Else return false.</returns>
        public async Task<ResultResponse> DeleteMemberTaskAsync(Guid taskId, Guid userObjectId, Guid projectId)
        {
            var taskDetails = this.repositoryAccessor.TaskRepository.GetTask(taskId);

            // Do not allow to delete task, if
            // 1. Task is not added by project member.
            // 2. Logged-in user is not the one who created a task.
            if (taskDetails == null || !taskDetails.IsAddedByMember || taskDetails.MemberMapping?.UserId != userObjectId || taskDetails.ProjectId != projectId)
            {
                this.logger.LogInformation("Task not found");
                return new ResultResponse
                {
                    ErrorMessage = "Task not found",
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                };
            }

            taskDetails.IsRemoved = true;

            this.repositoryAccessor.TaskRepository.Update(taskDetails);
            if (await this.repositoryAccessor.SaveChangesAsync() > 0)
            {
                return new ResultResponse
                {
                    StatusCode = System.Net.HttpStatusCode.NoContent,
                };
            }

            this.logger.LogInformation("Error occurred while deleting task");
            return new ResultResponse
            {
                ErrorMessage = "Error occurred while deleting task.",
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
            };
        }
    }
}
