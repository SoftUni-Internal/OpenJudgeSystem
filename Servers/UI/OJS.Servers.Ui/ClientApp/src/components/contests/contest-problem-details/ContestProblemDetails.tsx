import React from 'react';

import concatClassNames from '../../../utils/class-names';
import Heading, { HeadingType } from '../../guidelines/headings/Heading';
import ProblemResources from '../../problems/problem-resources/ProblemResources';
import ProblemSubmissions from '../../problems/problem-submissions/ProblemSubmissions';

import styles from './ContestProblemDetails.module.scss';

const ContestProblemDetails = () => {
    const contestTabControlsClass = 'contestTabControls';
    const problemDetailsContainer = concatClassNames(styles.problemDetailsContainer, contestTabControlsClass);
    // const contestTabsClassName = 'contestTabs';

    return (
        <div className={problemDetailsContainer}>
            <div className={styles.problemItems}>
                <Heading type={HeadingType.secondary}>
                    Resources
                </Heading>
                <ProblemResources />
            </div>
            <div className={styles.problemItems}>
                <ProblemSubmissions />
            </div>
        </div>
    );
};

export default ContestProblemDetails;
