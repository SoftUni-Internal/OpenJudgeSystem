import React, { useEffect } from 'react';
import { useParams } from 'react-router';
import isNil from 'lodash/isNil';

import { ContestParticipationType, ContestResultType } from '../../common/constants';
import { contestParticipationType } from '../../common/contest-helpers';
import { IContestDetailsResponseType } from '../../common/types';
import ContestBreadcrumbs from '../../components/contests/contest-breadcrumbs/ContestBreadcrumbs';
import ContestResultsGrid from '../../components/contests/contest-results-grid/ContestResultsGrid';
import ErrorWithActionButtons from '../../components/error/ErrorWithActionButtons';
import { LinkButton, LinkButtonType } from '../../components/guidelines/buttons/Button';
import Heading, { HeadingType } from '../../components/guidelines/headings/Heading';
import SpinningLoader from '../../components/guidelines/spinning-loader/SpinningLoader';
import { setContestCategories, setContestDetails } from '../../redux/features/contestsSlice';
import { useGetContestCategoriesQuery, useGetContestResultsQuery } from '../../redux/services/contestsService';
import { useAppDispatch, useAppSelector } from '../../redux/store';
import isNilOrEmpty from '../../utils/check-utils';
import { getErrorMessage } from '../../utils/http-utils';
import { flexCenterObjectStyles } from '../../utils/object-utils';
import { capitalizeFirstLetter } from '../../utils/string-utils';
import { getContestDetailsAppUrl } from '../../utils/urls';
import { makePrivate } from '../shared/make-private';
import { setLayout } from '../shared/set-layout';

import styles from './ContestResultPage.module.scss';

const ContestResultsPage = () => {
    const params = useParams();
    const { contestId, participationType: participationUrlType, resultType } = params;
    const official = participationUrlType === ContestParticipationType.Compete;
    const full = resultType === ContestResultType.Full;

    const participationType = contestParticipationType(official);

    const { contestDetails } = useAppSelector((state) => state.contests);

    const {
        data: contestResults,
        isLoading,
        error: contestResultsError,
        refetch,
    } = useGetContestResultsQuery({
        id: Number(contestId),
        official,
        full,
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
                } as IContestDetailsResponseType ?? null,
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
                              to={getContestDetailsAppUrl(Number(contestId!), contestResults?.name)}
                              text={contestResults?.name}
                              type={LinkButtonType.plain}
                              className={styles.contestName}
                            />
                        </Heading>
                        <ContestResultsGrid items={contestResults ?? null} />
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

export default makePrivate(setLayout(ContestResultsPage));
