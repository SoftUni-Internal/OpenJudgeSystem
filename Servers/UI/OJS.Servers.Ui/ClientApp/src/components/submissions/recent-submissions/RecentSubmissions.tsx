/* eslint-disable react-hooks/exhaustive-deps */
import { useCallback, useEffect, useState } from 'react';
import IconButton from '@mui/material/IconButton';
import isEmpty from 'lodash/isEmpty';
import isNil from 'lodash/isNil';
import { SubmissionStatus } from 'src/common/enums';
import {
    applyDefaultQueryValues,
} from 'src/components/filters/Filter';
import usePreserveScrollOnSearchParamsChange from 'src/hooks/common/usePreserveScrollOnSearchParamsChange';

import { IDictionary } from '../../../common/common-types';
import { IPagedResultType, IPublicSubmission } from '../../../common/types';
import { IGetSubmissionsUrlParams } from '../../../common/url-types';
import {
    useGetLatestSubmissionsQuery,
    useLazyGetLatestSubmissionsInRoleQuery,
    useLazyGetUnprocessedCountQuery,
} from '../../../redux/services/submissionsService';
import { useAppSelector } from '../../../redux/store';
import { flexCenterObjectStyles } from '../../../utils/object-utils';
import Heading, { HeadingType } from '../../guidelines/headings/Heading';
import IconSize from '../../guidelines/icons/common/icon-sizes';
import RefreshIcon from '../../guidelines/icons/RefreshIcon';
import SpinningLoader from '../../guidelines/spinning-loader/SpinningLoader';
import SubmissionsGrid from '../submissions-grid/SubmissionsGrid';

import SubmissionStateLink from './SubmissionStateLink';

import styles from './RecentSubmissions.module.scss';

const selectedSubmissionsStateMapping = {
    1: 'All',
    2: 'Processing',
    3: 'Enqueued',
    4: 'Pending',
} as IDictionary<string>;

const RecentSubmissions = () => {
    const [ searchParams, setSearchParams ] = usePreserveScrollOnSearchParamsChange([ 'page' ]);
    const [ queryParams, setQueryParams ] = useState<IGetSubmissionsUrlParams>(applyDefaultQueryValues(searchParams));
    const [ status, setStatus ] = useState<SubmissionStatus>(SubmissionStatus.All);
    const [ selectedActive, setSelectedActive ] = useState<number>(1);
    const [ shouldLoadRegularUserSubmissions, setShouldLoadRegularUserSubmissions ] = useState<boolean>(false);

    const [ latestSubmissions, setLatestSubmissions ] = useState<IPagedResultType<IPublicSubmission>>({
        items: [],
        totalItemsCount: 0,
        itemsPerPage: 0,
        pagesCount: 0,
        pageNumber: 0,
    });

    const { internalUser: user } = useAppSelector((state) => state.authorization);

    const loggedInUserInRole = !isEmpty(user.id) && user.isAdmin;

    const {
        isLoading: regularUserIsLoading,
        isFetching: regularUserIsFetching,
        data: regularUserData,
    } = useGetLatestSubmissionsQuery(
        { ...queryParams, status },
        { skip: !shouldLoadRegularUserSubmissions },
    );

    const [
        getUnprocessedSubmissionsCount, { data: unprocessedSubmissionsCount },
    ] = useLazyGetUnprocessedCountQuery();

    const [
        getLatestSubmissionsInRole, {
            isLoading: inRoleLoading,
            isFetching: inRoleIsFetching,
            data: inRoleData,
        },
    ] = useLazyGetLatestSubmissionsInRoleQuery();

    const areSubmissionsLoading =
        loggedInUserInRole
            ? inRoleLoading
            : regularUserIsLoading;

    const areSubmissionsFetching =
        loggedInUserInRole
            ? inRoleIsFetching
            : regularUserIsFetching;

    const inRoleSubmissionsReady = loggedInUserInRole && !isNil(inRoleData) && !inRoleLoading && !inRoleIsFetching;
    const regularUserSubmissionsReady = !loggedInUserInRole && !isNil(regularUserData) && !regularUserIsLoading && !regularUserIsFetching;

    useEffect(() => {
        if (loggedInUserInRole) {
            getLatestSubmissionsInRole({ ...queryParams, status });
            getUnprocessedSubmissionsCount(null);
        }
    }, [
        loggedInUserInRole,
        getLatestSubmissionsInRole,
        getUnprocessedSubmissionsCount,
        queryParams.page,
        queryParams.filter,
        queryParams.sorting,
    ]);

    useEffect(() => {
        if (!user.isAdmin) {
            setShouldLoadRegularUserSubmissions(true);
        }
    }, [ user ]);

    useEffect(() => {
        if (inRoleSubmissionsReady) {
            setLatestSubmissions(inRoleData);
        } else if (regularUserSubmissionsReady) {
            setLatestSubmissions(regularUserData);
        }
    }, [ inRoleData, inRoleSubmissionsReady, regularUserData, regularUserSubmissionsReady ]);

    const handleSelectSubmissionState = useCallback(
        (typeKey: number) => {
            if (selectedActive) {
                setQueryParams({
                    page: 1,
                    itemsPerPage: queryParams.itemsPerPage,
                    filter: queryParams.filter,
                    sorting: queryParams.sorting,
                });

                setStatus(typeKey);
                setSelectedActive(typeKey);
            }
        },
        [ selectedActive ],
    );

    const getSubmissionsAwaitingExecution = useCallback((state: string = '') => {
        if (isEmpty(unprocessedSubmissionsCount)) {
            return 0;
        }

        if (isEmpty(state)) {
            return Object.values(unprocessedSubmissionsCount).reduce((acc, curr) => acc + curr, 0);
        }

        return unprocessedSubmissionsCount[state];
    }, [ unprocessedSubmissionsCount ]);

    const renderSubmissionsStateAdminToggle = useCallback(() => {
        const { isAdmin } = user;

        return (
            isAdmin && (
            <Heading
              type={HeadingType.secondary}
            >
                Submissions awaiting execution:
                {' '}
                {getSubmissionsAwaitingExecution()}
                {' '}
                (
                <SubmissionStateLink
                  stateIndex={1}
                  isSelected={selectedActive === 1}
                  text={selectedSubmissionsStateMapping[1]}
                  handleOnSelect={handleSelectSubmissionState}
                />
                /
                <SubmissionStateLink
                  stateIndex={2}
                  isSelected={selectedActive === 2}
                  text={selectedSubmissionsStateMapping[2]}
                  count={getSubmissionsAwaitingExecution(selectedSubmissionsStateMapping[2])}
                  handleOnSelect={handleSelectSubmissionState}
                />
                /
                <SubmissionStateLink
                  stateIndex={3}
                  isSelected={selectedActive === 3}
                  text={selectedSubmissionsStateMapping[3]}
                  count={getSubmissionsAwaitingExecution(selectedSubmissionsStateMapping[3])}
                  handleOnSelect={handleSelectSubmissionState}
                />
                /
                <SubmissionStateLink
                  stateIndex={4}
                  isSelected={selectedActive === 4}
                  text={selectedSubmissionsStateMapping[4]}
                  count={getSubmissionsAwaitingExecution(selectedSubmissionsStateMapping[4])}
                  handleOnSelect={handleSelectSubmissionState}
                />
                )
                <IconButton
                  title="Refresh"
                  className={styles.refreshIcon}
                  onClick={() => setQueryParams((prev) => ({ ...prev, page: 1 }))}
                >
                    <RefreshIcon size={IconSize.Large} />
                </IconButton>
            </Heading>
            )
        );
    }, [ user, getSubmissionsAwaitingExecution, selectedActive, handleSelectSubmissionState ]);

    return (
        <div className={styles.recentSubmissionsWrapper}>
            {
                !user.canAccessAdministration && (
                    <Heading
                      type={HeadingType.primary}
                    >
                        Latest
                        {' '}
                        {latestSubmissions.items?.length}
                        {' '}
                        submissions out of
                        {' '}
                        {isNil(latestSubmissions.totalItemsCount)
                            ? '...'
                            : latestSubmissions.totalItemsCount }
                        {' '}
                        total
                    </Heading>
                )
            }
            {renderSubmissionsStateAdminToggle()}
            {
                areSubmissionsFetching && queryParams.page === 1
                    ? (
                        <div style={{ ...flexCenterObjectStyles, marginTop: '10px' }}>
                            <SpinningLoader />
                        </div>
                    )
                    : (
                        <SubmissionsGrid
                          className={styles.recentSubmissionsGrid}
                          isDataLoaded={!areSubmissionsLoading}
                          submissions={latestSubmissions}
                          options={{
                              showDetailedResults: user.isAdmin,
                              showTaskDetails: true,
                              showCompeteMarker: user.isAdmin,
                              showSubmissionTypeInfo: true,
                              showParticipantUsername: true,
                          }}
                          searchParams={searchParams}
                          setSearchParams={setSearchParams}
                          setQueryParams={setQueryParams}
                        />
                    )
            }
        </div>
    );
};

export default RecentSubmissions;
