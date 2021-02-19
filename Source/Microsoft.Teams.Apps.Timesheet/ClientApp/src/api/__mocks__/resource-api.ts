// <copyright file="resource-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import TestData from "../../api/test-data/test-data";
import { StatusCodes } from "http-status-codes";

// Gets the application settings.
export const getResources = (handleTokenAccessFailure: (error: string) => void) => {
    return Promise.resolve({
        data: TestData.getResources,
        status: StatusCodes.OK
    });
}