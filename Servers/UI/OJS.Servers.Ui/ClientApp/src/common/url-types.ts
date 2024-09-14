/* eslint-disable import/prefer-default-export */
import { SubmissionStatus } from './enums';
import { IGetAllAdminParams } from './types';

interface IUserInfoUrlParams {
    username: string;
}

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

interface IGetByProblemId extends IGetAllAdminParams {
    problemId: number;
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

interface IRegisterForContestUrlParams {
    id: number;
    isOfficial: boolean;
}

interface ISubmitContestPasswordParams {
    contestId: string;
    isOfficial: boolean;
    password: string;
}

interface IStartContestParticipationUrlParams {
    id: number;
    isOfficial: boolean;
}

interface IGetContestParticipationScoresForParticipantUrlParams {
    participantId: number;
}

interface ISubmitContestPasswordUrlParams {
    id: number;
    isOfficial: boolean;
}

interface IDownloadProblemResourceUrlParams {
    id: number | null;
}

interface IGetSubmissionsUrlParams {
    status: SubmissionStatus;
    page: number;
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

interface IGetUserSubmissionsForProfileByContestUrlParams {
    username: string;
    page: number;
    contestId: string;
}

interface IGetSubmissionResultsByProblemUrlParams {
    problemId: number;
    isOfficial: boolean;
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
}

interface IRegisterUserForContestParams {
    password: string | null;
    isOfficial: boolean;
    id: number;
    hasConfirmedParticipation: boolean;
}

export type {
    IUserInfoUrlParams,
    IRegisterForContestUrlParams,
    ISubmitContestPasswordUrlParams,
    IContestDetailsUrlParams,
    IContestCategoriesUrlParams,
    IStartContestParticipationUrlParams,
    IGetContestParticipationScoresForParticipantUrlParams,
    IGetUserSubmissionsForProfileByContestUrlParams,
    IDownloadProblemResourceUrlParams,
    IGetSubmissionsUrlParams,
    IGetSubmissionResultsByProblemUrlParams,
    IGetContestResultsParams,
    IRetestSubmissionUrlParams,
    IGetUserSubmissionsUrlParams,
    IGetByExamGroupId,
    IGetByContestId,
    IProblemUrlById,
    IGetByProblemId,
    ISubmissionTypeDocumentUrlById,
    IExamGroupUrlParams,
    IGetByTestId,
    IGetByRoleId,
    IGetByUserId,
    ISubmitContestPasswordParams,
    ISubmitContestSolutionParams,
    IGetSubmissionsByUserParams,
    IRegisterUserForContestParams,
};
