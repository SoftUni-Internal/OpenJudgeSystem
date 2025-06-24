import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { defaultPathIdentifier } from 'src/common/constants';
import { testsServiceName } from 'src/common/reduxNames';
import { IGetTestDetailsParams } from 'src/common/url-types';
import { ITestDetails } from 'src/hooks/submissions/types';

const testsService = createApi({
    reducerPath: testsServiceName,
    baseQuery: fetchBaseQuery({
        baseUrl: `${import.meta.env.VITE_UI_SERVER_URL}/${defaultPathIdentifier}/`,
        credentials: 'include',
        prepareHeaders: (headers: Headers) => {
            headers.set('Content-Type', 'application/json');
            return headers;
        },
    }),
    endpoints: (builder) => ({
        getTestDetails: builder.query<
            ITestDetails,
            IGetTestDetailsParams>({
                query: ({ id, submissionId }) => (
                    { url: `Tests/GetDetailsForSubmission/${id}?submissionId=${submissionId}` }),
            }),
    }),
});

const { useLazyGetTestDetailsQuery } = testsService;

export { useLazyGetTestDetailsQuery };

export default testsService;
