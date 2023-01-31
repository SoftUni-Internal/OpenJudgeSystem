import React, { useCallback, useEffect, useMemo } from 'react';
import isNil from 'lodash/isNil';

import { IRegisterForContestTypeUrlParams } from '../../../common/app-url-types';
import { ContestParticipationType } from '../../../common/constants';
import { useSubmissionsDetails } from '../../../hooks/submissions/use-submissions-details';
import { useAppUrls } from '../../../hooks/use-app-urls';
import { useAuth } from '../../../hooks/use-auth';
import { useContests } from '../../../hooks/use-contests';
import { usePageTitles } from '../../../hooks/use-page-titles';
import concatClassNames from '../../../utils/class-names';
import { preciseFormatDate } from '../../../utils/dates';
import CodeEditor from '../../code-editor/CodeEditor';
import { ButtonSize, ButtonState, LinkButton, LinkButtonType } from '../../guidelines/buttons/Button';
import Heading, { HeadingType } from '../../guidelines/headings/Heading';
import IconSize from '../../guidelines/icons/common/icon-sizes';
import LeftArrowIcon from '../../guidelines/icons/LeftArrowIcon';
import SubmissionResults from '../submission-results/SubmissionResults';
import RefreshableSubmissionsList from '../submissions-list/RefreshableSubmissionsList';

import styles from './SubmissionDetails.module.scss';

const SubmissionDetails = () => {
    const {
        state: {
            currentSubmission,
            currentProblemSubmissionResults,
        },
        actions: { getSubmissionResults },
    } = useSubmissionsDetails();
    const { actions: { setPageTitle } } = usePageTitles();
    const { state: { user: { permissions: { canAccessAdministration } } } } = useAuth();
    const { getAdministrationRetestSubmissionInternalUrl } = useAppUrls();
    const {
        state: { contest },
        actions: { loadContestByProblemId },
    } = useContests();

    const { getRegisterContestTypeUrl } = useAppUrls();

    useEffect(() => {
        if (isNil(currentSubmission)) {
            return;
        }

        const { problem: { id } } = currentSubmission;

        loadContestByProblemId(id);
    }, [ currentSubmission, loadContestByProblemId ]);

    const submissionTitle = useMemo(
        () => `Submission №${currentSubmission?.id}`,
        [ currentSubmission?.id ],
    );

    const canBeCompeted = useMemo(
        () => contest?.canBeCompeted,
        [ contest ],
    );

    const participationType = useMemo(
        () => canBeCompeted
            ? ContestParticipationType.Compete
            : ContestParticipationType.Practice,
        [ canBeCompeted ],
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

    const registerContestTypeUrl = useMemo(
        () => getRegisterContestTypeUrl({ id: contest?.id, participationType } as IRegisterForContestTypeUrlParams),
        [ contest?.id, participationType, getRegisterContestTypeUrl ],
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

    const backButtonState = useMemo(
        () => isNil(contest)
            ? ButtonState.disabled
            : ButtonState.enabled,
        [ contest ],
    );

    if (isNil(currentSubmission)) {
        return <div>No details fetched.</div>;
    }

    return (
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
                    <div className={styles.btnContainer}>
                        <LeftArrowIcon className={styles.leftArrow} size={IconSize.Large} />
                        <LinkButton
                          type={LinkButtonType.secondary}
                          size={ButtonSize.small}
                          to={registerContestTypeUrl}
                          className={styles.backBtn}
                          text="Back To Contest"
                          state={backButtonState}
                        />
                    </div>
                    <div>
                        {problemNameHeadingText}
                    </div>
                    <div className={styles.itemInvisible}>Other</div>
                </Heading>
                <CodeEditor
                  readOnly
                  code={currentSubmission?.content}
                  selectedSubmissionType={submissionType}
                />
            </div>
            <div className={submissionDetailsClassName}>
                <Heading type={HeadingType.secondary}>{detailsHeadingText}</Heading>
                <SubmissionResults
                  testRuns={currentSubmission.testRuns}
                  compilerComment={currentSubmission?.compilerComment}
                  isCompiledSuccessfully={currentSubmission?.isCompiledSuccessfully}
                />
            </div>
        </div>
    );
};

export default SubmissionDetails;
