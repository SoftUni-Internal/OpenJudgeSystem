import { ContestParticipationType } from './constants';

interface ISubmissionDetailsUrlParams {
    submissionId: number;
}

interface IContestsResultsPageUrlParams {
    contestName?: string;
    contestId?: number;
    participationType: ContestParticipationType;
    isSimple: boolean;
}

interface IAllContestsPageUrlParams {
    categoryId?: number | string | null;
    strategyId?: number | string | null;
    categoryName?: string;
}

interface IContestsSolutionSubmitPageUrlParams {
    isCompete?: boolean;
    contestId?: number | string;
    contestName?: string | null;
    problemId?: number;
    orderBy?: number;
}

interface IContestsDetailsPageUrlParams {
    contestId?: number;
    contestName?: string | null;
}

interface IContestsRegisterPageUrlParams {
    isCompete?: boolean;
    contestId?: number | string;
    contestName?: string | null;
}

export type {
    ISubmissionDetailsUrlParams,
    IContestsResultsPageUrlParams,
    IAllContestsPageUrlParams,
    IContestsSolutionSubmitPageUrlParams,
    IContestsDetailsPageUrlParams,
    IContestsRegisterPageUrlParams,
};
