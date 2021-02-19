// <copyright file="ProjectHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Timesheet.Extensions;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers.Task;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Repositories;

    /// <summary>
    /// Provides helper methods for managing projects.
    /// </summary>
    public class ProjectHelper : IProjectHelper
    {
        /// <summary>
        /// The instance of project model mapper.
        /// </summary>
        private readonly IProjectMapper projectMapper;

        /// <summary>
        /// The instance of timesheet database context.
        /// </summary>
        private readonly TimesheetContext context;

        /// <summary>
        /// The instance of repository accessors to access repositories.
        /// </summary>
        private readonly IRepositoryAccessors repositoryAccessors;

        /// <summary>
        /// Instance of member mapper.
        /// </summary>
        private readonly IMemberMapper memberMapper;

        /// <summary>
        /// Instance of task mapper.
        /// </summary>
        private readonly ITaskMapper taskMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectHelper"/> class.
        /// </summary>
        /// <param name="context">The timesheet database context.</param>
        /// <param name="repositoryAccessors">The instance of repository accessors.</param>
        /// <param name="projectMapper">The instance of project model mapper.</param>
        /// <param name="memberMapper">Instance of member mapper.</param>
        /// <param name="taskMapper">Instance of task mapper.</param>
        public ProjectHelper(
            TimesheetContext context,
            IRepositoryAccessors repositoryAccessors,
            IProjectMapper projectMapper,
            IMemberMapper memberMapper,
            ITaskMapper taskMapper)
        {
            this.context = context;
            this.repositoryAccessors = repositoryAccessors;
            this.projectMapper = projectMapper;
            this.memberMapper = memberMapper;
            this.taskMapper = taskMapper;
        }

        /// <summary>
        /// Creates a new project.
        /// </summary>
        /// <param name="projectDetails">The project details.</param>
        /// <param name="userObjectId">The user object Id of project creator.</param>
        /// <returns>Returns project details.</returns>
        public async Task<Project> CreateProjectAsync(ProjectDTO projectDetails, Guid userObjectId)
        {
            projectDetails = projectDetails ?? throw new ArgumentNullException(nameof(projectDetails), "Project details cannot be null.");

            var project = this.projectMapper.MapForCreateModel(projectDetails, userObjectId);

            using (var transaction = this.repositoryAccessors.Context.Database.BeginTransaction())
            {
                try
                {
                    project = this.repositoryAccessors.ProjectRepository.CreateProject(project);

                    if (await this.context.SaveChangesAsync() > 0)
                    {
                        transaction.Commit();
                        return project;
                    }
                }
#pragma warning disable CA1031 // Handled general exception
                catch
#pragma warning restore CA1031 // Handled general exception
                {
                    transaction.Rollback();
                }
            }

            return null;
        }

        /// <summary>
        /// Updates the details of a project.
        /// </summary>
        /// <param name="project">The project details that need to be updated.</param>
        /// <param name="userObjectId">The user object id who is going to update a project.</param>
        /// <returns>Return true if project is updated, else return false.</returns>
        public async Task<bool> UpdateProjectAsync(ProjectUpdateDTO project, Guid userObjectId)
        {
            project = project ?? throw new ArgumentException("The project details must be provided.");

            // Null check is not required, policy has already taken care of it.
            var projectDetails = this.repositoryAccessors.ProjectRepository.GetProjectById(project.Id, userObjectId);

            this.projectMapper.MapForUpdateModel(project, projectDetails);

            using (var transaction = this.repositoryAccessors.Context.Database.BeginTransaction())
            {
                try
                {
                    this.repositoryAccessors.ProjectRepository.Update(projectDetails);

                    if (await this.context.SaveChangesAsync() > 0)
                    {
                        transaction.Commit();
                        return true;
                    }
                }
#pragma warning disable CA1031 // Handled general exception
                catch
#pragma warning restore CA1031 // Handled general exception
                {
                    transaction.Rollback();
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a project by Id.
        /// </summary>
        /// <param name="projectId">The project Id of the project to fetch.</param>
        /// <param name="userObjectId">The user object Id of project creator.</param>
        /// <returns>Returns project details.</returns>
        public ProjectDTO GetProjectById(Guid projectId, Guid userObjectId)
        {
            var response = this.repositoryAccessors.ProjectRepository.GetProjectById(projectId, userObjectId);
            if (response == null)
            {
                return null;
            }

            return this.projectMapper.MapForViewModel(response);
        }

        /// <summary>
        /// Get project utilization details between date range.
        /// </summary>
        /// <param name="projectId">The project Id of which details to fetch.</param>
        /// <param name="managerId">The manger Id who created the project.</param>
        /// <param name="startDate">Start date of the date range.</param>
        /// <param name="endDate">End date of the date range.</param>
        /// <returns>Returns project utilization detail.</returns>
        public ProjectUtilizationDTO GetProjectUtilization(Guid projectId, string managerId, DateTime startDate, DateTime endDate)
        {
            var project = this.repositoryAccessors.ProjectRepository.GetProjectById(projectId, Guid.Parse(managerId));
            if (project == null)
            {
                return null;
            }

            var timesheets = this.repositoryAccessors.TimesheetRepository.GetTimesheetRequestsByProjectId(projectId, TimesheetStatus.Approved, startDate, endDate).ToList();
            var members = this.repositoryAccessors.MemberRepository.GetMembers(projectId);

            // Map project with approved timesheet hours.
            return this.projectMapper.MapForProjectUtilizationViewModel(project, timesheets, members);
        }

        /// <summary>
        /// Add users in project.
        /// </summary>
        /// <param name="projectId">The Id of the project in which members need to be added.</param>
        /// <param name="members">The list of members to be added.</param>
        /// <returns>Return true if project members are added, else return false.</returns>
        public async Task<bool> AddProjectMembersAsync(Guid projectId, IEnumerable<MemberDTO> members)
        {
            var memberIds = members.Select(member => member.UserId);

            var projectMembers = this.repositoryAccessors.MemberRepository.GetAllMembers(projectId);

            // Get members who are already part of the project.
            var membersToUpdate = projectMembers.Where(projectMember => memberIds.Contains(projectMember.UserId));
            using (var transaction = this.repositoryAccessors.Context.Database.BeginTransaction())
            {
                try
                {
                    if (!membersToUpdate.IsNullOrEmpty())
                    {
                        membersToUpdate = this.memberMapper.MapForExistingMembers(members.ToList(), membersToUpdate.ToList());
                        this.repositoryAccessors.MemberRepository.UpdateMembers(membersToUpdate.ToList());
                    }

                    var updatedMemberIds = membersToUpdate.Select(memberToUpdate => memberToUpdate.UserId);

                    // Filter out members which are newly added.
                    var newMembers = members.Where(member => !updatedMemberIds.Contains(member.UserId));

                    if (!newMembers.IsNullOrEmpty())
                    {
                        var membersToAdd = this.memberMapper.MapForCreateModel(projectId, newMembers);
                        await this.repositoryAccessors.MemberRepository.AddUsersAsync(membersToAdd);
                    }

                    var result = await this.repositoryAccessors.SaveChangesAsync();

                    if (result == members.Count())
                    {
                        transaction.Commit();
                        return true;
                    }
                }
#pragma warning disable CA1031 // Handled general exception
                catch
#pragma warning restore CA1031 // Handled general exception
                {
                    transaction.Rollback();
                }

                return false;
            }
        }

        /// <summary>
        /// Create tasks in project.
        /// </summary>
        /// <param name="projectId">The Id of the project in which tasks need to be created.</param>
        /// <param name="tasks">The list of tasks details to be created.</param>
        /// <returns>Returns true if tasks are added, else false.</returns>
        public async Task<bool> AddProjectTasksAsync(Guid projectId, IEnumerable<TaskDTO> tasks)
        {
            var tasksCount = tasks.Count();

            if (tasks.IsNullOrEmpty())
            {
                throw new ArgumentException("Task list is either null or empty.");
            }

            var tasksToAdd = this.taskMapper.MapForCreateModel(projectId, tasks);
            using (var transaction = this.repositoryAccessors.Context.Database.BeginTransaction())
            {
                try
                {
                    await this.repositoryAccessors.TaskRepository.CreateTasksAsync(tasksToAdd);
                    var responseCount = await this.repositoryAccessors.SaveChangesAsync();

                    if (responseCount == tasksCount)
                    {
                        transaction.Commit();
                        return true;
                    }
                }
#pragma warning disable CA1031 // Handled general exception
                catch
#pragma warning restore CA1031 // Handled general exception
                {
                    transaction.Rollback();
                }
            }

            return false;
        }

        /// <summary>
        /// Delete members from project.
        /// </summary>
        /// <param name="members">The list of members to be deleted.</param>
        /// <returns>Returns true if members are deleted, else false.</returns>
        public async Task<bool> DeleteProjectMembersAsync(List<Member> members)
        {
#pragma warning disable CA1062 // Null check is handled by controller.
            for (int i = 0; i < members.Count; i++)
#pragma warning restore CA1062 // Null check is handled by controller.
            {
                // We are not hard deleting members, hence change IsRemoved flag of members to true.
                members[i].IsRemoved = true;
            }

            using (var transaction = this.repositoryAccessors.Context.Database.BeginTransaction())
            {
                try
                {
                    // Update members.
                    this.repositoryAccessors.MemberRepository.UpdateMembers(members);

                    var result = await this.repositoryAccessors.SaveChangesAsync();

                    if (result == members.Count)
                    {
                        transaction.Commit();
                        return true;
                    }
                }
#pragma warning disable CA1031 // Handled general exception
                catch
#pragma warning restore CA1031 // Handled general exception
                {
                    transaction.Rollback();
                }
            }

            return false;
        }

        /// <summary>
        /// Delete tasks from a project.
        /// </summary>
        /// <param name="tasks">The list of tasks to be deleted.</param>
        /// <returns>Returns true if tasks are deleted, else false.</returns>
        public async Task<bool> DeleteProjectTasksAsync(List<TaskEntity> tasks)
        {
#pragma warning disable CA1062 // Null check is handled by controller.
            for (int i = 0; i < tasks.Count; i++)
#pragma warning restore CA1062 // Null check is handled by controller.
            {
                // We are not hard deleting members, hence change IsRemoved flag of tasks to true.
                tasks[i].IsRemoved = true;
            }

            using (var transaction = this.repositoryAccessors.Context.Database.BeginTransaction())
            {
                try
                {
                    // Update tasks.
                    this.repositoryAccessors.TaskRepository.UpdateTasks(tasks);

                    var result = await this.repositoryAccessors.SaveChangesAsync();

                    if (result == tasks.Count)
                    {
                        transaction.Commit();
                        return true;
                    }
                }
#pragma warning disable CA1031 // Handled general exception
                catch
#pragma warning restore CA1031 // Handled general exception
                {
                    transaction.Rollback();
                }
            }

            return false;
        }

        /// <summary>
        /// Get members overview for a project.
        /// Overview contains member information along with burned efforts.
        /// </summary>
        /// <param name="projectId">The project Id of which members to fetch.</param>
        /// <param name="startDate">Start date of the date range.</param>
        /// <param name="endDate">End date of the date range.</param>
        /// <returns>Returns list of project members overview.</returns>
        public IEnumerable<ProjectMemberOverviewDTO> GetProjectMembersOverview(Guid projectId, DateTime startDate, DateTime endDate)
        {
            var members = this.repositoryAccessors.MemberRepository.GetMembers(projectId);

            if (members.IsNullOrEmpty())
            {
                return Enumerable.Empty<ProjectMemberOverviewDTO>();
            }

            // Get approved timesheets filled by project members for given date range.
            var timesheets = this.repositoryAccessors.TimesheetRepository.GetTimesheetRequestsByProjectId(projectId, TimesheetStatus.Approved, startDate, endDate);

            return this.memberMapper.MapForProjectMembersViewModel(members, timesheets);
        }

        /// <summary>
        /// Get tasks overview for a project.
        /// Overview contains task information along with burned efforts.
        /// </summary>
        /// <param name="projectId">The project Id of which details to fetch.</param>
        /// <param name="startDate">Start date of the date range.</param>
        /// <param name="endDate">End date of the date range.</param>
        /// <returns>Returns list of project tasks overview.</returns>
        public IEnumerable<ProjectTaskOverviewDTO> GetProjectTasksOverview(Guid projectId, DateTime startDate, DateTime endDate)
        {
            var tasks = this.repositoryAccessors.TaskRepository.GetTasksByProjectId(projectId);

            if (tasks.IsNullOrEmpty())
            {
                return Enumerable.Empty<ProjectTaskOverviewDTO>();
            }

            // Get approved timesheets filled by project members for given date range.
            var timesheets = this.repositoryAccessors.TimesheetRepository.GetTimesheetRequestsByProjectId(projectId, TimesheetStatus.Approved, startDate, endDate);
            return this.taskMapper.MapForProjectTasksViewModel(tasks, timesheets);
        }

        /// <summary>
        /// Get members of a project.
        /// </summary>
        /// <param name="projectId">The project Id of which members to fetch.</param>
        /// <param name="memberIds">Ids of member.</param>
        /// <returns>Returns null if all members doesn't belongs to project, else return members.</returns>
        public async Task<IEnumerable<Member>> GetProjectMembersAsync(Guid projectId, IEnumerable<Guid> memberIds)
        {
            var membersModel = await this.repositoryAccessors.MemberRepository.FindAsync(member => memberIds.Contains(member.Id) && member.ProjectId == projectId);

            if (membersModel.Count() != memberIds.Count())
            {
                return null;
            }

            return membersModel;
        }

        /// <summary>
        /// Get tasks of a project.
        /// </summary>
        /// <param name="projectId">The project Id of which tasks to fetch.</param>
        /// <param name="taskIds">Ids of tasks.</param>
        /// <returns>Returns null if all tasks doesn't belongs to project, else return tasks.</returns>
        public async Task<IEnumerable<TaskEntity>> GetProjectTasksAsync(Guid projectId, IEnumerable<Guid> taskIds)
        {
            var tasksModel = await this.repositoryAccessors.TaskRepository.FindAsync(task => taskIds.Contains(task.Id) && task.ProjectId == projectId);

            if (tasksModel.Count() != taskIds.Count())
            {
                return null;
            }

            return tasksModel;
        }
    }
}