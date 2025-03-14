import React from 'react';
import { Link } from 'react-router-dom';

import { IProblemSearchType } from '../../../common/types';
import { getContestsDetailsPageUrl } from '../../../common/urls/compose-client-urls';
import useTheme from '../../../hooks/use-theme';
import ContestButton from '../../contests/contest-button/ContestButton';

import styles from './ProblemSearchCard.module.scss';

interface IProblemSearchCardProps {
    problem: IProblemSearchType;
}
const ProblemSearchCard = (props: IProblemSearchCardProps) => {
    const { problem } = props;
    const { contest, id, name, orderBy } = problem;
    const { isDarkMode, getColorClassName, themeColors } = useTheme();

    const textColorClassName = getColorClassName(themeColors.textColor);
    const backgroundColorClassName = getColorClassName(isDarkMode
        ? themeColors.baseColor200
        : themeColors.baseColor100);

    return (
        <div className={`${styles.problemsSearchCardWrapper} ${textColorClassName} ${backgroundColorClassName}`}>
            <div>
                <Link
                  to={getContestsDetailsPageUrl({ contestId: contest.id, contestName: contest.name })}
                  className={`${styles.problemName} ${textColorClassName}`}
                >
                    {name}
                </Link>
                <div className={styles.contestName}>{contest.name}</div>
            </div>
            <div className={styles.buttonsWrapper}>
                <ContestButton
                  isCompete
                  isDisabled={!contest.canBeCompeted}
                  id={contest.id}
                  problemId={id}
                  orderBy={orderBy}
                  name={name}
                />
                <ContestButton
                  isCompete={false}
                  isDisabled={!contest.canBePracticed}
                  id={contest.id}
                  problemId={id}
                  orderBy={orderBy}
                  name={name}
                />
            </div>
        </div>
    );
};

export default ProblemSearchCard;
