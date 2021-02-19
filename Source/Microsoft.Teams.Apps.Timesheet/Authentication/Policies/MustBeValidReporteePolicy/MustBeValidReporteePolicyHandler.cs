// <copyright file="MustBeValidReporteePolicyHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Teams.Apps.Timesheet.Extensions;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// This authorization handler is created to check whether all the timesheet requests reportees reports to logged-in user.
    /// The class implements AuthorizationHandler for handling MustBeValidReporteePolicyRequirement authorization.
    /// </summary>
    public class MustBeValidReporteePolicyHandler : IAuthorizationHandler
    {
        /// <summary>
        /// The instance of user helper to fetch reportees of user.
        /// </summary>
        private readonly IUserHelper userHelper;

        /// <summary>
        /// The instance of HTTP context accessors.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MustBeValidReporteePolicyHandler"/> class.
        /// </summary>
        /// <param name="userHelper">The instance of user helper to fetch reportees of user.</param>
        /// <param name="httpContextAccessor">The instance of HTTP context accessors.</param>
        public MustBeValidReporteePolicyHandler(IUserHelper userHelper, IHttpContextAccessor httpContextAccessor)
        {
            this.userHelper = userHelper;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));

            var oidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

            var claim = context.User.Claims.FirstOrDefault(p => oidClaimType.Equals(p.Type, StringComparison.OrdinalIgnoreCase));

            foreach (var requirement in context.Requirements)
            {
                if (requirement is MustBeValidReporteePolicyRequirement)
                {
                    var isValuePresent = this.httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("reporteeId", out object reporteeIdFromRoute);

                    if (isValuePresent)
                    {
                        if (await this.ValidateReporteeAsync(Guid.Parse(claim.Value), (string)reporteeIdFromRoute))
                        {
                            context.Succeed(requirement);
                        }
                    }
                    else
                    {
                        List<RequestApprovalDTO> requestApprovalDTOs = new List<RequestApprovalDTO>();

                        // Wrap the request stream so that we can rewind it back to the start for regular request processing.
                        this.httpContextAccessor.HttpContext.Request.EnableBuffering();

                        // Read the request body, parse out the team tag entity object to get requests list.
                        var streamReader = new StreamReader(
                            this.httpContextAccessor.HttpContext.Request.Body,
                            Encoding.UTF8,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true);

                        using (var jsonReader = new JsonTextReader(streamReader))
                        {
                            var obj = await JArray.LoadAsync(jsonReader);
                            requestApprovalDTOs = obj.ToObject<List<RequestApprovalDTO>>();
                            this.httpContextAccessor.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                        }

                        if (!requestApprovalDTOs.IsNullOrEmpty())
                        {
                            if (await this.ValidateReporteesAsync(Guid.Parse(claim.Value), requestApprovalDTOs))
                            {
                                context.Succeed(requirement);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if reportees in timesheet requests reports to logged-in user.
        /// </summary>
        /// <param name="userAadObjectId">The user's Azure Active Directory object id.</param>
        /// <param name="timesheetRequests">Timesheet requests for approval.</param>
        /// <returns>The flag indicates whether all the requests are valid for logged-in user.</returns>
        private async Task<bool> ValidateReporteesAsync(Guid userAadObjectId, List<RequestApprovalDTO> timesheetRequests)
        {
            // Get all reportees of logged-in user.
            var reportees = await this.userHelper.GetAllReporteesAsync(managerObjectId: userAadObjectId);
            var requestedReporteeIds = timesheetRequests.Select(timesheet => timesheet.UserId.ToString()).Distinct();
            var validReportees = reportees.Where(reportee => requestedReporteeIds.Contains(reportee.Id));

            // Check if filtered reportees count matches with provided timesheet request reportees count.
            return validReportees.Count() == requestedReporteeIds.Count();
        }

        /// <summary>
        /// Checks if reportee reports to logged-in user.
        /// </summary>
        /// <param name="userAadObjectId">The user's Azure Active Directory object id.</param>
        /// <param name="reporteeId">Reportee Id.</param>
        /// <returns>The flag indicates whether all the requests are valid for logged-in user.</returns>
        private async Task<bool> ValidateReporteeAsync(Guid userAadObjectId, string reporteeId)
        {
            // Get all reportees of logged-in user.
            var reportees = await this.userHelper.GetAllReporteesAsync(managerObjectId: userAadObjectId);
            return reportees.Where(reportee => reporteeId == reportee.Id).Any();
        }
    }
}
