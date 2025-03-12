/* eslint-disable max-len */
import { createApi } from '@reduxjs/toolkit/query/react';

import {
    IAccessLogAdministrationModel,
    IAccessLogInListModel,
    IGetAllAdminParams,
    IPagedResultType,
} from '../../../common/types';
import {
    GET_ALL_ENDPOINT, GET_ENDPOINT,
} from '../../../common/urls/administration-urls';
import getCustomBaseQuery from '../../middlewares/customBaseQuery';

const accessLogsAdminService = createApi({
    reducerPath: 'accessLogsAdminService',
    baseQuery: getCustomBaseQuery('accessLogs'),
    endpoints: (builder) => ({
        getAllAccessLogs: builder.query<IPagedResultType<IAccessLogInListModel>, IGetAllAdminParams>({
            query: ({ filter, page, itemsPerPage, sorting }) => ({
                url: `/${GET_ALL_ENDPOINT}`,
                method: 'GET',
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
            keepUnusedDataFor: 3,
        }),
        getAccessLogById: builder.query<IAccessLogAdministrationModel, number>({ query: (id) => ({ url: `/${GET_ENDPOINT}/${id}` }), keepUnusedDataFor: 0 }),
    }),
});

export const {
    useGetAllAccessLogsQuery,
    useGetAccessLogByIdQuery,
} = accessLogsAdminService;

export default accessLogsAdminService;
