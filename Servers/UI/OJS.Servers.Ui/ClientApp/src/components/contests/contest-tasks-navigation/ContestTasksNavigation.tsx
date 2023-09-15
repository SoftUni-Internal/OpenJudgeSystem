import React, { useCallback, useMemo } from 'react';
import isEmpty from 'lodash/isEmpty';
import isNil from 'lodash/isNil';

import { ContestParticipationType } from '../../../common/constants';
import { IProblemType } from '../../../common/types';
import { useAppUrls } from '../../../hooks/use-app-urls';
import { useCurrentContest } from '../../../hooks/use-current-contest';
import { useProblems } from '../../../hooks/use-problems';
import concatClassNames from '../../../utils/class-names';
import { Button, ButtonType, LinkButton, LinkButtonType } from '../../guidelines/buttons/Button';
import Heading, { HeadingType } from '../../guidelines/headings/Heading';
import List, { ListType } from '../../guidelines/lists/List';
import SubmissionResultPointsLabel from '../../submissions/submission-result-points-label/SubmissionResultPointsLabel';

import styles from './ContestTasksNavigation.module.scss';

const compareByOrderBy = (p1: IProblemType, p2: IProblemType) => p1.orderBy - p2.orderBy;

const ContestTasksNavigation = () => {
    const {
        state: {
            currentProblem,
            problems,
        },
        actions: { selectCurrentProblem },
    } = useProblems();

    const {
        state: {
            contest,
            currentContestParticipantScores,
            isOfficial,
        },
    } = useCurrentContest();
    const { getContestResultsUrl } = useAppUrls();

    const participationType = useMemo(
        () => isOfficial
            ? ContestParticipationType.Compete
            : ContestParticipationType.Practice,
        [ isOfficial ],
    );

    const renderTask = useCallback(
        (problem: IProblemType) => {
            const { id: currentId } = currentProblem || {};
            const { id } = problem;

            const selectedClassName = currentId === id
                ? styles.selected
                : '';

            const className = concatClassNames(
                styles.taskSideNavigationItem,
                selectedClassName,
            );

            const maxParticipantScoreLoaded = currentContestParticipantScores
                .find((ps) => ps.problemId === problem.id);

            const problemScore = isNil(maxParticipantScoreLoaded)
                ? 0
                : maxParticipantScoreLoaded.points;

            return (
                <>
                    <Button
                      onClick={() => selectCurrentProblem(id)}
                      className={className}
                      type={ButtonType.plain}
                    >
                        {problem.name}
                    </Button>
                    <SubmissionResultPointsLabel
                      points={problemScore}
                      maximumPoints={problem.maximumPoints}
                      isProcessed={!isNil(maxParticipantScoreLoaded)}
                    />
                </>
            );
        },
        [ currentContestParticipantScores, currentProblem, selectCurrentProblem ],
    );

    const sideBarTasksList = 'all-tasks-list';
    const sideBarTasksListClassName = concatClassNames(styles.tasksListSideNavigation, sideBarTasksList);
    const renderTasksList = useCallback(
        () => isEmpty(contest?.problems)
            ? (
                <Heading
                  type={HeadingType.secondary}
                  className={styles.noTasksText}
                >
                    There are no tasks for this contest, yet.
                </Heading>
            )
            : (
                <List
                  values={problems.sort(compareByOrderBy)}
                  itemFunc={renderTask}
                  className={sideBarTasksListClassName}
                  itemClassName={styles.taskListItem}
                  type={ListType.numbered}
                />
            ),
        [ problems, renderTask, sideBarTasksListClassName, contest?.problems ],
    );

    const resultsButtonClass = 'resultsButton';
    const refreshButtonClassName = concatClassNames(styles.resultsButton, resultsButtonClass);

    return (
        <div className={styles.tasksSideNavigation}>
            <Heading type={HeadingType.secondary}>Tasks</Heading>
            {renderTasksList()}
            <LinkButton
              type={LinkButtonType.secondary}
              to={getContestResultsUrl({ id: contest?.id, participationType })}
              text="Results"
              className={refreshButtonClassName}
            />
        </div>
    );
};

export default ContestTasksNavigation;
