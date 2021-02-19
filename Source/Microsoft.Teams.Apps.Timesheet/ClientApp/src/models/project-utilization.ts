// <copyright file="project-utilization.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

export default interface IProjectUtilization {
    id: string;
    title: string;
    billableUtilizedHours: number;
    underutilizedBillableHours: number;
    nonBillableUtilizedHours: number;
    underutilizedNonBillableHours: number;
    totalHours: number;
    projectStartDate: Date;
    projectEndDate: Date;
}