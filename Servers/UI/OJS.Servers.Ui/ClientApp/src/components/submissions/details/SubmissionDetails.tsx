import React, { useCallback, useEffect, useMemo } from 'react';
import isNil from 'lodash/isNil';

import { useSubmissionsDetails } from '../../../hooks/submissions/use-submissions-details';
import { useAppUrls } from '../../../hooks/use-app-urls';
import { useAuth } from '../../../hooks/use-auth';
import { usePageTitles } from '../../../hooks/use-page-titles';
import concatClassNames from '../../../utils/class-names';
import { preciseFormatDate } from '../../../utils/dates';
import CodeEditor from '../../code-editor/CodeEditor';
import { ButtonSize, LinkButton, LinkButtonType } from '../../guidelines/buttons/Button';
import Heading, { HeadingType } from '../../guidelines/headings/Heading';
import SubmissionResults from '../submission-results/SubmissionResults';
import RefreshableSubmissionsList from '../submissions-list/RefreshableSubmissionsList';

import styles from './SubmissionDetails.module.scss';

const SubmissionDetails = () => {
    const {
        state: {
            currentSubmission,
            currentProblemSubmissionResults,
            validationResult,
        },
        actions: { getSubmissionResults },
    } = useSubmissionsDetails();
    const { actions: { setPageTitle } } = usePageTitles();
    const { state: { user: { permissions: { canAccessAdministration } } } } = useAuth();
    const { getAdministrationRetestSubmissionInternalUrl } = useAppUrls();

    const submissionTitle = useMemo(
        () => `Submission №${currentSubmission?.id}`,
        [ currentSubmission?.id ],
    );

    useEffect(() => {
        setPageTitle(submissionTitle);
    }, [ setPageTitle, submissionTitle ]);

    const problemNameHeadingText = useMemo(
        () => `${currentSubmission?.problem.name} - ${currentSubmission?.problem.id}`,
        [ currentSubmission?.problem.id, currentSubmission?.problem.name ],
    );

    const detailsHeadingText = useMemo(
        () => `Details #${currentSubmission?.id}`,
        [ currentSubmission?.id ],
    );

    const { submissionType } = currentSubmission || {};

    const submissionsNavigationClassName = 'submissionsNavigation';

    const submissionsDetails = 'submissionDetails';
    const submissionDetailsClassName = concatClassNames(styles.navigation, styles.submissionDetails, submissionsDetails);

    useEffect(() => {
        if (isNil(currentSubmission)) {
            return;
        }

        const { problem: { id: problemId }, isOfficial, user: { id: userId } } = currentSubmission;

        (async () => {
            await getSubmissionResults(problemId, isOfficial, userId);
        })();
    }, [ currentSubmission, getSubmissionResults ]);

    const renderRetestButton = useCallback(
        () => {
            if (!canAccessAdministration) {
                return null;
            }

            return (
                <LinkButton
                  type={LinkButtonType.secondary}
                  size={ButtonSize.medium}
                  to={getAdministrationRetestSubmissionInternalUrl()}
                  text="Retest"
                  className={styles.retestButton}
                />
            );
        },
        [ canAccessAdministration, getAdministrationRetestSubmissionInternalUrl ],
    );

    const renderSubmissionInfo = useCallback(
        () => {
            if (!canAccessAdministration || isNil(currentSubmission)) {
                return null;
            }

            const { createdOn, modifiedOn, user: { userName } } = currentSubmission;

            return (
                <div className={styles.submissionInfo}>
                    <p className={styles.submissionInfoParagraph}>
                        Created on:
                        {' '}
                        {preciseFormatDate(createdOn)}
                    </p>
                    <p className={styles.submissionInfoParagraph}>
                        Modified on:
                        {' '}
                        {isNil(modifiedOn)
                            ? 'never'
                            : preciseFormatDate(modifiedOn)}
                    </p>
                    <p className={styles.submissionInfoParagraph}>
                        Username:
                        {' '}
                        {userName}
                    </p>
                </div>
            );
        },
        [ currentSubmission, canAccessAdministration ],
    );

    const renderSubmissionDetails = useCallback(
        () => (
            <div className={styles.detailsWrapper}>
                <div className={styles.navigation}>
                    <div className={submissionsNavigationClassName}>
                        <Heading type={HeadingType.secondary}>Submissions</Heading>
                    </div>
                    <RefreshableSubmissionsList
                      items={currentProblemSubmissionResults}
                      selectedSubmission={currentSubmission}
                      className={styles.submissionsList}
                    />
                    { renderRetestButton() }
                    { renderSubmissionInfo() }
                </div>
                <div className={styles.code}>
                    <Heading
                      type={HeadingType.secondary}
                      className={styles.taskHeading}
                    >
                        {problemNameHeadingText}
                    </Heading>
                    <CodeEditor
                      readOnly
                      code={currentSubmission?.content}
                      selectedSubmissionType={submissionType}
                    />
                </div>
                <div className={submissionDetailsClassName}>
                    <Heading type={HeadingType.secondary}>{detailsHeadingText}</Heading>
                    {isNil(currentSubmission)
                        ? ''
                        : (
                            <SubmissionResults
                              testRuns={currentSubmission.testRuns}
                              compilerComment={currentSubmission?.compilerComment}
                              isCompiledSuccessfully={currentSubmission?.isCompiledSuccessfully}
                            />
                        )}

                </div>
            </div>
        ),
        [ currentProblemSubmissionResults,
            currentSubmission,
            detailsHeadingText,
            problemNameHeadingText,
            renderRetestButton,
            renderSubmissionInfo,
            submissionDetailsClassName,
            submissionType ],
    );

    const renderErrorMessage = useCallback(() => (
        <div className={styles.errorMessage}>
            {validationResult.message}
        </div>
    ), [ validationResult ]);

    const renderPage = useCallback(
        () => isNil(validationResult)
            ? <div>Loading data</div>
            : validationResult.isValid
                ? renderSubmissionDetails()
                : renderErrorMessage(),
        [ renderErrorMessage, renderSubmissionDetails, validationResult ],
    );

    if (isNil(currentSubmission)) {
        return <div>No details fetched.</div>;
    }

    return renderPage();
};

export default SubmissionDetails;
