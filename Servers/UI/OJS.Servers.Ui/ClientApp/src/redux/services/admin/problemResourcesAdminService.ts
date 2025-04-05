 
import { createApi } from '@reduxjs/toolkit/query/react';

import { IFileModel, IGetAllAdminParams, IPagedResultType, IProblemResouceInLinstModel, IProblemResourceAdministrationModel } from '../../../common/types';
import { CREATE_ENDPOINT, DELETE_ENDPOINT, EXCEL_RESULTS_ENDPOINT, GET_ENDPOINT, UPDATE_ENDPOINT } from '../../../common/urls/administration-urls';
import getCustomBaseQuery from '../../middlewares/customBaseQuery';

export const problemResourcesAdminService = createApi({
    reducerPath: 'problemResources',
    baseQuery: getCustomBaseQuery('problemResources'),
    endpoints: (builder) => ({
        getAllAdminProblemResources: builder.query<IPagedResultType<IProblemResouceInLinstModel>, IGetAllAdminParams>({
            query: ({ filter, page, itemsPerPage, sorting }) => ({
                url: 'GetAll',
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
            keepUnusedDataFor: 0,
        }),

        deleteProblemResource: builder.mutation<string, number >({
            query: (id) => ({
                url: `/${DELETE_ENDPOINT}/${id}`,
                method: 'DELETE',
            }),
        }),

        getProblemResourceById:
        builder.query<IProblemResourceAdministrationModel, number>({
            query: (id) => ({ url: `/${GET_ENDPOINT}/${id}` }),
            keepUnusedDataFor: 0,
        }),

        updateProblemResource: builder.mutation<string, FormData >({
            query: (resource: FormData) => ({
                url: `/${UPDATE_ENDPOINT}`,
                method: 'PATCH',
                body: resource,
            }),
        }),
        downloadResource: builder.query<{ blob: Blob; filename: string }, number>({
            query: (resourceId) => ({ url: `/Download/${resourceId}` }),
            keepUnusedDataFor: 5,
        }),
        createProblemResource: builder.mutation<string, FormData >({
            query: (resource:FormData) => ({
                url: `/${CREATE_ENDPOINT}`,
                method: 'POST',
                body: resource,
            }),
        }),
        exportProblemResourcesToExcel: builder.query<IFileModel, IGetAllAdminParams>({
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
    useGetAllAdminProblemResourcesQuery,
    useDeleteProblemResourceMutation,
    useGetProblemResourceByIdQuery,
    useCreateProblemResourceMutation,
    useUpdateProblemResourceMutation,
    useDownloadResourceQuery,
    useLazyExportProblemResourcesToExcelQuery,
} = problemResourcesAdminService;

export default problemResourcesAdminService;
