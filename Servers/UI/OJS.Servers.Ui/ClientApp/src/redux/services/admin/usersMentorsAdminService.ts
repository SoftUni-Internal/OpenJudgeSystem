import { createApi } from '@reduxjs/toolkit/query/react';

import {
    IGetAllAdminParams,
    IPagedResultType,
    IUserMentorAdministrationModel,
    IUserMentorInListModel,
} from '../../../common/types';
import { GET_ENDPOINT, UPDATE_ENDPOINT } from '../../../common/urls/administration-urls';
import getCustomBaseQuery from '../../middlewares/customBaseQuery';

const usersMentorsAdminService = createApi({
    reducerPath: 'usersMentorsAdminService',
    baseQuery: getCustomBaseQuery('mentor'),
    endpoints: (builder) => ({
        getAllUsersMentors: builder.query<IPagedResultType<IUserMentorInListModel>, IGetAllAdminParams>({
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
        getUserMentorById: builder.query<IUserMentorAdministrationModel, string>({
            query: (id) => ({ url: `/${GET_ENDPOINT}/${id}` }),
            keepUnusedDataFor: 0,
        }),
        deleteUserMentor: builder.mutation<string, number>({ query: (id) => ({ url: `/Delete/${id}`, method: 'DELETE' }) }),
        updateUserMentor: builder.mutation({
            query: (userMentor: IUserMentorAdministrationModel) => ({
                url: `/${UPDATE_ENDPOINT}`,
                method: 'PATCH',
                body: userMentor,
            }),
        }),
    }),
});

export const {
    useGetAllUsersMentorsQuery,
    useGetUserMentorByIdQuery,
    useDeleteUserMentorMutation,
    useUpdateUserMentorMutation,
} = usersMentorsAdminService;

export default usersMentorsAdminService;
