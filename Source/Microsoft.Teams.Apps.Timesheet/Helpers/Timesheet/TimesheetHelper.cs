// <copyright file="TimesheetHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Timesheet.Extensions;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Repositories;

    /// <summary>
    /// Provides helper methods for managing operations related to timesheet.
    /// </summary>
    public class TimesheetHelper : ITimesheetHelper
    {
        /// <summary>
        /// Holds instance of repository accessors to access repositories.
        /// </summary>
        private readonly IRepositoryAccessors repositoryAccessors;

        /// <summary>
        /// Holds the instance of timesheet mapper.
        /// </summary>
        private readonly ITimesheetMapper timesheetMapper;

        /// <summary>
        /// Logs errors and information.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// A set of key/value application configuration properties.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimesheetHelper"/> class.
        /// </summary>
        /// <param name="botOptions">A set of key/value application configuration properties.</param>
        /// <param name="repositoryAccessors">The instance of repository accessors.</param>
        /// <param name="timesheetMapper">The instance of timesheet mapper.</param>
        /// <param name="logger">The ILogger object which logs errors and information.</param>
        public TimesheetHelper(
            IOptions<BotSettings> botOptions,
            IRepositoryAccessors repositoryAccessors,
            ITimesheetMapper timesheetMapper,
            ILogger<TimesheetHelper> logger)
        {
            this.repositoryAccessors = repositoryAccessors;
            this.timesheetMapper = timesheetMapper;
            this.logger = logger;
            this.botOptions = botOptions;
        }

        /// <summary>
        /// Duplicates the efforts of source date timesheet to the target dates.
        /// </summary>
        /// <param name="sourceDate">The source date of which efforts needs to be duplicated.</param>
        /// <param name="targetDates">The target dates to which efforts needs to be duplicated.</param>
        /// <param name="clientLocalCurrentDate">The client's local current date.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>Return true if efforts duplicated successfully. Else returns false.</returns>
        public async Task<ResultResponse> DuplicateEffortsAsync(DateTime sourceDate, IEnumerable<DateTime> targetDates, DateTime clientLocalCurrentDate, Guid userObjectId)
        {
            var result = new ResultResponse { Response = Enumerable.Empty<TimesheetDTO>() };

            // Validates whether client provided local date is valid.
            if (!this.IsClientCurrentDateValid(clientLocalCurrentDate, DateTime.UtcNow))
            {
                this.logger.LogInformation("The timesheet can not be filled as provided current date is invalid.");
                result.StatusCode = HttpStatusCode.BadRequest;
                result.ErrorMessage = "The timesheet can not be filled as provided current date is invalid.";
                return result;
            }

            // Get target dates those aren't frozen.
            var notYetFrozenTimesheetDates = this.GetNotYetFrozenTimesheetDates(targetDates, clientLocalCurrentDate);

            // If all target dates are frozen.
            if (notYetFrozenTimesheetDates.IsNullOrEmpty())
            {
                this.logger.LogInformation("The timesheet can not be filled as the target dates are frozen.");
                result.StatusCode = HttpStatusCode.BadRequest;
                result.ErrorMessage = "The timesheet can not be filled as the target dates are frozen.";
                return result;
            }

            targetDates = notYetFrozenTimesheetDates;

            var timesheets = await this.GetTimesheetsAsync(sourceDate, sourceDate, userObjectId);
            var sourceDateTimesheet = timesheets.FirstOrDefault();

            if (sourceDateTimesheet == null || sourceDateTimesheet.ProjectDetails.IsNullOrEmpty())
            {
                this.logger.LogInformation("The source date must have projects.");
                result.StatusCode = HttpStatusCode.BadRequest;
                result.ErrorMessage = "The source date must have projects.";
                return result;
            }

            var sourceDateProjectIds = sourceDateTimesheet.ProjectDetails.Select(project => project.Id);

            // Get timesheet details for target dates with source date project Ids.
            var targetDatesTimesheets = this.repositoryAccessors.TimesheetRepository.GetTimesheetsOfUser(targetDates, userObjectId, sourceDateProjectIds);
            var sourceDateFilledEfforts = this.GetTotalEffortsByDate(new List<UserTimesheet> { sourceDateTimesheet });
            var duplicatedTimesheets = new List<TimesheetDTO>();

            foreach (var targetDate in targetDates)
            {
                if (this.WillWeeklyEffortsLimitExceed(targetDate, sourceDateFilledEfforts, userObjectId))
                {
                    continue;
                }

                foreach (var project in sourceDateTimesheet.ProjectDetails)
                {
                    // Do not copy efforts if:
                    // 1. Target date is less than project start date
                    // 2. OR target date is greater than project end date
                    // 3. OR if timesheet wasn't filled for project of source date
                    if (targetDate < project.StartDate
                        || targetDate > project.EndDate
                        || project.TimesheetDetails.IsNullOrEmpty())
                    {
                        continue;
                    }

                    foreach (var timesheet in project.TimesheetDetails)
                    {
                        // Get timesheet to update from timesheet entity.
                        var timesheetToUpdate = targetDatesTimesheets
                            .Where(timesheetDetails => timesheetDetails.TimesheetDate.Equals(targetDate)
                                && timesheetDetails.TaskId.Equals(timesheet.TaskId)
                                && timesheetDetails.Task.ProjectId.Equals(project.Id)).FirstOrDefault();

                        // If timesheet details exists (user filled timesheet), then update it with updated details.
                        if (timesheetToUpdate != null)
                        {
                            timesheetToUpdate.Hours = timesheet.Hours;
                            timesheetToUpdate.Status = (int)TimesheetStatus.Saved;

                            this.repositoryAccessors.TimesheetRepository.Update(timesheetToUpdate);
                            duplicatedTimesheets.Add(this.timesheetMapper.MapForViewModel(timesheetToUpdate));
                        }
                        else
                        {
                            // Create new timesheet in timesheet entity (as user didn't filled timesheet previously).
                            var newTimesheetDetails = new TimesheetEntity
                            {
                                TimesheetDate = targetDate,
                                TaskId = timesheet.TaskId,
                                TaskTitle = timesheet.TaskTitle,
                                Hours = timesheet.Hours,
                                Status = (int)TimesheetStatus.Saved,
                                UserId = userObjectId,
                            };

                            this.repositoryAccessors.TimesheetRepository.Add(newTimesheetDetails);
                            duplicatedTimesheets.Add(this.timesheetMapper.MapForViewModel(newTimesheetDetails));
                        }
                    }
                }
            }

            var isEffortsDuplicated = await this.repositoryAccessors.SaveChangesAsync() > 0;

            if (isEffortsDuplicated)
            {
                result.StatusCode = HttpStatusCode.OK;
                result.Response = duplicatedTimesheets;
            }
            else
            {
                this.logger.LogInformation("Failed to duplicate efforts.");
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.ErrorMessage = "Failed to duplicate efforts.";
            }

            return result;
        }

        /// <summary>
        /// Creates a new timesheet entry for a date if not exists or updates the existing one for provided dates
        /// with status as "Saved".
        /// </summary>
        /// <param name="userTimesheets">The timesheet details that need to be saved.</param>
        /// <param name="clientLocalCurrentDate">The client's local current date.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>Saved timesheet entries.</returns>
        public async Task<ResultResponse> SaveTimesheetsAsync(IEnumerable<UserTimesheet> userTimesheets, DateTime clientLocalCurrentDate, Guid userObjectId)
        {
            var result = new ResultResponse { Response = Enumerable.Empty<TimesheetDTO>() };

            if (!this.IsClientCurrentDateValid(clientLocalCurrentDate, DateTime.UtcNow.Date))
            {
                result.StatusCode = HttpStatusCode.BadRequest;
                result.ErrorMessage = "The provided current date is invalid.";
                return result;
            }

            var userTimesheetsToSave = userTimesheets.Where(timesheet =>
                timesheet.ProjectDetails != null && timesheet.ProjectDetails.Any(project => project.TimesheetDetails.Any()));

            // Filter timesheet dates those aren't frozen.
            var notYetFrozenTimesheetDates = this.GetNotYetFrozenTimesheetDates(userTimesheetsToSave.Select(x => x.TimesheetDate), DateTime.UtcNow.Date);

            userTimesheetsToSave = userTimesheetsToSave.Where(x => notYetFrozenTimesheetDates.Contains(x.TimesheetDate));

            if (userTimesheetsToSave.IsNullOrEmpty())
            {
                this.logger.LogInformation("The timesheet can not be filled for frozen timesheet dates.");
                result.StatusCode = HttpStatusCode.BadRequest;
                result.ErrorMessage = "The timesheet can not be filled for frozen timesheet dates.";
                return result;
            }

            // Get active projects between particular date span by calculating minimum and maximum date from 'userTimesheetsToSave'.
            var minimumTimesheetDateToSave = userTimesheetsToSave.Min(timesheet => timesheet.TimesheetDate);
            var maximumTimesheetDateToSave = userTimesheetsToSave.Max(timesheet => timesheet.TimesheetDate);

            var userProjects = await this.repositoryAccessors.ProjectRepository.GetProjectsAsync(minimumTimesheetDateToSave, maximumTimesheetDateToSave, userObjectId);

            // If logged-in user is not part of any project.
            if (userProjects.IsNullOrEmpty())
            {
                this.logger.LogInformation("There are no active projects assigned.");
                result.StatusCode = HttpStatusCode.BadRequest;
                result.ErrorMessage = "There are no active projects assigned.";
                return result;
            }

            var userProjectIds = userProjects.Select(userProject => userProject.Id);
            var savedTimesheets = new List<TimesheetDTO>();

            using (var transaction = this.repositoryAccessors.Context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var userTimesheet in userTimesheetsToSave)
                    {
                        var effortsToSave = this.GetTotalEffortsByDate(new List<UserTimesheet> { userTimesheet });

                        // If daily efforts limit exceeded.
                        if (effortsToSave > this.botOptions.Value.DailyEffortsLimit)
                        {
                            continue;
                        }

                        if (this.WillWeeklyEffortsLimitExceed(userTimesheet.TimesheetDate.Date, effortsToSave, userObjectId))
                        {
                            continue;
                        }

                        userTimesheet.ProjectDetails = userTimesheet.ProjectDetails.Where(project => project.TimesheetDetails.Any()).ToList();

                        foreach (var project in userTimesheet.ProjectDetails)
                        {
                            // If project is not assigned to logged-in user.
                            if (!userProjectIds.Contains(project.Id))
                            {
                                transaction.Rollback();

                                this.logger.LogInformation("Unable to save timesheets as some of the projects are not assigned.");
                                result.StatusCode = HttpStatusCode.BadRequest;
                                result.ErrorMessage = "Unable to save timesheets as some of the projects are not assigned.";
                                return result;
                            }

                            // Get task Ids for which timesheet needs to be saved.
                            var taskIdsToBeSaved = project.TimesheetDetails.Select(timesheet => timesheet.TaskId);

                            // Get timesheets which are already filled by user for task Ids in 'taskIdsToBeSaved'.
                            var timesheetsFilledByUser = this.repositoryAccessors.TimesheetRepository.GetTimesheets(userTimesheet.TimesheetDate, taskIdsToBeSaved, userObjectId);

                            // Get task Ids for which timesheet already filled.
                            var filledTimesheetsTaskIds = timesheetsFilledByUser.Select(timesheet => timesheet.TaskId);

                            // Filter out the timesheets for which timesheet is not filled.
                            var newTimesheets = project.TimesheetDetails
                                .Where(timesheet => !filledTimesheetsTaskIds.Contains(timesheet.TaskId) && timesheet.Hours > 0);

                            foreach (var newTimesheet in newTimesheets)
                            {
                                var newTimesheetDetails = this.timesheetMapper.MapForCreateModel(userTimesheet.TimesheetDate.Date, newTimesheet, userObjectId);
                                newTimesheetDetails.Status = (int)TimesheetStatus.Saved;

                                this.repositoryAccessors.TimesheetRepository.Add(newTimesheetDetails);
                                savedTimesheets.Add(this.timesheetMapper.MapForViewModel(newTimesheetDetails));
                            }

                            // Filter out the timesheets which needs to be updated in database.
                            var timesheetsToUpdate = project.TimesheetDetails
                                .Where(timesheet => filledTimesheetsTaskIds.Contains(timesheet.TaskId));

                            foreach (var timesheetToUpdate in timesheetsToUpdate)
                            {
                                var timesheetEntity = timesheetsFilledByUser.Where(timesheet => timesheet.TaskId == timesheetToUpdate.TaskId).First();
                                this.timesheetMapper.MapForUpdateModel(timesheetToUpdate, timesheetEntity);

                                // If hours updated to 0, then set status of task as unfilled.
                                if (timesheetToUpdate.Hours <= 0)
                                {
                                    timesheetEntity.Status = (int)TimesheetStatus.None;
                                }
                                else
                                {
                                    timesheetEntity.Status = (int)TimesheetStatus.Saved;
                                }

                                this.repositoryAccessors.TimesheetRepository.Update(timesheetEntity);
                                savedTimesheets.Add(this.timesheetMapper.MapForViewModel(timesheetEntity));
                            }
                        }
                    }

                    var isTimesheetsSaved = await this.repositoryAccessors.SaveChangesAsync() > 0;
                    transaction.Commit();

                    if (isTimesheetsSaved)
                    {
                        result.StatusCode = HttpStatusCode.OK;
                        result.Response = savedTimesheets;
                    }
                    else
                    {
                        this.logger.LogInformation("Failed to save timesheets.");
                        result.StatusCode = HttpStatusCode.InternalServerError;
                        result.ErrorMessage = "Failed to save timesheets.";
                    }
                }
#pragma warning disable CA1031 // Handled general exception
                catch
#pragma warning restore CA1031 // Handled general exception
                {
                    transaction.Rollback();

                    this.logger.LogInformation("Failed to save timesheets.");
                    result.StatusCode = HttpStatusCode.InternalServerError;
                    result.ErrorMessage = "Failed to save timesheets.";
                }
            }

            return result;
        }

        /// <summary>
        /// Saves the timesheets if provided and updates the status of all saved timesheets to submitted.
        /// </summary>
        /// <param name="clientLocalCurrentDate">The client's local current date.</param>
        /// <param name="userTimesheetsToSave">The timesheet details that need to be saved.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>Returns true if timesheets submitted successfully. Else returns false.</returns>
        public async Task<ResultResponse> SubmitTimesheetsAsync(DateTime clientLocalCurrentDate, IEnumerable<UserTimesheet> userTimesheetsToSave, Guid userObjectId)
        {
            // Save timesheet details.
            var result = await this.SaveTimesheetsAsync(userTimesheetsToSave, clientLocalCurrentDate, userObjectId);

            // If timesheet details weren't saved, then do not submit them.
            if (!userTimesheetsToSave.IsNullOrEmpty() && result.StatusCode != HttpStatusCode.OK)
            {
                return result;
            }

            // Get all saved timesheets of user.
            var savedTimesheets = await this.repositoryAccessors.TimesheetRepository.FindAsync(timesheet =>
                timesheet.Status == (int)TimesheetStatus.Saved && timesheet.UserId == userObjectId);

            if (!savedTimesheets.Any())
            {
                this.logger.LogInformation("Unable to submit timesheets as there are no saved timesheets found.");
                result.StatusCode = HttpStatusCode.BadRequest;
                result.ErrorMessage = "Unable to submit timesheets as there are no saved timesheets found.";
                return result;
            }

            // Filter timesheet dates those aren't frozen.
            var notFrozenTimesheetDates = this.GetNotYetFrozenTimesheetDates(savedTimesheets.Select(x => x.TimesheetDate), DateTime.UtcNow.Date);

            savedTimesheets = savedTimesheets.Where(x => notFrozenTimesheetDates.Contains(x.TimesheetDate));

            if (savedTimesheets.IsNullOrEmpty())
            {
                this.logger.LogInformation("The timesheet can not be filled for frozen timesheet dates.");
                result.StatusCode = HttpStatusCode.BadRequest;
                result.ErrorMessage = "The timesheet can not be filled for frozen timesheet dates.";
                return result;
            }

            var submittedTimesheets = new List<TimesheetDTO>();

            using (var transaction = this.repositoryAccessors.Context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var timesheet in savedTimesheets)
                    {
                        timesheet.Status = (int)TimesheetStatus.Submitted;
                        submittedTimesheets.Add(this.timesheetMapper.MapForViewModel(timesheet));
                    }

                    this.repositoryAccessors.TimesheetRepository.Update(savedTimesheets);

                    var isTimesheetsSubmitted = await this.repositoryAccessors.SaveChangesAsync() > 0;
                    transaction.Commit();

                    if (isTimesheetsSubmitted)
                    {
                        result.StatusCode = HttpStatusCode.OK;
                        result.Response = submittedTimesheets;
                    }
                    else
                    {
                        this.logger.LogInformation("Failed to submit timesheets.");
                        result.StatusCode = HttpStatusCode.InternalServerError;
                        result.ErrorMessage = "Failed to submit timesheets.";
                    }
                }
#pragma warning disable CA1031 // Handled general exception
                catch
#pragma warning restore CA1031 // Handled general exception
                {
                    transaction.Rollback();

                    this.logger.LogInformation("Failed to submit timesheets.");
                    result.StatusCode = HttpStatusCode.InternalServerError;
                    result.ErrorMessage = "Failed to submit timesheets.";
                }
            }

            return result;
        }

        /// <summary>
        /// Gets timesheets of user between specified date range.
        /// </summary>
        /// <param name="calendarStartDate">The start date from which timesheets to get.</param>
        /// <param name="calendarEndDate">The end date up to which timesheets to get.</param>
        /// <param name="userObjectId">The user Id of which timesheets to get.</param>
        /// <returns>Returns timesheets of user on particular date range.</returns>
        public async Task<IEnumerable<UserTimesheet>> GetTimesheetsAsync(DateTime calendarStartDate, DateTime calendarEndDate, Guid userObjectId)
        {
            calendarStartDate = calendarStartDate.Date;
            calendarEndDate = calendarEndDate.Date;

            var projects = await this.repositoryAccessors.ProjectRepository.GetProjectsAsync(calendarStartDate, calendarEndDate, userObjectId);
            var filledTimesheets = await this.repositoryAccessors.TimesheetRepository.GetTimesheetsAsync(calendarStartDate, calendarEndDate, userObjectId);

            var timesheetDetails = new List<UserTimesheet>();
            UserTimesheet timesheetData = null;

            double totalDays = calendarEndDate.Subtract(calendarStartDate).TotalDays;

            // Iterate on total number of days between specified start and end date to get timesheet data of each day.
            for (int i = 0; i <= totalDays; i++)
            {
                timesheetData = new UserTimesheet
                {
                    TimesheetDate = calendarStartDate.AddDays(i),
                };

                // Retrieves projects of particular calendar date ranges in specified start and end date.
                var filteredProjects = projects.Where(project => timesheetData.TimesheetDate >= project.StartDate.Date
                    && timesheetData.TimesheetDate <= project.EndDate.Date);

                if (filteredProjects.IsNullOrEmpty())
                {
                    continue;
                }

                timesheetData.ProjectDetails = new List<ProjectDetails>();

                // Iterate on each project to get task and timesheet details.
                foreach (var project in filteredProjects)
                {
                    var memberDetails = project.Members.Where(member => member.UserId == userObjectId).FirstOrDefault();

                    if (memberDetails == null)
                    {
                        timesheetDetails.Add(timesheetData);
                        continue;
                    }

                    // Filter out valid tasks.
                    var filteredTasks = project.Tasks.Where(task => !task.IsRemoved
                        && (!task.IsAddedByMember || (task.MemberMapping != null && task.MemberMappingId == memberDetails.Id))
                        && (timesheetData.TimesheetDate >= task.StartDate && timesheetData.TimesheetDate <= task.EndDate))
                        .ToList();

                    timesheetData.ProjectDetails.Add(new ProjectDetails
                    {
                        Id = project.Id,
                        Title = project.Title,
                        StartDate = project.StartDate,
                        EndDate = project.EndDate,
                        TimesheetDetails = filteredTasks.Select(task =>
                        {
                            var timesheetFilledForTask = filledTimesheets.Where(timesheet => timesheet.TaskId == task.Id
                                && timesheet.TimesheetDate == timesheetData.TimesheetDate).FirstOrDefault();

                            return new TimesheetDetails
                            {
                                TaskId = task.Id,
                                TaskTitle = task.Title,
                                IsAddedByMember = task.IsAddedByMember,
                                StartDate = task.StartDate.Date,
                                EndDate = task.EndDate.Date,
                                Hours = timesheetFilledForTask == null ? 0 : timesheetFilledForTask.Hours,
                                ManagerComments = timesheetFilledForTask == null ? string.Empty : timesheetFilledForTask.ManagerComments,
                                Status = timesheetFilledForTask == null ? (int)TimesheetStatus.None : timesheetFilledForTask.Status,
                            };
                        }).ToList(),
                    });
                }

                timesheetDetails.Add(timesheetData);
            }

            return timesheetDetails;
        }

        /// <summary>
        /// Gets timesheet dates those aren't frozen.
        /// </summary>
        /// <param name="timesheetDates">The timesheet dates that need to be filtered.</param>
        /// <param name="clientLocalCurrentDate">The client's local current date.</param>
        /// <returns>Returns true if a timesheet date is frozen. Else return false.</returns>
        public IEnumerable<DateTime> GetNotYetFrozenTimesheetDates(IEnumerable<DateTime> timesheetDates, DateTimeOffset clientLocalCurrentDate)
        {
            var clientCurrentMonthEndDate = new DateTime(clientLocalCurrentDate.Year, clientLocalCurrentDate.Month, DateTime.DaysInMonth(clientLocalCurrentDate.Year, clientLocalCurrentDate.Month));
            var timesheetFreezeDayOfMonth = this.botOptions.Value.TimesheetFreezeDayOfMonth;
            var daysInClientLocalCurrentMonth = DateTime.DaysInMonth(clientLocalCurrentDate.Year, clientLocalCurrentDate.Month);

            // If specified timesheet freeze day of month is greater than total days in current month, then reset
            // timesheet freeze day to last day of client current month.
            if (timesheetFreezeDayOfMonth > daysInClientLocalCurrentMonth)
            {
                timesheetFreezeDayOfMonth = daysInClientLocalCurrentMonth;
            }

            // Logic to not save or submit timesheet dates after freezing day of month.
            if (clientLocalCurrentDate.Day >= timesheetFreezeDayOfMonth)
            {
                var startDateOfCurrentMonth = new DateTime(clientLocalCurrentDate.Year, clientLocalCurrentDate.Month, 01);

                // Get timesheet details for calendar dates those are belongs to current month.
                return timesheetDates
                    .Where(timesheetDate => timesheetDate.Date >= startDateOfCurrentMonth.Date && timesheetDate.Date <= clientCurrentMonthEndDate.Date);
            }
            else
            {
                // Get timesheet details for calendar dates belongs to previous month and current month.
                var previousMonthStartDate = new DateTime(clientLocalCurrentDate.Year, clientLocalCurrentDate.Month, 01).Date.AddMonths(-1);

                return timesheetDates
                    .Where(timesheetDate => timesheetDate.Date >= previousMonthStartDate.Date && timesheetDate.Date <= clientCurrentMonthEndDate.Date);
            }
        }

        /// <summary>
        /// Checks whether client current date is valid.
        /// </summary>
        /// <param name="clientCurrentDate">The client's local current date.</param>
        /// <param name="utcDate">The current UTC date.</param>
        /// <returns>Returns true if the current date is valid. Else returns false.</returns>
        public bool IsClientCurrentDateValid(DateTime clientCurrentDate, DateTime utcDate)
        {
            var utcDateWithMinimumOffset = utcDate.AddHours(-12);
            var utcDateWithMaximumOffset = utcDate.AddHours(14);

            return clientCurrentDate.Date >= utcDateWithMinimumOffset.Date && clientCurrentDate.Date <= utcDateWithMaximumOffset.Date;
        }

        /// <summary>
        /// To approve or reject the timesheet requests.
        /// </summary>
        /// <param name="timesheets">Timesheet requests to be approved or rejected.</param>
        /// <param name="requestApprovals">The details of timesheet requests which are approved or reject by the manager.</param>
        /// <param name="status">If true, the requests get approved. Else requests get rejected.</param>
        /// <returns>Returns true if requests approved or rejected successfully. Else returns false.</returns>
        public async Task<bool> ApproveOrRejectTimesheetRequestsAsync(IEnumerable<TimesheetEntity> timesheets, IEnumerable<RequestApprovalDTO> requestApprovals, TimesheetStatus status)
        {
            var timesheetsCount = timesheets.Count();
#pragma warning disable CA1062 // Null check is handled by controller.
            foreach (var timesheetRequest in timesheets)
#pragma warning restore CA1062 // Null check is handled by controller.
            {
                var approvalDetails = requestApprovals.Where(requestApproval => requestApproval.TimesheetId == timesheetRequest.Id).First();
                timesheetRequest.Status = (int)status;
                timesheetRequest.ManagerComments = status == TimesheetStatus.Rejected ? approvalDetails.ManagerComments : string.Empty;
            }

            using (var transaction = this.repositoryAccessors.Context.Database.BeginTransaction())
            {
                try
                {
                    this.repositoryAccessors.TimesheetRepository.Update(timesheets);
                    var response = await this.repositoryAccessors.SaveChangesAsync();
                    if (response == timesheetsCount)
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
        /// Gets the active timesheet requests.
        /// </summary>
        /// <param name="reporteeObjectId">The user Id of which requests to get.</param>
        /// <param name="status">Timesheet status for filtering.</param>
        /// <returns>Returns the list of timesheet requests.</returns>
        public IEnumerable<SubmittedRequestDTO> GetTimesheetRequestsByStatusAsync(string reporteeObjectId, TimesheetStatus status)
        {
            var userObjectIds = new List<Guid>
            {
                Guid.Parse(reporteeObjectId),
            };

            var timesheetRequests = this.repositoryAccessors.TimesheetRepository.GetTimesheetRequestsOfUsersByStatus(userObjectIds, status);

            // Map timesheet to pending request view model.
            return this.timesheetMapper.MapToViewModel(timesheetRequests.Values.First()).OrderBy(timesheet => timesheet.TimesheetDate);
        }

        /// <summary>
        /// Gets submitted timesheet requests by Ids.
        /// </summary>
        /// <param name="managerObjectId">Manager object Id who has created the project.</param>
        /// <param name="timesheetRequestIds">Ids of timesheet to fetch.</param>
        /// <returns>Return timesheet if all timesheet found, else return null.</returns>
        public IEnumerable<TimesheetEntity> GetSubmittedTimesheetsByIds(Guid managerObjectId, IEnumerable<Guid> timesheetRequestIds)
        {
            // Check if timesheet request has status 'Submitted' and saved against project which has been created by logged in manager.
            var validTimesheetRequests = this.repositoryAccessors.TimesheetRepository
                    .GetSubmittedTimesheetByIds(managerObjectId, timesheetRequestIds);
            var validTimesheetRequestIds = validTimesheetRequests.Select(validTimesheetRequest => validTimesheetRequest.Id);

            // Check if all user provided Ids matches with database timesheets.
            if (timesheetRequestIds.All(timesheet => validTimesheetRequestIds.Contains(timesheet)))
            {
                return validTimesheetRequests;
            }

            return null;
        }

        /// <summary>
        /// Indicates whether weekly efforts limit will get exceeded.
        /// </summary>
        /// <param name="timesheetDate">The timesheet date of which efforts to be saved.</param>
        /// <param name="effortsToSave">The efforts to be saved.</param>
        /// <param name="userObjectId">The logged-in user object Id.</param>
        /// <returns>Returns true if weekly limit get exceed. Else returns false.</returns>
        private bool WillWeeklyEffortsLimitExceed(DateTime timesheetDate, int effortsToSave, Guid userObjectId)
        {
            var startOfWeek = timesheetDate.Date.AddDays(-(int)timesheetDate.Date.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);

            var timesheetsOfWeek = this.repositoryAccessors.TimesheetRepository.GetTimesheetsOfUser(startOfWeek, endOfWeek, userObjectId);

            if (!timesheetsOfWeek.Any())
            {
                return effortsToSave > this.botOptions.Value.WeeklyEffortsLimit;
            }

            // Filter out timesheets other than current timesheet date received.
            timesheetsOfWeek = timesheetsOfWeek.Where(timesheet => timesheet.TimesheetDate != timesheetDate.Date);

            var filledEffortsForWeek = timesheetsOfWeek.Sum(timesheet => timesheet.Hours);

            return (filledEffortsForWeek + effortsToSave) > this.botOptions.Value.WeeklyEffortsLimit;
        }

        /// <summary>
        /// Gets the total efforts by date.
        /// </summary>
        /// <param name="userTimesheets">The timesheet details.</param>
        /// <returns>The total efforts.</returns>
        private int GetTotalEffortsByDate(IEnumerable<UserTimesheet> userTimesheets)
        {
            var totalEfforts = 0;
            userTimesheets = userTimesheets.Where(x => x.ProjectDetails != null && x.ProjectDetails.Any());

            foreach (var userTimesheet in userTimesheets)
            {
                for (int i = 0; i < userTimesheet.ProjectDetails.Count; i++)
                {
                    if (userTimesheet.ProjectDetails[i].TimesheetDetails.IsNullOrEmpty())
                    {
                        continue;
                    }

                    totalEfforts += userTimesheet.ProjectDetails[i].TimesheetDetails.Sum(x => x.Hours);
                }
            }

            return totalEfforts;
        }
    }
}
