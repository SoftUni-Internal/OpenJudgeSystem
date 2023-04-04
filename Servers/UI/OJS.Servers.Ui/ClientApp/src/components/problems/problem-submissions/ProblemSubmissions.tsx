import React, { useCallback } from 'react';
import isEmpty from 'lodash/isEmpty';
import isNil from 'lodash/isNil';

import { useProblemSubmissions } from '../../../hooks/submissions/use-problem-submissions';
import { useCurrentContest } from '../../../hooks/use-current-contest';
import { useProblems } from '../../../hooks/use-problems';
import concatClassNames from '../../../utils/class-names';
import { Button, ButtonType } from '../../guidelines/buttons/Button';
import SubmissionsList from '../../submissions/submissions-list/SubmissionsList';

import styles from './ProblemSubmissions.module.scss';

const ProblemSubmissions = () => {
    const {
        state: { submissions },
        actions: { loadSubmissions },
    } = useProblemSubmissions();

    const { actions: { loadParticipantScores } } = useCurrentContest();
    const { state: { currentProblem } } = useProblems();

    const reload = useCallback(
        async () => {
            if (!isNil(currentProblem)) {
                const { id } = currentProblem;
                await loadSubmissions(id);
                await loadParticipantScores();
            }
        },
        [ loadParticipantScores, loadSubmissions, currentProblem ],
    );

    const handleReloadClick = useCallback(async () => {
        await reload();
    }, [ reload ]);

    const refreshButtonClass = 'refreshButton';
    const refreshButtonClassName = concatClassNames(styles.refreshBtn, refreshButtonClass);

    // const submissionResultsListItemClass = 'submission results';
    // const submissionResultsListItemClassName =
    // concatClassNames(styles.submissionItem, submissionResultsListItemClass);
    const submissionResultsContentClass = 'submissionResultsContent';
    const submissionResultsContentClassName = concatClassNames(styles.submissionResultsContent, submissionResultsContentClass);
    const renderSubmissions = () => {
        if (isNil(submissions) || isEmpty(submissions)) {
            return (
                <p> No results for this problem yet.</p>
            );
        }

        return (
            <SubmissionsList
              items={submissions}
              selectedSubmission={null}
              className={styles.submissionsList}
            />
        );
    };

    return (
        <div className={submissionResultsContentClassName}>
            {renderSubmissions()}
            <Button
              type={ButtonType.secondary}
              className={refreshButtonClassName}
              onClick={() => handleReloadClick()}
              text="Refresh"
            />
        </div>
    );
};

export default ProblemSubmissions;
