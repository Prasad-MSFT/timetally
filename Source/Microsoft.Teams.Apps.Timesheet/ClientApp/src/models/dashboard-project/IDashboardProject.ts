// <copyright file="IDashboardProject.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { Guid } from "guid-typescript";

export interface IDashboardProject {
    id: Guid;
    title: string;
    utilizedHours: number;
    totalHours: number;
}