import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useParams } from 'react-router-dom';
import isNil from 'lodash/isNil';

import MetaTags from '../../components/common/MetaTags';
import ErrorWithActionButtons from '../../components/error/ErrorWithActionButtons';
import Breadcrumbs, { IPageBreadcrumbsItem } from '../../components/guidelines/breadcrumb/Breadcrumbs';
import Button, { ButtonType } from '../../components/guidelines/buttons/Button';
import LegacyInfoMessage from '../../components/guidelines/legacy-info-message/LegacyInfoMessage';
import SpinningLoader from '../../components/guidelines/spinning-loader/SpinningLoader';
import ProfileAboutInfo from '../../components/profile/profile-about-info/ProfileAboutInfo';
import ProfileContestParticipations
    from '../../components/profile/profile-contest-participations/ProfileContestParticipations';
import ProfileSubmissions from '../../components/profile/profile-submissions/ProfileSubmisssions';
import useTheme from '../../hooks/use-theme';
import { setProfile } from '../../redux/features/usersSlice';
import { useLazyGetProfileQuery } from '../../redux/services/usersService';
import { useAppDispatch, useAppSelector } from '../../redux/store';
import isNilOrEmpty from '../../utils/check-utils';
import { decodeFromUrlParam } from '../../utils/urls';
import setLayout from '../shared/set-layout';

import styles from './ProfilePage.module.scss';

const ProfilePage = () => {
    const { internalUser, isLoggedIn, isGetUserInfoCompleted } = useAppSelector((reduxState) => reduxState.authorization);
    const { profile } = useAppSelector((reduxState) => reduxState.users);
    const [ toggleValue, setToggleValue ] = useState<number>(1);
    const [ currentUserIsProfileOwner, setCurrentUserIsProfileOwner ] = useState<boolean>(false);

    const { username } = useParams();
    const { themeColors, getColorClassName } = useTheme();
    const dispatch = useAppDispatch();

    // If {username} is present in url, then the the profile should be loaded for this username,
    // otherwise the profile is loaded for the logged in user
    const profileUsername = useMemo(
        () => !isNil(username)
            ? decodeFromUrlParam(username)
            : internalUser.userName,
        [ internalUser, username ],
    );

    const [ getProfileQuery, {
        data: profileInfo,
        isLoading: isProfileInfoLoading,
        error: isError,
    } ] = useLazyGetProfileQuery();

    useEffect(() => {
        if (isGetUserInfoCompleted && !isNilOrEmpty(profileUsername)) {
            getProfileQuery({ username: profileUsername });
        }
    }, [ getProfileQuery, isGetUserInfoCompleted, profileUsername ]);

    useEffect(
        () => {
            if (isNil(profileInfo)) {
                return;
            }

            dispatch(setProfile(profileInfo));
        },
        [ dispatch, profileInfo ],
    );

    useEffect(() => {
        if (!isLoggedIn || isNil(profile)) {
            return;
        }

        setCurrentUserIsProfileOwner(profile.userName === internalUser.userName);
    }, [ internalUser, profile, isLoggedIn ]);

    const renderError = useCallback(() => {
        let text = 'Could not load profile.';

        if (isError) {
            text += ' ';
            text += 'Are you sure this user exists?';
        }

        return (
            <ErrorWithActionButtons
              message={text}
            />
        );
    }, [ isError ]);

    return (
        <>
            <MetaTags
              title={`${currentUserIsProfileOwner
                  ? 'My'
                  : `${profileUsername}'s`} Profile - SoftUni Judge`}
              description={
                    `Explore ${currentUserIsProfileOwner
                        ? 'your'
                        : `${profileUsername}'s`} SoftUni Judge profile. ` +
                    'View submissions, contest participations, and track coding progress.'
                }
            />
            {isProfileInfoLoading || !isGetUserInfoCompleted
                ? (
                    <SpinningLoader />
                )
                : isNil(profile)
                    ? renderError()
                    : (
                        <div className={getColorClassName(themeColors.textColor)}>
                            <Breadcrumbs
                              keyPrefix="profile"
                              items={[
                                {
                                    text: `${currentUserIsProfileOwner
                                        ? 'My'
                                        : ''} Profile`,
                                } as IPageBreadcrumbsItem,
                              ]}
                            />
                            <ProfileAboutInfo
                              userProfile={profile}
                              isUserAdmin={internalUser.isAdmin}
                              isUserLecturer={internalUser.isInRole}
                              isUserProfileOwner={currentUserIsProfileOwner}
                            />
                            {currentUserIsProfileOwner && <LegacyInfoMessage />}
                            {(currentUserIsProfileOwner || internalUser.canAccessAdministration) && (
                            <div className={styles.submissionsAndParticipationsToggle}>
                                <Button
                                  type={toggleValue === 1
                                      ? ButtonType.primary
                                      : ButtonType.secondary}
                                  className={styles.toggleBtn}
                                  text={currentUserIsProfileOwner
                                      ? 'My Submissions'
                                      : 'User Submissions'}
                                  onClick={() => setToggleValue(1)}
                                />
                                <Button
                                  type={toggleValue === 2
                                      ? ButtonType.primary
                                      : ButtonType.secondary}
                                  className={styles.toggleBtn}
                                  text={currentUserIsProfileOwner
                                      ? 'My Contests'
                                      : 'User Contests'}
                                  onClick={() => setToggleValue(2)}
                                />
                            </div>
                            )}
                            <ProfileSubmissions
                              userIsProfileOwner={currentUserIsProfileOwner}
                              isChosenInToggle={toggleValue === 1}
                            />
                            <ProfileContestParticipations
                              userIsProfileOwner={currentUserIsProfileOwner}
                              isChosenInToggle={toggleValue === 2}
                            />
                        </div>
                    )}
        </>
    );
};

export default setLayout(ProfilePage);
