import { ISubmissionTypeType, IUserProfileType } from '../../common/types';

interface IProblemType {
    id: number;
    name: string;
    maximumPoints: number;
    orderBy: number;
    contestId:number;
}

interface ITestRunType {
    id: number;
    timeUsed: number;
    memoryUsed: number;
    executionComment: string;
    checkerComment: string;
    resultType: number;
    expectedOutputFragment: string;
    userOutputFragment: string;
    orderBy: number;
    input: string;
    isTrialTest: boolean;
    showInput: boolean;
    testId: number;
}

interface ITestRunIcon {
    id: number;
    isTrialTest: boolean;
    resultType: number;
}

interface ISubmissionDetailsResponseType {
    id: number;
    problem: IProblemType;
    points: number;
    content: string;
    testRuns: ITestRunType[];
    user: IUserProfileType;
    userIsInRoleForContest: boolean;
    submissionType: ISubmissionTypeType;
    maxUsedTime: number;
    maxUsedMemory: number;
    isOfficial: boolean;
    isCompiledSuccessfully: boolean;
    isEligibleForRetest: boolean;
    compilerComment: string | null;
    isProcessed: boolean;
    createdOn: Date;
    modifiedOn: Date;
    startedExecutionOn: Date;
    completedExecutionOn: Date;
    workerName: string | null;
    processingComment: string | null;
    totalTests: number;
    contestId: number;
    contestName: string | null;
    contestCategoryId: number;
    maxPoints: number;
}

interface ITestRun {
    id: number;
    timeUsed: number;
    memoryUsed: number;
    submissionId: number;
    executionComment: string;
    checkerComment: string;
    resultType: number;
    expectedOutputFragment: string;
    userOutputFragment: string;
    isTrialTest: boolean;
    input: string;
    orderBy: number;
    showInput: boolean;
    testId: number;
}

export type {
    ITestRunType,
    ITestRunIcon,
    ISubmissionDetailsResponseType,
    ITestRun,
};
