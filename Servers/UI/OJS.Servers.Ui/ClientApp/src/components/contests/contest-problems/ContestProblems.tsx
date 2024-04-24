import { useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

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

    const backgroundColorClassName = getColorClassName(themeColors.baseColor200);
    const darkBackgroundClassName = getColorClassName(themeColors.baseColor500);

    useEffect(() => {
        if (!problems) {
            return;
        }

        const selectedProblem = problems.find((prob) => prob.id === Number(hash.substring(1)));
        if (selectedProblem) {
            dispatch(setSelectedContestDetailsProblem({ selectedProblem }));
        } else {
            dispatch(setSelectedContestDetailsProblem({ selectedProblem: problems[0] }));
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const onProblemClick = (problem: IProblemType) => {
        navigate(`#${problem.id}`);
        onContestProblemChange();
        dispatch(setSelectedContestDetailsProblem({ selectedProblem: problem }));
    };

    return (
        <div>
            <div className={styles.wrapper}>
                <div className={`${styles.problemsWrapper} ${backgroundColorClassName}`}>
                    <div className={styles.problemsHeader} style={{ borderBottom: `1px solid ${themeColors.textColor}` }}>
                        <div>Tasks</div>
                        <div>Points</div>
                    </div>
                    {problems.map((problem) => {
                        const isActive = selectedContestDetailsProblem?.id === problem.id;
                        return (
                            <div
                              key={`contest-problem-${problem.id}`}
                              className={`${styles.problem} ${isActive
                                  ? styles.activeProblem
                                  : ''}`}
                              style={{ borderBottom: `1px solid ${themeColors.textColor}` }}
                              onClick={() => onProblemClick(problem)}
                            >
                                <div>
                                    {problem.name}
                                    {problem.isExcludedFromHomework && <span className={styles.excludedMark}>*</span>}
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
