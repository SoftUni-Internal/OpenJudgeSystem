import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router';
import { Link, useNavigate } from 'react-router-dom';

import CodeEditor from '../../../components/code-editor/CodeEditor';
import ContestBreadcrumbs from '../../../components/contests/contest-breadcrumbs/ContestBreadcrumbs';
import Button from '../../../components/guidelines/buttons/Button';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import SubmissionTestRun from '../../../components/submissions/submission-test-run/SubmissionTestRun';
import SubmissionTestRuns from '../../../components/submissions/submission-test-runs/SubmissionTestRuns';
import { ITestRunType } from '../../../hooks/submissions/types';
import useTheme from '../../../hooks/use-theme';
import { setContestDetails } from '../../../redux/features/contestsSlice';
import { useLazyGetContestByIdQuery } from '../../../redux/services/contestsService';
import {
    useGetSubmissionDetailsQuery,
    useLazyGetSubmissionUploadedFileQuery,
    useLazyRetestSubmissionQuery,
} from '../../../redux/services/submissionsService';
import { useAppDispatch, useAppSelector } from '../../../redux/store';
import { preciseFormatDate } from '../../../utils/dates';
import downloadFile from '../../../utils/file-download-utils';
import { flexCenterObjectStyles } from '../../../utils/object-utils';

import styles from './SubmissionsDetailsPage.module.scss';

const SubmissionDetailsPage = () => {
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const { submissionId } = useParams();
    const { themeColors, getColorClassName } = useTheme();

    const { internalUser: user } = useAppSelector((state) => state.authorization);
    const { contestDetails } = useAppSelector((state) => state.contests);

    const textColorClassName = getColorClassName(themeColors.textColor);

    const [ downloadSolutionErrorMessage, setDownloadSolutionErrorMessage ] = useState<string>('');
    const { data, isLoading, error } = useGetSubmissionDetailsQuery({ id: Number(submissionId) });
    const [ getContestById ] = useLazyGetContestByIdQuery();
    const [ downloadUploadedFile ] = useLazyGetSubmissionUploadedFileQuery();
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
        if (!data?.contestId) {
            return;
        }
        if (!contestDetails || contestDetails?.id !== data?.contestId) {
            const fetchContestById = async () => {
                const { data: contestData } = await getContestById({ id: data?.contestId });
                dispatch(setContestDetails({ contest: contestData || null }));
            };
            fetchContestById();
        }
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [ data?.contestId ]);

    const { name } = contestDetails || {};
    const {
        id: solutionId,
        isProcessed,
        contestId,
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
        userIsInRoleForContest,
    } = data || {};

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
            <Link to={`/contests/${contestId}/practice#${problem?.id}`}>{problem?.name}</Link>
        </div>
    ), [ solutionId, contestUser?.userName, problem?.name, problem?.id, contestId ]);

    const renderSolutionDetails = useCallback(() => {
        const { allowBinaryFilesUpload } = submissionType || {};

        return (
            <>
                <div className={styles.submissionDetailsWrapper}>
                    <div>
                        {allowBinaryFilesUpload && (
                            <div className={styles.buttonWrapper}>
                                <Button id="download-binary-file" text="Download binary file" onClick={handleDownloadFile} />
                            </div>
                        )}
                        <div className={styles.detailsRow}>
                            <span>
                                Submitted on:
                            </span>
                            <span>{preciseFormatDate(createdOn!)}</span>
                        </div>

                        { user.canAccessAdministration && (
                            <>
                                <div className={styles.detailsRow}>
                                    <span>Modified on:</span>
                                    { modifiedOn && <span>{preciseFormatDate(modifiedOn)}</span> }
                                </div>
                                <div className={styles.detailsRow}>
                                    <span>Started Execution on:</span>
                                    { startedExecutionOn && <span>{preciseFormatDate(startedExecutionOn)}</span> }
                                </div>
                                <div className={styles.detailsRow}>
                                    <span>Completed Execution on:</span>
                                    { completedExecutionOn && <span>{preciseFormatDate(completedExecutionOn)}</span> }
                                </div>
                            </>
                        )}
                    </div>
                    <div>
                        <div className={styles.strategyWrapper}>
                            <span style={{
                                border: `2px solid ${themeColors.baseColor100}`,
                                color: `${themeColors.baseColor100}`,
                            }}
                            >
                                {submissionType?.name}
                            </span>
                        </div>
                    </div>
                </div>
                {downloadSolutionErrorMessage &&
                    (<div className={styles.solutionDownloadFileErrorWrapper}>{downloadSolutionErrorMessage}</div>)}
            </>
        );
    }, [
        user.canAccessAdministration,
        submissionType,
        createdOn,
        downloadSolutionErrorMessage,
        handleDownloadFile,
        completedExecutionOn,
        modifiedOn,
        startedExecutionOn,
        themeColors.baseColor100,
    ]);

    const renderSolutionTestDetails = useCallback(() => {
        if (!isProcessed) {
            return (
                <div className={`${styles.submissionInQueueWrapper} ${textColorClassName}`}>
                    The submission is in queue and will be processed shortly. Please wait.
                </div>
            );
        }

        if (!isCompiledSuccessfully) {
            return (
                <div className={`${styles.compileTimeErrorWrapper} ${textColorClassName}`}>
                    <div>A compile time error occurred:</div>
                    { compilerComment && <div>{compilerComment}</div>}
                </div>
            );
        }

        if (isEligibleForRetest) {
            return (
                <div className={`${styles.retestWrapper} ${textColorClassName}`}>
                    <div>
                        The input/ output data changed. Your (
                        {points}
                        /100) submission is now outdated. Click &quot;Restart&quot; to resubmit
                        your solution for re-evaluation against the new test cases. Your score may change.
                    </div>
                    <Button text="RETEST" onClick={() => retestSubmission({ id: solutionId! })} />
                </div>
            );
        }

        const sortByTrialTest = (a: ITestRunType, b: ITestRunType) => {
            if (a.isTrialTest && !b.isTrialTest) {
                return -1;
            }
            if (!a.isTrialTest && b.isTrialTest) {
                return 1;
            }
            return 0;
        };

        const sortedTestRuns = [ ...testRuns || [] ]?.sort(sortByTrialTest);

        return sortedTestRuns.map((testRun: ITestRunType, idx: number) => (
            <SubmissionTestRun
              testRun={testRun}
              idx={idx + 1}
              shouldRenderAdminData={userIsInRoleForContest}
            />
        ));
    }, [
        isCompiledSuccessfully,
        isEligibleForRetest,
        points,
        testRuns,
        compilerComment,
        retestSubmission,
        solutionId,
        isProcessed,
        textColorClassName,
        userIsInRoleForContest,
    ]);

    const renderAdminButtons = useCallback(() => {
        const onViewCodeClick = () => {
            const scrollToElement =
                document.querySelector('#code-content-wrapper') ||
                document.querySelector('#download-binary-file');

            if (!scrollToElement) { return; }

            const yCoordinate = scrollToElement.getBoundingClientRect().top + window.scrollY;
            window.scrollTo({ top: yCoordinate, behavior: 'smooth' });
        };

        const goToAdministrationForContest = () => navigate(`/administration-new/contests/${contestId}`);

        return (
            <div className={styles.adminButtonsWrapper}>
                <Button text="View Code" onClick={onViewCodeClick} />
                { userIsInRoleForContest && (
                    <>
                        <Button text="OPEN IN ADMINISTRATION" onClick={goToAdministrationForContest} />
                        <Button text="Retest" onClick={() => retestSubmission({ id: solutionId! })} />
                    </>
                )}
            </div>
        );
    }, [ contestId, navigate, retestSubmission, solutionId, userIsInRoleForContest ]);

    if (isLoading || retestIsLoading) {
        return (
            <div style={{ ...flexCenterObjectStyles }}>
                <SpinningLoader />
            </div>
        );
    }
    if (error || retestError) {
        return (
            <div>
                { retestError
                    ? 'Error retesting solution. Please try again!'
                    : 'Error fetching submission data!' }
            </div>
        );
    }
    return (
        <div className={`${styles.submissionsDetailsWrapper} ${textColorClassName}`}>
            { retestSuccess && (
                <div className={styles.succesfulRetestWrapper}>
                    Submission has been retested successfully, reload page to refresh results.
                </div>
            )}
            <ContestBreadcrumbs />
            <div>
                <div className={styles.submissionTitle}>
                    {renderSolutionTitle()}
                </div>
                <div className={styles.bodyWrapper}>
                    <SubmissionTestRuns testRuns={testRuns || []} />
                    <div className={styles.innerBodyWrapper}>
                        <Link to={`/contests/${contestId}`}>{name}</Link>
                        {renderAdminButtons()}
                        {renderSolutionTestDetails()}
                        {!isEligibleForRetest && content && (
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

export default SubmissionDetailsPage;
