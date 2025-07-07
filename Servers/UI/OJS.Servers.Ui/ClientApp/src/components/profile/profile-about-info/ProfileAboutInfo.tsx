import React from 'react';
import { ButtonSize, LinkButton, LinkButtonType } from 'src/components/guidelines/buttons/Button';

import MyProfileSvg from '../../../assets/my-profile.svg';
import { IUserProfileType } from '../../../common/types';
import useTheme from '../../../hooks/use-theme';
import isNilOrEmpty from '../../../utils/check-utils';
import concatClassNames from '../../../utils/class-names';

import styles from './ProfileAboutInfo.module.scss';

interface IProfileAboutInfoProps {
    userProfile: IUserProfileType;
    isUserAdmin : boolean;
    isUserLecturer : boolean;
    isUserProfileOwner: boolean | null;
}

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const ProfileAboutInfo = ({ userProfile, isUserAdmin, isUserLecturer, isUserProfileOwner } : IProfileAboutInfoProps) => {
    const { themeColors, getColorClassName, isDarkMode } = useTheme();

    const firstNameOrLastNameHaveValue = !isNilOrEmpty(userProfile.firstName) || !isNilOrEmpty(userProfile.lastName);

    const aboutInfoClassName = concatClassNames(
        styles.profileAboutInfo,
        getColorClassName(themeColors.textColor),
    );

    return (
        <div className={concatClassNames(
            styles.profileAbout,
            isDarkMode
                ? getColorClassName(themeColors.baseColor200)
                : [ getColorClassName(themeColors.baseColor100), styles.lightAboutInfo ],
        )}
        >
            <div className={aboutInfoClassName}>
                <div className={styles.profileAboutInfoGroupControl}>
                    <span className={styles.profileAboutInfoLabel}>Username:</span>
                    <span className={styles.profileAboutInfoValue}>{userProfile.userName}</span>
                </div>
                {
                    (isUserAdmin || isUserProfileOwner) && firstNameOrLastNameHaveValue && 
                        <div className={styles.profileAboutInfoGroupControl}>
                            <span className={styles.profileAboutInfoLabel}>Name:</span>
                            <span className={styles.profileAboutInfoValue}>
                                {userProfile.firstName}
                                {' '}
                                {userProfile.lastName}
                            </span>
                        </div>
                    
                }
                {
                    (isUserAdmin || isUserProfileOwner) && 
                        <div className={styles.profileAboutInfoGroupControl}>
                            <span className={styles.profileAboutInfoLabel}>Email:</span>
                            <span className={styles.profileAboutInfoValue}>{userProfile.email}</span>
                        </div>
                    
                }
                {
                    isUserAdmin && 
                        <div className={styles.profileAboutInfoGroupControl}>
                            <span className={styles.profileAboutInfoLabel}>Id:</span>
                            <span className={styles.profileAboutInfoValue}>{userProfile.id}</span>
                        </div>
                    
                }
                {
                    (isUserAdmin || isUserProfileOwner) && !isNilOrEmpty(userProfile.age) && 
                        <div className={styles.profileAboutInfoGroupControl}>
                            <span className={styles.profileAboutInfoLabel}>Age:</span>
                            <span className={styles.profileAboutInfoValue}>{userProfile.age}</span>
                        </div>
                    
                }
                {
                    (isUserAdmin || isUserProfileOwner) && !isNilOrEmpty(userProfile.city) &&
                        
                    <div className={styles.profileAboutInfoGroupControl}>
                        <span className={styles.profileAboutInfoLabel}>City:</span>
                        <span className={styles.profileAboutInfoValue}>{userProfile.city}</span>
                    </div>
                        
                }
            </div>

            <div className={styles.imageAndLogoutButtonContainer}>
                <img height={150} width={150} src={MyProfileSvg} alt="my-profile" />
                {
                        isUserProfileOwner && 
                            <LinkButton
                              to="/logout"
                              text="LOG OUT"
                              type={LinkButtonType.secondary}
                              size={ButtonSize.small}
                            />
                        
                    }
            </div>
        </div>
    );
};

export default ProfileAboutInfo;
