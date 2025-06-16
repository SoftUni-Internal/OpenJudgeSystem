import { createApi } from '@reduxjs/toolkit/query/react';

import { IFileModel, IGetAllAdminParams, IPagedResultType, ITestRunInListModel } from '../../../common/types';
import { IGetByParentId, IGetByTestId } from '../../../common/url-types';
import { CREATE_ENDPOINT, DELETE_ENDPOINT, EXCEL_RESULTS_ENDPOINT, GET_ENDPOINT, UPDATE_ENDPOINT } from '../../../common/urls/administration-urls';
import { ITestAdministration, ITestInListData } from '../../../components/administration/tests/types';
import getCustomBaseQuery from '../../middlewares/customBaseQuery';

const testsAdminService = createApi({
    reducerPath: 'tests',
    baseQuery: getCustomBaseQuery('tests'),
    endpoints: (builder) => ({
        getAllAdminTests: builder.query<IPagedResultType<ITestInListData>, IGetAllAdminParams>({
            query: ({ filter, page, itemsPerPage, sorting }) => ({
                url: 'GetAll',
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
        }),
        deleteTest: builder.mutation<string, number>({ query: (id) => ({ url: `/${DELETE_ENDPOINT}/${id}`, method: 'DELETE' }) }),
        getTestById: builder.query<ITestAdministration, number>({
            query: (id) => ({ url: `/${GET_ENDPOINT}/${id}` }),
            keepUnusedDataFor: 0,
        }),
        updateTest: builder.mutation<string, ITestAdministration >({
            query: (test) => ({
                url: `/${UPDATE_ENDPOINT}`,
                method: 'PATCH',
                body: test,
            }),
        }),
        createTest: builder.mutation<string, ITestAdministration >({
            query: (test) => ({
                url: `/${CREATE_ENDPOINT}`,
                method: 'POST',
                body: test,
            }),
        }),
        getTestsByProblemId: builder.query<IPagedResultType<ITestInListData>, IGetByParentId>({
            query: ({ parentId: problemId, filter, page, itemsPerPage, sorting }) => ({
                url: `/GetByProblemId/${problemId}`,
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
        }),
        deleteByProblem: builder.mutation<string, number>({
            query: (problemId) => ({
                url: `/DeleteAll/${problemId}`,
                method: 'DELETE',
            }),
        }),
        importTests: builder.mutation<string, FormData>({
            query: (tests) => ({
                url: '/Import',
                method: 'POST',
                body: tests,
            }),
        }),
        exportZip: builder.query<IFileModel, number>({ query: (problemId) => ({ url: `/ExportZip/${problemId}` }), keepUnusedDataFor: 5 }),
        getTestRunsByTestId: builder.query<IPagedResultType<ITestRunInListModel>, IGetByTestId>({
            query: ({ testId, filter, page, itemsPerPage, sorting }) => ({
                url: `/GetTestRunsByTestId/${testId}`,
                params: {
                    filter,
                    page,
                    itemsPerPage,
                    sorting,
                },
            }),
        }),
        exportTestsToExcel: builder.query<IFileModel, IGetAllAdminParams>({
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
    useGetAllAdminTestsQuery,
    useCreateTestMutation,
    useDeleteTestMutation,
    useUpdateTestMutation,
    useGetTestByIdQuery,
    useGetTestsByProblemIdQuery,
    useDeleteByProblemMutation,
    useImportTestsMutation,
    useExportZipQuery,
    useGetTestRunsByTestIdQuery,
    useLazyExportTestsToExcelQuery,
} = testsAdminService;
export default testsAdminService;
