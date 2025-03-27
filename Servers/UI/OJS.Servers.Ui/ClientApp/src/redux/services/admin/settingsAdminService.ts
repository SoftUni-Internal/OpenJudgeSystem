import { createApi } from '@reduxjs/toolkit/query/react';

import { IFileModel, IGetAllAdminParams, IPagedResultType, ISettingAdministrationModel, ISettingInListModel } from '../../../common/types';
import { CREATE_ENDPOINT, DELETE_ENDPOINT, EXCEL_RESULTS_ENDPOINT, GET_ENDPOINT, UPDATE_ENDPOINT } from '../../../common/urls/administration-urls';
import customBaseQuery from '../../middlewares/customBaseQuery';

const settingsAdminService = createApi({
    reducerPath: 'settings',
    baseQuery: customBaseQuery('settings'),
    endpoints: (builder) => ({
        getAllSettings: builder.query<IPagedResultType<ISettingInListModel>, IGetAllAdminParams>({
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

        getSettingById:
        builder.query<ISettingAdministrationModel, number>({ query: (id) => ({ url: `/${GET_ENDPOINT}/${id}` }), keepUnusedDataFor: 0 }),

        updateSetting: builder.mutation<string, ISettingAdministrationModel >({
            query: (setting) => ({
                url: `/${UPDATE_ENDPOINT}`,
                method: 'PATCH',
                body: setting,
            }),
        }),

        createSetting: builder.mutation<string, ISettingAdministrationModel >({
            query: (setting) => ({
                url: `/${CREATE_ENDPOINT}`,
                method: 'POST',
                body: setting,
            }),
        }),

        deleteSetting: builder.mutation<string, string >({ query: (id) => ({ url: `/${DELETE_ENDPOINT}/${id}`, method: 'DELETE' }) }),

        exportSettingsToExcel: builder.query<IFileModel, IGetAllAdminParams>({
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
    useGetAllSettingsQuery,

    useGetSettingByIdQuery,
    useCreateSettingMutation,
    useUpdateSettingMutation,
    useDeleteSettingMutation,
    useLazyExportSettingsToExcelQuery,
} = settingsAdminService;

export default settingsAdminService;
