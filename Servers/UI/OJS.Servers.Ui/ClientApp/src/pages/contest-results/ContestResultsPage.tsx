import React, { useEffect, useMemo } from 'react';
import { useParams } from 'react-router';
import isNil from 'lodash/isNil';

import { ContestParticipationType, ContestResultType } from '../../common/constants';
import { contestParticipationType } from '../../common/contest-helpers';
import { IContestDetailsResponseType } from '../../common/types';
import { getContestsDetailsPageUrl } from '../../common/urls/compose-client-urls';
import ContestBreadcrumbs from '../../components/contests/contest-breadcrumbs/ContestBreadcrumbs';
import ContestResultsGrid from '../../components/contests/contest-results-grid/ContestResultsGrid';
import ErrorWithActionButtons from '../../components/error/ErrorWithActionButtons';
import { LinkButton, LinkButtonType } from '../../components/guidelines/buttons/Button';
import Heading, { HeadingType } from '../../components/guidelines/headings/Heading';
import PaginationControls from '../../components/guidelines/pagination/PaginationControls';
import SpinningLoader from '../../components/guidelines/spinning-loader/SpinningLoader';
import usePreserveScrollOnSearchParamsChange from '../../hooks/common/usePreserveScrollOnSearchParamsChange';
import { setContestCategories, setContestDetails } from '../../redux/features/contestsSlice';
import { useGetContestCategoriesQuery, useGetContestResultsQuery } from '../../redux/services/contestsService';
import { useAppDispatch, useAppSelector } from '../../redux/store';
import isNilOrEmpty from '../../utils/check-utils';
import { getErrorMessage } from '../../utils/http-utils';
import { flexCenterObjectStyles } from '../../utils/object-utils';
import { capitalizeFirstLetter } from '../../utils/string-utils';
import makePrivate from '../shared/make-private';
import setLayout from '../shared/set-layout';
import withTitle from '../shared/with-title';

import styles from './ContestResultPage.module.scss';

const ContestResultsPage = () => {
    const params = useParams();
    const { contestId, participationType: participationUrlType, resultType } = params;
    const { searchParams, setSearchParams } = usePreserveScrollOnSearchParamsChange();
    const official = participationUrlType === ContestParticipationType.Compete;
    const full = resultType === ContestResultType.Full;

    const participationType = contestParticipationType(official);

    const { contestDetails } = useAppSelector((state) => state.contests);

    const selectedPage = useMemo(() => {
        if (!searchParams.get('page')) {
            return 1;
        }
        return Number(searchParams.get('page'));
    }, [ searchParams ]);

    const {
        data: contestResults,
        isLoading,
        error: contestResultsError,
        refetch,
    } = useGetContestResultsQuery({
        id: Number(contestId),
        official,
        full,
        page: selectedPage,
    }, { skip: !contestId });

    const { data: contestCategories } = useGetContestCategoriesQuery();

    const dispatch = useAppDispatch();

    useEffect(() => {
        if (contestId) {
            refetch();
        }
    }, [ contestId, refetch ]);

    useEffect(() => {
        if (!contestResults) {
            return;
        }

        if (!contestDetails || contestDetails?.id !== contestResults?.id) {
            dispatch(setContestDetails({
                contest: {
                    id: contestResults!.id,
                    name: contestResults!.name,
                    categoryId: contestResults!.categoryId,
                    isAdminOrLecturerInContest: contestResults!.userIsInRoleForContest,
                } as IContestDetailsResponseType,
            }));
        }
    }, [ contestResults, contestDetails, dispatch ]);

    useEffect(() => {
        if (!isNilOrEmpty(contestCategories)) {
            dispatch(setContestCategories({ contestCategories: contestCategories || [] }));
        }
    }, [ contestCategories, dispatch ]);

    return (
        isNil(contestResultsError)
            ? !isLoading
                ? (
                    <>
                        <div>
                            <ContestBreadcrumbs />
                        </div>
                        <Heading
                          type={HeadingType.primary}
                          className={styles.contestResultsHeading}
                        >
                            {capitalizeFirstLetter(participationType)}
                            {' '}
                            Results For
                            {' '}
                            <LinkButton
                              to={getContestsDetailsPageUrl({ contestId: Number(contestId!), contestName: contestResults?.name })}
                              text={contestResults?.name}
                              type={LinkButtonType.plain}
                              className={styles.contestName}
                            />
                        </Heading>
                        <PaginationControls
                          count={contestResults?.pagedResults.pagesCount ?? 0}
                          page={selectedPage}
                          onChange={(page:number) => {
                              searchParams.set('page', page.toString());
                              setSearchParams(searchParams);
                          }}
                          className={`${styles.paginationControlsUpper}`}
                        />
                        <ContestResultsGrid
                          items={contestResults ?? null}
                        />
                        <PaginationControls
                          count={contestResults?.pagedResults.pagesCount ?? 0}
                          page={selectedPage}
                          onChange={(page:number) => {
                              searchParams.set('page', page.toString());
                              setSearchParams(searchParams);
                          }}
                          className={`${styles.paginationControlsLower}`}
                        />
                    </>
                )
                : (
                    <div style={{ ...flexCenterObjectStyles }}>
                        <SpinningLoader />
                    </div>
                )
            : (
                <ErrorWithActionButtons
                  message={getErrorMessage(contestResultsError)}
                />
            )

    );
};

export default makePrivate(setLayout(withTitle(ContestResultsPage, (params) => `Results #${params.contestId}`)));
