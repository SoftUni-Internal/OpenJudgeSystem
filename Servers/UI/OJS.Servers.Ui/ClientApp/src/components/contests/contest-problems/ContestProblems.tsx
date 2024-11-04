import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import { Popover } from '@mui/material';

import { IProblemType } from '../../../common/types';
import useTheme from '../../../hooks/use-theme';
import { setSelectedContestDetailsProblem } from '../../../redux/features/contestsSlice';
import { useAppDispatch, useAppSelector } from '../../../redux/store';
import { LinkButton, LinkButtonType } from '../../guidelines/buttons/Button';

import styles from './ContestProblems.module.scss';

interface IContestProblemsProps {
    problems: Array<IProblemType>;
    onContestProblemChange: (submissionId: number) => void;
    totalParticipantsCount?: number;
    sumMyPoints?: number;
    sumTotalPoints: number;
}

const ContestProblems = (props: IContestProblemsProps) => {
    const {
        problems,
        onContestProblemChange,
        totalParticipantsCount,
        sumMyPoints = 0,
        sumTotalPoints,
    } = props;

    const { hash } = useLocation();
    const dispatch = useAppDispatch();
    const { isDarkMode, themeColors, getColorClassName } = useTheme();
    const { selectedContestDetailsProblem } = useAppSelector((state) => state.contests);

    const [ excludedFromHomeworkAnchorElement, setExcludedFromHomeworkAnchorElement ] = useState<HTMLElement | null>(null);

    const backgroundColorClassName = getColorClassName(isDarkMode
        ? themeColors.baseColor200
        : themeColors.baseColor100);
    const modalBackgroundColorClassName = getColorClassName(themeColors.baseColor100);
    const headersClassname = getColorClassName(themeColors.baseColor300);

    const colorClassName = getColorClassName(themeColors.textColor);

    const isExcludedFromHomeworkModalOpen = Boolean(excludedFromHomeworkAnchorElement);

    useEffect(() => {
        if (!problems) {
            return;
        }

        const selectedProblem = problems.find((prob) => prob.orderBy === Number(hash.substring(1)));
        if (selectedProblem) {
            dispatch(setSelectedContestDetailsProblem({ selectedProblem }));
        } else {
            dispatch(setSelectedContestDetailsProblem({ selectedProblem: problems[0] }));
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const onProblemClick = (problem: IProblemType) => {
        onContestProblemChange(problem.defaultSubmissionTypeId ?? 0);
        dispatch(setSelectedContestDetailsProblem({ selectedProblem: problem }));
    };

    return (
        <div>
            <div className={`${styles.problemsHeader} ${headersClassname}`}>
                <div>Tasks</div>
                <div>Points</div>
            </div>
            <div className={`${styles.problemsWrapper} ${backgroundColorClassName}`}>
                <div className={styles.problemsInnerWrapper}>
                    {problems.map((problem, idx) => {
                        const isActive = selectedContestDetailsProblem?.id === problem.id;
                        const isLast = idx === problems.length - 1;
                        return (
                            <LinkButton
                              to={`#${problem.orderBy}`}
                              type={LinkButtonType.plain}
                              preventScrollReset
                              key={`contest-problem-${problem.id}`}
                              className={`${styles.problem} ${colorClassName} ${isActive
                                  ? styles.activeProblem
                                  : ''}`}
                              style={{
                                  borderBottom: `${isLast
                                      ? 0
                                      : 1}px solid ${themeColors.textColor}`,
                              }}
                              onClick={() => onProblemClick(problem)}
                            >
                                <div className={styles.problemName}>
                                    {problem.name}
                                    {problem.isExcludedFromHomework && (
                                    <div
                                      style={{ display: 'inline' }}
                                      onMouseEnter={(e) => setExcludedFromHomeworkAnchorElement(e.currentTarget)}
                                      onMouseLeave={() => setExcludedFromHomeworkAnchorElement(null)}
                                    >
                                        <span className={styles.excludedMark}>*</span>
                                        {' '}
                                        <Popover
                                          open={isExcludedFromHomeworkModalOpen}
                                          anchorEl={excludedFromHomeworkAnchorElement}
                                          anchorOrigin={{
                                              vertical: 'bottom',
                                              horizontal: 'left',
                                          }}
                                          transformOrigin={{
                                              vertical: 'top',
                                              horizontal: 'left',
                                          }}
                                          sx={{ pointerEvents: 'none' }}
                                          onClose={() => setExcludedFromHomeworkAnchorElement(null)}
                                          disableRestoreFocus
                                        >
                                            <div className={`${styles.excludedFromHomeworkModal} ${modalBackgroundColorClassName}`}>
                                                The score received from this problem would not be included
                                                in the final results for this contest.
                                            </div>
                                        </Popover>
                                    </div>
                                    )}
                                </div>
                                <div>
                                    {problem.points || 0}
                                    /
                                    {problem.maximumPoints}
                                </div>
                            </LinkButton>
                        );
                    })}
                </div>
            </div>
            <div className={`${styles.problemsInfoSection} ${headersClassname}`}>
                <div className={styles.participantsInfo}>
                    Total participants:
                    {' '}
                    {totalParticipantsCount}
                </div>
                <div className={styles.pointsInfo}>
                    {sumMyPoints}
                    /
                    {sumTotalPoints}
                </div>
            </div>
        </div>
    );
};

export default ContestProblems;
