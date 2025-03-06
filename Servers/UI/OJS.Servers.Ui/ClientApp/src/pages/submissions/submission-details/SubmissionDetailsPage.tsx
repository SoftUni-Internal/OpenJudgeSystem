import { useCallback, useEffect, useMemo, useState } from 'react';
import { useParams } from 'react-router';
import { Link } from 'react-router-dom';
import CheckBox from 'src/components/guidelines/checkbox/CheckBox';
import Mentor from 'src/components/mentor/Mentor';

import { sortTestRunsByTrialTest } from '../../../common/submissions-utils';
import { getContestsDetailsPageUrl, getContestsSolutionSubmitPageUrl } from '../../../common/urls/compose-client-urls';
import CodeEditor from '../../../components/code-editor/CodeEditor';
import MultiLineTextDisplay from '../../../components/common/MultiLineTextDisplay';
import ContestBreadcrumbs from '../../../components/contests/contest-breadcrumbs/ContestBreadcrumbs';
import ErrorWithActionButtons from '../../../components/error/ErrorWithActionButtons';
import AdministrationLink from '../../../components/guidelines/buttons/AdministrationLink';
import Button, { ButtonSize, ButtonType } from '../../../components/guidelines/buttons/Button';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import SubmissionTestRun from '../../../components/submissions/submission-test-run/SubmissionTestRun';
import SubmissionTestRuns from '../../../components/submissions/submission-test-runs/SubmissionTestRuns';
import { ITestRunType } from '../../../hooks/submissions/types';
import useTheme from '../../../hooks/use-theme';
import { setContestDetailsIdAndCategoryId } from '../../../redux/features/contestsSlice';
import {
    useGetSubmissionDetailsQuery,
    useLazyGetSubmissionLogFileQuery,
    useLazyGetSubmissionUploadedFileQuery,
    useLazyRetestSubmissionQuery,
} from '../../../redux/services/submissionsService';
import { useAppDispatch, useAppSelector } from '../../../redux/store';
import concatClassNames from '../../../utils/class-names';
import { preciseFormatDate } from '../../../utils/dates';
import downloadFile from '../../../utils/file-download-utils';
import { getErrorMessage } from '../../../utils/http-utils';
import { flexCenterObjectStyles } from '../../../utils/object-utils';
import makePrivate from '../../shared/make-private';
import setLayout from '../../shared/set-layout';
import withTitle from '../../shared/with-title';

import styles from './SubmissionsDetailsPage.module.scss';

const SubmissionDetailsPage = () => {
    const dispatch = useAppDispatch();
    const { submissionId } = useParams();
    const { themeColors, getColorClassName, isDarkMode } = useTheme();
    const [ isRetestingStarted, setIsRetestingStarted ] = useState(false);
    const [ retestVerbosely, setRetestVerbosely ] = useState(false);

    const { internalUser: user } = useAppSelector((state) => state.authorization);
    const { contestDetails, breadcrumbItems } = useAppSelector((state) => state.contests);
    const textColorClassName = getColorClassName(themeColors.textColor);

    const [ downloadSolutionErrorMessage, setDownloadSolutionErrorMessage ] = useState<string>('');
    const { data, isLoading, error } = useGetSubmissionDetailsQuery({ id: Number(submissionId) });
    const [ downloadUploadedFile ] = useLazyGetSubmissionUploadedFileQuery();
    const [
        downloadLogFile,
        {
            isError: isDownloadLogsError,
            error: downloadLogsError,
        } ] = useLazyGetSubmissionLogFileQuery();
    const [
        retestSubmission,
        {
            isLoading: retestIsLoading,
            isError: retestError,
            isSuccess: retestSuccess,
        },
    ] = useLazyRetestSubmissionQuery();

    // fetch submission details if not present (when opened from url directly)
    // in order to load breadcrumbs and name of contest properly
    useEffect(() => {
        if (!data?.contestId || !data?.contestName) {
            return;
        }

        if (!contestDetails || contestDetails?.id !== data?.contestId) {
            dispatch(setContestDetailsIdAndCategoryId({
                id: data.contestId,
                name: data.contestName,
                categoryId: data.contestCategoryId,
            }));
        }
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [ data?.contestId ]);

    const {
        id: solutionId,
        isProcessed,
        contestId,
        contestName,
        problem,
        user: contestUser,
        content,
        points,
        submissionType,
        testRuns,
        createdOn,
        isCompiledSuccessfully,
        compilerComment,
        isEligibleForRetest,
        modifiedOn,
        startedExecutionOn,
        completedExecutionOn,
        workerName,
        userIsInRoleForContest,
        isOfficial,
        maxPoints,
        processingComment,
        allowMentor,
    } = data || {};

    const categoryName = useMemo(() => breadcrumbItems.at(-1)?.name ?? undefined, [ breadcrumbItems ]);

    const handleRetestSubmission = useCallback(() => {
        setIsRetestingStarted(true);
        retestSubmission({ id: solutionId!, verbosely: retestVerbosely });
    }, [ retestSubmission, solutionId, retestVerbosely ]);

    const handleDownloadFile = useCallback(async () => {
        try {
            setDownloadSolutionErrorMessage('');
            const response = await downloadUploadedFile({ id: solutionId! });

            if (response.data) {
                downloadFile(response.data.blob, `submission-${solutionId}.zip`);
            }
        } catch {
            setDownloadSolutionErrorMessage('Error download solution submitted file!');
        }
    }, [ downloadUploadedFile, solutionId ]);

    const handleDownloadLogsFile = useCallback(async () => {
        const response = await downloadLogFile({ id: solutionId! });

        if (response.data) {
            downloadFile(response.data.blob, `submission-${solutionId}-log.txt`);
        }
    }, [ downloadLogFile, solutionId ]);

    const renderSolutionTitle = useCallback(() => (
        <div className={styles.solutionTitle}>
            Solution #
            {solutionId}
            {' '}
            by
            {' '}
            <Link to={`/profile/${contestUser?.userName}`}>
                {contestUser?.userName}
            </Link>
            {' '}
            for problem
            {' '}
            <Link to={getContestsSolutionSubmitPageUrl({
                isCompete: isOfficial,
                contestId,
                contestName,
                problemId: problem?.id,
                orderBy: problem?.orderBy,
            })}
            >
                {problem?.name}
            </Link>
        </div>
    ), [ solutionId, contestUser?.userName, contestName, contestId, isOfficial, problem?.id, problem?.name, problem?.orderBy ]);

    const renderAdminButtons = useCallback(() => {
        const onViewCodeClick = () => {
            const scrollToElement =
                document.querySelector('#code-content-wrapper') ||
                document.querySelector('#download-submission');

            if (!scrollToElement) { return; }

            const yCoordinate = scrollToElement.getBoundingClientRect().top + window.scrollY;
            window.scrollTo({ top: yCoordinate, behavior: 'smooth' });
        };

        return (
            <div className={styles.adminButtonsWrapper}>
                <Button text="View Code" type={ButtonType.secondary} size={ButtonSize.small} onClick={onViewCodeClick} />
                { userIsInRoleForContest && (
                    <>
                        <Button
                          text="Download logs"
                          type={ButtonType.secondary}
                          size={ButtonSize.small}
                          onClick={handleDownloadLogsFile}
                        />
                        <AdministrationLink
                          text="Open In Administration"
                          to={`/submissions?filter=id~equals~${solutionId}`}
                        />
                        <AdministrationLink
                          text="Tests"
                          to={`/problems/${
                                problem!.id
                          }#tab-tests`}
                        />
                        <Button
                          text="Retest"
                          size={ButtonSize.small}
                          type={ButtonType.secondary}
                          onClick={() => handleRetestSubmission()}
                        />
                        <CheckBox id="retest-verbosely-checkbox" onChange={setRetestVerbosely} label="Retest verbosely" />
                        {isDownloadLogsError &&
                            (<div className={styles.logsDownloadFileErrorWrapper}>{getErrorMessage(downloadLogsError)}</div>)}
                    </>
                )}
            </div>
        );
    }, [
        handleRetestSubmission,
        problem,
        solutionId,
        userIsInRoleForContest,
        handleDownloadLogsFile,
        isDownloadLogsError,
        downloadLogsError,
    ]);

    const renderSubmissionExecutionDetails = useCallback(() => {
        if (!isProcessed || isRetestingStarted) {
            return (
                <div className={`${styles.submissionInQueueWrapper}`}>
                    The submission is in queue and will be processed shortly. Please wait.
                </div>
            );
        }

        const compileTimeErrorClasses = concatClassNames(
            styles.compileTimeErrorWrapper,
            getColorClassName(themeColors.baseColor200),
            textColorClassName,
            textColorClassName,
            isDarkMode
                ? styles.darkTheme
                : '',
        );

        if (!isCompiledSuccessfully) {
            return (
                <div className={compileTimeErrorClasses}>
                    <div>A compile time error occurred:</div>
                    <MultiLineTextDisplay text={compilerComment} maxVisibleLines={50} />
                </div>
            );
        }

        if (isEligibleForRetest) {
            return (
                <div className={`${styles.retestWrapper}`}>
                    <div>
                        The input / output data changed. Your (
                        {points}
                        {' '}
                        /
                        {' '}
                        {maxPoints}
                        ) submission is now outdated. Click &quot;Retest&quot; to resubmit
                        your solution for re-evaluation against the new test cases. Your score may change.
                    </div>
                    <Button text="RETEST" onClick={() => handleRetestSubmission()} />
                </div>
            );
        }

        const renderTestRun = (testRun: ITestRunType, idx: number) => (
            <SubmissionTestRun
              key={`t-r-${testRun.testId}`}
              testRun={testRun}
              idx={idx}
              shouldRenderAdminData={userIsInRoleForContest}
            />
        );

        const sortedTestRuns = [ ...testRuns || [] ]?.sort(sortTestRunsByTrialTest);

        const firstCompeteTestIndex = sortedTestRuns.findIndex((tr) => !tr.isTrialTest);

        const renderTrialTestRuns = () => sortedTestRuns?.slice(0, firstCompeteTestIndex)
            .map((testRun: ITestRunType, idx: number) => renderTestRun(testRun, idx + 1));

        const renderCompeteTestRuns = () => sortedTestRuns?.slice(firstCompeteTestIndex, sortedTestRuns?.length)
            .map((testRun: ITestRunType, idx: number) => renderTestRun(testRun, idx + 1));

        return (
            <>
                {renderTrialTestRuns()}
                {renderCompeteTestRuns()}
            </>
        );
    }, [
        isProcessed,
        isRetestingStarted,
        testRuns,
        isCompiledSuccessfully,
        isEligibleForRetest,
        textColorClassName,
        isDarkMode,
        getColorClassName,
        themeColors,
        compilerComment,
        points,
        maxPoints,
        handleRetestSubmission,
        userIsInRoleForContest,
    ]);

    const renderSolutionDetails = useCallback(() => {
        const { allowBinaryFilesUpload } = submissionType || {};
        /*
            The data will not be loaded when the submission is being retested.
            -> '!isRetestingStarted' - the submission's retesting has not been started,
                the data should be loaded.
            -> 'testRuns && testRuns.length !== 0' - if the tests' count is not 0, then the
                submission is not being retested, the data should be loaded.
            -> '!isCompiledSuccessfully' - if the submission is not compiled successfully,
                the data should be loaded.
         */
        const shouldLoadData = !isRetestingStarted && ((testRuns && testRuns.length !== 0) || !isCompiledSuccessfully);

        return (
            <>
                <div className={styles.submissionDetailsWrapper}>
                    <div>
                        {allowBinaryFilesUpload && (
                            <div className={styles.buttonWrapper}>
                                <Button
                                  id="download-submission"
                                  text="Download solution"
                                  onClick={handleDownloadFile}
                                />
                            </div>
                        )}
                        <div className={styles.detailsRow}>
                            <span>Strategy: </span>
                            {submissionType?.name}
                        </div>
                        <div className={styles.detailsRow}>
                            <span>
                                Submitted on:
                            </span>
                            <span>{preciseFormatDate(createdOn!)}</span>
                        </div>

                        {user.canAccessAdministration && (
                            <>
                                <div className={styles.detailsRow}>
                                    <span>Modified on:</span>
                                    {modifiedOn && shouldLoadData && <span>{preciseFormatDate(modifiedOn)}</span>}
                                </div>
                                <div className={styles.detailsRow}>
                                    <span>Started Execution on:</span>
                                    {startedExecutionOn && shouldLoadData && <span>{preciseFormatDate(startedExecutionOn)}</span>}
                                </div>
                                <div className={styles.detailsRow}>
                                    <span>Completed Execution on:</span>
                                    {completedExecutionOn && shouldLoadData && <span>{preciseFormatDate(completedExecutionOn)}</span>}
                                </div>
                                <div className={styles.detailsRow}>
                                    <span>Worker Executed on:</span>
                                    <span>{workerName}</span>
                                </div>
                            </>
                        )}
                    </div>
                </div>
                {downloadSolutionErrorMessage &&
                    (<div className={styles.solutionDownloadFileErrorWrapper}>{downloadSolutionErrorMessage}</div>)}
            </>
        );
    }, [
        isRetestingStarted,
        testRuns,
        user.canAccessAdministration,
        submissionType,
        createdOn,
        downloadSolutionErrorMessage,
        handleDownloadFile,
        completedExecutionOn,
        modifiedOn,
        startedExecutionOn,
        isCompiledSuccessfully,
        workerName,
    ]);

    const renderProcessingComment = useCallback(() => (
        <div className={concatClassNames(
            styles.processingErrorWrapper,
            textColorClassName,
            isDarkMode
                ? styles.darkTheme
                : '',
        )}
        >
            <div>A processing error occurred:</div>
            <MultiLineTextDisplay text={processingComment} maxVisibleLines={50} />
        </div>
    ), [ isDarkMode, processingComment, textColorClassName ]);

    if (isLoading || retestIsLoading) {
        return (
            <div style={{ ...flexCenterObjectStyles }}>
                <SpinningLoader />
            </div>
        );
    }

    if (retestError) {
        return (
            <ErrorWithActionButtons
              message="Error retesting solution. Please try again!"
              backToUrl={`/submissions/${submissionId}/details`}
              backToText="Back to submission"
            />
        );
    }

    if (error) {
        return (
            <ErrorWithActionButtons
              message={getErrorMessage(error)}
              backToUrl="/submissions"
              backToText="Back to submissions"
            />
        );
    }

    return (
        <div className={`${styles.submissionsDetailsWrapper} ${textColorClassName}`}>
            { isRetestingStarted && retestSuccess && (
                <div className={styles.succesfulRetestWrapper}>
                    Submission retest started!
                </div>
            )}
            <ContestBreadcrumbs />
            <Mentor
              problemId={problem?.id}
              problemName={problem?.name}
              contestId={contestId}
              contestName={contestName ?? undefined}
              categoryName={categoryName}
              submissionTypeName={submissionType?.name}
              isMentorAllowed={allowMentor ?? false}
            />
            <div>
                <div className={styles.submissionTitle}>
                    {renderSolutionTitle()}
                </div>
                <div className={styles.bodyWrapper}>
                    <SubmissionTestRuns testRuns={(!isRetestingStarted
                        ? testRuns
                        : []) || []}
                    />
                    <div className={styles.innerBodyWrapper}>
                        <div className={styles.contestTitle}>
                            <Link
                              to={getContestsDetailsPageUrl({ contestId, contestName })}
                            >
                                {contestName}
                            </Link>
                            <div className={styles.pointsResult}>
                                {points}
                                /
                                {maxPoints}
                            </div>
                        </div>
                        {renderAdminButtons()}
                        {processingComment && user.isAdmin && renderProcessingComment()}
                        {renderSubmissionExecutionDetails()}
                        {content && (
                            <div className={styles.codeContentWrapper} id="code-content-wrapper">
                                <div>Source Code</div>
                                <CodeEditor code={content} readOnly />
                            </div>
                        )}
                        {renderSolutionDetails()}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default makePrivate(setLayout(withTitle(SubmissionDetailsPage, (params) => `Submission #${params.submissionId}`)));
