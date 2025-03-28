import React, { useCallback, useEffect, useMemo, useState } from 'react';
import isEmpty from 'lodash/isEmpty';
import isNil from 'lodash/isNil';

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
}

const ProfileContestParticipations = ({ userIsProfileOwner, isChosenInToggle }: IProfileContestParticipationsProps) => {
    const [ shouldRender, setShouldRender ] = useState<boolean>(false);
    const [ userContestParticipationsPage, setUserContestParticipationsPage ] = useState<number>(1);
    const [ selectedContest, setSelectedContest ] = useState<IDropdownItem | null>({ id: 0, name: '' });
    const [ selectedCategory, setSelectedCategory ] = useState<IDropdownItem | null>({ id: 0, name: '' });

    const { internalUser, isLoggedIn } = useAppSelector((reduxState) => reduxState.authorization);
    const { profile } = useAppSelector((reduxState) => reduxState.users);
    const { getColorClassName, themeColors } = useTheme();

    const canFetchParticipations = useMemo(() => {
        if (!isLoggedIn && isNil(profile)) {
            return false;
        }

        if (isLoggedIn && !isNil(profile)) {
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
        if (((userIsProfileOwner || internalUser.canAccessAdministration) && !isChosenInToggle) ||
            areContestParticipationsLoading ||
            isNil(userContestParticipations)) {
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
    }, []);

    // Precompute contests for each category
    const categoryContestsMap = useMemo(() => {
        if (!allParticipatedContests) {
            return new Map<number, IIndexContestsType[]>();
        }

        const categoryMap = new Map<number, IIndexContestsType[]>();

        allParticipatedContests.forEach((contest) => {
            if (contest.categoryId) {
                if (!categoryMap.has(contest.categoryId)) {
                    categoryMap.set(contest.categoryId, []);
                }
                categoryMap.get(contest.categoryId)!.push(contest);
            }
        });

        return categoryMap;
    }, [ allParticipatedContests ]);

    // Precompute dropdown items for each category
    const contestDropdownItemsMap = useMemo(() => {
        const dropdownMap = new Map<number, IDropdownItem[]>();

        categoryContestsMap.forEach((contests, categoryId) => {
            dropdownMap.set(
                categoryId,
                contests.map((contest) => ({
                    id: contest.id,
                    name: `${contest.name} (${contest.category})`,
                })),
            );
        });

        // Add a special entry for "all contests" (category not selected)
        dropdownMap.set(0, allParticipatedContests?.map((contest) => ({
            id: contest.id,
            name: `${contest.name} (${contest.category})`,
        })) || []);

        return dropdownMap;
    }, [ categoryContestsMap, allParticipatedContests ]);

    // Get contests for the selected category efficiently
    const filteredContestDropdownItems = useMemo(
        () => contestDropdownItemsMap
            .get(selectedCategory === null
                ? 0
                : Number(selectedCategory.id)) ||
            [],
        [ selectedCategory, contestDropdownItemsMap ],
    );

    const categoryDropdownItems = useMemo(() => {
        if (!allParticipatedContests) {
            return [];
        }

        const uniqueCategories = new Map<number, string>();
        allParticipatedContests.forEach((contest) => {
            if (contest.categoryId && contest.category && !uniqueCategories.has(contest.categoryId)) {
                uniqueCategories.set(contest.categoryId, contest.category);
            }
        });

        return Array.from(uniqueCategories.entries())
            .map(([ id, name ]) => ({
                id,
                name: `${name} (#${id})`,
            }))
            .sort((a, b) => a.name.localeCompare(b.name));
    }, [ allParticipatedContests ]);

    const handleContestSelect = useCallback((item: IDropdownItem | undefined) => {
        setSelectedContest(item || null);
        setUserContestParticipationsPage(1);
    }, []);

    const handleContestClear = useCallback(() => {
        setSelectedContest(null);
        setUserContestParticipationsPage(1);
    }, []);

    const handleCategorySelect = useCallback((item: IDropdownItem | undefined) => {
        setSelectedCategory(item || null);
        setSelectedContest(null);
        setUserContestParticipationsPage(1);
    }, []);

    const handleCategoryClear = useCallback(() => {
        setSelectedCategory(null);
        setUserContestParticipationsPage(1);
    }, []);

    const renderContestCard = useCallback((contest: IIndexContestsType) => (
        <ContestCard
          key={contest.id}
          contest={contest}
          showPoints={userIsProfileOwner || internalUser.isAdmin}
        />
    ), [ internalUser, userIsProfileOwner ]);

    if (areContestParticipationsLoading || areAllContestsLoading) {
        return (
            <div style={{ ...flexCenterObjectStyles, minHeight: '200px' }}>
                <SpinningLoader />
            </div>
        );
    }

    if (!isNil(contestParticipationsQueryError)) {
        return <span>Error fetching user contest participations</span>;
    }

    if (!shouldRender) {
        return null;
    }

    return (
        <div>
            { ((!isLoggedIn || (isLoggedIn && !userIsProfileOwner && !internalUser.canAccessAdministration)) &&
                userContestParticipations !== undefined &&
                !isNilOrEmpty(userContestParticipations.items) &&
                <h2 className={styles.participationsHeading}>Participated In:</h2>)}
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
                      isDisabled={(selectedContest !== null && selectedContest.id !== 0) && (selectedCategory === null || selectedCategory.id === 0)}
                    />
                </div>
            )}
            <List
              values={userContestParticipations?.items || []}
              itemFunc={renderContestCard}
              orientation={Orientation.vertical}
              fullWidth
            />
            {!isEmpty(userContestParticipations?.items) && userContestParticipations!.pagesCount > 1 && (
                <PaginationControls
                  count={userContestParticipations!.pagesCount}
                  page={userContestParticipations!.pageNumber}
                  onChange={onPageChange}
                />
            )}
            { isChosenInToggle &&
                isNilOrEmpty(userContestParticipations?.items) &&
                (
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
