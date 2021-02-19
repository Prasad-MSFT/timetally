// <copyright file="project-member-overview.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

export default interface IProjectMemberOverview {
    id: string;
    userName: string;
    userId: string;
    projectId: string;
    isBillable: boolean
    totalHours: number;
    isSelected: boolean;
    isRemoved: boolean;
}