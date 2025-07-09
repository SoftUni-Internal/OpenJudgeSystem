 
import { createApi } from '@reduxjs/toolkit/query/react';

import {
    IChangeParticipationTimeForMultipleParticipants, IChangeParticipationTimeForSingleParticipant,
    IFileModel,
    IGetAllAdminParams,
    IPagedResultType,
    IParticipantAdministrationModel,
    IParticipantInListModel,
} from '../../../common/types';
import { IGetByContestId } from '../../../common/url-types';
import { CREATE_ENDPOINT, DELETE_ENDPOINT, EXCEL_RESULTS_ENDPOINT } from '../../../common/urls/administration-urls';
import getCustomBaseQuery from '../../middlewares/customBaseQuery';

const participantsAdminService = createApi({
    reducerPath: 'participants',
    baseQuery: getCustomBaseQuery('participants'),
    endpoints: (builder) => ({
        getByContestId: builder.query<IPagedResultType<IParticipantInListModel>, IGetByContestId>({
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
        getAllParticipants: builder.query<IPagedResultType<IParticipantInListModel>, IGetAllAdminParams>({
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
        deleteParticipant: builder.mutation<string, number >({ query: (id) => ({ url: `/${DELETE_ENDPOINT}/${id}`, method: 'DELETE' }) }),
        createParticipant: builder.mutation<string, IParticipantAdministrationModel >({
            query: (participant) => ({
                url: `/${CREATE_ENDPOINT}`,
                method: 'POST',
                body: participant,
            }),
        }),
        exportParticipantsToExcel: builder.query<IFileModel, IGetAllAdminParams>({
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
        changeParticipationTimeForMultipleParticipants: builder.mutation<string, IChangeParticipationTimeForMultipleParticipants>({
            query: (model) => ({
                url: '/ChangeParticipationTimeForMultipleParticipants',
                method: 'POST',
                body: model,
            }),
        }),
        changeParticipationTimeForSingleParticipant: builder.mutation<string, IChangeParticipationTimeForSingleParticipant>({
            query: (model) => ({
                url: '/ChangeParticipationTimeForSingleParticipant',
                method: 'POST',
                body: model,
            }),
        }),
    }),
});

export const {
    useGetByContestIdQuery,
    useCreateParticipantMutation,
    useDeleteParticipantMutation,
    useGetAllParticipantsQuery,
    useLazyExportParticipantsToExcelQuery,
    useChangeParticipationTimeForMultipleParticipantsMutation,
    useChangeParticipationTimeForSingleParticipantMutation,
} = participantsAdminService;

export default participantsAdminService;
