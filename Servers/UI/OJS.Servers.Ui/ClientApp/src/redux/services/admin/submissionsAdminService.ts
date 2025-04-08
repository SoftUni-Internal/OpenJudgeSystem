import { createApi } from '@reduxjs/toolkit/query/react';

import {
    IFileModel,
    IGetAllAdminParams,
    IPagedResultType,
    ISubmissionsAdminGridViewType,
} from '../../../common/types';
import { IRetestSubmissionUrlParams } from '../../../common/url-types';
import { EXCEL_RESULTS_ENDPOINT } from '../../../common/urls/administration-urls';
import getCustomBaseQuery from '../../middlewares/customBaseQuery';

const submissionsAdminService = createApi({
    reducerPath: 'submissionsAdminService',
    baseQuery: getCustomBaseQuery('submissions'),
    endpoints: (builder) => ({
        getAllSubmissions: builder.query<IPagedResultType<ISubmissionsAdminGridViewType>, IGetAllAdminParams>({
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
        deleteSubmission: builder.mutation({
            query: (id) => ({
                url: `/Delete/${id}`,
                method: 'DELETE',
            }),
        }),
        retest: builder.mutation({
            query: (submissionId) => ({
                url: `/Retest/${submissionId}`,
                method: 'POST',
            }),
        }),
         
        downloadFileSubmission: builder.query<IFileModel, IRetestSubmissionUrlParams>({ query: ({ id }) => ({ url: `/download/${id}` }) }),
        exportSubmissionsToExcel: builder.query<IFileModel, IGetAllAdminParams>({
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
    useGetAllSubmissionsQuery,
    useDownloadFileSubmissionQuery,
    useDeleteSubmissionMutation,
    useRetestMutation,
    useLazyExportSubmissionsToExcelQuery,
} = submissionsAdminService;

export default submissionsAdminService;
