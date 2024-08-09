import { SerializedError } from '@reduxjs/toolkit';
import { FetchBaseQueryError } from '@reduxjs/toolkit/query';
import axios, { ResponseType } from 'axios';
import isFunction from 'lodash/isFunction';
import isNil from 'lodash/isNil';

import { IDictionary, UrlType } from '../common/common-types';
import { ExceptionData } from '../common/types';
import { IErrorDataType } from '../hooks/use-http';

const getUrl = <P>(url: UrlType<P>, params?: IDictionary<P> | null) => (
    isFunction(url)
        ? url(params)
        : url
);

const getErrorMessage = (
    err: FetchBaseQueryError | SerializedError | ExceptionData[] | undefined,
    defaultErrorMessage = 'Something went wrong, please try again!',
): string => {
    if (isNil(err) || !err) {
        return defaultErrorMessage;
    }

    if (Array.isArray(err) && err.length === 1) {
        return getErrorMessage(err[0]);
    }

    // we should unify the return object from BE on error
    // in order to implement better logic in this function
    if ('data' in err) {
        if ((err.data as any).detail) {
            return (err.data as any).detail as string;
        }
        if (err.data) {
            return err.data as string;
        }

        return defaultErrorMessage;
    }

    if ('status' in err) {
        return 'error' in err
            ? err.error.replace(/"/g, '')
            : ((err as any).data as IErrorDataType).detail.replace(/"/g, '');
    }

    if ((err as SerializedError).message) {
        return (err as SerializedError).message!.replace(/"/g, '');
    }

    if ((err as any).detail) {
        return (err as any).detail.replace(/"/g, '');
    }

    return defaultErrorMessage;
};

interface IHttpCallParams {
    url: string;
    method: string;
    headers?: IDictionary<string>;
    responseType: string;
    onSuccess: (response: any) => Promise<void>;
    onError: (err: any) => Promise<void>;
    body?: any;
    onBeforeCall?: () => Promise<void>;
    onAfterCall?: () => Promise<void>;
}

const makeHttpCall = async ({
    url,
    method,
    headers = {},
    body = null,
    responseType,
    onSuccess,
    onError,
    onBeforeCall = () => Promise.resolve(),
    onAfterCall = () => Promise.resolve(),
}: IHttpCallParams) => {
    const makeCall = () => axios({
        method,
        url,
        data: body,
        responseType: responseType as ResponseType,
        withCredentials: true,
        headers,
    });

    try {
        await onBeforeCall();
        const resp = await makeCall();
        await onSuccess(await resp);
    } catch (err: any) {
        await onError(err);
    } finally {
        await onAfterCall();
    }
};

export {
    getUrl,
    makeHttpCall,
    getErrorMessage,
};
