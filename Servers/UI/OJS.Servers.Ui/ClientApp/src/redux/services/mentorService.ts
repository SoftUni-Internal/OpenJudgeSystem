import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { ChatMessageRole } from 'src/common/enums';
import { updateMessages } from 'src/redux/features/mentorSlice';

import { defaultPathIdentifier } from '../../common/constants';
import { IMentorConversationMessage, IMentorConversationRequestModel, IMentorConversationResponseModel } from '../../common/types';

const mentorService = createApi({
    reducerPath: 'mentorService',
    baseQuery: fetchBaseQuery({
        baseUrl: `${import.meta.env.VITE_UI_SERVER_URL}/${defaultPathIdentifier}/mentor`,
        credentials: 'include',
        prepareHeaders: (headers: any) => {
            headers.set('Content-Type', 'application/json');
            return headers;
        },
    }),
    endpoints: (builder) => ({
        startConversation: builder.mutation<IMentorConversationResponseModel, IMentorConversationRequestModel>({
            query: ({ ...mentorConversationRequestModel }) => ({
                url: '/StartConversation',
                method: 'POST',
                body: mentorConversationRequestModel,
            }),
            async onQueryStarted(arg, { dispatch, queryFulfilled }) {
                try {
                    const { data } = await queryFulfilled;
                    dispatch(updateMessages({
                        userId: data.userId,
                        problemId: data.problemId,
                        messages: data.messages.filter((m) => m.role !== ChatMessageRole.System),
                    }));
                } catch {
                    /* ignore, slice already has optimistic copy */
                }
            },
        }),
        getSystemMessage: builder.query<IMentorConversationMessage, Omit<IMentorConversationRequestModel, 'messages'>>({
            query: (params) => ({
                url: '/GetSystemMessage',
                method: 'GET',
                params,
            }),
        }),
    }),
});

export const {
    useStartConversationMutation,
    useLazyGetSystemMessageQuery,
} = mentorService;

export default mentorService;
