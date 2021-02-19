// <copyright file="IDashboardRequest.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { Guid } from "guid-typescript";

export interface IDashboardRequest {
    userId: Guid;
    userName: string;
    numberOfDays: number;
    totalHours: number;
    isSelected: boolean;
    status?: number;
    requestedForDates: Date[][];
    submittedTimesheetRequestIds: Guid[];
}