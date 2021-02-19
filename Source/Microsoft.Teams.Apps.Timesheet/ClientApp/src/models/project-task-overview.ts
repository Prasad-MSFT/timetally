// <copyright file="project-task-overview.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

export default interface IProjectTaskOverview {
    id: string;
    title: string;
    projectId: string;
    totalHours: number;
    isSelected: boolean;
    isRemoved: boolean;
    startDate: Date;
    endDate: Date;

}