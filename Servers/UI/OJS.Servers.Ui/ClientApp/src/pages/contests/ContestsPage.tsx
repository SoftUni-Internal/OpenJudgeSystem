import { useCallback, useEffect, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';

import { SortType } from '../../common/contest-types';
import { IContestsSortAndFilterOptions, IIndexContestsType } from '../../common/types';
import ContestBreadcrumbs from '../../components/contests/contest-breadcrumbs/ContestBreadcrumbs';
import ContestCard from '../../components/contests/contest-card/ContestCard';
import { ContestCetegories } from '../../components/contests/contest-categories/ContestCetegories';
import ContestStrategies from '../../components/contests/contest-strategies/ContestStrategies';
import Heading, { HeadingType } from '../../components/guidelines/headings/Heading';
import List, { Orientation } from '../../components/guidelines/lists/List';
import PaginationControls from '../../components/guidelines/pagination/PaginationControls';
import SpinningLoader from '../../components/guidelines/spinning-loader/SpinningLoader';
import useTheme from '../../hooks/use-theme';
import { clearContestCategoryBreadcrumbItems } from '../../redux/features/contestsSlice';
import { useGetAllContestsQuery } from '../../redux/services/contestsService';
import { useAppDispatch, useAppSelector } from '../../redux/store';
import { flexCenterObjectStyles } from '../../utils/object-utils';
import { setLayout } from '../shared/set-layout';

import styles from './ContestsPage.module.scss';

const ContestsPage = () => {
    const dispatch = useAppDispatch();
    const { breadcrumbItems } = useAppSelector((state) => state.contests);
    const { themeColors, getColorClassName } = useTheme();
    const { selectedCategory, selectedStrategy } = useAppSelector((state) => state.contests);
    const [ searchParams, setSearchParams ] = useSearchParams();

    const textColorClassName = getColorClassName(themeColors.textColor);

    useEffect(() => {
        if (!searchParams.get('category') && breadcrumbItems.length > 0) {
            dispatch(clearContestCategoryBreadcrumbItems());
        }
    });

    useEffect(() => {
        if (!searchParams.get('page')) {
            searchParams.set('page', '1');
            setSearchParams(searchParams);
        }
        if (!searchParams.get('category') || searchParams.get('category') === 'undefined') {
            searchParams.delete('category');
            setSearchParams(searchParams);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const selectedPage = useMemo(() => {
        if (!searchParams.get('page')) {
            return 1;
        }
        return Number(searchParams.get('page'));
    }, [ searchParams ]);

    const contestParams = useMemo(() => {
        const params: IContestsSortAndFilterOptions = {
            sortType: SortType.OrderBy,
            page: selectedPage,
        };
        if (selectedCategory) {
            params.category = selectedCategory.id;
        }
        if (selectedStrategy) {
            params.strategy = selectedStrategy.id;
        }

        return params;
    }, [ selectedCategory, selectedStrategy, selectedPage ]);

    const {
        data: allContests,
        isLoading: areContestsLoading,
        error: allContestsError,
    } = useGetAllContestsQuery({ ...contestParams });

    const renderContest = useCallback((contest: IIndexContestsType) => (
        <ContestCard contest={contest} />
    ), []);

    const renderContests = useCallback(() => {
        if (areContestsLoading) {
            return <div style={{ ...flexCenterObjectStyles }}><SpinningLoader /></div>;
        }

        if (!allContests?.items?.length) {
            return (
                <Heading type={HeadingType.secondary} className={`${textColorClassName} ${styles.contestHeading}`}>
                    No contests apply for this filter
                </Heading>
            );
        }

        return (
            <div className={styles.contestsListContainer}>
                <List
                  values={allContests?.items}
                  itemFunc={renderContest}
                  className={styles.contestsList}
                  orientation={Orientation.vertical}
                />
                <PaginationControls
                  count={allContests?.pagesCount}
                  page={selectedPage}
                  onChange={(page:number) => {
                      searchParams.set('page', page.toString());
                      setSearchParams(searchParams);
                  }}
                />
            </div>
        );
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [ allContests, areContestsLoading ]);

    if (allContestsError) { return <div className={`${textColorClassName}`}>Error loading contests</div>; }

    return (
        <div style={{ padding: '20px 40px' }}>
            <ContestBreadcrumbs />
            <div className={styles.contestsContainer}>
                <ContestCetegories />
                <div style={{ width: '100%' }}>
                    <div className={`${styles.headingWrapper} ${textColorClassName}`}>
                        <div>
                            { selectedCategory
                                ? selectedCategory.name
                                : 'All Categories'}
                        </div>
                        <ContestStrategies />
                    </div>
                    {renderContests()}
                </div>
            </div>
        </div>
    );
};

export default setLayout(ContestsPage, true);
