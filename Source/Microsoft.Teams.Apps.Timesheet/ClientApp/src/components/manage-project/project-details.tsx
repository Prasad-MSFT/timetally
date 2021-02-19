// <copyright file="project-details.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Text, Status } from '@fluentui/react-northstar';
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import Constants from "../../constants/constants";
import IProjectUtilization from "../../models/project-utilization";
import Donut from "react-donut";

import "react-circular-progressbar/dist/styles.css";
import "./manage-project.scss";

interface IProjectDetailsProps extends WithTranslation {
    projectDetail: IProjectUtilization;
    isMobile: boolean;
}

/**
 * Renders the project details and donut chart.
 * @param props The props with type IProjectDetailsProps.
 */
const ProjectDetails: React.FunctionComponent<IProjectDetailsProps> = props => {
    const localize: TFunction = props.t;
    const [isUtilized, setIsUtilized] = React.useState(false);

    React.useEffect(() => {
        setIsUtilized(props.projectDetail.underutilizedBillableHours === 0 && props.projectDetail.underutilizedNonBillableHours === 0);
    }, [props.projectDetail]);

    /**
     * Get project details.
     */
    const getProjectDetail = () => {
        let projectDetailCard =
            (<div >
                <Flex vAlign="center">
                    <Flex.Item >
                        <div className="donut-container">
                            <Donut
                                chartData={[
                                    { name: ` `, data: props.projectDetail.billableUtilizedHours },
                                    { name: ` `, data: props.projectDetail.underutilizedBillableHours },
                                    { name: ` `, data: props.projectDetail.nonBillableUtilizedHours },
                                    { name: ` `, data: props.projectDetail.underutilizedNonBillableHours },
                                ]}
                                chartThemeConfig={{
                                    series: {
                                        colors: [Constants.billableStatusColor, Constants.underUtilizedBillableStatusColor, Constants.nonBillableStatusColor, Constants.underutilizedNonUtilizedStatusColor],
                                    },
                                }}
                                showChartLabel={false}
                                chartRadiusRange={[`90%`, `100%`]}
                                chartWidth={250}
                                chartHeight={200}
                                title=""
                                legendAlignment={"top"}
                            />
                            <Flex vAlign="center" hAlign="center">
                                <Text size="medium" content={`${props.projectDetail.totalHours} ${localize("hoursCapitalLabel")}`} weight="semibold" />
                            </Flex>
                        </div>
                    </Flex.Item>
                    <Flex.Item >
                        <Flex gap="gap.small">
                            <Flex className="text-container" space="between" column>
                                <Flex vAlign="center" gap="gap.medium">
                                    <Status className="status-bullets" color={Constants.billableStatusColor} title={localize("billableUtilized")} />
                                    <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} size="medium" content={localize("billableUtilized")} />
                                </Flex>
                                <Flex vAlign="center" gap="gap.medium">
                                    <Status className="status-bullets" color={Constants.underUtilizedBillableStatusColor} title={localize("billableUnutilized")} />
                                    <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} content={localize("billableUnutilized")} />
                                </Flex>
                                <Flex vAlign="center" gap="gap.medium">
                                    <Status className="status-bullets" color={Constants.nonBillableStatusColor} title={localize("nonBillableUtilized")} />
                                    <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} content={localize("nonBillableUtilized")} />
                                </Flex>
                                <Flex vAlign="center" gap="gap.medium">
                                    <Status className="status-bullets" color={Constants.underutilizedNonUtilizedStatusColor} title={localize("nonBillableUnutilized")} />
                                    <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} content={localize("nonBillableUnutilized")} />
                                </Flex>
                            </Flex>
                            <Flex className="text-container" space="between" column>
                                <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} size="medium" content={localize("hours", { hourNumber: props.projectDetail.billableUtilizedHours })} />
                                <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} content={localize("hours", { hourNumber: props.projectDetail.underutilizedBillableHours })} />
                                <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} content={localize("hours", { hourNumber: props.projectDetail.nonBillableUtilizedHours })} />
                                <Text className={props.isMobile ? "status-label-mobile" : "status-label-web"} content={localize("hours", { hourNumber: props.projectDetail.underutilizedNonBillableHours })} />
                            </Flex>
                        </Flex>
                    </Flex.Item>
                </Flex>
                <Text size="small" content={isUtilized ? localize("projectUtilizedLabel") : localize("projectUnderutilizedLabel")} />
            </div>);

        return projectDetailCard;
    };
    return (
        <div className="project-details-container" >{getProjectDetail()}</div>
    );
};

export default withTranslation()(ProjectDetails);