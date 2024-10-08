import { ITestRunType } from '../hooks/submissions/types';

import { TestRunResultType } from './constants';

const getTestResultColorId = (resultType: string) => {
    switch (resultType.toLowerCase()) {
    case TestRunResultType.CorrectAnswer.toLowerCase():
        // primary-green
        return '#23be5e';
    case TestRunResultType.TimeLimit.toLowerCase():
    case TestRunResultType.MemoryLimit.toLowerCase():
        // warning
        return '#fec112';
    default:
        // primary-red
        return '#fc4c50';
    }
};

const getResultTypeText = (resType: string) => {
    switch (resType.toLowerCase()) {
    // TODO: https://github.com/SoftUni-Internal/exam-systems-issues/issues/1287
    case TestRunResultType.CorrectAnswer.toLowerCase():
        return 'Correct Answer';
    case TestRunResultType.WrongAnswer.toLowerCase():
        return 'Wrong Answer';
    case TestRunResultType.MemoryLimit.toLowerCase():
        return 'Memory Limit';
    case TestRunResultType.TimeLimit.toLowerCase():
        return 'Time Limit';
    case TestRunResultType.RunTimeError.toLowerCase():
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
