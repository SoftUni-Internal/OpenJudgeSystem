import React, { FC, useCallback } from 'react';
import { useSelector } from 'react-redux';
import {
    Navigate,
    useLocation,
} from 'react-router';

import { Anything } from '../../common/common-types';
import { IHaveChildrenProps } from '../../components/common/Props';
import { IAuthorizationReduxState } from '../../redux/features/authorizationSlice';

type IPrivatePageProps = IHaveChildrenProps

const PrivatePage = ({ children }: IPrivatePageProps) => {
    const location = useLocation();
    const { isLoggedIn } = useSelector((state: {authorization: IAuthorizationReduxState}) => state.authorization);

    const renderPageOrRedirectToLogin = useCallback(() => {
        if (!isLoggedIn) {
            const state = { from: location };

            return <Navigate to="/login" state={state} replace />;
        }

        // eslint-disable-next-line react/jsx-no-useless-fragment
        return <>{children}</>;
    }, [ isLoggedIn, children, location ]);

    return renderPageOrRedirectToLogin();
};

const makePrivate = (ComponentToWrap: FC) => (props: Anything) => (
    <PrivatePage>
        <ComponentToWrap {...props} />
    </PrivatePage>
);

export default makePrivate;
