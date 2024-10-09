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
        return '#fec112';
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
};
