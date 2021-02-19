// <copyright file="MemberRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;

    /// <summary>
    /// This class manages all database operations related to user project mapping entity.
    /// </summary>
    public class MemberRepository : BaseRepository<Member>, IMemberRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberRepository"/> class.
        /// </summary>
        /// <param name="context">The timesheet context.</param>
        public MemberRepository(TimesheetContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Add users entries.
        /// </summary>
        /// <param name="users">The list of users entries to be added.</param>
        /// <returns>Returns whether the operation is successful or not.</returns>
        public async Task<bool> AddUsersAsync(IEnumerable<Member> users)
        {
            await this.Context.Members.AddRangeAsync(users);
            return this.Context.SaveChanges() > 0;
        }

        /// <summary>
        /// Gets members of project.
        /// </summary>
        /// <param name="projectId">The project Id of which members to fetch.</param>
        /// <returns>Return list of members entity model.</returns>
        public List<Member> GetMembers(Guid projectId)
        {
            var members = this.Context.Members.
                Where(member => member.ProjectId == projectId && member.IsRemoved == false).ToList();
            return members;
        }

        /// <summary>
        /// Gets all members of project.
        /// </summary>
        /// <param name="projectId">The project Id of which members to fetch.</param>
        /// <returns>Return list of members entity model.</returns>
        public List<Member> GetAllMembers(Guid projectId)
        {
            var members = this.Context.Members.
                Where(member => member.ProjectId == projectId).ToList();
            return members;
        }

        /// <summary>
        /// Updates the details of a project.
        /// </summary>
        /// <param name="members">The project details that need to be updated.</param>
        /// <returns>Returns true if project details updated successfully. Else returns false.</returns>
        public bool UpdateMembers(List<Member> members)
        {
            this.Context.Members.UpdateRange(members);
            return this.Context.SaveChanges() > 0;
        }

        /// <summary>
        /// Gets members of project.
        /// </summary>
        /// <param name="memberIds">The project Id of which members to fetch.</param>
        /// <returns>Return list of members entity model.</returns>
        public List<Member> GetMembersByMembersId(List<Guid> memberIds)
        {
            var members = this.Context.Members.
                Where(member => memberIds.Contains(member.Id)).ToList();
            return members;
        }
    }
}