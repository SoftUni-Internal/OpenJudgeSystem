 
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

import { defaultPathIdentifier } from '../../common/constants';
import { authorizationServiceName } from '../../common/reduxNames';
import { IUserResponseType } from '../../common/types';

interface ILoginDetailsType {
    userName: string;
    password: string;
    rememberMe: boolean;
}

const authorizationService = createApi({
    reducerPath: authorizationServiceName,
    baseQuery: fetchBaseQuery({
        baseUrl: `${import.meta.env.VITE_UI_SERVER_URL}/${defaultPathIdentifier}/`,
        credentials: 'include',
        prepareHeaders: (headers: any) => {
            headers.set('Content-Type', 'application/json');
            return headers;
        },
    }),
    endpoints: (builder) => ({
        getUserinfo: builder.query<IUserResponseType, null>({ query: () => ({ url: 'users/getuserAuthInfo' }) }),
        login: builder.mutation<string, ILoginDetailsType>({
            query: (loginDetails) => ({
                url: 'account/Login',
                method: 'POST',
                body: loginDetails,
            }),
        }),
        logOut: builder.mutation<string, null>({
            query: () => ({
                url: 'account/LogOut',
                method: 'POST',
            }),
        }),
    }),
});

export const {
    useLoginMutation,
    useGetUserinfoQuery,
    useLogOutMutation,
} = authorizationService;

export default authorizationService;
