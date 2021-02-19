// <copyright file="resource-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";

// Gets the application settings.
export const getResources = (handleTokenAccessFailure: (error: string) => void) => {
    let requestUrl = `/api/settings`;

    return axios.get(requestUrl, handleTokenAccessFailure);
}