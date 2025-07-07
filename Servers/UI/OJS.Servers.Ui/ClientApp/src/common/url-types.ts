 
import { SubmissionStatus } from './enums';
import { IGetAllAdminParams } from './types';

interface IContestDetailsUrlParams {
    id: number;
}

interface IExamGroupUrlParams {
    id: number;
}

interface IGetByContestId extends IGetAllAdminParams {
    contestId: number;
}

interface IGetByRoleId extends IGetAllAdminParams {
    roleId: string;
}

interface IGetByUserId extends IGetAllAdminParams {
    userId: string;
}

interface IGetByParentId extends IGetAllAdminParams {
    parentId: number;
}

interface IGetByTestId extends IGetAllAdminParams {
    testId: number;
}

interface IGetByExamGroupId extends IGetAllAdminParams {
    examGroupId: number;
}

interface IContestCategoriesUrlParams {
    id: number;
}

interface IGetSubmissionsUrlParams {
    itemsPerPage: number;
    page: number;
    filter?: string;
    sorting?: string;
}

interface IGetRecentSubmissionsUrlParams extends IGetSubmissionsUrlParams {
    status: SubmissionStatus;
}

interface IGetProfileSubmissionsUrlParams extends IGetSubmissionsUrlParams {
    username: string;
}

interface IGetSubmissionResultsByProblemUrlParams extends IGetSubmissionsUrlParams {
    problemId: number;
    isOfficial: boolean;
}

interface IGetSubmissionsByUserParams {
    id: number;
    page: number;
    isOfficial: boolean;
}

interface IGetUserSubmissionsUrlParams {
    username: string;
    page: number;
}

interface IGetContestResultsParams {
    id: number;
    official: boolean;
    full: boolean;
    page: number;
}

interface IRetestSubmissionUrlParams {
    id: number;
    verbosely?: boolean;
}

interface IProblemUrlById {
    id: number;
}

interface ISubmissionTypeDocumentUrlById {
    id: number;
}

interface ISubmitContestSolutionParams {
    content: string | File;
    official: boolean;
    problemId: number;
    submissionTypeId: number;
    contestId: number;
    isWithRandomTasks?: boolean;
    verbosely?: boolean;
}

interface IRegisterUserForContestParams {
    password: string | null;
    isOfficial: boolean;
    id: number;
    hasConfirmedParticipation: boolean;
}

interface IGetTestDetailsParams {
    id: number;
    submissionId: number;
}

export type {
    IContestDetailsUrlParams,
    IContestCategoriesUrlParams,
    IGetSubmissionsUrlParams,
    IGetRecentSubmissionsUrlParams,
    IGetProfileSubmissionsUrlParams,
    IGetSubmissionResultsByProblemUrlParams,
    IGetContestResultsParams,
    IRetestSubmissionUrlParams,
    IGetUserSubmissionsUrlParams,
    IGetByExamGroupId,
    IGetByContestId,
    IProblemUrlById,
    IGetByParentId,
    ISubmissionTypeDocumentUrlById,
    IExamGroupUrlParams,
    IGetByTestId,
    IGetByRoleId,
    IGetByUserId,
    ISubmitContestSolutionParams,
    IGetSubmissionsByUserParams,
    IRegisterUserForContestParams,
    IGetTestDetailsParams,
};
