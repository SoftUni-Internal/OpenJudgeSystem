import { useCallback, useEffect, useMemo, useState } from 'react';
import isNil from 'lodash/isNil';
import { IGetSubmissionsUrlParams } from 'src/common/url-types';
import { applyDefaultQueryValues } from 'src/components/filters/Filter';
import usePreserveScrollOnSearchParamsChange from 'src/hooks/common/usePreserveScrollOnSearchParamsChange';
import { useSyncQueryParamsFromUrl } from 'src/utils/url-utils';

import { useGetUserSubmissionsQuery } from '../../../redux/services/submissionsService';
import { useAppSelector } from '../../../redux/store';
import SubmissionsGrid from '../../submissions/submissions-grid/SubmissionsGrid';

import styles from './ProfileSubmissions.module.scss';

interface IProfileSubmissionsProps {
    userIsProfileOwner: boolean | null;
    isChosenInToggle: boolean;
}

const ProfileSubmissions = ({ userIsProfileOwner, isChosenInToggle }: IProfileSubmissionsProps) => {
    const { searchParams, setSearchParams } = usePreserveScrollOnSearchParamsChange();
    const [ queryParams, setQueryParams ] = useState<IGetSubmissionsUrlParams>(applyDefaultQueryValues(searchParams));
    const [ shouldRender, setShouldRender ] = useState<boolean>(false);

    const { internalUser, isLoggedIn } = useAppSelector((reduxState) => reduxState.authorization);
    const { profile } = useAppSelector((state) => state.users);

    const canFetchSubmissions = useMemo(() => {
        const isProfileAvailable = !isNil(profile);
        const canAccess = isLoggedIn && isProfileAvailable;
        const hasAdminAccess = internalUser.canAccessAdministration;
        const isOwnerAccessNotAllowed = userIsProfileOwner && !isChosenInToggle;
        const isNonOwnerAccessNotAllowed = !userIsProfileOwner && (!hasAdminAccess || !isChosenInToggle);

        return canAccess && !isOwnerAccessNotAllowed && !isNonOwnerAccessNotAllowed;
    }, [ profile, isLoggedIn, internalUser, isChosenInToggle, userIsProfileOwner ]);

    const {
        data: userSubmissions,
        isLoading: areSubmissionsLoading,
        error: userSubmissionsQueryError,
    } = useGetUserSubmissionsQuery(
        // eslint-disable-next-line @typescript-eslint/no-non-null-asserted-optional-chain
        { ...queryParams, username: profile?.userName! },
        { skip: !canFetchSubmissions },
    );

    useSyncQueryParamsFromUrl(searchParams, setQueryParams);

    useEffect(() => {
        if (!isChosenInToggle || areSubmissionsLoading || isNil(userSubmissions)) {
            setShouldRender(false);
            return;
        }

        setShouldRender(true);
    }, [ areSubmissionsLoading, isChosenInToggle, userSubmissions ]);

    const render = useCallback(() => {
        if (!isNil(userSubmissionsQueryError)) {
            return <span>Error fetching user submissions</span>;
        }

        if (!shouldRender || isNil(userIsProfileOwner)) {
            return null;
        }

        return (
            <SubmissionsGrid
              isDataLoaded={!areSubmissionsLoading}
              submissions={userSubmissions!}
              className={styles.profileSubmissionsGrid}
              options={{
                  showTaskDetails: true,
                  showDetailedResults: internalUser.canAccessAdministration || userIsProfileOwner,
                  showCompeteMarker: true,
                  showSubmissionTypeInfo: false,
                  showParticipantUsername: false,
              }}
              searchParams={searchParams}
              setSearchParams={setSearchParams}
              setQueryParams={setQueryParams}
            />
        );
    }, [
        areSubmissionsLoading,
        internalUser.canAccessAdministration,
        searchParams,
        setSearchParams,
        shouldRender,
        userIsProfileOwner,
        userSubmissions,
        userSubmissionsQueryError ]);

    return render();
};

export default ProfileSubmissions;
