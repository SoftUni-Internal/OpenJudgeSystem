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
        prepareHeaders: (headers) => headers,
        responseHandler: async (response: Response) => {
            const contentType = response.headers.get('Content-Type');

            if (contentType?.includes('application/octet-stream') ||
                contentType?.includes('application/zip')) {
                const contentDisposition = response.headers.get('Content-Disposition');
                let filename = 'file.zip';

                if (contentDisposition) {
                    const match = contentDisposition.match(/filename\*?=\s*UTF-8''(.+?)(;|$)/);
                    if (match) {
                        filename = decodeURIComponent(match[1]);
                    }
                }
                const blob = await response.blob();

                return { blob, fileName: filename };
            }

            if (response.headers.get('Content-Length')) {
                return '';
            }

            return response.json();
        },
    }),
    // baseQuery: getCustomBaseQuery('contests'),
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
            keepUnusedDataFor: 60,
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
            query: ({ content, official, submissionTypeId, problemId }) => {
                const formData = new FormData();
                formData.append('content', content);
                formData.append('official', official
                    ? 'true'
                    : 'false');
                formData.append('problemId', problemId.toString());
                formData.append('submissionTypeId', submissionTypeId.toString());

                return {
                    url: '/Compete/SubmitFileSubmission',
                    method: 'POST',
                    body: formData,
                };
            },
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
            { isRegisteredSuccessfully: boolean },
            IRegisterUserForContestParams>({
                query: ({ password, isOfficial, id, hasConfirmedParticipation }) => ({
                    url: `/compete/${id}/register`,
                    method: 'POST',
                    params: { isOfficial },
                    body: { password, hasConfirmedParticipation },
                }),
            }),
        getRegisteredUserForContest: builder.query<
            IRegisterUserForContestResponseType,
            { id: number; isOfficial: boolean }>({
                query: ({ id, isOfficial }) => ({
                    url: `/compete/${id}/register`,
                    params: { isOfficial },
                }),
                keepUnusedDataFor: 2,
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
        downloadContestProblemResource: builder.query<{ blob: Blob; fileName: string }, { id: number }>({
            query: ({ id }) => ({
                url: `/ProblemResources/GetResource/${id}`,
            }),
            keepUnusedDataFor: 0,
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
    useLazyDownloadContestProblemResourceQuery,
    useGetRegisteredUserForContestQuery,
} = contestsService;
