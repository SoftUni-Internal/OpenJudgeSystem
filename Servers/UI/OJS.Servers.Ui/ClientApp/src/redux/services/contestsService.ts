import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

import { defaultPathIdentifier } from '../../common/constants';
import { IContestStrategyFilter } from '../../common/contest-types';
import {
    IGetAllContestsOptions,
    IGetContestsForIndexResponseType, IIndexContestsType,
    IPagedResultType,
} from '../../common/types';

// eslint-disable-next-line import/group-exports
export const contestsService = createApi({
    reducerPath: 'contest',
    baseQuery: fetchBaseQuery({
        baseUrl: `${import.meta.env.VITE_UI_SERVER_URL}/${defaultPathIdentifier}/`,
        credentials: 'include',
        prepareHeaders: (headers: any) => {
            headers.set('Content-Type', 'application/json');
            return headers;
        },
    }),
    endpoints: (builder) => ({
        getAllContests: builder.query<IPagedResultType<IIndexContestsType>, IGetAllContestsOptions>({
            query: ({ status, sortType, page, category, strategy }) => ({
                url: '/Contests/GetAll',
                params: {
                    status,
                    sortType,
                    page,
                    category,
                    strategy,
                },
            }),
        }),
        getIndexContests: builder.query<IGetContestsForIndexResponseType, void>({ query: () => '/Contests/GetForHomeIndex' }),
        getContestCategories: builder.query<any, void>({ query: () => '/ContestCategories/GetCategoriesTree' }),
        getContestStrategies: builder.query<IContestStrategyFilter[], void>({ query: () => '/SubmissionTypes/GetAllOrderedByLatestUsage' }),
        getContestById: builder.query<any, void>({ query: (contestId) => `contests/${contestId}` }),
        getContestByProblemId: builder.query<any, void>({ query: (problemId) => `/contest/${problemId}` }),
    }),
});

// eslint-disable-next-line import/group-exports
export const {
    useGetAllContestsQuery,
    useGetIndexContestsQuery,
    useGetContestCategoriesQuery,
    useGetContestStrategiesQuery,
    useGetContestByIdQuery,
    useGetContestByProblemIdQuery,
} = contestsService;
