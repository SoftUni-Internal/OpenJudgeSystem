/* eslint-disable max-len */
import { useCallback, useEffect, useRef, useState } from 'react';

import { CheckboxSearchValues } from '../../common/enums';
import {
    IIndexContestsType,
    IPagedResultType,
    IProblemSearchType,
    IUSerSearchCardProps,
    IUserType,
} from '../../common/types';
import MetaTags from '../../components/common/MetaTags';
import ContestCard from '../../components/contests/contest-card/ContestCard';
import List, { Orientation } from '../../components/guidelines/lists/List';
import PaginationControls from '../../components/guidelines/pagination/PaginationControls';
import SpinningLoader from '../../components/guidelines/spinning-loader/SpinningLoader';
import ProblemSearchCard from '../../components/search/profile-search-card/ProblemSearchCard';
import ProfileSearchList from '../../components/search/profile-search-list/ProfileSearchList';
import UserSearchCard from '../../components/search/user-search-card/UserSearchCard';
import usePreserveScrollOnSearchParamsChange from '../../hooks/common/usePreserveScrollOnSearchParamsChange';
import useTheme from '../../hooks/use-theme';
import { setSearchValue } from '../../redux/features/searchSlice';
import {
    useLazyGetContestsSearchQuery,
    useLazyGetProblemsSearchQuery,
    useLazyGetUsersSearchQuery,
} from '../../redux/services/searchService';
import { useAppDispatch, useAppSelector } from '../../redux/store';
import getTotalHeight from '../../utils/dom-element-utils';
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
    const { searchParams, setSearchParams } = usePreserveScrollOnSearchParamsChange();
    const { getColorClassName, themeColors } = useTheme();
    const { searchValue, selectedTerms } = useAppSelector((state) => state.search);
    const [ selectedContestsPage, setSelectedContestsPage ] = useState(1);
    const [ selectedProblemsPage, setSelectedProblemsPage ] = useState(1);
    const [ selectedUsersPage, setSelectedUsersPage ] = useState(1);
    const [ contestsContentHeight, setContestsContentHeight ] = useState(0);
    const [ problemsContentHeight, setProblemsContentHeight ] = useState(0);
    const [ usersContentHeight, setUsersContentHeight ] = useState(0);

    const contestsContentRef = useRef<HTMLDivElement>(null);
    const problemsContentRef = useRef<HTMLDivElement>(null);
    const usersContentRef = useRef<HTMLDivElement>(null);

    const textColorClassName = getColorClassName(themeColors.textColor);

    const [
        getContestsSearch, {
            data: contestsSearchData,
            isFetching: isContestsSearchFetching,
            error: contestsSearchError,
            isError: areContestsSearchError,
        } ] = useLazyGetContestsSearchQuery();
    const [
        getProblemsSearch, {
            data: problemsSearchData,
            isFetching: isProblemsSearchFetching,
            error: problemsSearchError,
            isError: areProblemsSearchError,
        } ] = useLazyGetProblemsSearchQuery();
    const [
        getUsersSearch, {
            data: usersSearchData,
            isFetching: isUsersSearchFetching,
            error: usersSearchError,
            isError: areUsersSearchError,
        },
    ] = useLazyGetUsersSearchQuery();

    const shouldIncludeContests = searchParams.get('Contests');
    const shouldIncludeProblems = searchParams.get('Problems');
    const shouldIncludeUsers = searchParams.get('Users');

    useEffect(() => {
        const searchTerm = searchParams.get('searchTerm');
        if (searchTerm) {
            dispatch(dispatch(setSearchValue(searchTerm)));
        }
    }, [ dispatch, searchParams ]);

    useEffect(() => {
        setSelectedContestsPage(1);
        setSelectedProblemsPage(1);
        setSelectedUsersPage(1);
    }, [ searchValue ]);

    useEffect(() => {
        if (contestsContentRef.current) {
            setContestsContentHeight(getTotalHeight(contestsContentRef.current));
        }
    }, [ contestsSearchData, isContestsSearchFetching ]);

    useEffect(() => {
        if (problemsContentRef.current) {
            setProblemsContentHeight(getTotalHeight(problemsContentRef.current));
        }
    }, [ problemsSearchData, isProblemsSearchFetching ]);

    useEffect(() => {
        if (usersContentRef.current) {
            setUsersContentHeight(getTotalHeight(usersContentRef.current));
        }
    }, [ usersSearchData, isUsersSearchFetching ]);

    // initiate the search for each of the selected search types
    useEffect(() => {
        if (searchValue.trim().length >= 3 && selectedTerms.includes(CheckboxSearchValues.contests)) {
            getContestsSearch({ searchTerm: searchValue, page: selectedContestsPage, itemsPerPage: ITEMS_PER_SEARCH });
        }
    }, [ searchValue, selectedTerms, selectedContestsPage, getContestsSearch ]);

    useEffect(() => {
        if (searchValue.trim().length >= 3 && selectedTerms.includes(CheckboxSearchValues.problems)) {
            getProblemsSearch({ searchTerm: searchValue, page: selectedProblemsPage, itemsPerPage: ITEMS_PER_SEARCH });
        }
    }, [ searchValue, selectedTerms, selectedProblemsPage, getProblemsSearch ]);

    useEffect(() => {
        if (searchValue.trim().length >= 3 && selectedTerms.includes(CheckboxSearchValues.users)) {
            getUsersSearch({ searchTerm: searchValue, page: selectedUsersPage, itemsPerPage: USER_ITEMS_PER_SEARCH });
        }
    }, [ searchValue, selectedTerms, selectedUsersPage, getUsersSearch ]);

    const onPaginationChange = useCallback((page: number, searchName: SearchTypeEnums) => {
        if (searchName === SearchTypeEnums.CONTESTS) {
            setSelectedContestsPage(page);
        } else if (searchName === SearchTypeEnums.PROBLEMS) {
            setSelectedProblemsPage(page);
        } else if (searchName === SearchTypeEnums.USERS) {
            setSelectedUsersPage(page);
        }

        searchParams.set(`${searchName.toLowerCase()}Page`, page.toString());
        setSearchParams(searchParams, { replace: true });
    }, [ searchParams, setSearchParams ]);

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

        const selectedPageValue = () => {
            if (searchName === SearchTypeEnums.CONTESTS) {
                return selectedContestsPage;
            } if (searchName === SearchTypeEnums.PROBLEMS) {
                return selectedProblemsPage;
            }
            return selectedUsersPage;
        };

        const renderData = () => {
            const renderFunction = (renderElement: any) => {
                if (searchName === SearchTypeEnums.CONTESTS) {
                    return <ContestCard contest={(renderElement as IIndexContestsType)} />;
                } if (searchName === SearchTypeEnums.PROBLEMS) {
                    return <ProblemSearchCard problem={(renderElement as IProblemSearchType)} />;
                }
                return <UserSearchCard user={renderElement as IUSerSearchCardProps} />;
            };

            const contentRef = searchName === SearchTypeEnums.CONTESTS
                ? contestsContentRef
                : searchName === SearchTypeEnums.PROBLEMS
                    ? problemsContentRef
                    : usersContentRef;

            return (
                <div className={styles.searchResultsWrapper} ref={contentRef}>
                    {!data || data.totalItemsCount === 0
                        ? <div>No items found</div>
                        : searchName === SearchTypeEnums.USERS
                            ? <ProfileSearchList data={(data.items as IIndexContestsType[])} />
                            : (
                                <List
                                  values={data.items}
                                  itemFunc={renderFunction}
                                  orientation={Orientation.vertical}
                                />
                            )}
                </div>
            );
        };

        const contentHeight = searchName === SearchTypeEnums.CONTESTS
            ? contestsContentHeight
            : searchName === SearchTypeEnums.PROBLEMS
                ? problemsContentHeight
                : usersContentHeight;

        return (
            <div>
                <div className={styles.searchSectionHeader}>
                    {searchName}
                </div>
                { isError
                    ? renderErrorFragment()
                    : isLoading
                        ? (
                            <div className={styles.spinningLoader} style={{ minHeight: `${contentHeight}px` }}>
                                <SpinningLoader />
                            </div>
                        )
                        : renderData()}
                { data && data.totalItemsCount > 0 && (
                    <PaginationControls
                      count={data?.pagesCount || 0}
                      page={selectedPageValue()}
                      onChange={(page: number) => {
                          onPaginationChange(page, searchName);
                      }}
                    />
                )}
                { searchName !== SearchTypeEnums.USERS && (<hr className={styles.line} />)}
            </div>
        );
    }, [
        contestsContentHeight,
        onPaginationChange,
        problemsContentHeight,
        selectedContestsPage,
        selectedProblemsPage,
        selectedUsersPage,
        usersContentHeight,
    ]);

    return (
        <div className={`${styles.searchPageWrapper} ${textColorClassName}`}>
            <MetaTags
              title={`Search results for "${searchParams.get('searchTerm')}" - SoftUni Judge`}
              description={
                    `Browse SoftUni Judge search results for "${searchParams.get('searchTerm')}". ` +
                    'Find relevant contests, problems, and user profiles.'
                }
            />
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
                            renderSearchFragmentResults(SearchTypeEnums.CONTESTS, contestsSearchData, isContestsSearchFetching, areContestsSearchError, contestsSearchError)}
                        {shouldIncludeProblems &&
                            renderSearchFragmentResults(SearchTypeEnums.PROBLEMS, problemsSearchData, isProblemsSearchFetching, areProblemsSearchError, problemsSearchError)}
                        {shouldIncludeUsers &&
                            renderSearchFragmentResults(SearchTypeEnums.USERS, usersSearchData, isUsersSearchFetching, areUsersSearchError, usersSearchError)}
                    </>
                )}
        </div>
    );
};

export default SearchPage;
