import React, { createContext, useContext, useMemo } from 'react';
import isNil from 'lodash/isNil';

import {
    IAllContestsUrlParams,
    IDownloadProblemResourceUrlParams,
    IGetContestByProblemUrlParams,
    IGetContestParticipationScoresForParticipantUrlParams,
    IGetContestResultsParams,
    IGetSubmissionDetailsByIdUrlParams,
    IGetSubmissionResultsByProblemUrlParams,
    IRetestSubmissionUrlParams,
    IStartContestParticipationUrlParams,
    ISubmitContestPasswordUrlParams,
} from '../common/url-types';
import { IHaveChildrenProps } from '../components/common/Props';

interface IUrlsContext {
    getLoginSubmitUrl: () => string;
    getLogoutUrl: () => string;
    getAdministrationContestsGridUrl: () => string;
    getAdministrationNavigation: () => string;
    getProfileInfoUrl: () => string;
    getSubmissionsForProfileUrl: () => string;
    getParticipationsForProfileUrl: () => string;
    getIndexContestsUrl: () => string;
    getAllContestsUrl: (params: IAllContestsUrlParams) => string;
    getRegisterForContestUrl: (params: IStartContestParticipationUrlParams) => string;
    getSubmitContestPasswordUrl: (params: ISubmitContestPasswordUrlParams) => string;
    getStartContestParticipationUrl: (params: IStartContestParticipationUrlParams) => string;
    getContestParticipantScoresForParticipantUrl: (params: IGetContestParticipationScoresForParticipantUrlParams) => string;
    getSubmissionResultsByProblemUrl: (params: IGetSubmissionResultsByProblemUrlParams) => string;
    getPublicSubmissionsUrl: () => string;
    getSubmissionsTotalCountUrl: () => string;
    getSubmissionsDetailsUrl: () => string;
    getSubmissionDetailsByIdUrl: (params: IGetSubmissionDetailsByIdUrlParams) => string;
    getSubmitUrl: () => string;
    getSubmitFileUrl: () => string;
    getDownloadProblemResourceUrl: (params: IDownloadProblemResourceUrlParams) => string;
    getCategoriesTreeUrl: () => string;
    getAllContestStrategyFiltersUrl: () => string;
    getContestResultsUrl: (params: IGetContestResultsParams) => string;
    getHomeStatisticsUrl: () => string;
    getAdministrationRetestSubmission: (params: IRetestSubmissionUrlParams) => string;
    getContestByProblemUrl: (params: IGetContestByProblemUrlParams) => string;
}

const UrlsContext = createContext<IUrlsContext>({} as IUrlsContext);

type IUrlsProviderProps = IHaveChildrenProps

const
    {
        URLS:
            {
                UI_URL: baseUrl,
                ADMINISTRATION_URL: administrationBaseUrl,
            },
    } = window;

const baseApiUrl = `${baseUrl}/api`;

// auth
const getLoginSubmitUrl = () => `${baseUrl}/Account/Login`;
const getLogoutUrl = () => `${baseUrl}/Account/Logout`;

// admin
const getAdministrationContestsGridUrl = () => `${administrationBaseUrl}/Contests`;
const getAdministrationNavigation = () => '/administration';
const getAdministrationRetestSubmission = ({ id }: IRetestSubmissionUrlParams) => `
${administrationBaseUrl}/Submissions/Retest?PK=${id}`;

// profile
const getProfileInfoUrl = () => `${baseApiUrl}/Users/GetProfileInfo`;
const getSubmissionsForProfileUrl = () => `${baseApiUrl}/Submissions/GetForProfile`;
const getParticipationsForProfileUrl = () => `${baseApiUrl}/Participations/GetForProfile`;

// contests
const getIndexContestsUrl = () => `${baseApiUrl}/Contests/GetForHomeIndex`;
const getAllContestsUrl = ({ filters, sorting, page }: IAllContestsUrlParams) => {
    const filtersQuery = `${filters
        .map(({ value, type }) => `${type.toLowerCase()}=${value}`)
        .join('&')
    }`;

    const sortingQuery = `${sorting
        .map(({ value, type }) => `${type.toLowerCase()}=${value}`)
        .join('&')
    }`;

    const pageQuery = isNil(page)
        ? ''
        : `page=${page}`;

    return `${baseApiUrl}/Contests/GetAll?${filtersQuery}&${sortingQuery}&${pageQuery}`;
};

const getRegisterForContestUrl = ({
    id,
    isOfficial,
}: IStartContestParticipationUrlParams) => `${baseApiUrl}/Contests/Register/${id}?official=${isOfficial}`;

const getSubmitContestPasswordUrl = ({
    id,
    isOfficial,
}: ISubmitContestPasswordUrlParams) => `${baseApiUrl}/Contests/SubmitContestPassword/${id}?official=${isOfficial}`;

const getStartContestParticipationUrl = ({
    id,
    isOfficial,
}: IStartContestParticipationUrlParams) => `${baseApiUrl}/Compete/Index/${id}?official=${isOfficial}`;

const getContestParticipantScoresForParticipantUrl =
    ({ participantId }: IGetContestParticipationScoresForParticipantUrlParams) => `
    ${baseApiUrl}/ParticipantScores/GetScoresForParticipant/${participantId}`;

const getContestByProblemUrl = ({ problemId }: IGetContestByProblemUrlParams) => `${baseApiUrl}/Contests/GetByProblem/${problemId}`;

const getCategoriesTreeUrl =
    () => `${baseApiUrl}/ContestCategories/GetCategoriesTree`;

const getContestResultsUrl = ({
    id,
    official,
    full,
} : IGetContestResultsParams) => `${baseApiUrl}/ContestResults/GetResults/${id}?official=${official}&full=${full}`;

// submissions
const getSubmissionResultsByProblemUrl = ({
    id,
    isOfficial,
    take,
}: IGetSubmissionResultsByProblemUrlParams) => `
    ${baseApiUrl}/Submissions/GetSubmissionResultsByProblem/${id}?isOfficial=${isOfficial}&take=${take}`;
const getPublicSubmissionsUrl = () => `${baseApiUrl}/Submissions/Public`;
const getSubmissionsTotalCountUrl = () => `${baseApiUrl}/Submissions/TotalCount`;
const getSubmissionsDetailsUrl = () => `${baseApiUrl}/Submissions/Details`;
const getSubmissionDetailsByIdUrl =
    ({ submissionId }: IGetSubmissionDetailsByIdUrlParams) => `${getSubmissionsDetailsUrl()}/${submissionId}`;
const getSubmitUrl = () => `${baseApiUrl}/Compete/Submit`;
const getSubmitFileUrl = () => `${baseApiUrl}/Compete/SubmitFileSubmission`;

// Submission types
const getAllContestStrategyFiltersUrl =
    () => `${baseApiUrl}/SubmissionTypes/GetAllOrderedByLatestUsage`;

// problem resources
const getDownloadProblemResourceUrl = ({ id }: IDownloadProblemResourceUrlParams) => `${baseApiUrl}/ProblemResources/GetResource/${id}`;

// Statistics
const getHomeStatisticsUrl = () => `${baseApiUrl}/StatisticsPreview/GetForHome`;

const UrlsProvider = ({ children }: IUrlsProviderProps) => {
    const value = useMemo(
        () => ({
            getLoginSubmitUrl,
            getLogoutUrl,
            getAdministrationContestsGridUrl,
            getAdministrationNavigation,
            getAllContestsUrl,
            getRegisterForContestUrl,
            getSubmitContestPasswordUrl,
            getStartContestParticipationUrl,
            getContestParticipantScoresForParticipantUrl,
            getDownloadProblemResourceUrl,
            getSubmissionResultsByProblemUrl,
            getPublicSubmissionsUrl,
            getSubmissionsTotalCountUrl,
            getIndexContestsUrl,
            getProfileInfoUrl,
            getSubmissionsDetailsUrl,
            getSubmissionDetailsByIdUrl,
            getSubmissionsForProfileUrl,
            getParticipationsForProfileUrl,
            getSubmitUrl,
            getSubmitFileUrl,
            getCategoriesTreeUrl,
            getAllContestStrategyFiltersUrl,
            getContestResultsUrl,
            getHomeStatisticsUrl,
            getAdministrationRetestSubmission,
            getContestByProblemUrl,
        }),
        [],
    );

    return (
        <UrlsContext.Provider value={value}>
            {children}
        </UrlsContext.Provider>
    );
};

const useUrls = () => useContext(UrlsContext);

export default UrlsProvider;

export {
    useUrls,
};
