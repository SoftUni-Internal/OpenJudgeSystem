import { createApi } from '@reduxjs/toolkit/query/react';

import {
    IMappingEntityId,
    ISubmissionTypeDocumentAdministrationModel,
} from '../../../common/types';
import { ISubmissionTypeDocumentUrlById } from '../../../common/url-types';
import {
    CREATE_ENDPOINT,
    GET_ENDPOINT,
    UPDATE_ENDPOINT,
} from '../../../common/urls/administration-urls';
import getCustomBaseQuery from '../../middlewares/customBaseQuery';

const submissionTypeDocumentsAdminService = createApi({
    reducerPath: 'submissionTypeDocuments',
    baseQuery: getCustomBaseQuery('submissionTypeDocuments'),
    endpoints: (builder) => ({
        // eslint-disable-next-line max-len
        getSubmissionTypeDocumentById: builder.query<ISubmissionTypeDocumentAdministrationModel, ISubmissionTypeDocumentUrlById>({ query: (submissionTypeDocument) => ({ url: `/${GET_ENDPOINT}/${submissionTypeDocument.id}` }), keepUnusedDataFor: 0 }),
        createSubmissionTypeDocument: builder.mutation<string, ISubmissionTypeDocumentAdministrationModel>({
            query: (submissionTypeDocument) => ({
                url: `/${CREATE_ENDPOINT}`,
                method: 'POST',
                body: submissionTypeDocument,
            }),
        }),
        updateSubmissionTypeDocument: builder.mutation<string, ISubmissionTypeDocumentAdministrationModel>({
            query: (submissionTypeDocument) => ({
                url: `/${UPDATE_ENDPOINT}`,
                method: 'PATCH',
                body: submissionTypeDocument,
            }),
        }),
        deleteSubmissionTypeDocumentByIdAndSubmissionTypeId: builder.mutation<string, IMappingEntityId>({
            query: (deleteSubmissionTypeDocumentModel) => ({
                url: '/DeleteByIdAndSubmissionTypeId',
                method: 'DELETE',
                body: {
                    submissionTypeId: deleteSubmissionTypeDocumentModel.firstEntityId,
                    submissionTypeDocumentId: deleteSubmissionTypeDocumentModel.secondEntityId,
                },
            }),
        }),
    }),
});

export const {
    useCreateSubmissionTypeDocumentMutation,
    useUpdateSubmissionTypeDocumentMutation,
    useDeleteSubmissionTypeDocumentByIdAndSubmissionTypeIdMutation,
    useGetSubmissionTypeDocumentByIdQuery,
} = submissionTypeDocumentsAdminService;

export default submissionTypeDocumentsAdminService;
