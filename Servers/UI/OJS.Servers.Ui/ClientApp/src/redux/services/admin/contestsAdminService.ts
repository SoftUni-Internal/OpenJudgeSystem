import { createApi } from '@reduxjs/toolkit/query/react';

import {
    IContestActivity, IContestAdministration, IContestAutocomplete, IContestsBulkEdit, IFileModel, IGetAllAdminParams,
    IIndexContestsType,
    IPagedResultType,
} from '../../../common/types';
import { IContestDetailsUrlParams } from '../../../common/url-types';
import { CREATE_ENDPOINT, DELETE_ENDPOINT, EXCEL_RESULTS_ENDPOINT, GET_ALL_ENDPOINT, GET_ENDPOINT, UPDATE_ENDPOINT } from '../../../common/urls/administration-urls';
import { SimillarityType } from '../../../pages/administration-new/submissions-simillarity/SubmissionsSimillarity';
import getCustomBaseQuery from '../../middlewares/customBaseQuery';

const contestService = createApi({
    reducerPath: 'contestsAdminService',
    baseQuery: getCustomBaseQuery('contests'),
    endpoints: (builder) => ({
        getAllAdminContests: builder.query<IPagedResultType<IIndexContestsType>, IGetAllAdminParams>({
            query: ({ filter, page, itemsPerPage, sorting }) => ({
                url: `/${GET_ALL_ENDPOINT}`,
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
            keepUnusedDataFor: 3,
        }),

        getForLecturerInContest: builder.query<Array<IContestAutocomplete>, string>({
            query: (id) => ({ url: `/GetForLecturerInContest?userId=${id}` }),
            keepUnusedDataFor: 0,
        }),

        getContestById: builder.query<IContestAdministration, IContestDetailsUrlParams>({
            query: ({ id }) => ({ url: `/${GET_ENDPOINT}/${id}` }),
            keepUnusedDataFor: 0,
        }),

        deleteContest: builder.mutation<string, number >({ query: (id) => ({ url: `/${DELETE_ENDPOINT}/${id}`, method: 'DELETE' }) }),

        updateContest: builder.mutation<string, IContestAdministration >({
            query: ({ ...contestAdministrationModel }) => ({
                url: `/${UPDATE_ENDPOINT}`,
                method: 'PATCH',
                body: contestAdministrationModel,
            }),
        }),

        createContest: builder.mutation<string, IContestDetailsUrlParams & IContestAdministration >({
            query: ({ ...contestAdministrationModel }) => ({
                url: `/${CREATE_ENDPOINT}`,
                method: 'POST',
                body: contestAdministrationModel,
            }),
        }),

        getContestAutocomplete: builder.query<Array<IContestAutocomplete>, string>({
            query: (queryString) => ({ url: `/GetAllForProblem?searchString=${encodeURIComponent(queryString)}` }),
            keepUnusedDataFor: 10,
        }),

        downloadResults: builder.mutation<{ blob: Blob; filename: string }, {id:number; type:number} >({
            query: ({ ...contestAdministrationModel }) => ({
                url: '/ExportResults',
                method: 'POST',
                body: contestAdministrationModel,
            }),
        }),

        downloadSubmissions: builder.mutation<{ blob: Blob; filename: string },
        {
            contestId: number;
            contestExportResultType:number;
            submissionExportType:number;
        }>({
            query: ({ ...contestAdministrationModel }) => ({
                url: '/DownloadSubmissions',
                method: 'POST',
                body: contestAdministrationModel,
            }),
        }),

        exportContestsToExcel: builder.query<IFileModel, IGetAllAdminParams>({
            query: ({ filter, page, itemsPerPage, sorting }) => ({
                url: `/${EXCEL_RESULTS_ENDPOINT}`,
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
            keepUnusedDataFor: 0,
        }),

        getContestActivity: builder.query<IContestActivity, number>({
            query: (id) => ({ url: `/Activity?contestId=${id}` }),
            keepUnusedDataFor: 5,
        }),

        exportSimilaritiesToExcel: builder.mutation<IFileModel, {contestIds:Array<number>; similarityCheckType: SimillarityType} >({
            query: ({ ...similarityCheckModel }) => ({
                url: '/CheckSimilarity',
                method: 'POST',
                body: similarityCheckModel,
            }),
        }),

        transferParticipants: builder.mutation<string, number>({
            query: (id) => ({
                url: `/TransferParticipants?contestId=${id}`,
                method: 'PATCH',
            }),
        }),

        bulkEditContests: builder.mutation<string, IContestsBulkEdit>({
            query: (model) => ({
                url: '/BulkEditContests',
                method: 'POST',
                body: model,
            }),
        }),
    }),
});

export const {
    useGetAllAdminContestsQuery,
    useGetForLecturerInContestQuery,
    useGetContestByIdQuery,
    useDeleteContestMutation,
    useUpdateContestMutation,
    useCreateContestMutation,
    useDownloadResultsMutation,
    useDownloadSubmissionsMutation,
    useGetContestAutocompleteQuery,
    useLazyExportContestsToExcelQuery,
    useGetContestActivityQuery,
    useExportSimilaritiesToExcelMutation,
    useTransferParticipantsMutation,
    useBulkEditContestsMutation,
} = contestService;
export default contestService;
