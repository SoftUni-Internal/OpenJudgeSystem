/* eslint-disable object-curly-newline */
/* eslint-disable max-len */
import { createApi } from '@reduxjs/toolkit/query/react';

import {
    IFileModel,
    IGetAllAdminParams,
    IPagedResultType,
    ISubmissionForProcessingAdminGridViewType,
} from '../../../common/types';
import { EXCEL_RESULTS_ENDPOINT, GET_ENDPOINT } from '../../../common/urls/administration-urls';
import getCustomBaseQuery from '../../middlewares/customBaseQuery';

const submissionsForProcessingAdminService = createApi({
    reducerPath: 'submissionsForProcessing',
    baseQuery: getCustomBaseQuery('submissionsForProcessing'),
    endpoints: (builder) => ({
        getAllSubmissions: builder.query<IPagedResultType<ISubmissionForProcessingAdminGridViewType>, IGetAllAdminParams>({
            query: ({
                filter, page, itemsPerPage, sorting }) => ({
                url: '/getAll',
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                } }) }),
        getById: builder.query<ISubmissionForProcessingAdminGridViewType, { id:number }>({
            query: ({ id }) => ({
                url: `/${GET_ENDPOINT}/${id}`,
            }),
            keepUnusedDataFor: 0,
        }),

        exportSubmissionsForProcessingToExcel: builder.query<IFileModel, IGetAllAdminParams>({
            query: ({
                filter, page, itemsPerPage, sorting }) => ({
                url: `/${EXCEL_RESULTS_ENDPOINT}`,
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                } }),
            keepUnusedDataFor: 0,
        }),
    }),
});

export const {
    useGetAllSubmissionsQuery,
    useGetByIdQuery,
    useLazyExportSubmissionsForProcessingToExcelQuery,
} = submissionsForProcessingAdminService;

export default submissionsForProcessingAdminService;
