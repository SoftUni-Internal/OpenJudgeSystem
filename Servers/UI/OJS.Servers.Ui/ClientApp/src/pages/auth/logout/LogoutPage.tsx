import React, { useEffect } from 'react';
import { useDispatch } from 'react-redux';
import BackToTop from 'src/components/common/back-to-top/BackToTop';
import { clearAllMessages } from 'src/redux/features/mentorSlice';

import useTheme from '../../../hooks/use-theme';
import cacheService from '../../../redux/cacheService';
import { resetInInternalUser, setIsLoggedIn } from '../../../redux/features/authorizationSlice';
import { useLogOutMutation } from '../../../redux/services/authorizationService';
import { persistor } from '../../../redux/store';
import concatClassNames from '../../../utils/class-names';
import wait from '../../../utils/promise-utils';

import styles from './LogoutPage.module.scss';

const LogoutPage = () => {
    const [ logout ] = useLogOutMutation();
    const dispatch = useDispatch();
    const { getColorClassName, themeColors } = useTheme();
    const { resetCache } = cacheService();

    useEffect(() => {
        (async () => {
            await logout(null);

            dispatch(resetInInternalUser());
            dispatch(setIsLoggedIn(false));
            dispatch(clearAllMessages());

            await persistor.purge();
            resetCache();
            await wait(0.7);
            window.location.href = '/';
        })();
        // Adding resetCache as dependency results in logout request being made multiple times
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [ logout ]);

    return (
        <>
            <BackToTop />
            <div className={concatClassNames(getColorClassName(themeColors.textColor), styles.logout)}>
                You are now successfully logged out and will be redirected to home page shortly.
            </div>
        </>
    );
};

export default LogoutPage;
