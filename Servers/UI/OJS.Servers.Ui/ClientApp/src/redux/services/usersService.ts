 
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

import { defaultPathIdentifier } from '../../common/constants';
import { usersServiceName } from '../../common/reduxNames';
import { IUserProfileType } from '../../common/types';

interface IGetUserProfileUrlParams {
    username: string;
}

const usersService = createApi({
    reducerPath: usersServiceName,
    baseQuery: fetchBaseQuery({
        baseUrl: `${import.meta.env.VITE_UI_SERVER_URL}/${defaultPathIdentifier}/`,
        credentials: 'include',
        prepareHeaders: (headers: any) => {
            headers.set('Content-Type', 'application/json');
            return headers;
        },
    }),
    endpoints: (builder) => ({
        getProfile: builder.query<IUserProfileType, IGetUserProfileUrlParams>({
            query: ({ username }) => (
                { url: `/Users/GetProfileInfo?username=${username}` }),
        }),
    }),
});

export const { useLazyGetProfileQuery } = usersService;

export default usersService;
