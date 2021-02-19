// <copyright file="dashboard-projects-wrapper.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Text } from "@fluentui/react-northstar";
import ProjectCard from "../../components/project-card/project-card";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { IDashboardProject } from "../../models/dashboard/dashboard-project";

interface IDashboardProjectsProps extends WithTranslation {
    onProjectCardClick: (projectId: string) => void;
    projects: IDashboardProject[];
    isMobileView: boolean;
}

/**
 * Renders the project cards for the user.
 * @param props The props of type IDashboardProjectsProps.
 */
const DashboardProjects: React.FunctionComponent<IDashboardProjectsProps> = props => {
    const localize: TFunction = props.t;

    /** 
     * Renders project card for each project.
     */
    const renderProjects = () => {
        if (!props.projects || props.projects.length === 0) {
            return <Text content={localize("noProjectsAvailable")} />;
        }

        let projects = props.projects.map((project: IDashboardProject, index: number) => {
            return <ProjectCard key={`project-parent-${index}`} projectCardKey={`project-${index}`} projectDetail={project} onClick={props.onProjectCardClick} />;
        });

        return <Flex vAlign="center" hAlign={props.isMobileView ? undefined : "center"} >{projects}</Flex>;
    };

    return renderProjects();
};

export default withTranslation()(DashboardProjects);