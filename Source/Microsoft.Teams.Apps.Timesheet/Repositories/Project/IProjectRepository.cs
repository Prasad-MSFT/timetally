// <copyright file="IProjectRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Exposes methods which will be used to perform database operations on project entity.
    /// </summary>
    public interface IProjectRepository : IBaseRepository<Project>
    {
        /// <summary>
        /// Creates new project.
        /// </summary>
        /// <param name="projectDetails">The project details.</param>
        /// <returns>Returns boolean indication whether create project was successful.</returns>
        Project CreateProject(Project projectDetails);

        /// <summary>
        /// Get all active projects created by manager.
        /// </summary>
        /// <param name="userObjectId">The user Id who created a project.</param>
        /// <returns>Returns list of projects.</returns>
        IEnumerable<Project> GetActiveProjectsForManager(Guid userObjectId);

        /// <summary>
        /// Get all managers graph user Ids.
        /// </summary>
        /// <returns>Returns the project details along with tasks and members details.</returns>
        List<Guid> GetAllManagersUserIDs();

        /// <summary>
        /// Get project details by project id.
        /// </summary>
        /// <param name="projectId">The project id of which details need to be retrieved.</param>
        /// <returns>Returns the project details along with tasks and members details.</returns>
        public List<Project> GetProjectDetailByProjectIds(List<Guid> projectId);

        /// <summary>
        /// Gets all active projects along with tasks assigned to user between specified date range.
        /// </summary>
        /// <param name="calendarStartDate">The start date from which timesheets to get.</param>
        /// <param name="calendarEndDate">The end date up to which timesheets to get.</param>
        /// <param name="userObjectId">The user Id of which projects to get.</param>
        /// <returns>Returns all active projects assigned to user on particular date.</returns>
        Task<IEnumerable<Project>> GetProjectsAsync(DateTime calendarStartDate, DateTime calendarEndDate, Guid userObjectId);

        /// <summary>
        /// Get all active projects whose start date is greater than and end date is less than current date.
        /// </summary>
        /// <param name="managerUserObjectId">The manager user object Id who created a project.</param>
        /// <returns>Returns list of projects.</returns>
        IEnumerable<Project> GetActiveProjects(Guid managerUserObjectId);

        /// <summary>
        /// Get project details by project Id.
        /// </summary>
        /// <param name="projectId">The project Id of which details need to be retrieved.</param>
        /// <param name="userObjectId">The user object Id of manager who created a project.</param>
        /// <returns>Returns the project details along with tasks and members details.</returns>
        Project GetProjectById(Guid projectId, Guid userObjectId);

        /// <summary>
        /// Gets all active projects along with tasks assigned to user between specified date range.
        /// </summary>
        /// <param name="calendarStartDate">The start date from which timesheets to get.</param>
        /// <param name="calendarEndDate">The end date up to which timesheets to get.</param>
        /// <param name="reporteeObjectId">The user Id of which projects to get.</param>
        /// <param name="managerObjectId">Logged-In manager user object Id.</param>
        /// <returns>Returns all active projects assigned to user on particular date.</returns>
        IEnumerable<UserTimesheet> GetProjects(DateTime calendarStartDate, DateTime calendarEndDate, Guid reporteeObjectId, Guid managerObjectId);
    }
}