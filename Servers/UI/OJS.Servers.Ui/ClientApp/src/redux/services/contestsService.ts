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
    IProfilePageContests,
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
        }),
        getContestById: builder.query<IContestDetailsResponseType, IContestDetailsUrlParams>({
            query: ({ id }) => ({ url: `/Contests/Details/${id}` }),
            keepUnusedDataFor: 10,
        }),
        getContestCategories: builder.query<Array<IContestCategory>, void>({
            query: () => ({ url: '/ContestCategories/GetCategoriesTree' }),
            /* eslint-disable object-curly-newline */
        }),
        getContestStrategies: builder.query<IContestStrategyFilter[], { contestCategoryId: number }>({
            query: ({ contestCategoryId }) => ({ url: `/SubmissionTypes/GetAllForContestCategory?contestCategoryId=${contestCategoryId}` }),
            /* eslint-disable object-curly-newline */
        }),
        getContestsParticipationsForUser: builder.query<
            IPagedResultType<IIndexContestsType>,
            IGetContestParticipationsForUserQueryParams>({
                query: ({ username, sortType, sortTypeDirection, page, itemsPerPage, category, strategy, contestId, categoryId }) => ({
                    url: `/Contests/GetParticipatedByUser?username=${username}`,
                    params: {
                        sortType,
                        sortTypeDirection,
                        itemsPerPage,
                        page,
                        category,
                        strategy,
                        contestId,
                        categoryId,
                    },
                }),
            }),
        getContestUserParticipation: builder.query<ICompeteContestResponseType, { id: number; isOfficial: boolean }>({
            query: ({ id, isOfficial }) => ({
                url: `/compete/${id}`,
                params: { isOfficial },
            }),
            keepUnusedDataFor: 0,
        }),
        submitContestSolution: builder.mutation<void, ISubmitContestSolutionParams>({
            query: ({ content, official, problemId, submissionTypeId, contestId, isWithRandomTasks, verbosely }) => ({
                url: '/Compete/Submit',
                method: 'POST',
                body: { content, official, problemId, submissionTypeId, contestId, isWithRandomTasks, verbosely },
            }),
        }),
        submitContestSolutionFile: builder.mutation<void, ISubmitContestSolutionParams>({
            query: ({ content, official, submissionTypeId, problemId, contestId, isWithRandomTasks, verbosely }) => {
                const formData = new FormData();
                formData.append('content', content);
                formData.append('official', official
                    ? 'true'
                    : 'false');
                formData.append('problemId', problemId.toString());
                formData.append('submissionTypeId', submissionTypeId.toString());
                formData.append('contestId', contestId.toString());
                formData.append('isWithRandomTasks', isWithRandomTasks
                    ? 'true'
                    : 'false');
                formData.append('verbosely', verbosely
                    ? 'true'
                    : 'false');

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
                query: ({ id, official, full, page }) => ({
                    url: `/ContestResults/GetResults/${id}`,
                    params: {
                        official,
                        full,
                        page,
                    },
                }),
            }),
        getAllParticipatedContests: builder.query<IProfilePageContests[], { username: string }>({
            query: ({ username }) => ({
                url: '/Contests/GetAllParticipatedContests',
                params: { username },
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
    useGetContestsParticipationsForUserQuery,
    useSubmitContestSolutionMutation,
    useRegisterUserForContestMutation,
    useSubmitContestSolutionFileMutation,
    useGetContestUserParticipationQuery,
    useGetContestResultsQuery,
    useLazyDownloadContestProblemResourceQuery,
    useGetRegisteredUserForContestQuery,
    useGetAllParticipatedContestsQuery,
} = contestsService;
