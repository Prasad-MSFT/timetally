// <copyright file="ISubmittedRequest.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { Guid } from "guid-typescript";

export interface ISubmittedRequest {
    userId: Guid;
    timesheetDate: Date;
    totalHours: number;
    isSelected: boolean;
    status?: number;
    projectTitles: string[];
    submittedTimesheetIds: Guid[];
}