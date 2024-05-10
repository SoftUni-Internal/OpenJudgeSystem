/* eslint-disable @typescript-eslint/no-non-null-asserted-optional-chain */
/* eslint-disable consistent-return */
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { IoIosInformationCircleOutline, IoMdRefresh } from 'react-icons/io';
import { Link, useNavigate, useParams } from 'react-router-dom';
import Popover from '@mui/material/Popover';
import moment from 'moment';

import { ContestParticipationType } from '../../../common/constants';
import { IProblemResourceType, ISubmissionTypeType } from '../../../common/types';
import CodeEditor from '../../../components/code-editor/CodeEditor';
import ContestBreadcrumbs from '../../../components/contests/contest-breadcrumbs/ContestBreadcrumbs';
import ContestProblems from '../../../components/contests/contest-problems/ContestProblems';
import ErrorWithActionButtons from '../../../components/error/ErrorWithActionButtons';
import FileUploader from '../../../components/file-uploader/FileUploader';
import Button, { ButtonState, LinkButton } from '../../../components/guidelines/buttons/Button';
import Dropdown from '../../../components/guidelines/dropdown/Dropdown';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import ProblemResource from '../../../components/problem-resources/ProblemResource';
import SubmissionsGrid from '../../../components/submissions/submissions-grid/SubmissionsGrid';
import useTheme from '../../../hooks/use-theme';
import { setContestDetails } from '../../../redux/features/contestsSlice';
import {
    useGetContestUserParticipationQuery,
    useLazyGetContestByIdQuery,
    useSubmitContestSolutionFileMutation,
    useSubmitContestSolutionMutation,
} from '../../../redux/services/contestsService';
import { useLazyGetSubmissionResultsByProblemQuery } from '../../../redux/services/submissionsService';
import { useAppDispatch, useAppSelector } from '../../../redux/store';
import { calculatedTimeFormatted, transformDaysHoursMinutesTextToMinutes, transformSecondsToTimeSpan } from '../../../utils/dates';
import { flexCenterObjectStyles } from '../../../utils/object-utils';
import { setLayout } from '../../shared/set-layout';

import styles from './ContestSolutionSubmitPage.module.scss';

const ContestSolutionSubmitPage = () => {
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const { themeColors, getColorClassName } = useTheme();
    const { contestId, participationType } = useParams();

    const [ isSubmitButtonDisabled, setIsSubmitButtonDisabled ] = useState<boolean>(false);
    const [ remainingTime, setRemainingTime ] = useState<number>(0);
    const [ contestTimeHasExpired, setContestTimeHasExpired ] = useState<boolean>(false);
    const [ remainingTimeForCompete, setRemainingTimeForCompete ] = useState<string | null>();
    const [ selectedStrategyValue, setSelectedStrategyValue ] = useState<string>('');
    const [ selectedSubmissionType, setSelectedSubmissionType ] = useState<ISubmissionTypeType>();
    const [ submissionCode, setSubmissionCode ] = useState<string>();
    const [ anchorEl, setAnchorEl ] = useState<HTMLElement | null>(null);
    const [ selectedSubmissionsPage, setSelectedSubmissionsPage ] = useState<number>(1);
    const [ uploadedFile, setUploadedFile ] = useState<File | null>(null);
    const [ fileUploadError, setFileUploadError ] = useState<string>('');
    const [ solutionSubmitPreError, setSolutionSubmitPreError ] = useState<boolean>(false);

    const { selectedContestDetailsProblem, contestDetails } = useAppSelector((state) => state.contests);
    const { internalUser: user } = useAppSelector((state) => state.authorization);

    const [ submitSolution, {
        // isSuccess: submitSolutionSuccess,
        error: submitSolutionError,
        isError: submitSolutionHasError,
        isLoading: submitSolutionIsLoading,
    } ] = useSubmitContestSolutionMutation();

    const [ submitSolutionFile, {
        // isSuccess: submitSolutionFileSuccess,
        error: submitSolutionFileError,
        isError: submitSolutionFileHasError,
        isLoading: submitSolutionFileIsLoading,
    } ] = useSubmitContestSolutionFileMutation();

    const [ getContestById ] = useLazyGetContestByIdQuery();
    const [
        getSubmissionsData, {
            data: submissionsData,
            isError: submissionsError,
            isLoading: submissionsDataLoading,
        },
    ] = useLazyGetSubmissionResultsByProblemQuery();

    const isModalOpen = Boolean(anchorEl);
    const isCompete = participationType === ContestParticipationType.Compete;

    const textColorClassName = getColorClassName(themeColors.textColor);
    const lightBackgroundClassName = getColorClassName(themeColors.baseColor100);

    const {
        data,
        isLoading,
        isError,
        error,
        refetch,
    } = useGetContestUserParticipationQuery({ id: Number(contestId!), isOfficial: isCompete });

    const {
        contest,
        isRegisteredParticipant,
        isActiveParticipant,
        participantsCount,
        lastSubmissionTime,
        userSubmissionsTimeLimit,
        endDateTimeForParticipantOrContest,
    } = data || {};

    const { problems, allowedSubmissionTypes = [] } = contest || {};

    const {
        memoryLimit,
        timeLimit,
        fileSizeLimit,
        checkerName,
    } = selectedContestDetailsProblem || {};

    const onStrategyDropdownItemSelect = useCallback((s: any) => {
        const submissionType = allowedSubmissionTypes.find((type: ISubmissionTypeType) => type.id === s.id);

        setSelectedStrategyValue(s.id);
        setSelectedSubmissionType(submissionType);
    }, [ allowedSubmissionTypes ]);

    const strategyDropdownItems = useMemo(
        () => selectedContestDetailsProblem?.allowedSubmissionTypes?.map((item: ISubmissionTypeType) => ({ id: item.id, name: item.name })),
        [ selectedContestDetailsProblem ],
    );

    // this effect manages the disabling of the submit button as well as the
    // displaying of the seconds before the next submission would be enabled
    useEffect(() => {
        if (!lastSubmissionTime || !userSubmissionsTimeLimit) {
            return;
        }

        const intervalId = setInterval(() => {
            const currentTime = moment();
            const elapsedTimeInSeconds = moment.utc(currentTime).diff(moment.utc(lastSubmissionTime), 'seconds');
            const newRemainingTime = userSubmissionsTimeLimit - elapsedTimeInSeconds;

            if (newRemainingTime <= 0) {
                setIsSubmitButtonDisabled(false);
                setRemainingTime(0);
                clearInterval(intervalId);
            } else {
                setRemainingTime(newRemainingTime);
                setIsSubmitButtonDisabled(true);
            }
        });

        return () => {
            clearInterval(intervalId);
        };
    }, [ lastSubmissionTime, userSubmissionsTimeLimit ]);

    // managing the proper display of remaining time in compete contest
    useEffect(() => {
        if (!endDateTimeForParticipantOrContest) {
            return;
        }

        const remainingTimeForParticipantOrContest = moment.utc(moment()).diff(moment.utc(endDateTimeForParticipantOrContest));
        if (remainingTimeForParticipantOrContest > 0) {
            // Positive time means time is past end time for contest or participant
            setContestTimeHasExpired(true);
            return;
        }

        const intervalId = setInterval(() => {
            const currentTime = moment();
            const remainingCompeteTime = Math.abs(moment.utc(currentTime).diff(moment.utc(endDateTimeForParticipantOrContest)));

            if (remainingCompeteTime > 0) {
                const formattedTime = calculatedTimeFormatted(moment.duration(remainingCompeteTime, 'millisecond'));
                setRemainingTimeForCompete(formattedTime);
            } else {
                setContestTimeHasExpired(true);
                setRemainingTimeForCompete(null);
            }
        });

        return () => {
            clearInterval(intervalId);
        };
    }, [ endDateTimeForParticipantOrContest, setRemainingTimeForCompete ]);

    // in case of not registered user for compete contest, redirect
    // user to register page in order to keep the flow correct
    useEffect(() => {
        if (isLoading) {
            return;
        }
        if ((!isRegisteredParticipant && !isActiveParticipant) && !isError) {
            navigate(`/contests/register/${contestId}/${participationType}`, { replace: true });
        }
    }, [ isLoading, isError, isRegisteredParticipant, isActiveParticipant, contestId, participationType, navigate ]);

    useEffect(() => {
        setSubmissionCode('');
    }, [ selectedContestDetailsProblem ]);

    // in case of loading by url we need to have contest details set in state,
    // in order for breadcrumbs to load and work properly
    useEffect(() => {
        if (!contestDetails || contestDetails.id !== Number(contestId)) {
            const fetchAndSetContestDetails = async () => {
                const { data: contestDetailsData } = await getContestById({ id: Number(contestId) });
                dispatch(setContestDetails({ contest: contestDetailsData ?? null }));
            };

            fetchAndSetContestDetails();
        }
    }, [ contestDetails, contestId, getContestById, dispatch ]);

    // set dropdown data to the first element in the dropdown
    // instead of having the default empty one selected
    useEffect(() => {
        const previousSelectedStrategy = strategyDropdownItems?.find((strat) => strat.id === Number(selectedStrategyValue));
        if (strategyDropdownItems?.length && strategyDropdownItems?.length > 0) {
            if (!previousSelectedStrategy) {
                onStrategyDropdownItemSelect(strategyDropdownItems[0]);
            } else {
                onStrategyDropdownItemSelect(previousSelectedStrategy);
            }
        }
    }, [ strategyDropdownItems, onStrategyDropdownItemSelect, selectedStrategyValue ]);

    // fetching submissions only when we have selected problem,
    // otherwise the id is NaN and the query is invalid
    useEffect(() => {
        if (selectedContestDetailsProblem && isActiveParticipant && isRegisteredParticipant) {
            getSubmissionsData({
                id: Number(selectedContestDetailsProblem.id),
                page: selectedSubmissionsPage,
                isOfficial: isCompete,
            });
        }
        // rule is disabled because it requires adding fetchUserParticipationDetails to the
        // dependencies which makes it to call itself endlessly, which makes recursive
        // updates and crashes the application

        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [
        isActiveParticipant,
        isRegisteredParticipant,
        selectedContestDetailsProblem,
        participationType,
        getSubmissionsData,
        selectedSubmissionsPage,
        isCompete,
    ]);

    const onPopoverOpen = (event: React.MouseEvent<HTMLElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const onPopoverClose = () => {
        setAnchorEl(null);
    };

    const onSolutionSubmitCode = useCallback(async () => {
        setSolutionSubmitPreError(false);
        if (!submissionCode || submissionCode.length < 3) {
            setSolutionSubmitPreError(true);
            return;
        }
        try {
            await submitSolution({
                content: submissionCode!,
                official: isCompete,
                problemId: selectedContestDetailsProblem?.id!,
                submissionTypeId: selectedSubmissionType?.id!,
            });
            refetch();
            await getSubmissionsData({
                id: Number(selectedContestDetailsProblem!.id),
                page: selectedSubmissionsPage,
                isOfficial: isCompete,
            });
            setSubmissionCode('');
        } catch {
            setSubmissionCode('');
        }
    }, [
        getSubmissionsData,
        isCompete,
        refetch,
        selectedContestDetailsProblem,
        selectedSubmissionType?.id,
        selectedSubmissionsPage,
        submissionCode,
        submitSolution,
    ]);

    const onSolutionSubmitFile = useCallback(async () => {
        try {
            await submitSolutionFile({
                content: uploadedFile!,
                official: isCompete,
                problemId: selectedContestDetailsProblem?.id!,
                submissionTypeId: selectedSubmissionType?.id!,
            });
            refetch();
            await getSubmissionsData({
                id: Number(selectedContestDetailsProblem!.id),
                page: selectedSubmissionsPage,
                isOfficial: isCompete,
            });
            setUploadedFile(null);
        } catch {
            setUploadedFile(null);
        }
    }, [
        getSubmissionsData,
        isCompete,
        refetch,
        selectedContestDetailsProblem,
        selectedSubmissionType?.id,
        selectedSubmissionsPage,
        submitSolutionFile,
        uploadedFile,
    ]);

    const sumMyPoints = useMemo(() => contest
        ? contest.problems.reduce((accumulator, problem) => accumulator + problem.points, 0)
        : 0, [ contest ]);

    const sumAllContestPoints = useMemo(() => contest
        ? contest.problems.reduce((accumulator, problem) => accumulator + problem.maximumPoints, 0)
        : 0, [ contest ]);

    const renderProblemDescriptions = useCallback(() => {
        if (!selectedContestDetailsProblem) {
            return;
        }

        const { resources } = selectedContestDetailsProblem;

        // eslint-disable-next-line consistent-return
        return (
            <div className={styles.problemDescriptionsWrapper}>
                <div className={styles.problemDescriptions}>
                    { resources.map((resource: IProblemResourceType) => (
                        <ProblemResource
                          key={resource.id}
                          resource={resource}
                          problem={selectedContestDetailsProblem.name}
                        />
                    ))}
                </div>
                <div onMouseEnter={onPopoverOpen} onMouseLeave={onPopoverClose}>
                    <IoIosInformationCircleOutline />
                </div>
                <Popover
                  open={isModalOpen}
                  anchorEl={anchorEl}
                  anchorOrigin={{
                      vertical: 'bottom',
                      horizontal: 'left',
                  }}
                  transformOrigin={{
                      vertical: 'top',
                      horizontal: 'left',
                  }}
                  sx={{ pointerEvents: 'none' }}
                  onClose={onPopoverClose}
                  disableRestoreFocus
                >
                    <div className={`${styles.popoverContent} ${textColorClassName} ${lightBackgroundClassName}`}>
                        <div>
                            <span className={styles.title}>Allowed working time:</span>
                            {' '}
                            <span>{timeLimit}</span>
                            {' '}
                            sec
                        </div>
                        <div>
                            <span className={styles.title}>Allowed memory:</span>
                            {' '}
                            <span>{memoryLimit}</span>
                            {' '}
                            MB
                        </div>
                        <div>
                            <span className={styles.title}>Size limit:</span>
                            {' '}
                            <span>{fileSizeLimit}</span>
                            {' '}
                            KB
                        </div>
                        { checkerName && (
                            <div>
                                <span className={styles.title}>Checker:</span>
                                {' '}
                                {checkerName}
                            </div>
                        )}
                    </div>
                </Popover>
            </div>
        );
    }, [
        selectedContestDetailsProblem,
        anchorEl,
        isModalOpen,
        checkerName,
        fileSizeLimit,
        memoryLimit,
        timeLimit,
        lightBackgroundClassName,
        textColorClassName,
    ]);

    const renderRemainingTimeForContest = useCallback(() => {
        if (remainingTimeForCompete) {
            const leftMinutes = transformDaysHoursMinutesTextToMinutes(remainingTimeForCompete);
            return (
                <div className={leftMinutes <= 30
                    ? styles.errorText
                    : ''}
                >
                    Remaining time:
                    <b>
                        {remainingTimeForCompete}
                    </b>
                </div>
            );
        }

        // Contests without ending time
        if (!remainingTimeForCompete) {
            return (
                <div>
                    <b>No expire time</b>
                </div>
            );
        }

        return (
            <span className={styles.errorText}>
                Participation time has expired
            </span>
        );
    }, [ remainingTimeForCompete ]);

    const renderSubmissionsInput = useCallback(() => {
        const { allowBinaryFilesUpload, allowedFileExtensions } = selectedContestDetailsProblem?.allowedSubmissionTypes[0] || {};

        if (allowBinaryFilesUpload) {
            return (
                <div className={styles.fileUpload}>
                    <div>
                        <span>Allowed extensions:</span>
                        {' '}
                        {(allowedFileExtensions || []).join(', ')}
                    </div>
                    {fileUploadError && <div className={styles.fileUploadError}>{fileUploadError}</div>}
                    <FileUploader
                      file={uploadedFile}
                      problemId={selectedContestDetailsProblem?.id}
                      allowedFileExtensions={(allowedFileExtensions || [])}
                      onInvalidFileExtension={(e) => setFileUploadError(e.detail)}
                      onFileUpload={(file) => {
                          setFileUploadError('');
                          setUploadedFile(file);
                      }}
                    />
                    <div className={styles.remainingTimeNadSubmitButtonWrapper} style={{ height: 420 }}>
                        <Button
                          onClick={onSolutionSubmitFile}
                          text="Submit"
                          state={isSubmitButtonDisabled || submitSolutionFileIsLoading || contestTimeHasExpired
                              ? ButtonState.disabled
                              : ButtonState.enabled}
                        />
                        {remainingTime > 0 && (
                            <div className={styles.remainingTimeWrapper}>
                                {transformSecondsToTimeSpan(remainingTime)}
                                {' '}
                                until next submit
                            </div>
                        )}
                        {submitSolutionFileHasError && (
                            <div className={styles.solutionSubmitError}>
                                {(submitSolutionFileError as any).details || 'Error submitting solution. Please try again!'}
                            </div>
                        )}
                    </div>
                </div>
            );
        }

        return (
            <div>
                <CodeEditor
                  selectedSubmissionType={selectedSubmissionType}
                  code={submissionCode}
                  onCodeChange={(inputCode) => setSubmissionCode(inputCode)}
                />
                <div className={styles.submitSettings}>
                    <Dropdown
                      dropdownItems={strategyDropdownItems || []}
                      value={selectedStrategyValue}
                      handleDropdownItemClick={onStrategyDropdownItemSelect}
                    />
                    <div className={styles.remainingTimeNadSubmitButtonWrapper}>
                        <Button
                          state={isSubmitButtonDisabled || submitSolutionIsLoading || contestTimeHasExpired
                              ? ButtonState.disabled
                              : ButtonState.enabled}
                          onClick={onSolutionSubmitCode}
                          text="Submit"
                        />
                        {remainingTime > 0 && (
                            <div className={styles.remainingTimeWrapper}>
                                {transformSecondsToTimeSpan(remainingTime)}
                                {' '}
                                until next submit
                            </div>
                        )}
                    </div>
                </div>
                {(submitSolutionHasError || solutionSubmitPreError) && (
                    <div className={styles.solutionSubmitError}>
                        {(submitSolutionError as any).details || 'Error submitting solution. Please try again!'}
                    </div>
                )}
            </div>
        );
    }, [
        submitSolutionFileIsLoading,
        uploadedFile,
        submitSolutionHasError,
        isSubmitButtonDisabled,
        submitSolutionIsLoading,
        contestTimeHasExpired,
        remainingTime,
        selectedStrategyValue,
        strategyDropdownItems,
        submissionCode,
        selectedSubmissionType,
        fileUploadError,
        submitSolutionFileError,
        selectedContestDetailsProblem,
        onSolutionSubmitCode,
        submitSolutionError,
        submitSolutionFileHasError,
        solutionSubmitPreError,
        onSolutionSubmitFile,
        setSubmissionCode,
        setUploadedFile,
        setFileUploadError,
        onStrategyDropdownItemSelect,
    ]);

    if (isLoading) {
        return <div style={{ ...flexCenterObjectStyles }}><SpinningLoader /></div>;
    }

    if (error) {
        return (
            <div className={styles.contestSolutionSubmitWrapper}>
                <div className={textColorClassName}>Error fetching user participation data!</div>
            </div>
        );
    }

    if ((isRegisteredParticipant && !isActiveParticipant) || contestTimeHasExpired) {
        return (
            <ErrorWithActionButtons
              message="Access to this contest has expired!"
              backToText="Back to contests"
              backToUrl="/contests"
            />
        );
    }

    return (
        <div className={`${styles.contestSolutionSubmitWrapper} ${textColorClassName}`}>
            <ContestBreadcrumbs />
            <div className={styles.nameWrapper}>
                <Link to={`/contests/${contest?.id}`} className={`${styles.title} ${textColorClassName}`}>{contest?.name}</Link>
                <div
                  className={styles.allResultsLink}
                  onClick={() => navigate(`/contests/${contest?.id}/${participationType}/results/simple`)}
                >
                    Show all results
                </div>
            </div>
            { user.canAccessAdministration && (
                <div className={styles.administrationButtonWrapper}>
                    <LinkButton
                      to={`/administration-new/contests/${contestId}`}
                      isToExternal
                      text="Open in administration"
                    />
                </div>
            ) }
            <div className={styles.problemsAndEditorWrapper}>
                <ContestProblems
                  problems={problems || []}
                  onContestProblemChange={() => setSelectedSubmissionsPage(1)}
                  totalParticipantsCount={participantsCount}
                  sumMyPoints={sumMyPoints}
                  sumTotalPoints={sumAllContestPoints}
                />
                <div className={styles.selectedProblemWrapper}>
                    <div className={styles.problemNameAndTimeWrapper}>
                        <div className={styles.problemName}>
                            {selectedContestDetailsProblem?.name}
                            {selectedContestDetailsProblem?.isExcludedFromHomework && (
                                <span className={textColorClassName}>(not included in final score)</span>)}
                        </div>
                        {renderRemainingTimeForContest()}
                    </div>
                    {renderProblemDescriptions()}
                    {renderSubmissionsInput()}
                </div>
            </div>
            <div className={styles.submissionsWrapper}>
                <div className={styles.submissionsTitleWrapper}>
                    <span>Submissions</span>
                    <IoMdRefresh onClick={() => getSubmissionsData({
                        id: Number(selectedContestDetailsProblem!.id),
                        page: selectedSubmissionsPage,
                        isOfficial: isCompete,
                    })}
                    />
                </div>
                { submissionsError
                    ? <>Error loading submissions</>
                    : (
                        <SubmissionsGrid
                          isDataLoaded={!submissionsDataLoading}
                          submissions={submissionsData ?? undefined}
                          handlePageChange={(page: number) => setSelectedSubmissionsPage(page)}
                          options={{
                              showDetailedResults: true,
                              showTaskDetails: false,
                              showCompeteMarker: true,
                              showSubmissionTypeInfo: true,
                              showParticipantUsername: false,
                          }}
                        />
                    )}
            </div>
        </div>
    );
};

export default setLayout(ContestSolutionSubmitPage);
