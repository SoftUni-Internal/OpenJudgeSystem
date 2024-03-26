/* eslint-disable import/prefer-default-export */
import { IFilter, ISort } from './contest-types';
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

interface IUserUrlParams {
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
interface IGetByProblemGroupId extends IGetAllAdminParams {
    problemGroupId: number;

}

interface IGetByTestId extends IGetAllAdminParams {
    testId: number;
}

interface IGetByExamGroupId extends IGetAllAdminParams {
    examGroupId: number;
}

interface IGetContestByProblemUrlParams {
    problemId: number;
}

interface IAllContestsUrlParams {
    filters: IFilter[];
    sorting: ISort[];
    page?: number;
}

interface IContestCategoriesUrlParams {
    id: number;
}

interface IRegisterForContestUrlParams {
    id: number;
    isOfficial: boolean;
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

interface IDownloadSubmissionFileUrlParams {
    id: number | null;
}

interface IGetSubmissionsUrlParams {
    status: SubmissionStatus;
    page: number;
}

interface IGetUserSubmissionsForProfileUrlParams {
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
}

interface IGetSubmissionDetailsByIdUrlParams {
    submissionId: number;
    page: number;
}

interface IRetestSubmissionUrlParams {
    id: number;
}

interface IContestProblemsUrlParams {
    id: number;
}

interface IProblemUrlById {
    id: number;
}

interface IContestEditUrlParams {
    id: number;
}

interface ITerm {
    key: string;
    value: string;
}

interface IGetSearchResultsUrlParams {
    searchTerm: string;
    selectedTerms: ITerm[];
}

interface IGetSearchResultsParams {
    searchTerm: string;
    page: number;
    searchCategory: string;
}

export type {
    IUserInfoUrlParams,
    IRegisterForContestUrlParams,
    ISubmitContestPasswordUrlParams,
    IContestDetailsUrlParams,
    IAllContestsUrlParams,
    IGetSearchResultsParams,
    IContestCategoriesUrlParams,
    IStartContestParticipationUrlParams,
    IGetContestParticipationScoresForParticipantUrlParams,
    IGetUserSubmissionsForProfileByContestUrlParams,
    IDownloadProblemResourceUrlParams,
    IGetSubmissionsUrlParams,
    IGetSubmissionResultsByProblemUrlParams,
    IGetSubmissionDetailsByIdUrlParams,
    IGetContestResultsParams,
    IRetestSubmissionUrlParams,
    IGetSearchResultsUrlParams,
    IGetContestByProblemUrlParams,
    IGetUserSubmissionsForProfileUrlParams,
    IGetByExamGroupId,
    IDownloadSubmissionFileUrlParams,
    IContestProblemsUrlParams,
    IContestEditUrlParams,
    IGetByContestId,
    IProblemUrlById,
    IGetByProblemId,
    IGetByProblemGroupId,
    IExamGroupUrlParams,
    IUserUrlParams,
    IGetByTestId,
    IGetByRoleId,
    IGetByUserId,
};
