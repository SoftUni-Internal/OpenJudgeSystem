/* eslint-disable simple-import-sort/imports */
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

import { defaultPathIdentifier } from '../../common/constants';
import { submissionsServiceName } from '../../common/reduxNames';
import { IPagedResultType, IPublicSubmission } from '../../common/types';
import {
    IGetSubmissionsUrlParams,
    IGetUserSubmissionsUrlParams,
    IGetSubmissionsByUserParams,
    IRetestSubmissionUrlParams,
} from '../../common/url-types';
import { ISubmissionDetailsResponseType } from '../../hooks/submissions/types';

const submissionsService = createApi({
    reducerPath: submissionsServiceName,
    baseQuery: fetchBaseQuery({
        baseUrl: window.URLS.UI_URL,
        credentials: 'include',
        prepareHeaders: (headers: Headers) => {
            headers.set('Content-Type', 'application/json');
            return headers;
        },
        responseHandler: async (response: Response) => {
            const contentType = response.headers.get('Content-Type');

            if (contentType?.includes('application/octet-stream') ||
                contentType?.includes('application/zip')) {
                const blob = await response.blob();

                return { blob, fileName: 'file.zip' };
            }
            if (response.headers.get('Content-Length')) {
                return '';
            }

            return response.json();
        },
    }),
    endpoints: (builder) => ({
        getUnprocessedCount: builder.query<
            number,
            null>({ query: () => ({ url: `/${defaultPathIdentifier}/Submissions/UnprocessedTotalCount` }) }),
        // eslint-disable-next-line max-len
        getLatestSubmissions: builder.query<
            IPagedResultType<IPublicSubmission>,
            IGetSubmissionsUrlParams>({
                query: ({ status, page }) => (
                    { url: `/${defaultPathIdentifier}/Submissions/GetSubmissions?status=${status}&page=${page}` }),
            }),
        getLatestSubmissionsInRole: builder.query<
            IPagedResultType<IPublicSubmission>,
            IGetSubmissionsUrlParams>({
                query: ({ status, page }) => (
                    { url: `/${defaultPathIdentifier}/Submissions/GetSubmissionsForUserInRole?status=${status}&page=${page}` }),
            }),
        getSubmissionResultsByProblem: builder.query<IPagedResultType<IPublicSubmission>, IGetSubmissionsByUserParams>({
            query: ({ id, page, isOfficial }) => ({
                url: `${defaultPathIdentifier}/Submissions/GetUserSubmissionsByProblem/${id}`,
                params: {
                    isOfficial,
                    page,
                },
            }),
        }),
        getUserSubmissions: builder.query<
            IPagedResultType<IPublicSubmission>,
            IGetUserSubmissionsUrlParams>({
                query: ({ username, page }) => (
                    { url: `/${defaultPathIdentifier}/Submissions/GetUserSubmissions?username=${username}&page=${page}` }),
            }),
        getSubmissionDetails: builder.query<ISubmissionDetailsResponseType, { id: number }>({
            query: ({ id }) => (
                { url: `${defaultPathIdentifier}/Submissions/Details/${id}` }),
        }),
        getSubmissionUploadedFile: builder.query<{ blob: Blob }, { id: number }>({
            query: ({ id }) => (
                { url: `${defaultPathIdentifier}/Submissions/Download/${id}` }),
        }),
        retestSubmission: builder.query<
            void,
            IRetestSubmissionUrlParams>({
                query: ({ id }) => (
                    { url: `/${defaultPathIdentifier}/Compete/Retest/${id}`, method: 'POST' }),
            }),
    }),
});

const {
    useGetUnprocessedCountQuery,
    useGetLatestSubmissionsQuery,
    useGetLatestSubmissionsInRoleQuery,
    useLazyGetSubmissionResultsByProblemQuery,
    useLazyGetUserSubmissionsQuery,
    useGetSubmissionDetailsQuery,
    useLazyGetSubmissionUploadedFileQuery,
    useLazyRetestSubmissionQuery,
} = submissionsService;

export {
    useGetUnprocessedCountQuery,
    useGetLatestSubmissionsQuery,
    useGetSubmissionDetailsQuery,
    useGetLatestSubmissionsInRoleQuery,
    useLazyGetSubmissionResultsByProblemQuery,
    useLazyGetUserSubmissionsQuery,
    useLazyGetSubmissionUploadedFileQuery,
    useLazyRetestSubmissionQuery,
};

export default submissionsService;
