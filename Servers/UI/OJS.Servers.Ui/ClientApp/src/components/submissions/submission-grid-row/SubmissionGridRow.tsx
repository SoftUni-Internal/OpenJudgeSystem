import React, { useCallback, useState } from 'react';
import { FaFlagCheckered } from 'react-icons/fa';
import { useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { Popover } from '@mui/material';
import isNil from 'lodash/isNil';

import { IPublicSubmission } from '../../../common/types';
import { useUserProfileSubmissions } from '../../../hooks/submissions/use-profile-submissions';
import { useProblems } from '../../../hooks/use-problems';
import useTheme from '../../../hooks/use-theme';
import { IAuthorizationReduxState } from '../../../redux/features/authorizationSlice';
import { setProfile } from '../../../redux/features/usersSlice';
import { useAppDispatch } from '../../../redux/store';
import concatClassNames from '../../../utils/class-names';
import { defaultDateTimeFormatReverse, formatDate } from '../../../utils/dates';
import { fullStrategyNameToStrategyType, strategyTypeToIcon } from '../../../utils/strategy-type-utils';
import {
    encodeAsUrlParam,
    getContestDetailsAppUrl,
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
        user,
        result: { points, maxPoints },
        strategyName,
        problem: {
            id: problemId,
            name: problemName,
            contest: {
                id: contestId,
                name: contestName,
            },
        },
        isOfficial,
        isCompiledSuccessfully,
        maxMemoryUsed,
        maxTimeUsed,
        processed,
        testRuns,
    } = submission;

    const { actions: { initiateRedirectionToProblem } } = useProblems();
    const { internalUser } =
        useSelector((reduxState: {authorization: IAuthorizationReduxState}) => reduxState.authorization);
    const { actions: { getDecodedUsernameFromProfile } } = useUserProfileSubmissions();
    const dispatch = useAppDispatch();

    const [ competeIconAnchorElement, setCompeteIconAnchorElement ] = useState<HTMLElement | null>(null);
    const isCompeteIconModalOpen = Boolean(competeIconAnchorElement);

    const backgroundColorClassName = getColorClassName(themeColors.baseColor100);

    const usernameFromSubmission = isNil(user)
        ? getDecodedUsernameFromProfile()
        : user;

    const handleDetailsButtonSubmit = useCallback(
        () => {
            const submissionDetailsUrl = getSubmissionDetailsRedirectionUrl({ submissionId });

            initiateRedirectionToProblem(problemId, submissionDetailsUrl);
        },
        [ initiateRedirectionToProblem, problemId, submissionId ],
    );

    const handleContestDetailsButtonSubmit = useCallback(
        () => {
            navigate(getContestDetailsAppUrl(contestId));
        },
        [ navigate, contestId ],
    );

    const onPopoverOpen = (event: React.MouseEvent<HTMLElement>) => {
        setCompeteIconAnchorElement(event.currentTarget);
    };

    const hasTimeAndMemoryUsed = (s: IPublicSubmission) => (!isNil(s.maxMemoryUsed) && !isNil(s.maxTimeUsed)) ?? false;

    const rowClassName = concatClassNames(
        styles.row,
        isDarkMode
            ? styles.darkRow
            : styles.lightRow,
        getColorClassName(themeColors.textColor),
    );

    const renderUsername = useCallback(
        () => (
            <LinkButton
              type={LinkButtonType.plain}
              size={ButtonSize.none}
              to={getUserProfileInfoUrlByUsername(encodeAsUrlParam(usernameFromSubmission))}
              text={usernameFromSubmission}
              internalClassName={styles.redirectButton}
            />
        ),
        [ usernameFromSubmission ],
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
            if (usernameFromSubmission === internalUser.userName || internalUser.isAdmin) {
                return (
                    <Button
                      text="Details"
                      onClick={handleDetailsButtonSubmit}
                      type={ButtonType.secondary}
                    />
                );
            }

            return null;
        },
        [ handleDetailsButtonSubmit, internalUser, usernameFromSubmission ],
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
            <td>
                {
                    options.showTaskDetails
                        ? (
                            <>
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
                            </>
                        )
                        : null
                }
            </td>
            <td>
                <div>
                    {formatDate(createdOn, defaultDateTimeFormatReverse)}
                </div>
                {
                    options.showParticipantUsername
                        ? (
                            <span onClick={() => dispatch(setProfile(null))}>
                                {renderUsername()}
                            </span>
                        )
                        : null
                }
            </td>
            {
                options.showCompeteMarker
                    ? isOfficial
                        ? (
                            <td onMouseEnter={(e) => onPopoverOpen(e)} onMouseLeave={() => setCompeteIconAnchorElement(null)}>
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
                        )
                        : <td />
                    : null
            }
            {
                options.showDetailedResults
                    ? (
                        <td>
                            { hasTimeAndMemoryUsed(submission)
                                ? (
                                    <div className={styles.timeAndMemoryContainer}>
                                        <div className={styles.maxMemoryUsed}>
                                            <MemoryIcon
                                              size={IconSize.Large}
                                              className={styles.memoryIcon}
                                            />
                                            <span className={styles.timeAndMemoryText}>
                                                {(maxMemoryUsed / 1000000).toFixed(2)}
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
                                                {maxTimeUsed / 1000}
                                                {' '}
                                                s.
                                            </span>
                                        </div>
                                    </div>
                                )
                                : null}
                        </td>
                    )
                    : null
            }
            <td>
                <div className={styles.executionResultContainer}>
                    <ExecutionResult
                      points={points}
                      maxPoints={maxPoints}
                      testRuns={testRuns}
                      isCompiledSuccessfully={isCompiledSuccessfully}
                      isProcessed={processed}
                      showDetailedResults={options.showDetailedResults}
                    />
                </div>
            </td>
            {
                options.showSubmissionTypeInfo
                    ? (
                        <td className={styles.strategy}>
                            <div className={styles.strategyWrapper}>
                                {
                                internalUser.isAdmin
                                    ? renderStrategyIcon()
                                    : null
                            }
                                <span>{strategyName}</span>
                            </div>
                        </td>
                    )
                    : null
            }
            <td>
                {renderDetailsBtn()}
            </td>
        </tr>
    );
};

export default SubmissionGridRow;
