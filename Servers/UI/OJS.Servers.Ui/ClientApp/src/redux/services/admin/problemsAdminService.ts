import { createApi } from '@reduxjs/toolkit/query/react';

import {
    IFileModel,
    IGetAllAdminParams,
    IIndexProblemsType,
    IPagedResultType,
    IProblemAdministration,
    IProblemRetestValidationType,
    IResourceInListModel,
    ITestsDropdownData,
} from '../../../common/types';
import { IGetByContestId, IGetByParentId, IProblemUrlById } from '../../../common/url-types';
import {
    CREATE_ENDPOINT,
    EXCEL_RESULTS_ENDPOINT,
    GET_ENDPOINT,
    UPDATE_ENDPOINT,
} from '../../../common/urls/administration-urls';
import getCustomBaseQuery from '../../middlewares/customBaseQuery';

const problemsAdminService = createApi({
    reducerPath: 'problems',
    baseQuery: getCustomBaseQuery('problems'),
    endpoints: (builder) => ({
        getAllAdminProblems: builder.query<IPagedResultType<IIndexProblemsType>, IGetAllAdminParams>({
            query: ({ filter, page, itemsPerPage, sorting }) => ({
                url: 'GetAll',
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
            keepUnusedDataFor: 10,
        }),

        getProblemById: builder.query<IProblemAdministration, IProblemUrlById>({
            query: ({ id }) => ({ url: `/${GET_ENDPOINT}/${id}` }),
            keepUnusedDataFor: 0,
        }),

        deleteProblem: builder.mutation<string, number>({ query: (id) => ({ url: `/Delete/${id}`, method: 'DELETE' }) }),

        updateProblem: builder.mutation({
            query: (problem: FormData) => ({
                url: `/${UPDATE_ENDPOINT}`,
                method: 'PATCH',
                body: problem,
            }),
        }),

        createProblem: builder.mutation({
            query: (problem: FormData) => ({
                url: `/${CREATE_ENDPOINT}`,
                method: 'POST',
                body: problem,
            }),
        }),

        getContestProblems: builder.query<IPagedResultType<IIndexProblemsType>, IGetByContestId>({
            query: ({ contestId, filter, page, itemsPerPage, sorting }) => ({
                url: `/GetByContestId/${contestId}`,
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
        }),

        validateRetest: builder.mutation<IProblemRetestValidationType, number>({
            query: (problemId) => ({
                url: `/ValidateRetest/${problemId}`,
                method: 'POST',
            }),
        }),

        retestById: builder.mutation({
            query: (problem) => ({
                url: '/Retest',
                method: 'POST',
                body: problem,
            }),
        }),

        deleteByContest: builder.mutation({
            query: (contestId) => ({
                url: `/DeleteAll/${contestId}`,
                method: 'DELETE',
            }),
        }),

        copyAll: builder.mutation<string, { sourceContestId: number; destinationContestId: number }>({
            query: ({ sourceContestId, destinationContestId }) => ({
                url: 'CopyAll',
                method: 'POST',
                body: { sourceContestId, destinationContestId },
            }),
        }),

        copy: builder.mutation<string, {destinationContestId:number; problemId: number; problemGroupId: number | null} >({
            query: ({ destinationContestId, problemId, problemGroupId }) => ({
                url: 'Copy',
                method: 'POST',
                body: { destinationContestId, problemId, problemGroupId },
            }),
        }),

        getProblemResources: builder.query<IPagedResultType<IResourceInListModel>, IGetByParentId>({
            query: ({ parentId, filter, page, itemsPerPage, sorting }) => ({
                url: `GetResources/${parentId}`,
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
            keepUnusedDataFor: 5,
        }),

        getAllByName: builder.query<Array<ITestsDropdownData>, string>({
            query: (queryString) => ({ url: `/GetAllByName?searchString=${encodeURIComponent(queryString)}` }),
            keepUnusedDataFor: 10,
        }),

        exportProblemsToExcel: builder.query<IFileModel, IGetAllAdminParams>({
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

        // eslint-disable-next-line max-len
        downloadAdditionalFiles: builder.query<{ blob: Blob; filename: string }, number>({ query: (problemId) => ({ url: `/DownloadAdditionalFiles/${problemId}` }) }),
    }),
});

export const {
    useGetAllAdminProblemsQuery,
    useGetProblemByIdQuery,
    useDeleteProblemMutation,
    useUpdateProblemMutation,
    useGetContestProblemsQuery,
    useValidateRetestMutation,
    useRetestByIdMutation,
    useDeleteByContestMutation,
    useCopyAllMutation,
    useCreateProblemMutation,
    useCopyMutation,
    useGetProblemResourcesQuery,
    useLazyExportProblemsToExcelQuery,
    useDownloadAdditionalFilesQuery,

} = problemsAdminService;
export default problemsAdminService;
