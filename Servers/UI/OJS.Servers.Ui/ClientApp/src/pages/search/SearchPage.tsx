/* eslint-disable max-len */
import { useCallback, useEffect, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';

import { CheckboxSearchValues } from '../../common/enums';
import {
    IIndexContestsType,
    IPagedResultType,
    IProblemSearchType,
    IUSerSearchCardProps,
    IUserType,
} from '../../common/types';
import ContestCard from '../../components/contests/contest-card/ContestCard';
import List, { Orientation } from '../../components/guidelines/lists/List';
import PaginationControls from '../../components/guidelines/pagination/PaginationControls';
import SpinningLoader from '../../components/guidelines/spinning-loader/SpinningLoader';
import ProblemSearchCard from '../../components/search/profile-search-card/ProblemSearchCard';
import ProfileSearchList from '../../components/search/profile-search-list/ProfileSearchList';
import UserSearchCard from '../../components/search/user-search-card/UserSearchCard';
import useTheme from '../../hooks/use-theme';
import { setSearchValue } from '../../redux/features/searchSlice';
import {
    useLazyGetContestsSearchQuery,
    useLazyGetProblemsSearchQuery,
    useLazyGetUsersSearchQuery,
} from '../../redux/services/searchService';
import { useAppDispatch, useAppSelector } from '../../redux/store';
import { getErrorMessage } from '../../utils/http-utils';

import styles from './SearchPage.module.scss';

enum SearchTypeEnums {
    CONTESTS = 'Contests',
    PROBLEMS = 'Problems',
    USERS = 'Users',
}

const ITEMS_PER_SEARCH = 5;
const USER_ITEMS_PER_SEARCH = 35;

const SearchPage = () => {
    const dispatch = useAppDispatch();
    const [ searchParams, setSearchParams ] = useSearchParams();
    const { getColorClassName, themeColors } = useTheme();
    const { searchValue, selectedTerms } = useAppSelector((state) => state.search);

    const textColorClassName = getColorClassName(themeColors.textColor);

    const [
        getContestsSearch, {
            data: contestsSearchData,
            isLoading: contestsSearchLoading,
            error: contestsSearchError,
            isError: areContestsSearchError,
        } ] = useLazyGetContestsSearchQuery();
    const [
        getProblemsSearch, {
            data: problemsSearchData,
            isLoading: problemsSearchLoading,
            error: problemsSearchError,
            isError: areProblemsSearchError,
        } ] = useLazyGetProblemsSearchQuery();
    const [
        getUsersSearch, {
            data: usersSearchData,
            isLoading: usersSearchLoading,
            error: usersSearchError,
            isError: areUsersSearchError,
        },
    ] = useLazyGetUsersSearchQuery();

    const shouldIncludeContests = searchParams.get('Contests');
    const shouldIncludeProblems = searchParams.get('Problems');
    const shouldIncludeUsers = searchParams.get('Users');

    const selectedContestsPage = useMemo(() => {
        if (!searchParams.get('contestsPage')) {
            return 1;
        }
        return Number(searchParams.get('contestsPage'));
    }, [ searchParams ]);

    const selectedProblemsPage = useMemo(() => {
        if (!searchParams.get('problemsPage')) {
            return 1;
        }
        return Number(searchParams.get('problemsPage'));
    }, [ searchParams ]);

    const selectedUsersPage = useMemo(() => {
        if (!searchParams.get('usersPage')) {
            return 1;
        }
        return Number(searchParams.get('usersPage'));
    }, [ searchParams ]);

    useEffect(() => {
        const searchTerm = searchParams.get('searchTerm');
        if (searchTerm) {
            dispatch(dispatch(setSearchValue(searchTerm)));
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // initiate the search
    useEffect(() => {
        if (searchValue.length < 3 || selectedTerms.length === 0) {
            return;
        }
        if (selectedTerms.includes(CheckboxSearchValues.problems)) {
            getProblemsSearch({ searchTerm: searchValue, page: selectedProblemsPage, itemsPerPage: ITEMS_PER_SEARCH });
        }
        if (selectedTerms.includes(CheckboxSearchValues.contests)) {
            getContestsSearch({ searchTerm: searchValue, page: selectedContestsPage, itemsPerPage: ITEMS_PER_SEARCH });
        }
        if (selectedTerms.includes(CheckboxSearchValues.users)) {
            getUsersSearch({ searchTerm: searchValue, page: selectedUsersPage, itemsPerPage: USER_ITEMS_PER_SEARCH });
        }
    }, [
        searchValue,
        selectedTerms,
        selectedContestsPage,
        selectedProblemsPage,
        selectedUsersPage,
        getProblemsSearch,
        getContestsSearch,
        getUsersSearch,
    ]);

    const renderSearchFragmentResults = useCallback((
        searchName: SearchTypeEnums,
        data: IPagedResultType<IIndexContestsType> | IPagedResultType<IUserType> | IPagedResultType<IProblemSearchType> | undefined,
        isLoading: boolean,
        isError: boolean,
        error: any,
    ) => {
        const renderErrorFragment = () => (
            <div>
                <div className={styles.errorTextWrapper}>
                    {getErrorMessage(error)}
                </div>
                { searchName !== SearchTypeEnums.USERS && (<hr className={styles.line} />)}
            </div>
        );

        const renderData = () => {
            const renderFunction = (renderElement: any) => {
                if (searchName === SearchTypeEnums.CONTESTS) {
                    return <ContestCard contest={(renderElement as IIndexContestsType)} />;
                } if (searchName === SearchTypeEnums.PROBLEMS) {
                    return <ProblemSearchCard problem={(renderElement as IProblemSearchType)} />;
                }
                return <UserSearchCard user={renderElement as IUSerSearchCardProps} />;
            };

            const selectedPageValue = () => {
                if (searchName === SearchTypeEnums.CONTESTS) {
                    return selectedContestsPage;
                } if (searchName === SearchTypeEnums.PROBLEMS) {
                    return selectedProblemsPage;
                }
                return selectedUsersPage;
            };

            return (
                <div className={styles.searchResultsWrapper}>
                    {!data || data.totalItemsCount === 0
                        ? <div>No items found</div>
                        : (
                            <>
                                { searchName === SearchTypeEnums.USERS
                                    ? <ProfileSearchList data={(data.items as IIndexContestsType[])} />
                                    : (
                                        <List
                                          values={data.items}
                                          itemFunc={renderFunction}
                                          orientation={Orientation.vertical}
                                        />
                                    )}
                                <PaginationControls
                                  count={data?.pagesCount}
                                  page={selectedPageValue()}
                                  onChange={(page: number) => {
                                      searchParams.set(`${searchName.toLowerCase()}Page`, page.toString());
                                      setSearchParams(searchParams, { replace: true });
                                  }}
                                  shouldScrollDown
                                />
                            </>
                        )}
                    { searchName !== SearchTypeEnums.USERS && (<hr className={styles.line} />)}
                </div>
            );
        };

        if (isLoading) {
            return <SpinningLoader />;
        }

        return (
            <div>
                <div className={styles.searchSectionHeader}>
                    {searchName}
                </div>
                { isError
                    ? renderErrorFragment()
                    : renderData()}
            </div>
        );
    }, [ searchParams, selectedContestsPage, selectedProblemsPage, selectedUsersPage, setSearchParams ]);

    return (
        <div className={`${styles.searchPageWrapper} ${textColorClassName}`}>
            {searchValue.length < 3
                ? <div>The search term must be at least 3 characters!</div>
                : (
                    <>
                        <div className={styles.searchTextHeader}>
                            Search results for &quot;
                            {searchValue}
                            &quot;
                        </div>
                        {shouldIncludeContests &&
                            renderSearchFragmentResults(SearchTypeEnums.CONTESTS, contestsSearchData, contestsSearchLoading, areContestsSearchError, contestsSearchError)}
                        {shouldIncludeProblems &&
                            renderSearchFragmentResults(SearchTypeEnums.PROBLEMS, problemsSearchData, problemsSearchLoading, areProblemsSearchError, problemsSearchError)}
                        {shouldIncludeUsers &&
                            renderSearchFragmentResults(SearchTypeEnums.USERS, usersSearchData, usersSearchLoading, areUsersSearchError, usersSearchError)}
                    </>
                )}
        </div>
    );
};

export default SearchPage;
