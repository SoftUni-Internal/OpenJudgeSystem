/* eslint-disable prefer-destructuring */
/* eslint-disable max-len */
import { createApi } from '@reduxjs/toolkit/query/react';

import {
    IExamGroupAdministration,
    IFileModel,
    IGetAllAdminParams,
    IIndexExamGroupsType,
    IPagedResultType,
} from '../../../common/types';
import { IExamGroupUrlParams } from '../../../common/url-types';
import { EXCEL_RESULTS_ENDPOINT, GET_ENDPOINT } from '../../../common/urls/administration-urls';
import getCustomBaseQuery from '../../middlewares/customBaseQuery';

// eslint-disable-next-line import/group-exports
export const examGroupsService = createApi({
    reducerPath: 'examGroups',
    baseQuery: getCustomBaseQuery('examGroups'),
    endpoints: (builder) => ({
        getAllAdminExamGroups: builder.query<IPagedResultType<IIndexExamGroupsType>, IGetAllAdminParams>({
            query: ({ filter, page, itemsPerPage, sorting }) => ({
                url: '/getAll',
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
            keepUnusedDataFor: 10,
        }),
        deleteUserFromExamGroup: builder.mutation({
            query: (userInExamGroupDeleteModel) => ({
                url: '/RemoveUserFromExamGroup',
                method: 'POST',
                body: userInExamGroupDeleteModel,
            }),
        }),
        addUserInExamGroupById: builder.mutation({
            query: (userInExamGroupCreateModel) => ({
                url: '/AddToExamGroup',
                method: 'POST',
                body: userInExamGroupCreateModel,
            }),
        }),
        addBulkUsersInExamGroupById: builder.mutation({
            query: (bulkAddUsersToExamGroupModel) => ({
                url: '/AddMultipleUsersToExamGroup',
                method: 'POST',
                body: bulkAddUsersToExamGroupModel,
            }),
        }),
        getExamGroupById: builder.query<IExamGroupAdministration, IExamGroupUrlParams>({ query: ({ id }) => ({ url: `/${GET_ENDPOINT}/${id}` }), keepUnusedDataFor: 0 }),
        deleteExamGroup: builder.mutation<string, number >({ query: (id) => ({ url: `/Delete/${id}`, method: 'DELETE' }) }),
        updateExamGroup: builder.mutation<string, IExamGroupAdministration >({ query: ({ ...examGroupAdministrationModel }) => ({ url: '/Edit', method: 'PATCH', body: examGroupAdministrationModel }) }),
        createExamGroup: builder.mutation<string, IExamGroupUrlParams & IExamGroupAdministration >({ query: ({ ...examGroupAdministrationModel }) => ({ url: '/Create', method: 'POST', body: examGroupAdministrationModel }) }),

        exportExamGroupsToExcel: builder.query<IFileModel, IGetAllAdminParams>({
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

// eslint-disable-next-line import/group-exports
export const {
    useGetAllAdminExamGroupsQuery,
    useGetExamGroupByIdQuery,
    useDeleteExamGroupMutation,
    useUpdateExamGroupMutation,
    useCreateExamGroupMutation,
    useAddUserInExamGroupByIdMutation,
    useAddBulkUsersInExamGroupByIdMutation,
    useDeleteUserFromExamGroupMutation,
    useLazyExportExamGroupsToExcelQuery,
} = examGroupsService;
export default examGroupsService;
