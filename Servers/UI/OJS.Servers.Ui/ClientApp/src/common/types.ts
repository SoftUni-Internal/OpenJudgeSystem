import React from 'react';

import { ISubmissionDetailsType, ISubmissionResults, ITestRunType } from '../hooks/submissions/types';
import { PublicSubmissionState } from '../hooks/submissions/use-public-submissions';
import { IErrorDataType } from '../hooks/use-http';
import { IAdministrationFilter } from '../pages/administration-new/administration-filters/AdministrationFilters';
import { IAdministrationSorter } from '../pages/administration-new/administration-sorting/AdministrationSorting';

import { FilterColumnTypeEnum } from './enums';
import { SearchCategory } from './search-types';

interface ISubmissionTypeType {
    id: number;
    name: string;
    isSelectedByDefault: boolean;
    allowBinaryFilesUpload: boolean;
    allowedFileExtensions: string[];
}
interface ISubmissionTypeInProblem {
    id: number;
    name: string;
}
interface IPublicSubmissionContest {
    id: number;
    name: string;
}

interface IPublicSubmissionUser {
    id: string;
    username: string;
}

interface ISubmissionDetailsState {
    currentSubmission: ISubmissionDetailsType | null;
    currentSubmissionResults:IPagedResultType<ISubmissionResults>;
    validationErrors: IErrorDataType[];
    downloadErrorMessage: string | null;
}
interface ISubmissionDetailsReduxState extends ISubmissionDetailsState {
    currentPage: number;
}

interface IPublicSubmissionProblem {
    id: number;
    name: string;
    contest: IPublicSubmissionContest;
    orderBy: number;
}

interface IPublicSubmissionResult {
    points: number;
    maxPoints: number;
}

interface ISubmissionResponseModel {
    id: number;
    createdOn: Date;
    strategyName: string;
    user: IPublicSubmissionUser;
    problem: IPublicSubmissionProblem;
    result: IPublicSubmissionResult;
    state: PublicSubmissionState;
    isOfficial: boolean;
    isCompiledSuccessfully: boolean;
    maxMemoryUsed: number;
    maxTimeUsed: number;
    testRuns: ITestRunType[];
    processed: boolean;
}

interface IGetAllContestsOptions {
    status: string;
    sortType: string;
    page: number;
    category?: number | null;
    strategy?: number | null;
}

interface IGetAllAdminParams {
    filter?: string;
    ItemsPerPage: number;
    page: number;
    sorting?: string;
}

interface IProblemResourceType {
    id: number;
    name: string;
    link: string;
    type: number;
}

interface IProblemType {
    id: number;
    name: string;
    orderBy: number;
    points: number;
    maximumPoints: number;
    isExcludedFromHomework: boolean;
    memoryLimit: number;
    fileSizeLimit: number;
    timeLimit: number;
    checkerName: string;
    checkerDescription: string;
    resources: IProblemResourceType[];
    allowedSubmissionTypes: ISubmissionTypeType[];
}

interface IContestDetailsProblemType {
    name: string;
    orderBy: number;
    resources?: IProblemResourceType[];
}

interface IContestDetailsSubmissionType {
    id: number;
    name: string;
}

interface IContestDetailsResponseType {
    id: number;
    name: string;
    description: string;
    problems: IContestDetailsProblemType[];
    canViewResults: boolean;
    isOnlineExam: boolean;
    canBeCompeted: boolean;
    canBePracticed: boolean;
    isAdminOrLecturerInContest: boolean;
    allowedSubmissionTypes: IContestDetailsSubmissionType[];
    competeParticipantsCount: number;
    practiceParticipantsCount: number;
    categoryId: number;
}

interface IContestType {
    id: number;
    name: string;
    categoryId: number;
    categoryName: string;
    startTime: Date;
    endTime: Date;
    practiceStartTime: Date;
    practiceEndTime: Date;
    limitBetweenSubmissions: number;
    isDeleted: boolean;
    isVisible: boolean;
    isOnline: boolean;
    isExam: boolean;
    allowedSubmissionTypes: ISubmissionTypeType[];
    problems: IProblemType[];
    canBeCompeted: boolean;
    canBePracticed: boolean;
    resultsArePubliclyVisible: boolean;
    hasContestPassword: boolean;
    hasPracticePassword: boolean;
    userIsAdminOrLecturerInContest: boolean;
    userCanCompete: boolean;
    userIsParticipant: false;
    isActive: boolean;
    contestIsLoading: boolean;
    registerForContestLoading: boolean;
    submitContestPasswordIsLoading: boolean;
    getParticipantScoresIsLoading: boolean;
    duration: number;
    numberOfProblems: number;
}

interface IIndexContestsType {
    id: number;
    name: string;
    startTime: Date;
    endTime: Date;
    practiceStartTime: Date;
    practiceEndTime: Date;
    canBePracticed: boolean;
    canBeCompeted: boolean;
    hasContestPassword: boolean;
    hasPracticePassword: boolean;
    category: string;
    isLoading: boolean;
}

interface IParticiapntsInContestView {
    id:number;
    userName:string;
    contest:string;
    isOfficial:boolean;
}

interface IContestModalInfoType {
    id: number;
    name: string;
    duration: number;
    numberOfProblems: number;
}

interface IGetContestsForIndexResponseType {
    activeContests: IIndexContestsType[];
    pastContests: IIndexContestsType[];
}
interface IIndexProblemsType {
    id: number;
    name: string;
    group: number;
    groupType: string;
    contest: string;
    practiceTestsCount: number;
    competeTetstsCount: number;
    isDeleted: boolean;
}

interface IIndexContestsCategoriesType {
    id: number;
    name: string;
    parent: string;
    parentId: number;
    isDeleted: boolean;
    isVisible: boolean;
    orderBy: number;
    modifiedOn: Date;
    createdOn: Date;
    deletedOn: Date;
}

interface IRegisterForContestResponseType {
    id: number;
    name: string;
    requirePassword: boolean;
    participantId: number| null;
    isOnlineExam: boolean;
    duration: number;
    numberOfProblems: number;
    categoryId: number;
}

interface IStartParticipationResponseType {
    contest: IContestType;
    participantId: number;
    contestIsCompete: boolean;
    lastSubmissionTime: Date;
    endDateTimeForParticipantOrContest: Date | null;
    userSubmissionsTimeLimit: number;
    participantsCount: number;
}

interface IPagedResultType<TItem> {
    totalItemsCount: number;
    itemsPerPage: number;
    pagesCount: number;
    pageNumber: number;
    totalCount: number;
    items?: TItem[];
}

interface IAdminPagedResultType<TItem> {
    items?: TItem[];
    page: number;
    itemsPerPage: number;
    totalCount: number;
    totalPages: number;
}

interface IAdminContestResponseType {
    id: number;
    category: string;
    name: string;
    allowParallelSubmissionsInTasks: boolean;
    autoChangeTestsFeedbackVisibility: boolean;
    categoryId: number;
    startTime: string;
    endTime: string;
    contestPassword: string;
    description: string;
    isDeleted: boolean;
    isVisible: boolean;
    limitBetweenSubmissions: number;
}

interface IPage {
    page: number;
}

interface IUserType {
    id: string;
    username: string;
    email: string;
    permissions: IUserPermissionsType;
    isInRole: boolean;
    isAdmin: boolean;
    canAccessAdministration: boolean;
}

interface IProblemAdministration {
    id: number;
    name: string;
    maximumPoints: number;
    sourceCodeSizeLimit: number;
    orderBy: number;
    showResults: boolean;
    showDetailedFeedback: boolean;
    checkerId: number;
    problemGroupType: string;
    contestId: number;
    submissionTypes: Array<IProblemSubmissionType>;
    timeLimit: number;
    memoryLimit: number;
    // additionalFiles: File | null;
}

interface IUserRoleType {
    id: string;
    name: string;
}

interface IUserResponseType {
    id: string;
    userName: string;
    email: string;
    roles: IUserRoleType[];
}

interface IUserPermissionsType {
    canAccessAdministration: boolean;
}

interface ISearchProps<T> {
    searchTerm : string;
    searchCategory : SearchCategory;
    renderItem: (item: T) => React.ReactElement<any, string | React.JSXElementConstructor<any>>;
}

interface IContestAdministration {
    id: number;
    name: string;
    type: string;
    categoryId: number;
    categoryName: string;
    description: string | null;
    startTime: Date | null;
    endTime: Date | null;
    practiceStartTime: Date | null;
    practiceEndTime: Date | null;
    contestPassword: string | null;
    practicePassword: string | null;
    limitBetweenSubmissions: number;
    isVisible: boolean;
    newIpPassword: string | null;
    allowParallelSubmissionsInTasks: boolean;
    autoChangeTestsFeedbackVisibility: boolean;
    orderBy: number;
    allowedIps: string;
}
interface IContestAutocomplete {
    id: number;
    name: string;
}

interface IContestCategories {
    id: number;
    name: string;
}

interface IFilterColumn {
    columnName: string;
    columnType: FilterColumnTypeEnum;
}

interface IAdminSlice {
    [key: string]: null | {
        selectedFilters: IAdministrationFilter[];
        selectedSorters: IAdministrationSorter[];
    };
}

interface IRootStore {
    adminContests: IAdminSlice;
    adminProblems: IAdminSlice;
    adminContestsCategories: IAdminSlice;
}
type ExceptionData = {
    name: string;
    message: string;
}

interface IProblemSubmissionType{
    id: number;
    name: string;
    solutionSkeleton: string | null;
}

// eslint-disable-next-line import/prefer-default-export
export type {
    IIndexContestsType,
    ISearchProps,
    IGetContestsForIndexResponseType,
    IRegisterForContestResponseType,
    IStartParticipationResponseType,
    IContestType,
    IProblemType,
    IProblemResourceType,
    ISubmissionResponseModel,
    ISubmissionTypeType,
    IPagedResultType,
    IUserType,
    IPage,
    IUserResponseType,
    IUserPermissionsType,
    IContestModalInfoType,
    IContestDetailsResponseType,
    IContestDetailsProblemType,
    ISubmissionDetailsState,
    ISubmissionDetailsReduxState,
    IGetAllContestsOptions,
    IGetAllAdminParams,
    IAdminPagedResultType,
    IAdminContestResponseType,
    IContestAdministration,
    IFilterColumn,
    IParticiapntsInContestView,
    IAdminSlice,
    IRootStore,
    IContestCategories,
    ExceptionData,
    IIndexProblemsType,
    IProblemAdministration,
    IProblemSubmissionType,
    ISubmissionTypeInProblem,
    IContestAutocomplete,
    IIndexContestsCategoriesType,
};
