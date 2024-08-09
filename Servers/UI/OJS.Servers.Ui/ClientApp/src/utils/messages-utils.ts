/* eslint-disable valid-typeof */
/* eslint-disable @typescript-eslint/no-use-before-define */
import React from 'react';

import { ExceptionData } from '../common/types';

const getAndSetExceptionMessage = (exceptions: Array<unknown>, setState: React.Dispatch<React.SetStateAction<string[]>>) => {
    const messages = extractMessages(exceptions);
    setState(messages);
};

const getAndSetSuccessfulMessages = (data: Array<{message:string | undefined; shouldGet: boolean}>) => {
    const filteredData = data.filter((x) => x.message !== undefined && x.shouldGet);

    let message: string | null = null;
    if (filteredData.length > 0) {
        message = filteredData[0].message as string;
    }
    if (message) {
        return message;
    }
    return null;
};

const extractMessages = (error: Array<unknown>): Array<string> => {
    const filteredErrors = error.filter((x) => x !== undefined);

    let currentError = [] as Array<ExceptionData>;
    if (filteredErrors.length > 0) {
        currentError = filteredErrors[0] as Array<ExceptionData>;
    }
    if (currentError.length > 0) {
        return currentError.map((x) => x.message);
    }
    return [];
};

export {
    getAndSetExceptionMessage,
    getAndSetSuccessfulMessages,
};
