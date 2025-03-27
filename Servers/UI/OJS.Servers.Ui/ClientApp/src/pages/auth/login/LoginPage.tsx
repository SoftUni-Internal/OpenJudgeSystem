import React, { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { useLocation, useNavigate } from 'react-router-dom';
import BackToTop from 'src/components/common/back-to-top/BackToTop';

import LoginForm from '../../../components/auth/LoginForm';
import MetaTags from '../../../components/common/MetaTags';
import { IAuthorizationReduxState } from '../../../redux/features/authorizationSlice';

const LoginPage = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const { isLoggedIn, internalUser: user } =
    useSelector((state: {authorization: IAuthorizationReduxState}) => state.authorization);
    useEffect(() => {
        if (isLoggedIn) {
            // Needed ignore...
            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore
            const origin = location.state?.from?.pathname || '/';
            navigate(origin);
        }
    }, [ isLoggedIn, location, navigate, user ]);

    return (
        <>
            <BackToTop />
            <MetaTags
              title="Login - SoftUni Judge"
              description={
                    'Access your SoftUni Judge account to join coding contests, submit solutions, ' +
                    'and track your progress. Enhance your programming skills through challenges.'
                }
            />
            <LoginForm />
        </>
    );
};

export default LoginPage;
