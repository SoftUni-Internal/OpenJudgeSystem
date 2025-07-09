import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

import { defaultPathIdentifier } from '../../common/constants';

 
export const homeStatisticsService = createApi({
    reducerPath: 'homeStatistics',
    baseQuery: fetchBaseQuery({
        baseUrl: `${import.meta.env.VITE_UI_SERVER_URL}/${defaultPathIdentifier}`,
        prepareHeaders: (headers: any) => {
            headers.set('Content-Type', 'application/json');
            return headers;
        },
    }),
    endpoints: (builder) => ({ getHomeStatistics: builder.query<any, void>({ query: () => '/StatisticsPreview/GetForHome' }) }),
});

 
export const { useGetHomeStatisticsQuery } = homeStatisticsService;
