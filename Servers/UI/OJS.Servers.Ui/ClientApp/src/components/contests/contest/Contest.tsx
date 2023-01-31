import React, { useCallback, useEffect, useMemo } from 'react';
import isNil from 'lodash/isNil';

import { useAuth } from '../../../hooks/use-auth';
import { useCurrentContest } from '../../../hooks/use-current-contest';
import { usePageTitles } from '../../../hooks/use-page-titles';
import concatClassNames from '../../../utils/class-names';
import { convertToTwoDigitValues } from '../../../utils/dates';
import Countdown, { ICountdownRemainingType, Metric } from '../../guidelines/countdown/Countdown';
import Heading, { HeadingType } from '../../guidelines/headings/Heading';
import Text, { TextType } from '../../guidelines/text/Text';
import ContestProblemDetails from '../contest-problem-details/ContestProblemDetails';
import ContestTasksNavigation from '../contest-tasks-navigation/ContestTasksNavigation';
import SubmissionBox from '../submission-box/SubmissionBox';

import styles from './Contest.module.scss';

const Contest = () => {
    const {
        state: {
            contest,
            score,
            maxScore,
            remainingTimeInMilliseconds,
            validationResult,
            totalParticipantsCount,
            activeParticipantsCount,
            isOfficial,
        },
        actions: { setIsSubmitAllowed },
    } = useCurrentContest();
    const { state: { user: { permissions: { canAccessAdministration } } } } = useAuth();
    const { actions: { setPageTitle } } = usePageTitles();

    const navigationContestClass = styles.navigationContest.toString();
    const navigationContestClassName = concatClassNames(navigationContestClass);

    const submissionBoxClass = 'submissionBox';
    const submissionBoxClassName = concatClassNames(submissionBoxClass);

    const problemInfoClass = styles.problemInfo.toString();
    const problemInfoClassName = concatClassNames(problemInfoClass);

    const contestTitle = useMemo(
        () => `${contest?.name}`,
        [ contest?.name ],
    );

    useEffect(() => {
        setPageTitle(contestTitle);
    }, [ contestTitle, setPageTitle ]);

    const scoreText = useMemo(
        () => `${score}/${maxScore}`,
        [ maxScore, score ],
    );

    const scoreClassName = 'score';
    const renderScore = useCallback(
        () => {
            if (scoreText === '0/0') {
                return null;
            }

            return (
                <p className={scoreClassName}>
                    Score:
                    {' '}
                    <Text type={TextType.Bold}>
                        {scoreText}
                    </Text>
                </p>
            );
        },
        [ scoreText ],
    );

    const remainingTimeClassName = 'remainingTime';
    const renderCountdown = useCallback(
        (remainingTime: ICountdownRemainingType) => {
            const { hours, minutes, seconds } = convertToTwoDigitValues(remainingTime);

            return (
                <p className={remainingTimeClassName}>
                    Remaining time:
                    {' '}
                    <Text type={TextType.Bold}>
                        {hours}
                        :
                        {minutes}
                        :
                        {seconds}
                    </Text>
                </p>
            );
        },
        [],
    );

    const handleCountdownEnd = useCallback(
        () => {
            setIsSubmitAllowed(canAccessAdministration || false);
        },
        [ canAccessAdministration, setIsSubmitAllowed ],
    );

    const renderTimeRemaining = useCallback(
        () => {
            if (!remainingTimeInMilliseconds) {
                return null;
            }

            const currentSeconds = remainingTimeInMilliseconds / 1000;

            return (
                <Countdown
                  renderRemainingTime={renderCountdown}
                  duration={currentSeconds}
                  metric={Metric.seconds}
                  handleOnCountdownEnd={handleCountdownEnd}
                />
            );
        },
        [ handleCountdownEnd, remainingTimeInMilliseconds, renderCountdown ],
    );

    const secondaryHeadingClassName = useMemo(
        () => concatClassNames(styles.contestHeading, styles.contestInfoContainer),
        [],
    );

    const participantsStateText = useMemo(
        () => isOfficial
            ? 'Active'
            : 'Total',
        [ isOfficial ],
    );

    const participantsValue = useMemo(
        () => isOfficial
            ? activeParticipantsCount
            : totalParticipantsCount,
        [ activeParticipantsCount, isOfficial, totalParticipantsCount ],
    );

    const renderParticipants = useCallback(
        () => (
            <span>
                {participantsStateText}
                {' '}
                Participants:
                {' '}
                <Text type={TextType.Bold}>
                    {participantsValue}
                </Text>
            </span>
        ),
        [ participantsStateText, participantsValue ],
    );

    const renderErrorMessage = useCallback(() => (
        <div className={styles.headingContest}>
            <Heading
              type={HeadingType.primary}
              className={styles.contestHeading}
            >
                {contestTitle}
                {' '}
                -
                {' '}
                {validationResult.message}
            </Heading>
        </div>
    ), [ validationResult, contestTitle ]);

    const renderContest = useCallback(
        () => (
            <>
                <div className={styles.headingContest}>
                    <Heading
                      type={HeadingType.primary}
                      className={styles.contestHeading}
                    >
                        {contestTitle}
                    </Heading>
                    <Heading type={HeadingType.secondary} className={secondaryHeadingClassName}>
                        {renderParticipants()}
                        {renderTimeRemaining()}
                        {renderScore()}
                    </Heading>
                </div>

                <div className={styles.contestWrapper}>
                    <div className={navigationContestClassName}>
                        <ContestTasksNavigation />
                    </div>
                    <div className={submissionBoxClassName}>
                        <SubmissionBox />
                    </div>
                    <div className={problemInfoClassName}>
                        <ContestProblemDetails />
                    </div>
                </div>
            </>
        ),
        [
            contestTitle,
            navigationContestClassName,
            problemInfoClassName,
            renderScore,
            renderTimeRemaining,
            secondaryHeadingClassName,
            submissionBoxClassName,
            renderParticipants,
        ],
    );

    const renderPage = useCallback(
        () => isNil(validationResult)
            ? <div>Loading data</div>
            : validationResult.isValid
                ? renderContest()
                : renderErrorMessage(),
        [ renderErrorMessage, renderContest, validationResult ],
    );

    return renderPage();
};

export default Contest;
