import {
    IAllContestsPageUrlParams,
    IContestsDetailsPageUrlParams,
    IContestsRegisterPageUrlParams,
    IContestsResultsPageUrlParams,
    IContestsSolutionSubmitPageUrlParams,
} from '../app-url-types';
import { ContestParticipationType, ContestResultType } from '../constants';
import { createUrlFriendlyPath } from '../contest-helpers';

import { CONTESTS_PATH } from './client-urls';

const getContestsResultsPageUrl = ({
    contestName,
    contestId,
    participationType,
    isSimple,
}: IContestsResultsPageUrlParams) => `/contests${createUrlFriendlyPath(contestName)}/${contestId}/${participationType}/results/${isSimple
    ? ContestResultType.Simple
    : ContestResultType.Full}`;

const getAllContestsPageUrl = ({
    categoryName,
    strategyId,
    categoryId,
} : IAllContestsPageUrlParams) => {
    const slug = categoryName
        ? `${createUrlFriendlyPath(categoryName)}`
        : '';

    const categoryPath = categoryId
        ? `/by-category${slug}/${categoryId}`
        : '';

    const returnUrl = `/${CONTESTS_PATH}${categoryPath}`;
    const prefixes = [ `${strategyId
        ? `strategy=${strategyId}`
        : ''}` ];
    const composedString = prefixes.filter((prefix) => prefix).join('&');
    if (composedString.length > 0) {
        return `${returnUrl}?${composedString}`;
    }

    return returnUrl;
};

const getContestsSolutionSubmitPageUrl = ({
    isCompete,
    contestId,
    contestName,
    problemId,
    orderBy,
}: IContestsSolutionSubmitPageUrlParams) => `/${CONTESTS_PATH}${createUrlFriendlyPath(contestName)}/${contestId}/${isCompete
    ? ContestParticipationType.Compete
    : ContestParticipationType.Practice}${problemId
    ? `#${orderBy}`
    : ''}`;

const getContestsDetailsPageUrl = ({
    contestId,
    contestName,
}: IContestsDetailsPageUrlParams) => `/contests${createUrlFriendlyPath(contestName)}/${contestId}`;

const getContestsRegisterPageUrl = ({
    isCompete,
    contestId,
    contestName,
}: IContestsRegisterPageUrlParams) => `/contests${createUrlFriendlyPath(contestName)}/${contestId}/${isCompete
    ? ContestParticipationType.Compete
    : ContestParticipationType.Practice}/register`;

export {
    getContestsResultsPageUrl,
    getAllContestsPageUrl,
    getContestsSolutionSubmitPageUrl,
    getContestsDetailsPageUrl,
    getContestsRegisterPageUrl,
};
