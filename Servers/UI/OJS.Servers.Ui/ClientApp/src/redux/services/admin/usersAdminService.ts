/* eslint-disable prefer-destructuring */
import { createApi } from '@reduxjs/toolkit/query/react';

import { IFileModel, IGetAllAdminParams, IPagedResultType, IUserAdministrationModel, IUserAutocompleteData, IUserInExamGroupModel, IUserInListModel } from '../../../common/types';
import { IGetByExamGroupId, IGetByRoleId, IGetByUserId } from '../../../common/url-types';
import { EXCEL_RESULTS_ENDPOINT, GET_ENDPOINT, UPDATE_ENDPOINT } from '../../../common/urls/administration-urls';
import getCustomBaseQuery from '../../middlewares/customBaseQuery';

const usersAdminService = createApi({
    reducerPath: 'adminUsers',
    baseQuery: getCustomBaseQuery('users'),
    endpoints: (builder) => ({

        getUsersAutocomplete: builder.query<Array<IUserAutocompleteData>, Array<string>>({
            query: (queryString) => ({
                url: `/GetNameAndId?searchString=${encodeURIComponent(queryString[0])}&roleId=${queryString.length > 1
                    ? queryString[1]
                    : ''}`,
            }),
            keepUnusedDataFor: 10,
        }),

        getByExamGroupId: builder.query<IPagedResultType<IUserInExamGroupModel>, IGetByExamGroupId>({
            query: ({ examGroupId, filter, page, itemsPerPage, sorting }) => ({
                url: `/GetByExamGroupId/${examGroupId}`,
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
        }),

        getAllUsers: builder.query<IPagedResultType<IUserInListModel>, IGetAllAdminParams>({
            query: ({ filter, page, itemsPerPage, sorting }) => ({
                url: '/GetAll',
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
        }),

        getUserById:
        builder.query<IUserAdministrationModel, string>({ query: (id) => ({ url: `/${GET_ENDPOINT}/${id}` }), keepUnusedDataFor: 0 }),

        getUsersByRole: builder.query<IPagedResultType<IUserInListModel>, IGetByRoleId>({
            query: ({ roleId, filter, page, itemsPerPage, sorting }) => ({
                url: `/GetByRoleId/${roleId}`,
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
        }),

        updateUser: builder.mutation<string, IUserAdministrationModel >({
            query: (user) => ({
                url: `/${UPDATE_ENDPOINT}`,
                method: 'PATCH',
                body: user,
            }),
        }),

        getLecturerContests: builder.query<IPagedResultType<IUserInListModel>, IGetByUserId>({
            query: ({ userId, filter, page, itemsPerPage, sorting }) => ({
                url: `/GetLecturerContests/${userId}`,
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
        }),

        addLecturerToContest: builder.mutation<string, {lecturerId: string; contestId: number} >({
            query: ({ lecturerId, contestId }) => ({
                url: '/AddLecturerToContest',
                method: 'POST',
                body: { lecturerId, contestId },
            }),
        }),

        removeLecturerFromContest: builder.mutation<string, {lecturerId: string; contestId: number} >({
            query: ({ lecturerId, contestId }) => ({
                url: `/RemoveLecturerFromContest?lecturerId=${lecturerId}&contestId=${contestId}`,
                method: 'DELETE',
            }),
        }),

        getLecturerCategories: builder.query<IPagedResultType<IUserInListModel>, IGetByUserId>({
            query: ({ userId, filter, page, itemsPerPage, sorting }) => ({
                url: `/GetLecturerCategories/${userId}`,
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
        }),

        addLecturerToCategory: builder.mutation<string, {lecturerId: string; categoryId: number} >({
            query: ({ lecturerId, categoryId }) => ({
                url: '/AddLecturerToCategory',
                method: 'POST',
                body: { lecturerId, categoryId },
            }),
        }),

        removeLecturerFromCategory: builder.mutation<string, {lecturerId: string; categoryId: number} >({
            query: ({ lecturerId, categoryId }) => ({
                url: `/RemoveLecturerFromCategory?lecturerId=${lecturerId}&categoryId=${categoryId}`,
                method: 'DELETE',
            }),
        }),

        exportUsersToExcel: builder.query<IFileModel, IGetAllAdminParams>({
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
    useGetUsersAutocompleteQuery,
    useGetUsersByRoleQuery,
    useGetUserByIdQuery,
    useGetAllUsersQuery,
    useUpdateUserMutation,
    useGetLecturerContestsQuery,
    useAddLecturerToContestMutation,
    useRemoveLecturerFromContestMutation,
    useGetLecturerCategoriesQuery,
    useAddLecturerToCategoryMutation,
    useRemoveLecturerFromCategoryMutation,
    useLazyExportUsersToExcelQuery,
    useGetByExamGroupIdQuery,

} = usersAdminService;
export default usersAdminService;
