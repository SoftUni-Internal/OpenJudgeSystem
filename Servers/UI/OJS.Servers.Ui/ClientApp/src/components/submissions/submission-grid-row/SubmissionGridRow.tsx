import React, { useCallback, useMemo, useState } from 'react';
import { FaFlagCheckered } from 'react-icons/fa';
import { useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import AccessAlarmIcon from '@mui/icons-material/AccessAlarm';
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth';
import { Popover } from '@mui/material';
import isNil from 'lodash/isNil';
import { ITestRunIcon } from 'src/hooks/submissions/types';

import { IPublicSubmission } from '../../../common/types';
import { getContestsDetailsPageUrl } from '../../../common/urls/compose-client-urls';
import useTheme from '../../../hooks/use-theme';
import { IAuthorizationReduxState } from '../../../redux/features/authorizationSlice';
import { setProfile } from '../../../redux/features/usersSlice';
import { useAppDispatch } from '../../../redux/store';
import concatClassNames from '../../../utils/class-names';
import {
    defaultDateTimeFormatReverse,
    formatDate,
    preciseFormatDate,
    submissionsGridDateFormat,
    submissionsGridTimeFormat,
} from '../../../utils/dates';
import { fullStrategyNameToStrategyType, strategyTypeToIcon } from '../../../utils/strategy-type-utils';
import {
    encodeAsUrlParam,
    getSubmissionDetailsRedirectionUrl,
    getUserProfileInfoUrlByUsername,
} from '../../../utils/urls';
import { Button, ButtonSize, ButtonType, LinkButton, LinkButtonType } from '../../guidelines/buttons/Button';
import IconSize from '../../guidelines/icons/common/icon-sizes';
import MemoryIcon from '../../guidelines/icons/MemoryIcon';
import TimeLimitIcon from '../../guidelines/icons/TimeLimitIcon';
import ExecutionResult from '../execution-result/ExecutionResult';
import { ISubmissionsGridOptions } from '../submissions-grid/SubmissionsGrid';

import styles from './SubmissionGridRow.module.scss';

interface ISubmissionGridRowProps {
    submission: IPublicSubmission;
    options: ISubmissionsGridOptions;
}

const SubmissionGridRow = ({
    submission,
    options,
}: ISubmissionGridRowProps) => {
    const navigate = useNavigate();
    const { isDarkMode, getColorClassName, themeColors } = useTheme();
    const {
        id: submissionId,
        createdOn,
        user = '',
        result: { points, maxPoints },
        strategyName,
        problem: {
            id: problemId,
            name: problemName = '',
        },
        isOfficial,
        isCompiledSuccessfully,
        processed,
        testRuns,
        testRunsCache,
        maxTimeUsed,
        maxMemoryUsed,
    } = submission;

    const { id: contestId = 0, name: contestName = '' } = submission.problem.contest || {};

    const { internalUser } =
        useSelector((reduxState: { authorization: IAuthorizationReduxState }) => reduxState.authorization);
    const dispatch = useAppDispatch();

    const [ competeIconAnchorElement, setCompeteIconAnchorElement ] = useState<HTMLElement | null>(null);
    const isCompeteIconModalOpen = Boolean(competeIconAnchorElement);

    const backgroundColorClassName = getColorClassName(themeColors.baseColor100);

    const handleContestDetailsButtonSubmit = useCallback(
        () => {
            navigate(getContestsDetailsPageUrl({ contestId, contestName }));
        },
        [ navigate, contestId, contestName ],
    );

    const onPopoverOpen = (event: React.MouseEvent<HTMLElement>) => {
        setCompeteIconAnchorElement(event.currentTarget);
    };

    const hasTimeAndMemoryUsed = useCallback((s: IPublicSubmission) => !isNil(s.maxMemoryUsed) && !isNil(s.maxTimeUsed), []);

    const rowClassName = concatClassNames(
        styles.row,
        isDarkMode
            ? styles.darkRow
            : styles.lightRow,
        getColorClassName(themeColors.textColor),
    );

    const parsedTestRunsCache = useMemo(() => {
        if (!testRunsCache) {
            return {
                testRuns,
                maxTimeUsed: 0,
                maxMemoryUsed: 0,
            };
        }

        const [ testRunPart, timeMemoryPart ] = testRunsCache.split('|');

        if (!testRunPart) {
            return {
                testRuns,
                maxTimeUsed: 0,
                maxMemoryUsed: 0,
            };
        }

        const trialTestsCount = Number.parseInt(testRunPart[0], 10);
        if (Number.isNaN(trialTestsCount)) {
            return {
                testRuns,
                maxTimeUsed: null,
                maxMemoryUsed: null,
            };
        }

        const testRunResults = testRunPart.slice(1);
        const testRunsParsed: ITestRunIcon[] = Array.from(testRunResults)
            .map((resultType, index) => ({
                resultType: Number.parseInt(resultType, 10),
                id: index + 1,
                isTrialTest: index < trialTestsCount,
            }))
            .filter((run) => !Number.isNaN(run.resultType));

        let maxTimeUsedCache = null;
        let maxMemoryUsedCache = null;
        if (timeMemoryPart) {
            const [ timeString, memoryString ] = timeMemoryPart.split(',');
            if (timeString) {
                const parsedTime = parseInt(timeString, 10);
                if (!Number.isNaN(parsedTime)) {
                    maxTimeUsedCache = parsedTime;
                }
            }
            if (memoryString) {
                const parsedMemory = parseInt(memoryString, 10);
                if (!Number.isNaN(parsedMemory)) {
                    maxMemoryUsedCache = parsedMemory;
                }
            }
        }

        return {
            testRuns: testRunsParsed,
            maxTimeUsedCache,
            maxMemoryUsedCache,
        };
    }, [ testRunsCache, testRuns ]);

    const { testRuns: parsedTestRuns, maxTimeUsedCache, maxMemoryUsedCache } = parsedTestRunsCache;

    const renderUsername = useCallback(
        () =>
            <LinkButton
              type={LinkButtonType.plain}
              size={ButtonSize.none}
              to={getUserProfileInfoUrlByUsername(encodeAsUrlParam(user))}
              text={user}
              internalClassName={styles.redirectButton}
            />
        ,
        [ user ],
    );

    const renderProblemInformation = useCallback(
        () => {
            if (isNil(problemId)) {
                return null;
            }

            return (
                <div>
                    <span>{problemName}</span>
                </div>
            );
        },
        [ problemId, problemName ],
    );

    const renderStrategyIcon = useCallback(
        () => {
            const Icon = strategyTypeToIcon(fullStrategyNameToStrategyType(strategyName));

            if (isNil(Icon)) {
                return null;
            }

            return (
                <Icon
                  size={IconSize.Large}
                  className={getColorClassName(themeColors.textColor)}
                />
            );
        },
        [ getColorClassName, strategyName, themeColors.textColor ],
    );

    const renderDetailsBtn = useCallback(
        () => {
            if (!options.showParticipantUsername || user === internalUser.userName || internalUser.isAdmin) {
                return (
                    <LinkButton
                      to={getSubmissionDetailsRedirectionUrl({ submissionId })}
                      text="Details"
                      type={LinkButtonType.secondary}
                      size={ButtonSize.small}
                    />
                );
            }

            return null;
        },
        [ options.showParticipantUsername, user, internalUser.userName, internalUser.isAdmin, submissionId ],
    );

    return (
        <tr key={submission.id} className={rowClassName}>
            <td>
                <span>
                    {' '}
                    #
                    {submissionId}
                </span>
            </td>
            {
                options.showTaskDetails
                    ? <td>
                        {renderProblemInformation()}
                        {/* TODO: Fix this to use Link */}
                        <Button
                          type={ButtonType.secondary}
                          size={ButtonSize.small}
                          className={styles.link}
                          internalClassName={styles.redirectButton}
                          onClick={handleContestDetailsButtonSubmit}
                          text={contestName}
                        />
                    </td>

                    : null
            }
            <td>
                {internalUser.isAdmin
                    ? <div className={styles.fromDate}>
                        <span className={styles.fromDateRow}>
                            <CalendarMonthIcon className={styles.icon} />
                            {preciseFormatDate(createdOn, submissionsGridDateFormat)}
                        </span>
                        <span className={styles.fromDateRow}>
                            <AccessAlarmIcon className={styles.icon} />
                            {preciseFormatDate(createdOn, submissionsGridTimeFormat)}
                        </span>
                    </div>

                    : <div>
                        {formatDate(createdOn, defaultDateTimeFormatReverse)}
                    </div>
                    }
                {
                    options.showParticipantUsername
                        ? <span onClick={() => dispatch(setProfile(null))}>
                            {renderUsername()}
                        </span>

                        : null
                }
            </td>
            {
                options.showCompeteMarker
                    ? isOfficial
                        ? <td onMouseEnter={(e) => onPopoverOpen(e)} onMouseLeave={() => setCompeteIconAnchorElement(null)}>
                            <FaFlagCheckered className={styles.competeIcon} />
                            <Popover
                                  open={isCompeteIconModalOpen}
                                  anchorEl={competeIconAnchorElement}
                                  anchorOrigin={{
                                      vertical: 'top',
                                      horizontal: 'center',
                                  }}
                                  transformOrigin={{
                                      vertical: 'top',
                                      horizontal: 'left',
                                  }}
                                  sx={{ pointerEvents: 'none' }}
                                  onClose={() => setCompeteIconAnchorElement(null)}
                                  disableRestoreFocus
                                >
                                <div className={`${styles.competeIconModal} ${backgroundColorClassName}`}>
                                    This submission was done in compete mode.
                                </div>
                            </Popover>
                        </td>

                        : <td aria-hidden="true" />
                    : null
            }
            {
                options.showDetailedResults
                    ? <td>
                        {hasTimeAndMemoryUsed(submission)
                            ? <div className={styles.timeAndMemoryContainer}>
                                <div className={styles.maxMemoryUsed}>
                                    <MemoryIcon
                                              size={IconSize.Large}
                                              className={styles.memoryIcon}
                                            />
                                    <span className={styles.timeAndMemoryText}>
                                        {((maxMemoryUsedCache ?? maxMemoryUsed) / 1000000).toFixed(2)}
                                        {' '}
                                        MB
                                    </span>
                                </div>
                                <div className={styles.maxTimeUsed}>
                                    <TimeLimitIcon
                                              size={IconSize.Large}
                                              className={styles.timeIcon}
                                            />
                                    <span className={styles.timeAndMemoryText}>
                                        {((maxTimeUsedCache ?? maxTimeUsed) / 1000).toFixed(2)}
                                        {' '}
                                        s.
                                    </span>
                                </div>
                            </div>

                            : null}
                    </td>

                    : null
            }
            <td>
                <div className={styles.executionResultContainer}>
                    <ExecutionResult
                      points={points}
                      maxPoints={maxPoints}
                      testRuns={parsedTestRuns}
                      isCompiledSuccessfully={isCompiledSuccessfully}
                      isProcessed={processed}
                      showDetailedResults={options.showDetailedResults}
                    />
                </div>
            </td>
            {
                options.showSubmissionTypeInfo
                    ? <td className={styles.strategy}>
                        <div className={styles.strategyWrapper}>
                            {
                                    internalUser.isAdmin
                                        ? renderStrategyIcon()
                                        : null
                                }
                            <span>{strategyName}</span>
                        </div>
                    </td>

                    : null
            }
            <td>
                {renderDetailsBtn()}
            </td>
        </tr>
    );
};

export default SubmissionGridRow;
