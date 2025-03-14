import { createApi } from '@reduxjs/toolkit/query/react';

import {
    IGetAllAdminParams,
    IMentorPromptTemplateAdministrationModel,
    IMentorPromptTemplateInListModel,
    IPagedResultType,
} from '../../../common/types';
import { GET_ENDPOINT, UPDATE_ENDPOINT } from '../../../common/urls/administration-urls';
import getCustomBaseQuery from '../../middlewares/customBaseQuery';

const mentorPromptTemplatesAdminService = createApi({
    reducerPath: 'mentorPromptTemplatesAdminService',
    baseQuery: getCustomBaseQuery('mentorPromptTemplates'),
    endpoints: (builder) => ({
        getAllMentorPromptTemplates: builder.query<IPagedResultType<IMentorPromptTemplateInListModel>, IGetAllAdminParams>({
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
        getMentorPromptTemplateById: builder.query<IMentorPromptTemplateAdministrationModel, number>({
            query: (id) => ({ url: `/${GET_ENDPOINT}/${id}` }),
            keepUnusedDataFor: 0,
        }),
        deleteMentorPromptTemplate: builder.mutation<number, number>({ query: (id) => ({ url: `/Delete/${id}`, method: 'DELETE' }) }),
        updateMentorPromptTemplate: builder.mutation({
            query: (userMentor: IMentorPromptTemplateAdministrationModel) => ({
                url: `/${UPDATE_ENDPOINT}`,
                method: 'PATCH',
                body: userMentor,
            }),
        }),
    }),
});

export const {
    useGetAllMentorPromptTemplatesQuery,
    useGetMentorPromptTemplateByIdQuery,
    useDeleteMentorPromptTemplateMutation,
    useUpdateMentorPromptTemplateMutation,
} = mentorPromptTemplatesAdminService;

export default mentorPromptTemplatesAdminService;
