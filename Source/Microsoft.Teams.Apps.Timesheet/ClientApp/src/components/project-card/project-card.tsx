// <copyright file="project-card.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Text } from '@fluentui/react-northstar';
import { CircularProgressbar } from "react-circular-progressbar";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { IDashboardProject } from "../../models/dashboard/dashboard-project";

import "react-circular-progressbar/dist/styles.css";
import "./project-card.scss"

interface IProjectCardProps extends WithTranslation {
    projectDetail: IDashboardProject;
    onClick: (projectId: string) => void;
    projectCardKey: string;
}

/**
 * Renders the project card for manager.
 * @param props The props with type IProjectCardProps.
 */
const ProjectCard: React.FunctionComponent<IProjectCardProps> = props => {
    const localize: TFunction = props.t;
    const [isUtilized, setIsUtilized] = React.useState(false);

    React.useEffect(() => {
        let totalHours = props.projectDetail.totalHours;
        let hoursUtilized = props.projectDetail.utilizedHours;
        setIsUtilized(hoursUtilized === totalHours);
    }, [props.projectDetail]);

    /** 
     * Gets the percentage of project utilization 
     */
    const getPercentage = (projectDetail: IDashboardProject) => {
        if (projectDetail && props.projectDetail.totalHours !== 0) {
            return Math.ceil((props.projectDetail.utilizedHours / props.projectDetail.totalHours) * 100);
        }
        return 0;
    };

    /**
     * Gets project card.
     */
    const getProjectCard = () => {
        let projectCard =
            (
                <Flex key={props.projectCardKey} className="project-card-container" vAlign="center" hAlign="center" onClick={() => props.onClick(props.projectDetail.id.toString())}>
                    <Flex.Item >
                        <CircularProgressbar className="circular-progress" value={getPercentage(props.projectDetail)} text={`${getPercentage(props.projectDetail)}%`} />
                    </Flex.Item>
                    <Flex.Item push>
                        <Flex className="text-container" space="between" column fill>
                            <Text weight="semibold" className="project-title" title={props.projectDetail.title} content={props.projectDetail.title} truncated/>
                            <Text size="small" className="project-subtitle" content={`${props.projectDetail.utilizedHours}/${props.projectDetail.totalHours} ${localize("hoursUtilizeLabel")}`} />
                            <Text size="small" content={isUtilized ? localize("fullyUtilizedLabel") : localize("underutilizedLabel")} className={isUtilized ? "fully-utilized-text" : "underutilized-text"} />
                        </Flex>
                    </Flex.Item>
                </Flex>
            );

        return projectCard;
    };
    return (
        <div>{getProjectCard()}</div>
    );
};

export default withTranslation()(ProjectCard);