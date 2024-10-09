import React, { useCallback } from 'react';

import { ITestRunIcon, ITestRunType } from '../../../hooks/submissions/types';
import useTheme from '../../../hooks/use-theme';
import concatClassNames from '../../../utils/class-names';

import ErrorResult from './ErrorResult';
import TestRunIcon from './TestRunIcon';

import styles from './ExecutionResult.module.scss';

interface IExecutionResultDetailsProps {
    points: number;
    maxPoints: number;
    testRuns: ITestRunType[] | ITestRunIcon[];
    isCompiledSuccessfully: boolean;
    isProcessed: boolean;
    showDetailedResults: boolean;
}

const ExecutionResult = ({
    points,
    maxPoints,
    testRuns,
    isCompiledSuccessfully,
    isProcessed,
    showDetailedResults,
}: IExecutionResultDetailsProps) => {
    const { getColorClassName, themeColors } = useTheme();
    const renderTestRunIcons = useCallback(
        (runs: ITestRunType[] | ITestRunIcon[]) => runs
            .slice()
            .map((testRun) => <TestRunIcon key={`t-r-i-${testRun.id}`} testRun={testRun} />),
        [],
    );

    const listClassName = concatClassNames(
        styles.testResultsList,
        getColorClassName(themeColors.textColor),
    );

    const hasTestRuns = useCallback(() => (testRuns && testRuns.length > 0) ?? false, [ testRuns ]);

    const renderResult = useCallback(() => {
        if (isProcessed && !isCompiledSuccessfully) {
            return (<ErrorResult />);
        }

        if (!isProcessed && !isCompiledSuccessfully) {
            return <span>Processing</span>;
        }

        return (
            <div className={styles.executionResultInfo}>
                {
                    showDetailedResults && hasTestRuns() && (
                        <div className={styles.testRunsContainer}>
                            {renderTestRunIcons(testRuns)}
                        </div>
                    )
                }
                <span>
                    {points}
                    {' '}
                    /
                    {' '}
                    {maxPoints}
                </span>
            </div>
        );
    }, [ hasTestRuns, isCompiledSuccessfully, isProcessed, maxPoints, points, renderTestRunIcons, showDetailedResults, testRuns ]);

    return (
        <div className={listClassName}>
            {renderResult()}
        </div>
    );
};

export default ExecutionResult;
