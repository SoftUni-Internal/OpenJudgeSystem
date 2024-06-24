import React, { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { Popover } from '@mui/material';

import { IProblemType } from '../../../common/types';
import useTheme from '../../../hooks/use-theme';
import { setSelectedContestDetailsProblem } from '../../../redux/features/contestsSlice';
import { useAppDispatch, useAppSelector } from '../../../redux/store';

import styles from './ContestProblems.module.scss';

interface IContestProblemsProps {
    problems: Array<IProblemType>;
    onContestProblemChange: () => void;
    totalParticipantsCount?: number;
    sumMyPoints?: number;
    sumTotalPoints: number;
}

const ContestProblems = (props: IContestProblemsProps) => {
    const { problems, onContestProblemChange, totalParticipantsCount, sumMyPoints = 0, sumTotalPoints } = props;

    const { hash } = useLocation();
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const { themeColors, getColorClassName } = useTheme();
    const { selectedContestDetailsProblem } = useAppSelector((state) => state.contests);

    const [ excludedFromHomeworkAnchorElement, setExcludedFromHomeworkAnchorElement ] = useState<HTMLElement | null>(null);

    const backgroundColorClassName = getColorClassName(themeColors.baseColor200);
    const modalBackgroundColorClassName = getColorClassName(themeColors.baseColor100);
    const darkBackgroundClassName = getColorClassName(themeColors.baseColor500);

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
        navigate(`#${problem.orderBy}`);
        onContestProblemChange();
        dispatch(setSelectedContestDetailsProblem({ selectedProblem: problem }));
    };

    return (
        <div>
            <div className={`${styles.problemsHeader} ${darkBackgroundClassName}`}>
                <div>Tasks</div>
                <div>Points</div>
            </div>
            <div className={`${styles.problemsWrapper} ${backgroundColorClassName}`}>
                <div className={styles.problemsInnerWrapper}>
                    {problems.map((problem, idx) => {
                        const isActive = selectedContestDetailsProblem?.id === problem.id;
                        const isLast = idx === problems.length - 1;
                        return (
                            <div
                              key={`contest-problem-${problem.id}`}
                              className={`${styles.problem} ${isActive
                                  ? styles.activeProblem
                                  : ''}`}
                              style={{
                                  borderBottom: `${isLast
                                      ? 0
                                      : 1}px solid ${themeColors.textColor}`,
                              }}
                              onClick={() => onProblemClick(problem)}
                            >
                                <div>
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
                            </div>
                        );
                    })}
                </div>
            </div>
            <div className={`${styles.problemsInfoSection} ${darkBackgroundClassName}`}>
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
