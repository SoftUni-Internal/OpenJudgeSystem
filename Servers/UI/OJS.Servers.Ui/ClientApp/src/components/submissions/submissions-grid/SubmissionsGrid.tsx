import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useSelector } from 'react-redux';

import { IDictionary } from '../../../common/common-types';
import { ISubmissionResponseModel } from '../../../common/types';
import { usePublicSubmissions } from '../../../hooks/submissions/use-public-submissions';
import { usePages } from '../../../hooks/use-pages';
import { IAuthorizationReduxState } from '../../../redux/features/authorizationSlice';
import { format } from '../../../utils/number-utils';
import { flexCenterObjectStyles } from '../../../utils/object-utils';
import Heading, { HeadingType } from '../../guidelines/headings/Heading';
import List from '../../guidelines/lists/List';
import PaginationControls from '../../guidelines/pagination/PaginationControls';
import SpinningLoader from '../../guidelines/spinning-loader/SpinningLoader';
import SubmissionGridRow from '../submission-grid-row/SubmissionGridRow';

import SubmissionStateLink from './SubmissionStateLink';

import styles from './SubmissionsGrid.module.scss';

const selectedSubmissionsStateMapping = {
    1: 'All',
    2: 'In Queue',
    3: 'Pending',
} as IDictionary<string>;

const defaultState = {
    state: {
        selectedActive: 1,
    },
};

const SubmissionsGrid = () => {
    const [ selectedActive, setSelectedActive ] = useState<number>(defaultState.state.selectedActive);
    const {
        state: {
            publicSubmissions,
            totalSubmissionsCount,
            totalUnprocessedSubmissionsCount,
            areSubmissionsLoading
        },
        actions: {
            loadTotalUnprocessedSubmissionsCount,
            initiatePublicSubmissionsQuery,
            initiateUnprocessedSubmissionsQuery,
            initiatePendingSubmissionsQuery,
            clearPageValues,
            clearPageInformation,
        },
    } = usePublicSubmissions();

    const { internalUser: user } =
    useSelector((state: {authorization: IAuthorizationReduxState}) => state.authorization);
    const {
        state: { currentPage, pagesInfo },
        changePage,
    } = usePages();

    const selectedSubmissionStateToRequestMapping = useMemo(
        () => ({
            1: initiatePublicSubmissionsQuery,
            2: initiateUnprocessedSubmissionsQuery,
            3: initiatePendingSubmissionsQuery,
        } as IDictionary<() => void>),
        [ initiatePublicSubmissionsQuery, initiateUnprocessedSubmissionsQuery, initiatePendingSubmissionsQuery ],
    );

    useEffect(
        () => {
            if (!user.isAdmin) {
                return;
            }

            (async () => {
                await loadTotalUnprocessedSubmissionsCount();
            })();
        },
        [ loadTotalUnprocessedSubmissionsCount, user.isAdmin ],
    );

    const handlePageChange = useCallback(
        (page: number) => changePage(page),
        [ changePage ],
    );

    const handleSelectSubmissionType = useCallback(
        (typeKey: number) => {
            if (selectedActive) {
                clearPageValues();

                setSelectedActive(typeKey);
            }
        },
        [ clearPageValues, selectedActive ],
    );

    useEffect(
        () => {
            selectedSubmissionStateToRequestMapping[selectedActive]();
        },
        [
            initiatePendingSubmissionsQuery,
            initiatePublicSubmissionsQuery,
            initiateUnprocessedSubmissionsQuery,
            selectedActive,
            selectedSubmissionStateToRequestMapping,
            totalSubmissionsCount,
        ],
    );

    useEffect(
        () => () => {
            clearPageInformation();
        },
        [ clearPageInformation ],
    );

    const { pagesCount } = pagesInfo;
    const renderPrivilegedComponent = useCallback(
        () => {
            const { isAdmin } = user;

            return (
                <>
                    { isAdmin && (
                        <>
                            <Heading type={HeadingType.secondary}>
                                Submissions awaiting execution:
                                {' '}
                                {totalUnprocessedSubmissionsCount}
                                {' '}
                                (
                                <SubmissionStateLink
                                    stateIndex={1}
                                    isSelected={selectedActive === 1}
                                    text={selectedSubmissionsStateMapping[1]}
                                    handleOnSelect={handleSelectSubmissionType}
                                />
                                /
                                <SubmissionStateLink
                                    stateIndex={2}
                                    isSelected={selectedActive === 2}
                                    text={selectedSubmissionsStateMapping[2]}
                                    handleOnSelect={handleSelectSubmissionType}
                                />
                                /
                                <SubmissionStateLink
                                    stateIndex={3}
                                    isSelected={selectedActive === 3}
                                    text={selectedSubmissionsStateMapping[3]}
                                    handleOnSelect={handleSelectSubmissionType}
                                />
                                )
                            </Heading>
                            { publicSubmissions?.length > 0 && (
                                <PaginationControls
                                    count={pagesCount}
                                    page={currentPage}
                                    onChange={handlePageChange}
                                />
                            )}
                        </>
                    )}
                </>
            );
        },
        [
            user,
            publicSubmissions,
            totalUnprocessedSubmissionsCount,
            selectedActive,
            handleSelectSubmissionType,
            pagesCount,
            currentPage,
            handlePageChange,
        ],
    );

    const renderSubmissionRow = useCallback(
        (submission: ISubmissionResponseModel) => (
            <SubmissionGridRow submission={submission} />
        ),
        [],
    );

    const renderSubmissionsList = useCallback(
        () => {
            if (areSubmissionsLoading) {
                return (
                    <div style={{ ...flexCenterObjectStyles, marginTop: '10px' }}>
                        <SpinningLoader />
                    </div>
                );
            }

            if (publicSubmissions.length === 0) {
                return (
                    <div className={styles.noSubmissionsFound}>
                        No submissions found.
                    </div>
                );
            }

            return (
                <List
                  values={publicSubmissions}
                  itemFunc={renderSubmissionRow}
                  itemClassName={styles.submissionRow}
                  fullWidth
                />
            );
        },
        [
            publicSubmissions,
            renderSubmissionRow,
        ],
    );

    console.log(selectedActive);

    return (
        <>
            <Heading type={HeadingType.primary}>
                Latest
                {' '}
                {publicSubmissions.length}
                {' '}
                submissions out of
                {' '}
                {format(totalSubmissionsCount)}
                {' '}
                total
            </Heading>
            {renderPrivilegedComponent()}
            {renderSubmissionsList()}
        </>
    );
};

export default SubmissionsGrid;
