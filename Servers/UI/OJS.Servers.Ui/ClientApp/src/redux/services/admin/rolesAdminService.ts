import { createApi } from '@reduxjs/toolkit/query/react';

import { IFileModel, IGetAllAdminParams, IPagedResultType, IRoleAdministrationModel, IRoleInListModel } from '../../../common/types';
import { CREATE_ENDPOINT, DELETE_ENDPOINT, EXCEL_RESULTS_ENDPOINT, GET_ENDPOINT, UPDATE_ENDPOINT } from '../../../common/urls/administration-urls';
import customBaseQuery from '../../middlewares/customBaseQuery';

const rolesAdminService = createApi({
    reducerPath: 'roles',
    baseQuery: customBaseQuery('roles'),
    endpoints: (builder) => ({
        getAllRoles: builder.query<IPagedResultType<IRoleInListModel>, IGetAllAdminParams>({
            query: ({ filter, page, itemsPerPage, sorting }) => ({
                url: 'GetAll',
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
            keepUnusedDataFor: 5,
        }),

        getRoleById: builder.query<IRoleAdministrationModel, string>({
            query: (id) => ({ url: `/${GET_ENDPOINT}/${id}` }),
            keepUnusedDataFor: 0,
        }),

        getIdByName: builder.query<string, string>({ query: (name) => ({ url: `/GetIdByName?roleName=${name}` }) }),

        updateRole: builder.mutation<string, IRoleAdministrationModel >({
            query: (role) => ({
                url: `/${UPDATE_ENDPOINT}`,
                method: 'PATCH',
                body: role,
            }),
        }),

        createRole: builder.mutation<string, IRoleAdministrationModel >({
            query: (role) => ({
                url: `/${CREATE_ENDPOINT}`,
                method: 'POST',
                body: role,
            }),
        }),

        addUserToRole: builder.mutation<string, {userId: string; roleId: string} >({
            query: ({ userId, roleId }) => ({
                url: '/AddUserToRole',
                method: 'POST',
                body: { userId, roleId },
            }),
        }),

        removeUserFromRole: builder.mutation<string, {userId: string; roleId: string} >({
            query: ({ userId, roleId }) => ({
                url: `/RemoveFromRole?userId=${userId}&roleId=${roleId}`,
                method: 'DELETE',
            }),
        }),

        deleteRoles: builder.mutation<string, string >({ query: (id) => ({ url: `/${DELETE_ENDPOINT}/${id}`, method: 'DELETE' }) }),

        exportRolesToExcel: builder.query<IFileModel, IGetAllAdminParams>({
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
    useGetAllRolesQuery,
    useCreateRoleMutation,
    useUpdateRoleMutation,
    useGetRoleByIdQuery,
    useGetIdByNameQuery,
    useDeleteRolesMutation,
    useAddUserToRoleMutation,
    useRemoveUserFromRoleMutation,
    useLazyExportRolesToExcelQuery,
} = rolesAdminService;

export default rolesAdminService;
