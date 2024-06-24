/* eslint-disable jsx-a11y/click-events-have-key-events */
/* eslint-disable jsx-a11y/no-static-element-interactions */

import React, { useCallback, useEffect, useState } from 'react';
import { BsFillMoonFill } from 'react-icons/bs';
import { FaBars } from 'react-icons/fa';
import { RiSunLine } from 'react-icons/ri';
import { Link, useLocation } from 'react-router-dom';
import { ToggleButton, ToggleButtonGroup } from '@mui/material';

import MyProfileSvg from '../../assets/my-profile.svg';
import { NEW_ADMINISTRATION_PATH } from '../../common/urls/administration-urls';
import useTheme from '../../hooks/use-theme';
import { resetInInternalUser, setInternalUser, setIsGetUserInfoCompleted, setIsLoggedIn } from '../../redux/features/authorizationSlice';
import { setIsVisible } from '../../redux/features/searchSlice';
import { useGetUserinfoQuery } from '../../redux/services/authorizationService';
import { useAppDispatch, useAppSelector } from '../../redux/store';

import styles from './PageHeader.module.scss';

const PageHeader = () => {
    const dispatch = useAppDispatch();
    const { pathname } = useLocation();
    const { toggleSelectedTheme } = useTheme();

    const shouldRenderPageHeader = !pathname.includes('administration');

    const { mode } = useAppSelector((state) => state.theme);
    const { isVisible } = useAppSelector((state) => state.search);

    const [ areBurgerItemsOpened, setAreBurgerItemsOpened ] = useState<boolean>(false);
    const {
        isLoggedIn,
        internalUser: user,
    } = useAppSelector((state) => state.authorization);
    const {
        data: userData,
        isSuccess: isSuccessfulRequest,
        isFetching: isUserDataFetching,
    } = useGetUserinfoQuery(null);

    useEffect(() => {
        if (!isUserDataFetching) {
            if (isSuccessfulRequest && userData) {
                dispatch(setInternalUser(userData));
                dispatch(setIsLoggedIn(true));
            } else {
                dispatch(resetInInternalUser());
                dispatch(setIsLoggedIn(false));
            }
        }

        dispatch(setIsGetUserInfoCompleted(!isUserDataFetching));
    }, [ isUserDataFetching, isSuccessfulRequest, userData, dispatch ]);

    useEffect(() => {
        const handleResize = () => {
            if (areBurgerItemsOpened && window.innerWidth > 920) {
                setAreBurgerItemsOpened(false);
            }
        };
        handleResize();
        window.addEventListener('resize', handleResize);

        return () => {
            window.removeEventListener('resize', handleResize);
        };
    }, [ areBurgerItemsOpened ]);

    useEffect(() => {
        if (areBurgerItemsOpened) {
            setAreBurgerItemsOpened(false);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [ pathname ]);

    const renderThemeSwitcher = () => (
        <ToggleButtonGroup value={mode} className={styles.themeSwitchWrapper}>
            <ToggleButton
              value="light"
              onClick={toggleSelectedTheme}
            >
                <RiSunLine />
            </ToggleButton>
            <ToggleButton
              value="dark"
              onClick={toggleSelectedTheme}
            >
                <BsFillMoonFill />
            </ToggleButton>
        </ToggleButtonGroup>
    );

    const renderBurgerMenuItems = useCallback(
        () => {
            const onMenuItemClick = () => {
                setAreBurgerItemsOpened(false);
            };
            return (
                <div className={`${styles.burgerMenuItems} ${areBurgerItemsOpened
                    ? styles.burgerMenuItemsOpened
                    : styles.burgerMenuItemsClosed}`}
                >
                    <Link to="/contests" className={styles.burgerItemWrapper} onClick={onMenuItemClick}>Contests</Link>
                    <Link to="/submissions" className={styles.burgerItemWrapper} onClick={onMenuItemClick}>Submissions</Link>
                    {user.canAccessAdministration && (
                        <Link
                          to={`/${NEW_ADMINISTRATION_PATH}`}
                          target="_blank"
                          className={styles.burgerItemWrapper}
                          onClick={onMenuItemClick}
                        >
                            Administration
                        </Link>
                    )}
                    { isLoggedIn
                        ? (
                            <>
                                <Link to="/profile" className={styles.burgerItemWrapper} onClick={onMenuItemClick}>My Profile</Link>
                                <Link to="/logout" className={styles.burgerItemWrapper} onClick={onMenuItemClick}>Logout</Link>
                            </>
                        )
                        : (
                            <>
                                <Link to="/register" className={styles.burgerItemWrapper} onClick={onMenuItemClick}>Register</Link>
                                <Link to="/login" className={styles.burgerItemWrapper} onClick={onMenuItemClick}>Login</Link>
                            </>
                        )}
                </div>
            );
        },
        [ areBurgerItemsOpened, isLoggedIn, user.canAccessAdministration ],
    );

    const onBurgerClick = (e: any) => {
        e.stopPropagation();
        setAreBurgerItemsOpened(!areBurgerItemsOpened);
    };

    if (!shouldRenderPageHeader) {
        return null;
    }

    const isThemeSwitchVisible = `${import.meta.env.VITE_IS_THEME_SWITCH_VISIBLE}` === 'true';

    return (
        <header className={styles.header}>
            <div>
                <Link to="/" className={`${styles.logoBtn}`}>SoftUni Judge</Link>
                <div className={styles.navButtons}>
                    <Link to="/contests" className={styles.navButton}>CONTESTS</Link>
                    <Link to="/submissions" className={styles.navButton}>SUBMISSIONS</Link>
                    {user.canAccessAdministration &&
                        <Link to={`/${NEW_ADMINISTRATION_PATH}`} target="_blank" className={styles.navButton}>ADMINISTRATION</Link>}
                </div>
            </div>
            <div className={`${styles.authButtons} ${isThemeSwitchVisible
                ? styles.themeSwitchVisible
                : ''}`}
            >
                <i className={`fas fa-search ${styles.searchIcon}`} onClick={() => dispatch(setIsVisible(!isVisible))} />
                {isLoggedIn
                    ? (
                        <div className={`${styles.navButtons} ${styles.profileNavButton}`}>
                            {' '}
                            <Link to="/profile" className={`${styles.navButton}`}>
                                <img height={40} width={40} src={MyProfileSvg} alt="my-profile" />
                            </Link>
                        </div>
                    )
                    : (
                        <div className={styles.navButtons}>
                            <Link to="/login" className={styles.navButton}>LOGIN</Link>
                            <Link to="/register" className={styles.navButton}>
                                REGISTER
                            </Link>
                        </div>
                    )}
                <div
                  className={`${styles.burgerMenu}`}
                  onClick={onBurgerClick}
                >
                    <FaBars size={25} />
                </div>
            </div>
            {renderBurgerMenuItems()}
            {isThemeSwitchVisible && renderThemeSwitcher()}
        </header>
    );
};

export default PageHeader;
