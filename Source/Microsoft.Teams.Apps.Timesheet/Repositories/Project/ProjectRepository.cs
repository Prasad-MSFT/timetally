// <copyright file="ProjectRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Teams.Apps.Timesheet.Extensions;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// This class manages all database operations related to project entity.
    /// </summary>
    public class ProjectRepository : BaseRepository<Project>, IProjectRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectRepository"/> class.
        /// </summary>
        /// <param name="context">The timesheet context.</param>
        public ProjectRepository(TimesheetContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Creates new project.
        /// </summary>
        /// <param name="projectDetails">The project details to save.</param>
        /// <returns>Returns boolean indication whether create project was successful.</returns>
        public Project CreateProject(Project projectDetails)
        {
            var createdProject = this.Add(projectDetails);
            return createdProject;
        }

        /// <summary>
        /// Get all active projects created by manager.
        /// Get all active projects.
        /// </summary>
        /// <param name="userObjectId">The user Id who created a project.</param>
        /// <returns>Returns list of projects.</returns>
        public IEnumerable<Project> GetActiveProjectsForManager(Guid userObjectId)
        {
            return this.Context.Projects
                .Where(project => project.CreatedBy.Equals(userObjectId) && DateTime.UtcNow.Date >= project.StartDate.Date && DateTime.UtcNow.Date <= project.EndDate.Date)
                .OrderBy(project => project.CreatedOn)
                .ToList();
        }

        /// <summary>
        /// Get all active projects whose start date is greater than and end date is less than current date.
        /// </summary>
        /// <param name="managerUserObjectId">The manager user object Id who created a project.</param>
        /// <returns>Returns list of projects.</returns>
        public IEnumerable<Project> GetActiveProjects(Guid managerUserObjectId)
        {
            return this.Context.Projects
                .Where(project => project.CreatedBy.Equals(managerUserObjectId) && DateTime.Now.Date >= project.StartDate.Date && DateTime.Now.Date <= project.EndDate.Date)
                .OrderBy(project => project.CreatedOn)
                .ToList();
        }

        /// <summary>
        /// Get all managers user object IDs.
        /// </summary>
        /// <returns>Returns the project details along with tasks and members details.</returns>
        public List<Guid> GetAllManagersUserIDs()
        {
            return this.Context.Projects.Select(project => project.CreatedBy).Distinct().ToList();
        }

        /// <inheritdoc/>
        public List<Project> GetProjectDetailByProjectIds(List<Guid> projectId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all active projects along with tasks assigned to user between specified date range.
        /// </summary>
        /// <param name="calendarStartDate">The start date from which timesheets to get.</param>
        /// <param name="calendarEndDate">The end date up to which timesheets to get.</param>
        /// <param name="userObjectId">The user Id of which projects to get.</param>
        /// <returns>Returns all active projects assigned to user on particular date.</returns>
        public async Task<IEnumerable<Project>> GetProjectsAsync(DateTime calendarStartDate, DateTime calendarEndDate, Guid userObjectId)
        {
            return await this.Context.Projects
                .Where(project => project.Members.Where(member => member.UserId == userObjectId).Any()
                    && ((project.StartDate.Date >= calendarStartDate && project.StartDate.Date <= calendarEndDate) || (project.StartDate.Date < calendarStartDate && project.EndDate.Date >= calendarStartDate)))
                .Include(project => project.Tasks)
                .Include(project => project.Members)
                .ToListAsync();
        }

        /// <summary>
        /// Get project details by project Id.
        /// </summary>
        /// <param name="projectId">The project Id of which details need to be retrieved.</param>
        /// <param name="userObjectId">The user object Id of manager who created a project.</param>
        /// <returns>Returns the project details along with tasks and members details.</returns>
        public Project GetProjectById(Guid projectId, Guid userObjectId)
        {
            return this.Context.Projects
                .Where(project => project.Id.Equals(projectId) && project.CreatedBy.Equals(userObjectId))
                .Include(project => project.Tasks.Where(task => task.IsRemoved == false))
                .Include(project => project.Members.Where(member => member.IsRemoved == false))
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets all active projects along with tasks assigned to user between specified date range.
        /// </summary>
        /// <param name="calendarStartDate">The start date from which timesheets to get.</param>
        /// <param name="calendarEndDate">The end date up to which timesheets to get.</param>
        /// <param name="reporteeObjectId">The reportee Id of which projects to get.</param>
        /// <param name="managerObjectId">Logged-In manager user object Id.</param>
        /// <returns>Returns all active projects assigned to user on particular date.</returns>
        public IEnumerable<UserTimesheet> GetProjects(DateTime calendarStartDate, DateTime calendarEndDate, Guid reporteeObjectId, Guid managerObjectId)
        {
            // Get projects between specified start and end date along with task details.
            var projects = this.Context.Projects
                .Where(project => ((project.StartDate.Date >= calendarStartDate.Date && project.StartDate.Date <= calendarEndDate.Date) ||
                    (project.StartDate.Date < calendarStartDate.Date && project.EndDate.Date >= calendarStartDate.Date)) &&
                    project.Members.Where(member => member.UserId == reporteeObjectId).Any() &&
                    project.CreatedBy == managerObjectId)
                .Include(project => project.Tasks);

            var projectIds = projects.Select(project => project.Id);

            // Get timesheets of a user which were filled within specified start and end date.
            var filledTimesheets = this.Context.Timesheets
                .Where(timesheet => timesheet.UserId.Equals(reporteeObjectId)
                && timesheet.TimesheetDate.Date >= calendarStartDate.Date
                && timesheet.TimesheetDate.Date <= calendarEndDate.Date
                && projectIds.Contains(timesheet.Task.ProjectId))
                .Include(timesheet => timesheet.Task);

            var timesheetDetails = new List<UserTimesheet>();
            UserTimesheet timesheetData = null;

            // Iterate on total number of days between specified start and end date to get timesheet data of each day.
            for (int i = 0; i <= calendarEndDate.Subtract(calendarStartDate).TotalDays; i++)
            {
                timesheetData = new UserTimesheet
                {
                    TimesheetDate = calendarStartDate.AddDays(i).Date,
                };

                // Retrieves projects of particular calendar date ranges in specified start and end date.
                var filteredProjects = projects.Where(project => timesheetData.TimesheetDate >= project.StartDate && timesheetData.TimesheetDate <= project.EndDate);

                if (filteredProjects.IsNullOrEmpty())
                {
                    continue;
                }

                timesheetData.ProjectDetails = new List<ProjectDetails>();

                // Iterate on each project to get task and timesheet details.
                foreach (var project in filteredProjects)
                {
                    timesheetData.ProjectDetails.Add(new ProjectDetails
                    {
                        Id = project.Id,
                        Title = project.Title,
                        TimesheetDetails = project.Tasks.Select(task => new TimesheetDetails
                        {
                            TaskId = task.Id,
                            TaskTitle = task.Title,
                            Hours = filledTimesheets.Where(timesheet => timesheet.TaskId == task.Id && timesheet.TimesheetDate.Date == timesheetData.TimesheetDate.Date).ToList().Select(x => x.Hours).FirstOrDefault(),
                            ManagerComments = filledTimesheets.Where(timesheet => timesheet.TaskId == task.Id && timesheet.TimesheetDate.Date == timesheetData.TimesheetDate.Date).Select(x => x.ManagerComments).FirstOrDefault(),
                            Status = filledTimesheets.Where(timesheet => timesheet.TaskId == task.Id && timesheet.TimesheetDate.Date == timesheetData.TimesheetDate.Date).Select(x => x.Status).FirstOrDefault(),
                        }).ToList(),
                    });
                }

                timesheetDetails.Add(timesheetData);
            }

            return timesheetDetails;
        }
    }
}