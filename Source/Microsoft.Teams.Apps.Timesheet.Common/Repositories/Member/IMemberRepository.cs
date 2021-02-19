// <copyright file="IMemberRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;

    /// <summary>
    /// Exposes methods that will be used to manage operations on conversation entity.
    /// </summary>
    public interface IMemberRepository : IBaseRepository<Member>
    {
        /// <summary>
        /// Add users entries.
        /// </summary>
        /// <param name="users">The list of users entries to be added.</param>
        /// <returns>Returns whether the operation is successful or not</returns>
        public Task<bool> AddUsersAsync(IEnumerable<Member> users);

        /// <summary>
        /// Gets members of project.
        /// </summary>
        /// <param name="projectId">The project Id of which members to fetch.</param>
        /// <returns>Return list of members entity model.</returns>
        public List<Member> GetMembers(Guid projectId);

        /// <summary>
        /// Gets all members of project.
        /// </summary>
        /// <param name="projectId">The project Id of which members to fetch.</param>
        /// <returns>Return list of members entity model.</returns>
        public List<Member> GetAllMembers(Guid projectId);

        /// <summary>
        /// Gets members of project.
        /// </summary>
        /// <param name="memberIds">The project Id of which members to fetch.</param>
        /// <returns>Return list of members entity model.</returns>
        public List<Member> GetMembersByMembersId(List<Guid> memberIds);

        /// <summary>
        /// Updates the details of a members.
        /// </summary>
        /// <param name="members">The members details that need to be updated.</param>
        /// <returns>Returns true if members detail updated successfully. Else returns false.</returns>
        public bool UpdateMembers(List<Member> members);
    }
}
