// <copyright file="project-member.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

export default interface IProjectMember {
    // Unique project Id
    projectId: string;

    // User AAD object identifier.
    userId: string;

    // Boolean indicating whether member is billable.
    isBillable: boolean;
}