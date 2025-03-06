import { ExceptionType } from 'src/common/enums';

import { ITestRunType } from '../hooks/submissions/types';

import { TestRunResultType } from './constants';

const getTestResultColorId = (resultType: number) => {
    switch (resultType) {
    case TestRunResultType.CorrectAnswer:
        // primary-green
        return '#23be5e';
    case TestRunResultType.TimeLimit:
    case TestRunResultType.MemoryLimit:
        // warning
        return '#F2A000';
    default:
        // primary-red
        return '#fc4c50';
    }
};

const getResultTypeText = (resultType: number) => {
    switch (resultType) {
    case TestRunResultType.CorrectAnswer:
        return 'Correct Answer';
    case TestRunResultType.WrongAnswer:
        return 'Wrong Answer';
    case TestRunResultType.MemoryLimit:
        return 'Memory Limit';
    case TestRunResultType.TimeLimit:
        return 'Time Limit';
    case TestRunResultType.RunTimeError:
        return 'Runtime Error';
    default:
        return '';
    }
};

const getExceptionTypeMessage = (resultType?: number) => {
    switch (resultType) {
    case ExceptionType.Solution:
        return 'An error related to the user\'s submission occurred.';
    case ExceptionType.Configuration:
        return 'An error related to the problem\'s configurations ( settings / skeleton / tests ) occurred.';
    case ExceptionType.Remote:
        return 'A connectivity issue occurred. Please contact a developer.';
    case ExceptionType.Other:
        return 'An unexpected error occurred. Please contact a developer.';
    case ExceptionType.Strategy:
        return 'An error related to the strategy occurred. Please contact a developer.';
    default:
        return 'A processing error occurred:';
    }
};

const sortTestRunsByTrialTest = (a: ITestRunType, b: ITestRunType) => {
    if (a.isTrialTest && !b.isTrialTest) {
        return -1;
    }
    if (!a.isTrialTest && b.isTrialTest) {
        return 1;
    }
    return 0;
};

// eslint-disable-next-line import/prefer-default-export
export {
    getTestResultColorId,
    getResultTypeText,
    sortTestRunsByTrialTest,
    getExceptionTypeMessage,
};
