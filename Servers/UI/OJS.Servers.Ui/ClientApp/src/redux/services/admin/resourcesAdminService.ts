 
import { createApi } from '@reduxjs/toolkit/query/react';

import {
    IContestResourceInListModel, IFileModel,
    IGetAllAdminParams,
    IPagedResultType,
    IProblemResourceAdministrationModel,
    IProblemResourceInListModel,
} from '../../../common/types';
import { CREATE_ENDPOINT, DELETE_ENDPOINT, EXCEL_RESULTS_ENDPOINT, GET_ENDPOINT, UPDATE_ENDPOINT } from '../../../common/urls/administration-urls';
import getCustomBaseQuery from '../../middlewares/customBaseQuery';

export const resourcesAdminService = createApi({
    reducerPath: 'resources',
    baseQuery: getCustomBaseQuery('resources'),
    endpoints: (builder) => ({
        getAllAdminContestResources: builder.query<IPagedResultType<IContestResourceInListModel>, IGetAllAdminParams>({
            query: ({ filter, page, itemsPerPage, sorting }) => ({
                url: 'GetAllContestResources',
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
            keepUnusedDataFor: 0,
        }),

        getAllAdminProblemResources: builder.query<IPagedResultType<IProblemResourceInListModel>, IGetAllAdminParams>({
            query: ({ filter, page, itemsPerPage, sorting }) => ({
                url: 'GetAllProblemResources',
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
            keepUnusedDataFor: 0,
        }),

        deleteResource: builder.mutation<string, number >({
            query: (id) => ({
                url: `/${DELETE_ENDPOINT}/${id}`,
                method: 'DELETE',
            }),
        }),

        getResourceById:
        builder.query<IProblemResourceAdministrationModel, number>({
            query: (id) => ({ url: `/${GET_ENDPOINT}/${id}` }),
            keepUnusedDataFor: 0,
        }),

        updateResource: builder.mutation<string, FormData >({
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
        createResource: builder.mutation<string, FormData >({
            query: (resource:FormData) => ({
                url: `/${CREATE_ENDPOINT}`,
                method: 'POST',
                body: resource,
            }),
        }),
        exportResourcesToExcel: builder.query<IFileModel, IGetAllAdminParams>({
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
    useGetAllAdminContestResourcesQuery,
    useGetAllAdminProblemResourcesQuery,
    useDeleteResourceMutation,
    useGetResourceByIdQuery,
    useCreateResourceMutation,
    useUpdateResourceMutation,
    useDownloadResourceQuery,
    useLazyExportResourcesToExcelQuery,
} = resourcesAdminService;

export default resourcesAdminService;
