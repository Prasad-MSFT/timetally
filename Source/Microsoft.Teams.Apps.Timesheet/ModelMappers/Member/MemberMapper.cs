// <copyright file="MemberMapper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.ModelMappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// A model class that contains methods related to member models mapping.
    /// </summary>
    public class MemberMapper : IMemberMapper
    {
        /// <summary>
        /// Gets members model to be inserted in database.
        /// </summary>
        /// <param name="projectId">The Id of the project in which members need to be added.</param>
        /// <param name="membersViewModel">Members entity view model.</param>
        /// <returns>Returns list of members model.</returns>
        public IEnumerable<Member> MapForCreateModel(Guid projectId, IEnumerable<MemberDTO> membersViewModel)
        {
            membersViewModel = membersViewModel ?? throw new ArgumentNullException(nameof(membersViewModel));

            var members = membersViewModel.Select(member => new Member
            {
                IsBillable = member.IsBillable,
                IsRemoved = false,
                ProjectId = projectId,
                UserId = member.UserId,
            });

            return members;
        }

        /// <summary>
        /// Gets members model to be updated in database.
        /// </summary>
        /// <param name="updatedMembers">Members which needs to be updated in database.</param>
        /// <param name="existingMembers">List of existing members.</param>
        /// <returns>Returns list of member entity model.</returns>
        public IEnumerable<Member> MapForExistingMembers(List<MemberDTO> updatedMembers, List<Member> existingMembers)
        {
            updatedMembers = updatedMembers ?? throw new ArgumentNullException(nameof(updatedMembers));
            existingMembers = existingMembers ?? throw new ArgumentNullException(nameof(existingMembers));

            for (var i = 0; i < existingMembers.Count; i++)
            {
                var member = updatedMembers.Find(updateMember => updateMember.UserId == existingMembers[i].UserId);
                if (member != null)
                {
                    existingMembers[i].IsBillable = member.IsBillable;
                    existingMembers[i].IsRemoved = false;
                }
            }

            return existingMembers;
        }

        /// <summary>
        /// Get members overview for a project.
        /// Overview contains member information along with burned efforts.
        /// </summary>
        /// <param name="members">List of members entity model.</param>
        /// <param name="timesheets">List of timesheet entity model.</param>
        /// <returns>Returns a list of project member overview view entity model.</returns>
        public IEnumerable<ProjectMemberOverviewDTO> MapForProjectMembersViewModel(IEnumerable<Member> members, IEnumerable<TimesheetEntity> timesheets)
        {
            members = members ?? throw new ArgumentNullException(nameof(members));
            timesheets = timesheets ?? throw new ArgumentNullException(nameof(timesheets));

            var projectMembersOverview = members.Select(member => new ProjectMemberOverviewDTO
            {
                Id = member.Id,
                IsBillable = member.IsBillable,
                TotalHours = timesheets.Where(timesheet => timesheet.UserId == member.UserId).Sum(timesheet => timesheet.Hours),
                UserId = member.UserId,
                UserName = string.Empty,
            });

            return projectMembersOverview;
        }
    }
}