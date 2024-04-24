/* eslint-disable simple-import-sort/imports */
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

import { defaultPathIdentifier } from '../../common/constants';
import { IContestStrategyFilter } from '../../common/contest-types';
import {
    ICompeteContestResponseType,
    IContestCategory,
    IContestDetailsResponseType,
    IContestsSortAndFilterOptions,
    IGetContestParticipationsForUserQueryParams,
    IIndexContestsType,
    IPagedResultType,
    IRegisterUserForContestResponseType,
} from '../../common/types';
import { IContestResultsType } from '../../hooks/contests/types';
import {
    IContestDetailsUrlParams,
    IGetContestResultsParams,
    ISubmitContestPasswordParams,
    ISubmitContestSolutionParams,
    IRegisterUserForContestParams,
} from '../../common/url-types';

// eslint-disable-next-line import/group-exports
export const contestsService = createApi({
    reducerPath: 'contestService',
    baseQuery: fetchBaseQuery({
        baseUrl: `${import.meta.env.VITE_UI_SERVER_URL}/${defaultPathIdentifier}/`,
        credentials: 'include',
        prepareHeaders: (headers: any) => {
            headers.set('Content-Type', 'application/json');
            return headers;
        },
    }),
    endpoints: (builder) => ({
        getAllContests: builder.query<IPagedResultType<IIndexContestsType>, IContestsSortAndFilterOptions>({
            query: ({ sortType, page, category, strategy }) => ({
                url: '/Contests/GetAll',
                params: {
                    sortType,
                    page,
                    category,
                    strategy,
                },
            }),
        }),
        getContestById: builder.query<IContestDetailsResponseType, IContestDetailsUrlParams>({
            query: ({ id }) => ({ url: `/Contests/Details/${id}` }),
            keepUnusedDataFor: 10,
        }),
        getContestCategories: builder.query<Array<IContestCategory>, void>({
            query: () => ({ url: '/ContestCategories/GetCategoriesTree' }),
            /* eslint-disable object-curly-newline */
        }),
        getContestStrategies: builder.query<IContestStrategyFilter[], void>({
            query: () => ({ url: '/SubmissionTypes/GetAllOrderedByLatestUsage' }),
            /* eslint-disable object-curly-newline */
        }),
        getContestsParticipationsForUser: builder.query<
            IPagedResultType<IIndexContestsType>,
            IGetContestParticipationsForUserQueryParams>({
                query: ({ username, sortType, sortTypeDirection, page, itemsPerPage, category, strategy }) => ({
                    url: `/Contests/GetParticipatedByUser?username=${username}`,
                    params: {
                        sortType,
                        sortTypeDirection,
                        itemsPerPage,
                        page,
                        category,
                        strategy,
                    },
                }),
                keepUnusedDataFor: 0,
            }),
        getContestUserParticipation: builder.query<ICompeteContestResponseType, { id: number; isOfficial: boolean }>({
            query: ({ id, isOfficial }) => ({
                url: `/compete/${id}`,
                params: { isOfficial },
            }),
            keepUnusedDataFor: 0,
        }),
        submitContestSolution: builder.mutation<void, ISubmitContestSolutionParams>({
            query: ({ content, official, problemId, submissionTypeId }) => ({
                url: '/Compete/Submit',
                method: 'POST',
                body: { content, official, problemId, submissionTypeId },
            }),
        }),
        submitContestSolutionFile: builder.mutation<void, ISubmitContestSolutionParams>({
            query: ({ content, official, submissionTypeId, problemId }) => ({
                url: '/Compete/SubmitFileSubmission',
                method: 'POST',
                body: { content, official, problemId, submissionTypeId },
            }),
        }),
        submitContestPassword: builder.mutation<void, ISubmitContestPasswordParams>({
            query: ({ contestId, isOfficial, password }) => ({
                url: `/contests/SubmitContestPassword/${contestId}`,
                method: 'POST',
                params: { isOfficial },
                body: { password },
            }),
        }),
        registerUserForContest: builder.mutation<
            IRegisterUserForContestResponseType,
            IRegisterUserForContestParams>({
                query: ({ password, isOfficial, id, hasConfirmedParticipation }) => ({
                    url: `/compete/${id}/register`,
                    method: 'POST',
                    params: { isOfficial },
                    body: { password, hasConfirmedParticipation },
                }),
            }),
        getContestResults: builder.query<
            IContestResultsType,
            IGetContestResultsParams>({
                query: ({ id, official, full }) => ({
                    url: `/ContestResults/GetResults/${id}`,
                    params: {
                        official,
                        full,
                    },
                }),
            }),
    }),
});

// eslint-disable-next-line import/group-exports
export const {
    useGetAllContestsQuery,
    useGetContestCategoriesQuery,
    useGetContestStrategiesQuery,
    useGetContestByIdQuery,
    useLazyGetContestByIdQuery,
    useLazyGetContestsParticipationsForUserQuery,
    useSubmitContestSolutionMutation,
    useRegisterUserForContestMutation,
    useSubmitContestSolutionFileMutation,
    useGetContestUserParticipationQuery,
    useGetContestResultsQuery,
} = contestsService;
