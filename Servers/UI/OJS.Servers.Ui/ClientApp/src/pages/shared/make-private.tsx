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
    const { isLoggedIn } =
    useSelector((state: {authorization: IAuthorizationReduxState}) => state.authorization);

    const renderPageOrRedirectToLogin = useCallback(() => {
        if (!isLoggedIn) {
            const state = { from: location };

            return <Navigate to="/login" state={state} />;
        }

        // eslint-disable-next-line react/jsx-no-useless-fragment
        return <>{children}</>;
    }, [ location, isLoggedIn, children ]);

    return renderPageOrRedirectToLogin();
};

const makePrivate = (ComponentToWrap: FC) => (props: Anything) => (
    <PrivatePage>
        {/* eslint-disable-next-line react/jsx-props-no-spreading */}
        <ComponentToWrap {...props} />
    </PrivatePage>
);

export default PrivatePage;
export {
    makePrivate,
};
