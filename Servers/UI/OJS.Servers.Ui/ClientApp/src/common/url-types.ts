/* eslint-disable import/prefer-default-export */
import { IFilter, ISort } from './contest-types';

interface IStartContestUrlParams {
    id: number;
    official: boolean;
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

interface IGetSubmissionResultsByProblemUrlParams {
    id: number;
    isOfficial: boolean;
    take: number;
}

interface IGetContestResultsParams {
    id: number;
    official: boolean;
    full: boolean;
}

interface IGetSubmissionDetailsByIdUrlParams {
    submissionId: number;
}

interface IRetestSubmissionUrlParams {
    id: number;
}

interface IGetSearchResultsUrlParams {
    searchTerm: string;
    page: number;
    selectedTerms: [];
}

export type {
    IRegisterForContestUrlParams,
    ISubmitContestPasswordUrlParams,
    IStartContestUrlParams,
    IAllContestsUrlParams,
    IContestCategoriesUrlParams,
    IStartContestParticipationUrlParams,
    IGetContestParticipationScoresForParticipantUrlParams,
    IDownloadProblemResourceUrlParams,
    IGetSubmissionResultsByProblemUrlParams,
    IGetSubmissionDetailsByIdUrlParams,
    IGetContestResultsParams,
    IRetestSubmissionUrlParams,
    IGetSearchResultsUrlParams,
    IGetContestByProblemUrlParams,
};
