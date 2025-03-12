import { createApi } from '@reduxjs/toolkit/query/react';

import {
    IFileModel,
    IGetAllAdminParams,
    IPagedResultType,
    IReplaceSubmissionTypeModel,
    ISubmissionTypeAdministrationModel,
    ISubmissionTypeInDocument,
    ISubmissionTypeInProblem,
    ISubmissionTypesInListModel,
} from '../../../common/types';
import {
    CREATE_ENDPOINT,
    EXCEL_RESULTS_ENDPOINT, GET_ENDPOINT,
    UPDATE_ENDPOINT,
} from '../../../common/urls/administration-urls';
import getCustomBaseQuery from '../../middlewares/customBaseQuery';

const submissionTypesAdminService = createApi({
    reducerPath: 'submissionTypes',
    baseQuery: getCustomBaseQuery('submissionTypes'),
    endpoints: (builder) => ({
        getForProblem: builder.query<Array<ISubmissionTypeInProblem>, null>({ query: () => ({ url: '/GetForProblem' }) }),
        getForDocument: builder.query<Array<ISubmissionTypeInDocument>, null>({ query: () => ({ url: '/GetForDocument' }) }),
        getAllSubmissionTypes: builder.query<IPagedResultType<ISubmissionTypesInListModel>, IGetAllAdminParams>({
            query: ({ filter, page, itemsPerPage, sorting }) => ({
                url: 'GetAll',
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
            keepUnusedDataFor: 3,
        }),
        deleteSubmissionType: builder.mutation<string, number>({ query: (id) => ({ url: `/Delete/${id}`, method: 'DELETE' }) }),
        getCompilers: builder.query<Array<string>, null>({ query: () => ({ url: '/GetCompilers' }) }),
        getExecutionStrategies: builder.query<Array<string>, null>({ query: () => ({ url: '/GetExecutionStrategies' }) }),
        getById: builder.query<ISubmissionTypeAdministrationModel, number>({
            query: (id) => ({ url: `/${GET_ENDPOINT}/${id}` }),
            keepUnusedDataFor: 0,
        }),
        replaceSubmissionType: builder.mutation<string, IReplaceSubmissionTypeModel>({
            query: (replaceData) => ({
                url: '/ReplaceSubmissionTypes',
                method: 'POST',
                body: replaceData,
            }),
        }),
        updateSubmissionType: builder.mutation<string, ISubmissionTypeAdministrationModel >({
            query: (submissionType) => ({
                url: `/${UPDATE_ENDPOINT}`,
                method: 'PATCH',
                body: submissionType,
            }),
        }),
        createSubmissionType: builder.mutation<string, ISubmissionTypeAdministrationModel >({
            query: (submissionType) => ({
                url: `/${CREATE_ENDPOINT}`,
                method: 'POST',
                body: submissionType,
            }),
        }),
        exportSubmissionTypesToExcel: builder.query<IFileModel, IGetAllAdminParams>({
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
    }),
});

export const {
    useGetForProblemQuery,
    useGetForDocumentQuery,
    useGetAllSubmissionTypesQuery,
    useDeleteSubmissionTypeMutation,
    useGetCompilersQuery,
    useGetExecutionStrategiesQuery,
    useGetByIdQuery,
    useReplaceSubmissionTypeMutation,
    useUpdateSubmissionTypeMutation,
    useCreateSubmissionTypeMutation,
    useLazyExportSubmissionTypesToExcelQuery,
} = submissionTypesAdminService;
export default submissionTypesAdminService;
