/* eslint-disable simple-import-sort/imports */
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

import { defaultPathIdentifier } from '../../common/constants';
import { IPagedResultType, IPublicSubmission } from '../../common/types';
import {
    IRetestSubmissionUrlParams,
    IGetRecentSubmissionsUrlParams,
    IGetProfileSubmissionsUrlParams,
    IGetSubmissionResultsByProblemUrlParams,
} from '../../common/url-types';
import { ISubmissionDetailsResponseType } from '../../hooks/submissions/types';
import { submissionsServiceName } from '../../common/reduxNames';

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

            // Return empty string if response is empty. It's important to explicitly check for 0 length.
            if (response.headers.get('Content-Length') === '0') {
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
            IGetRecentSubmissionsUrlParams>({
                query: ({ status, itemsPerPage, page, filter, sorting }) => (
                    {
                        url: 'Submissions/GetSubmissions',
                        params: { status, itemsPerPage, page, filter, sorting },
                    }),
            }),
        getLatestSubmissionsInRole: builder.query<
            IPagedResultType<IPublicSubmission>,
            IGetRecentSubmissionsUrlParams>({
                query: ({ status, itemsPerPage, page, filter, sorting }) => (
                    {
                        url: 'Submissions/GetSubmissionsForUserInRole',
                        params: { status, itemsPerPage, page, filter, sorting },
                    }),
            }),
        getSubmissionResultsByProblem: builder.query<IPagedResultType<IPublicSubmission>, IGetSubmissionResultsByProblemUrlParams>({
            query: ({ problemId, isOfficial, itemsPerPage, page, filter, sorting }) => ({
                url: `Submissions/GetUserSubmissionsByProblem/${problemId}`,
                params: { isOfficial, itemsPerPage, page, filter, sorting },
            }),
        }),
        getUserSubmissions: builder.query<
            IPagedResultType<IPublicSubmission>,
            IGetProfileSubmissionsUrlParams>({
                query: ({ username, itemsPerPage, page, filter, sorting }) => (
                    {
                        url: 'Submissions/GetUserSubmissions',
                        params: { username, itemsPerPage, page, filter, sorting },
                    }),
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
