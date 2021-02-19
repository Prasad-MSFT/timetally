// <copyright file="ManagerDashboardHelper.cs" company="Microsoft">
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
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph;

    /// <summary>
    /// Provides helper methods for managing operations related to managers dashboard.
    /// </summary>
    public class ManagerDashboardHelper : IManagerDashboardHelper
    {
        /// <summary>
        /// The instance of repository accessors to access repositories.
        /// </summary>
        private readonly IRepositoryAccessors repositoryAccessors;

        /// <summary>
        /// The instance of user Graph service to access logged-in user's reportees and manager.
        /// </summary>
        private readonly IUsersService userGraphService;

        /// <summary>
        /// The instance of manager dashboard mapper.
        /// </summary>
        private readonly IManagerDashboardMapper managerDashboardMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerDashboardHelper"/> class.
        /// </summary>
        /// <param name="repositoryAccessors">The instance of repository accessors.</param>
        /// <param name="userGraphService">The instance of user Graph service to access logged-in user's reportees and manager.</param>
        /// <param name="managerDashboardMapper">The instance of manager dashboard mapper.</param>
        public ManagerDashboardHelper(IRepositoryAccessors repositoryAccessors, IUsersService userGraphService, IManagerDashboardMapper managerDashboardMapper)
        {
            this.repositoryAccessors = repositoryAccessors;
            this.userGraphService = userGraphService;
            this.managerDashboardMapper = managerDashboardMapper;
        }

        /// <summary>
        /// Get approved and active project details for dashboard between date range.
        /// </summary>
        /// <param name="managerUserObjectId">The manager user object Id who created a project.</param>
        /// <param name="startDate">Start date of the date range.</param>
        /// <param name="endDate">End date of the date range.</param>
        /// <returns>Returns list of dashboard projects.</returns>
        public IEnumerable<DashboardProjectDTO> GetDashboardProjects(Guid managerUserObjectId, DateTime startDate, DateTime endDate)
        {
            var projects = this.repositoryAccessors.ProjectRepository.GetActiveProjects(managerUserObjectId);

            if (projects.IsNullOrEmpty())
            {
                return null;
            }

            var dashboardProjects = new List<DashboardProjectDTO>();
            var projectIds = projects.Select(project => project.Id);
            var timesheets = this.repositoryAccessors.TimesheetRepository.GetTimesheetRequestsByProjectIds(projectIds, TimesheetStatus.Approved, startDate, endDate);

            foreach (var project in projects)
            {
                var projectTimesheets = timesheets.Where(timesheet => timesheet.Task.ProjectId == project.Id);
                dashboardProjects.Add(this.managerDashboardMapper.MapForDashboardProjectViewModel(project, projectTimesheets));
            }

            return dashboardProjects;
        }

        /// <summary>
        /// Gets timesheet requests which are pending for manager approval.
        /// </summary>
        /// <param name="managerObjectId">The manager Id for which request has been raised.</param>
        /// <param name="timesheetStatus">The status of requests to fetch.</param>
        /// <returns>Return list of submitted timesheet request.</returns>
        public async Task<IEnumerable<DashboardRequestDTO>> GetDashboardRequestsAsync(Guid managerObjectId, TimesheetStatus timesheetStatus)
        {
            // Get timesheet requests pending with manager.
            var response = this.repositoryAccessors.TimesheetRepository.GetTimesheetRequestsByManager(managerObjectId, timesheetStatus);

            if (!response.Any())
            {
                return null;
            }

            // Map timesheet entity to dashboard requests view model.
            var dashboardRequests = this.managerDashboardMapper.MapForViewModel(response.Values).ToList();

            var userIds = dashboardRequests.Select(dashboardRequest => dashboardRequest.UserId.ToString());
            var users = await this.userGraphService.GetUsersAsync(userIds);

            // Mapping users with their graph user display name.
            for (var i = 0; i < dashboardRequests.Count; i++)
            {
                var isUserFound = !users.Where(user => Guid.Parse(user.Id) == dashboardRequests[i].UserId).IsNullOrEmpty();
                if (isUserFound)
                {
                    dashboardRequests[i].UserName = users.Where(user => Guid.Parse(user.Id) == dashboardRequests[i].UserId).FirstOrDefault().DisplayName;
                }
            }

            return dashboardRequests;
        }
    }
}