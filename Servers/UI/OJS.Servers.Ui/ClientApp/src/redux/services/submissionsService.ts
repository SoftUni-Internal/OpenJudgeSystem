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
        baseUrl: `${import.meta.env.VITE_UI_SERVER_URL}/${defaultPathIdentifier}/`,
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
            Record<string, number>,
            null>({ query: () => ({ url: 'Submissions/UnprocessedTotalCount' }) }),
        // eslint-disable-next-line max-len
        getLatestSubmissions: builder.query<
            IPagedResultType<IPublicSubmission>,
            IGetSubmissionsUrlParams>({
                query: ({ status, page }) => (
                    { url: `Submissions/GetSubmissions?status=${status}&page=${page}` }),
            }),
        getLatestSubmissionsInRole: builder.query<
            IPagedResultType<IPublicSubmission>,
            IGetSubmissionsUrlParams>({
                query: ({ status, page }) => (
                    { url: `Submissions/GetSubmissionsForUserInRole?status=${status}&page=${page}` }),
            }),
        getSubmissionResultsByProblem: builder.query<IPagedResultType<IPublicSubmission>, IGetSubmissionsByUserParams>({
            query: ({ id, page, isOfficial }) => ({
                url: `Submissions/GetUserSubmissionsByProblem/${id}`,
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
                    { url: `Submissions/GetUserSubmissions?username=${username}&page=${page}` }),
            }),
        getSubmissionDetails: builder.query<ISubmissionDetailsResponseType, { id: number }>({
            query: ({ id }) => (
                { url: `Submissions/Details/${id}` }),
        }),
        getSubmissionUploadedFile: builder.query<{ blob: Blob }, { id: number }>({
            query: ({ id }) => (
                { url: `Submissions/Download/${id}` }),
        }),
        getSubmissionLogFile: builder.query<{ blob: Blob }, { id: number }>({
            query: ({ id }) => (
                { url: `Submissions/DownloadLogs/${id}` }),
        }),
        retestSubmission: builder.query<
            void,
            IRetestSubmissionUrlParams>({
                query: ({ id, verbosely }) => (
                    {
                        url: `Compete/Retest/${id}?verbosely=${verbosely}`,
                        method: 'POST',
                    }),
            }),
    }),
});

const {
    useLazyGetUnprocessedCountQuery,
    useGetLatestSubmissionsQuery,
    useLazyGetLatestSubmissionsInRoleQuery,
    useGetSubmissionResultsByProblemQuery,
    useGetUserSubmissionsQuery,
    useGetSubmissionDetailsQuery,
    useLazyGetSubmissionUploadedFileQuery,
    useLazyGetSubmissionLogFileQuery,
    useLazyRetestSubmissionQuery,
} = submissionsService;

export {
    useLazyGetUnprocessedCountQuery,
    useGetLatestSubmissionsQuery,
    useGetSubmissionDetailsQuery,
    useLazyGetLatestSubmissionsInRoleQuery,
    useGetSubmissionResultsByProblemQuery,
    useGetUserSubmissionsQuery,
    useLazyGetSubmissionUploadedFileQuery,
    useLazyGetSubmissionLogFileQuery,
    useLazyRetestSubmissionQuery,
};

export default submissionsService;
