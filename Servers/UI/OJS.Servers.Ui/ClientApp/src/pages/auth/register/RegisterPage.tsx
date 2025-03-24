import React, { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { useNavigate } from 'react-router';
import BackToTop from 'src/components/common/back-to-top/BackToTop';

import MetaTags from '../../../components/common/MetaTags';
import { IAuthorizationReduxState } from '../../../redux/features/authorizationSlice';
import { getPlatformRegisterUrl } from '../../../utils/urls';

const RegisterPage = () => {
    const { isLoggedIn } = useSelector((state: {authorization: IAuthorizationReduxState}) => state.authorization);
    const navigate = useNavigate();

    useEffect(() => {
        if (isLoggedIn) {
            navigate('/');
        }

        window.location.href = getPlatformRegisterUrl();
    }, [ isLoggedIn, navigate ]);

    return (
        <>
            <BackToTop />
            <MetaTags
              title="Register - SoftUni Judge"
              description={
                    'Create your SoftUni Judge account to participate in coding contests, ' +
                    'submit solutions, and track your progress. Join our community and improve your skills.'
                }
            />
            <div />
        </>
    );
};

export default RegisterPage;
