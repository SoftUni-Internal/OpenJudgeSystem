import React, { useCallback, useEffect, useState } from 'react';
import isEmpty from 'lodash/isEmpty';
import isNil from 'lodash/isNil';

import { SortType, SortTypeDirection } from '../../../common/contest-types';
import { IGetContestParticipationsForUserQueryParams, IIndexContestsType } from '../../../common/types';
import useTheme from '../../../hooks/use-theme';
import { useLazyGetContestsParticipationsForUserQuery } from '../../../redux/services/contestsService';
import { useAppSelector } from '../../../redux/store';
import isNilOrEmpty from '../../../utils/check-utils';
import concatClassNames from '../../../utils/class-names';
import { flexCenterObjectStyles } from '../../../utils/object-utils';
import ContestCard from '../../contests/contest-card/ContestCard';
import List, { Orientation } from '../../guidelines/lists/List';
import PaginationControls from '../../guidelines/pagination/PaginationControls';
import SpinningLoader from '../../guidelines/spinning-loader/SpinningLoader';

import styles from './ProfileContestParticipations.module.scss';

interface IProfileContestParticipationsProps {
    userIsProfileOwner: boolean;
    isChosenInToggle: boolean;
}

const ProfileContestParticipations = ({ userIsProfileOwner, isChosenInToggle }: IProfileContestParticipationsProps) => {
    const [ shouldRender, setShouldRender ] = useState<boolean>(false);
    const [ userContestParticipationsPage, setUserContestParticipationsPage ] = useState<number>(1);

    const { internalUser, isLoggedIn } = useAppSelector((reduxState) => reduxState.authorization);
    const { profile } = useAppSelector((reduxState) => reduxState.users);
    const { getColorClassName, themeColors } = useTheme();

    const [ getContestsParticipationsQuery, {
        data: userContestParticipations,
        isLoading: areContestParticipationsLoading,
        error: contestParticipationsQueryError,
    } ] = useLazyGetContestsParticipationsForUserQuery();

    useEffect(() => {
        if (
            // If anonymous user but profile is not fetched
            (!isLoggedIn && isNil(profile)) ||
            // User is profile owner but is not chosen in toggle
            (isLoggedIn && !isNil(profile) && !isChosenInToggle && (!userIsProfileOwner && internalUser.canAccessAdministration)) ||
            // User is profile owner but has not chosen in toggle
            (isLoggedIn && !isNil(profile) && !isChosenInToggle && userIsProfileOwner) ||
            // Profile is not fetched
            isNil(profile)) {
            return;
        }

        getContestsParticipationsQuery({
            username: profile?.userName,
            sortType: SortType.ParticipantRegistrationTime,
            sortTypeDirection: SortTypeDirection.Descending,
            page: userContestParticipationsPage,
        } as IGetContestParticipationsForUserQueryParams);
    }, [
        getContestsParticipationsQuery,
        internalUser,
        isChosenInToggle,
        isLoggedIn,
        profile,
        userContestParticipationsPage,
        userIsProfileOwner,
    ]);

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

    const renderContestCard = useCallback((contest: IIndexContestsType) => (
        <ContestCard
          contest={contest}
          showPoints={userIsProfileOwner || internalUser.isAdmin}
        />
    ), [ internalUser, userIsProfileOwner ]);

    const render = useCallback(() => {
        if (areContestParticipationsLoading) {
            return (<div style={{ ...flexCenterObjectStyles }}><SpinningLoader /></div>);
        }

        if (!isNil(contestParticipationsQueryError)) {
            return <span>{contestParticipationsQueryError.data.detail}</span>;
        }

        if (!shouldRender) {
            return null;
        }

        return (
            <div>
                { (!userIsProfileOwner || !internalUser.canAccessAdministration) && !isNilOrEmpty(userContestParticipations.items) &&
                    <h2 className={styles.participationsHeading}>Participated In:</h2>}
                <List
                  values={userContestParticipations!.items!}
                  itemFunc={renderContestCard}
                  orientation={Orientation.vertical}
                  fullWidth
                />
                {!isEmpty(userContestParticipations) && userContestParticipations.pagesCount > 1 && (
                    <PaginationControls
                      count={userContestParticipations.pagesCount}
                      page={userContestParticipations.pageNumber}
                      onChange={onPageChange}
                    />
                )}
                { isChosenInToggle && isEmpty(userContestParticipations.items) &&
                    (
                        <div className={concatClassNames(
                            styles.noParticipationsText,
                            getColorClassName(themeColors.textColor),
                        )}
                        >
                            No participations in contests yet
                        </div>
                    )}
            </div>
        );
    }, [
        areContestParticipationsLoading,
        contestParticipationsQueryError,
        userContestParticipations,
        userIsProfileOwner,
        internalUser,
        isChosenInToggle,
        shouldRender,
        getColorClassName, onPageChange, renderContestCard, themeColors.textColor,
    ]);

    return render();
};

export default ProfileContestParticipations;
