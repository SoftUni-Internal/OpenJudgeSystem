/* eslint-disable no-param-reassign */
import { BaseQueryApi, FetchArgs, fetchBaseQuery } from '@reduxjs/toolkit/query';

import { defaultPathIdentifier } from '../../common/constants';
import { SOMETHING_WENT_WRONG_MESSAGE, UNEXPECTED_ERROR_MESSAGE } from '../../common/messages';
import { ExceptionData } from '../../common/types';

type ExtraOptionsType = object
type ResultError = {
    data: Array<ExceptionData>;
}

const errorStatusCodes = [ 400, 401, 403, 422, 500 ];
const succesfullStatusCodes = [ 200, 204 ];
const getCustomBaseQuery = (baseUrl:string) => async (args: FetchArgs, api: BaseQueryApi, extraOptions:ExtraOptionsType) => {
    const baseQuery = fetchBaseQuery({
        credentials: 'include',
        baseUrl: `${import.meta.env.VITE_ADMINISTRATION_URL}/${defaultPathIdentifier}/${baseUrl}`,
        prepareHeaders: (headers) => headers,
        responseHandler: async (response: Response) => {
            const contentType = response.headers.get('Content-Type');
            if (contentType?.includes('application/octet-stream')) {
                const contentDisposition = response.headers.get('Content-Disposition');
                let filename = 'file.zip';
                if (contentDisposition) {
                    const match = contentDisposition.match(/filename="?(.+?)"?(;|$)/);
                    if (match) {
                        filename = decodeURIComponent(match[1]);
                    }
                }
                const blob = await response.blob();
                return { blob, filename };
            }

            if (response.headers.get('Content-Length')) {
                return '';
            }

            return response.json();
        },
    });

    const result = await baseQuery(args, api, extraOptions);
    const response = result.meta?.response;

    if (response && errorStatusCodes.some((status) => status === Number(response.status))) {
        const errorsArray = result.error as ResultError;
        let data = [] as Array<ExceptionData>;
        try {
            data = errorsArray.data as Array<ExceptionData>;
            data.forEach((x) => {
                if (!x.message) {
                    x.message = UNEXPECTED_ERROR_MESSAGE;
                }
            });
        } catch {
            data = [ { message: SOMETHING_WENT_WRONG_MESSAGE, name: '' } ] as Array<ExceptionData>;
        }
        return { error: data };
    }

    if (response && succesfullStatusCodes.some((status) => status === Number(response!.status))) {
        const contentType = response.headers.get('Content-Type');
        if (contentType?.includes('text')) {
            return { data: result.error?.data };
        }
    }

    return result;
};

export default getCustomBaseQuery;
