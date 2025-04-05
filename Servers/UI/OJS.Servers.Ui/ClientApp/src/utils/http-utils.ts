import { SerializedError } from '@reduxjs/toolkit';
import { FetchBaseQueryError } from '@reduxjs/toolkit/query';
import isNil from 'lodash/isNil';

import { IDictionary } from '../common/common-types';
import { ExceptionData } from '../common/types';

interface IErrorDataType {
    title: string;
    status: number;
    detail: string;
    extensions: IDictionary<object>;
}

const defaultErrorMessage = 'Something went wrong, please try again!';

const getErrorMessage = (
    err: FetchBaseQueryError | SerializedError | ExceptionData[] | undefined,
    defaultMessage: string = defaultErrorMessage,
): string => {
    if (isNil(err) || !err) {
        return defaultMessage;
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

export type {
    IErrorDataType,
};

export {
    getErrorMessage,
    defaultErrorMessage,
};
