import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { NavigateOptions, URLSearchParamsInit } from 'react-router-dom';
import isEmpty from 'lodash/isEmpty';
import isNil from 'lodash/isNil';
import { useDropdownUrlStateSync } from 'src/utils/url-utils';

import { SortType, SortTypeDirection } from '../../../common/contest-types';
import { IDropdownItem, IIndexContestsType } from '../../../common/types';
import useTheme from '../../../hooks/use-theme';
import {
    useGetAllParticipatedContestsQuery,
    useGetContestsParticipationsForUserQuery,
} from '../../../redux/services/contestsService';
import { useAppSelector } from '../../../redux/store';
import isNilOrEmpty from '../../../utils/check-utils';
import concatClassNames from '../../../utils/class-names';
import { flexCenterObjectStyles } from '../../../utils/object-utils';
import ContestCard from '../../contests/contest-card/ContestCard';
import Dropdown from '../../guidelines/dropdown/Dropdown';
import List, { Orientation } from '../../guidelines/lists/List';
import PaginationControls from '../../guidelines/pagination/PaginationControls';
import SpinningLoader from '../../guidelines/spinning-loader/SpinningLoader';

import styles from './ProfileContestParticipations.module.scss';

interface IProfileContestParticipationsProps {
    userIsProfileOwner: boolean | null;
    isChosenInToggle: boolean;
    setSearchParams: (newParams: URLSearchParamsInit, navigateOpts?: NavigateOptions) => void;
    searchParams: URLSearchParams;
}

const ProfileContestParticipations = ({ userIsProfileOwner, isChosenInToggle, setSearchParams, searchParams }: IProfileContestParticipationsProps) => {
    const [ shouldRender, setShouldRender ] = useState<boolean>(false);
    const [ userContestParticipationsPage, setUserContestParticipationsPage ] = useState<number>(1);
    const [ selectedContest, setSelectedContest ] = useState<IDropdownItem | undefined>({ id: 0, name: '' });
    const [ selectedCategory, setSelectedCategory ] = useState<IDropdownItem | undefined>({ id: 0, name: '' });

    const { internalUser, isLoggedIn } = useAppSelector((reduxState) => reduxState.authorization);
    const { profile } = useAppSelector((reduxState) => reduxState.users);
    const { getColorClassName, themeColors } = useTheme();

    const canFetchParticipations = useMemo(() => {
        if (!isLoggedIn && isNil(profile)) {
            return false;
        }

        if (isLoggedIn && profile) {
            if (!isChosenInToggle && userIsProfileOwner) {
                return false;
            }
            if (!isChosenInToggle && !userIsProfileOwner && internalUser.canAccessAdministration) {
                return false;
            }
        }

        return !isNil(profile);
    }, [ isLoggedIn, profile, isChosenInToggle, userIsProfileOwner, internalUser ]);

    const {
        data: userContestParticipations,
        isLoading: areContestParticipationsLoading,
        error: contestParticipationsQueryError,
    } = useGetContestsParticipationsForUserQuery(
        {
            // eslint-disable-next-line @typescript-eslint/no-non-null-asserted-optional-chain
            username: profile?.userName!,
            sortType: SortType.ParticipantRegistrationTime,
            sortTypeDirection: SortTypeDirection.Descending,
            itemsPerPage: 6,
            page: userContestParticipationsPage,
            contestId: selectedContest?.id
                ? Number(selectedContest.id)
                : undefined,
            categoryId: selectedCategory?.id
                ? Number(selectedCategory.id)
                : undefined,
        },
        {
            skip: !canFetchParticipations,
            refetchOnMountOrArgChange: true,
        },
    );

    const {
        data: allParticipatedContests,
        isLoading: areAllContestsLoading,
    } = useGetAllParticipatedContestsQuery(
        // eslint-disable-next-line @typescript-eslint/no-non-null-asserted-optional-chain
        { username: profile?.userName! },
        { skip: !canFetchParticipations },
    );

    useEffect(() => {
        const pageParam = parseInt(searchParams.get('page') || '1', 10);
        if (Number.isInteger(pageParam) && pageParam >= 1) {
            setUserContestParticipationsPage(pageParam);
        }
    }, [ searchParams ]);

    useEffect(() => {
        if (
            ((userIsProfileOwner || internalUser.canAccessAdministration) && !isChosenInToggle) ||
            areContestParticipationsLoading ||
            isNil(userContestParticipations)
        ) {
            setShouldRender(false);
            return;
        }

        setShouldRender(true);
    }, [
        areContestParticipationsLoading,
        userContestParticipations,
        internalUser,
        isChosenInToggle,
        isLoggedIn,
        profile,
        userIsProfileOwner,
    ]);

    const onPageChange = useCallback((page: number) => {
        setUserContestParticipationsPage(page);
        const newParams = new URLSearchParams(searchParams.toString());
        newParams.set('page', page.toString());
        setSearchParams(newParams);
    }, [ searchParams, setSearchParams ]);

    const categoryContestsMap = useMemo(() => {
        const map = new Map<number, IIndexContestsType[]>();
        allParticipatedContests?.forEach((contest) => {
            if (contest.categoryId) {
                if (!map.has(contest.categoryId)) { map.set(contest.categoryId, []); }
                map.get(contest.categoryId)!.push(contest);
            }
        });
        return map;
    }, [ allParticipatedContests ]);

    const contestDropdownItemsMap = useMemo(() => {
        const map = new Map<number, IDropdownItem[]>();
        categoryContestsMap.forEach((contests, categoryId) => {
            map.set(categoryId, contests.map((c) => ({ id: c.id, name: `${c.name} (${c.category})` })));
        });
        map.set(0, allParticipatedContests?.map((c) => ({ id: c.id, name: `${c.name} (${c.category})` })) || []);
        return map;
    }, [ categoryContestsMap, allParticipatedContests ]);

    const filteredContestDropdownItems = useMemo(
        () => contestDropdownItemsMap.get(selectedCategory?.id
            ? Number(selectedCategory.id)
            : 0) || [],
        [ selectedCategory, contestDropdownItemsMap ],
    );

    const categoryDropdownItems = useMemo(() => {
        const map = new Map<number, string>();
        allParticipatedContests?.forEach((contest) => {
            if (contest.categoryId && contest.category) {
                map.set(contest.categoryId, contest.category);
            }
        });
        return Array.from(map.entries())
            .map(([ id, name ]) => ({ id, name: `${name} (#${id})` }))
            .sort((a, b) => a.name.localeCompare(b.name));
    }, [ allParticipatedContests ]);

    const { updateUrl: updateContestUrl } = useDropdownUrlStateSync<IDropdownItem>(
        'contestId',
        filteredContestDropdownItems,
        setSelectedContest,
        setSearchParams,
        searchParams,
    );

    const { updateUrl: updateCategoryUrl, batchUpdateUrl: batchUpdateCategoryUrl } = useDropdownUrlStateSync<IDropdownItem>(
        'categoryId',
        categoryDropdownItems,
        setSelectedCategory,
        setSearchParams,
        searchParams,
        true,
    );

    const handleContestSelect = useCallback((item: IDropdownItem | undefined) => {
        const selected = item || undefined;
        setSelectedContest(selected);
        updateContestUrl(selected);
        setUserContestParticipationsPage(1);
    }, [ updateContestUrl ]);

    const handleContestClear = useCallback(() => {
        setSelectedContest(undefined);
        updateContestUrl(undefined);
        setUserContestParticipationsPage(1);
    }, [ updateContestUrl ]);

    const handleCategorySelect = useCallback((item: IDropdownItem | undefined) => {
        const selected = item || undefined;
        setSelectedCategory(selected);
        updateCategoryUrl(selected);
        setUserContestParticipationsPage(1);
    }, [ updateCategoryUrl ]);

    const handleCategoryClear = useCallback(() => {
        setSelectedCategory(undefined);
        updateCategoryUrl(undefined);
        batchUpdateCategoryUrl?.([
            { key: 'contestId', value: undefined },
            { key: 'categoryId', value: undefined },
        ]);
        setUserContestParticipationsPage(1);
    }, [ batchUpdateCategoryUrl, updateCategoryUrl ]);

    const renderContestCard = useCallback((contest: IIndexContestsType) => (
        <ContestCard
          key={contest.id}
          contest={contest}
          showPoints={userIsProfileOwner || internalUser.isAdmin}
        />
    ), [ internalUser, userIsProfileOwner ]);

    if (areContestParticipationsLoading || areAllContestsLoading) {
        return <div style={{ ...flexCenterObjectStyles, minHeight: '200px' }}><SpinningLoader /></div>;
    }

    if (!isNil(contestParticipationsQueryError)) {
        return <span>Error fetching user contest participations</span>;
    }

    if (!shouldRender) { return null; }

    return (
        <div>
            {(!isLoggedIn || (!userIsProfileOwner && !internalUser.canAccessAdministration)) &&
                userContestParticipations &&
                !isNilOrEmpty(userContestParticipations.items) && (
                    <h2 className={styles.participationsHeading}>Participated In:</h2>
            )}
            {isChosenInToggle && (
                <div className={styles.filterContainer}>
                    <Dropdown
                      dropdownItems={filteredContestDropdownItems}
                      value={selectedContest ?? { id: 0, name: '' }}
                      handleDropdownItemClick={handleContestSelect}
                      handleDropdownItemClear={handleContestClear}
                      placeholder="Filter by contest"
                      isSearchable
                    />
                    <Dropdown
                      dropdownItems={categoryDropdownItems}
                      value={selectedCategory ?? { id: 0, name: '' }}
                      handleDropdownItemClick={handleCategorySelect}
                      handleDropdownItemClear={handleCategoryClear}
                      placeholder="Filter by category"
                      isSearchable
                      isDisabled={
                            (selectedContest && selectedContest.id !== 0) &&
                            (!selectedCategory || selectedCategory.id === 0)
                        }
                    />
                </div>
            )}
            <List
              values={userContestParticipations?.items || []}
              itemFunc={renderContestCard}
              orientation={Orientation.vertical}
              fullWidth
            />
            {!isEmpty(userContestParticipations?.items) &&
                userContestParticipations && userContestParticipations.pagesCount > 1 && (
                    <PaginationControls
                      count={userContestParticipations.pagesCount}
                      page={userContestParticipations.pageNumber}
                      onChange={onPageChange}
                    />
            )}
            {isChosenInToggle && isNilOrEmpty(userContestParticipations?.items) && (
                <div className={concatClassNames(
                    styles.noParticipationsText,
                    getColorClassName(themeColors.textColor),
                )}
                >
                    You have not participated in any contests yet.
                </div>
            )}
        </div>
    );
};

export default ProfileContestParticipations;
