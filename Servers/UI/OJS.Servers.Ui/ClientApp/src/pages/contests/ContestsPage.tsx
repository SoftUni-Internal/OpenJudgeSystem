import { useCallback, useEffect, useMemo } from 'react';
import { useParams } from 'react-router-dom';

import { SortType } from '../../common/contest-types';
import { IContestsSortAndFilterOptions, IIndexContestsType } from '../../common/types';
import MetaTags from '../../components/common/MetaTags';
import ContestCard from '../../components/contests/contest-card/ContestCard';
import ContestStrategies from '../../components/contests/contest-strategies/ContestStrategies';
import Heading, { HeadingType } from '../../components/guidelines/headings/Heading';
import List, { Orientation } from '../../components/guidelines/lists/List';
import PaginationControls from '../../components/guidelines/pagination/PaginationControls';
import SpinningLoader from '../../components/guidelines/spinning-loader/SpinningLoader';
import usePreserveScrollOnSearchParamsChange from '../../hooks/common/usePreserveScrollOnSearchParamsChange';
import useTheme from '../../hooks/use-theme';
import {
    clearContestCategoryBreadcrumbItems,
    setContests,
    setContestsCacheIsReset,
} from '../../redux/features/contestsSlice';
import { useGetAllContestsQuery } from '../../redux/services/contestsService';
import { useAppDispatch, useAppSelector } from '../../redux/store';
import isNilOrEmpty from '../../utils/check-utils';
import { flexCenterObjectStyles } from '../../utils/object-utils';

import styles from './ContestsPage.module.scss';

const ContestsPage = () => {
    const dispatch = useAppDispatch();
    const { categoryId } = useParams();
    const { breadcrumbItems } = useAppSelector((state) => state.contests);
    const { themeColors, getColorClassName } = useTheme();
    const {
        contests,
        contestsCacheIsReset,
        selectedCategory,
        selectedStrategy,
    } = useAppSelector((state) => state.contests);

    const [ searchParams, setSearchParams ] = usePreserveScrollOnSearchParamsChange([ 'page' ]);

    const textColorClassName = getColorClassName(themeColors.textColor);

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
        refetch: refetchAllContests,
        error: allContestsError,
        isFetching: areContestsFetching,
    } = useGetAllContestsQuery({ ...contestParams });

    useEffect(() => {
        if (!categoryId && breadcrumbItems.length > 0) {
            dispatch(clearContestCategoryBreadcrumbItems());
        }
    });

    useEffect(() => {
        if (contestsCacheIsReset) {
            refetchAllContests();
            dispatch(setContestsCacheIsReset(false));
        }
    }, [ refetchAllContests, contestsCacheIsReset, dispatch ]);

    useEffect(() => {
        if (allContests && !isNilOrEmpty(allContests)) {
            dispatch(setContests(allContests));
        }
    }, [ allContests, dispatch ]);

    const renderContest = useCallback((contest: IIndexContestsType) => (
        <ContestCard contest={contest} />
    ), []);

    const renderContests = useCallback(() => {
        if (areContestsFetching) {
            return <div style={{ ...flexCenterObjectStyles }}><SpinningLoader /></div>;
        }

        if (!contests?.items?.length) {
            return (
                <Heading type={HeadingType.secondary} className={`${textColorClassName} ${styles.contestHeading}`}>
                    No contests apply for this filter
                </Heading>
            );
        }

        return (
            <>
                <List
                  values={contests?.items}
                  itemFunc={renderContest}
                  className={styles.contestsList}
                  orientation={Orientation.vertical}
                />
                <PaginationControls
                  count={contests?.pagesCount}
                  page={selectedPage}
                  onChange={(page:number) => {
                      searchParams.set('page', page.toString());
                      setSearchParams(searchParams);
                  }}
                  className={`${styles.paginationControlsLower}`}
                />
            </>
        );
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [ contests, areContestsFetching, searchParams ]);

    if (allContestsError) { return <div className={`${textColorClassName}`}>Error loading contests</div>; }

    return (
        <div className={styles.contestsContainer}>
            <MetaTags
              title={`Contests ${selectedCategory?.name
                  ? `in ${selectedCategory?.name}`
                  : ''}${selectedPage > 1
                  ? ` - page ${selectedPage}`
                  : ''} - SoftUni Judge`}
              description={
                    'Explore coding contests on SoftUni Judge. Filter by category and strategy ' +
                    'to find your perfect challenge. Participate and improve your programming skills.'
                }
            />
            <div style={{ width: '100%' }}>
                <div className={`${styles.headingWrapper} ${textColorClassName}`}>
                    <div>
                        { selectedCategory
                            ? selectedCategory.name
                            : 'All Categories'}
                    </div>
                    <ContestStrategies />
                </div>
                <div className={styles.contestsListContainer}>
                    <PaginationControls
                      count={contests?.pagesCount || 0}
                      page={selectedPage}
                      onChange={(page:number) => {
                          searchParams.set('page', page.toString());
                          setSearchParams(searchParams);
                      }}
                      className={styles.paginationControlsUpper}
                    />
                    {renderContests()}
                </div>
            </div>
        </div>
    );
};

export default ContestsPage;
