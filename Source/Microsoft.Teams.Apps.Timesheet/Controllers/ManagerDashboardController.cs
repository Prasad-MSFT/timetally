// <copyright file="ManagerDashboardController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Timesheet.Extensions;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Manager dashboard controller is responsible to expose API endpoints for performing CRUD operation related to dashboard.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ManagerDashboardController : BaseController
    {
        /// <summary>
        /// Logs errors and information.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The instance of manager dashboard helper which helps in managing operations on dashboard entity.
        /// </summary>
        private readonly IManagerDashboardHelper managerDashboardHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerDashboardController"/> class.
        /// </summary>
        /// <param name="logger">The ILogger object which logs errors and information.</param>
        /// <param name="managerDashboardHelper">The instance of manager dashboard helper.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        public ManagerDashboardController(
            ILogger<ManagerDashboardController> logger,
            IManagerDashboardHelper managerDashboardHelper,
            TelemetryClient telemetryClient)
            : base(telemetryClient)
        {
            this.managerDashboardHelper = managerDashboardHelper;
            this.logger = logger;
        }

        /// <summary>
        /// Gets timesheet requests which are pending for manager approval.
        /// </summary>
        /// <returns>List of submitted requests.</returns>
        [HttpGet]
        public async Task<IActionResult> GetDashboardRequestsAsync()
        {
            this.RecordEvent("Get dashboard requests- The HTTP call to GET dashboard requests has been initiated.", RequestType.Initiated);
            try
            {
                var dashboardTimesheetRequests = await this.managerDashboardHelper.GetDashboardRequestsAsync(Guid.Parse(this.UserAadId), TimesheetStatus.Submitted);

                if (!dashboardTimesheetRequests.IsNullOrEmpty())
                {
                    this.RecordEvent("Get dashboard requests- The HTTP call to GET dashboard requests has been succeeded.", RequestType.Succeeded);
                    return this.Ok(dashboardTimesheetRequests);
                }

                this.RecordEvent("Get dashboard requests- The HTTP call to GET dashboard requests has been failed.", RequestType.Failed);
                return this.NotFound("Timesheets not found.");
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get dashboard requests- The HTTP call to GET dashboard requests has been failed.", RequestType.Failed);
                this.logger.LogError(ex, "Error occurred while fetching dashboard requests.");
                throw;
            }
        }
    }
}