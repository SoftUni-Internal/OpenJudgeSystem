import { ITestRun } from '../hooks/submissions/types';

import { ContestVariation, SortType, SortTypeDirection } from './contest-types';
import { CheckboxSearchValues, FilterColumnTypeEnum, ProblemResourceType } from './enums';

interface ISubmissionTypeType {
    id: number;
    name: string;
    isSelectedByDefault: boolean;
    allowBinaryFilesUpload: boolean;
    allowedFileExtensions: string[];
    timeLimit: number;
    memoryLimit: number;
}
interface ISubmissionTypeInProblem {
    id: number;
    name: string;
}

interface ISubmissionTypeInDocument {
    id: number;
    name: string;
}
interface IPublicSubmissionContest {
    id: number;
    name: string;
}

interface IUserProfileType {
    id: string;
    userName: string;
    firstName: string;
    lastName: string;
    email: string;
    city?: string;
    age?: number;
}

interface IUsersState {
    profile: IUserProfileType | null;
}

interface ISearchSliceState {
    isVisible: boolean;
    searchValue: string;
    selectedTerms: Array<CheckboxSearchValues.contests | CheckboxSearchValues.problems | CheckboxSearchValues.users>;
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

interface IPublicSubmission {
    id: number;
    createdOn: Date;
    strategyName: string;
    user?: string;
    problem: IPublicSubmissionProblem;
    result: IPublicSubmissionResult;
    isOfficial: boolean;
    isCompiledSuccessfully: boolean;
    maxMemoryUsed: number;
    maxTimeUsed: number;
    testRuns: ITestRun[];
    testRunsCache?: string;
    processed: boolean;
}

interface ITestRunInListModel {
    id: number;
    timeUsed: number;
    memoryUsed: number;
    submissionId: number;
    executionComment: string;
    checkerComment: string;
    resultType: number;
}

interface IGetAllAdminParams {
    filter?: string;
    itemsPerPage: number;
    page: number;
    sorting?: string;
}

interface IContestsSortAndFilterOptions {
    strategy?: number;
    sortType: SortType;
    sortTypeDirection?: SortTypeDirection;
    page: number;
    itemsPerPage?: number;
    category?: number | null;
}

// TODO: Unify these types, some are called params, others options
interface IGetContestParticipationsForUserQueryParams extends IContestsSortAndFilterOptions {
    username: string;
}

interface IAllowedStrategyType {
    id: number;
    name: string;
}

interface IContestCategory {
    allowedStrategyTypes: Array<IAllowedStrategyType>;
    children: Array<IContestCategory>;
    id: number;
    name: string;
    nameUrl: string;
    orderBy: number;
    parentId: null | number;
}

interface IContestCategoryHierarchy {
    id: number;
    name: string;
    children: Array<IContestCategoryHierarchy>;
    parentId?: number;
}

interface IProblemResourceType {
    id: number;
    name: string;
    link: string;
    type: number;
}

interface IProblemResourceAdministrationModel {
    id: number;
    name: string;
    link: string;
    type: ProblemResourceType;
    orderBy: number;
    file: File | null;
    hasFile: boolean;
    problemId: number;
}

interface IProblemResouceInLinstModel {
    id: number;
    name: string;
    link: string;
    type: string;
    fileExtension: string;
    orderBy: number;
    isDeleted: boolean;
    problemId: number;
    problemName: string;
    createdOn: Date;
    modifiedOn: Date;
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

interface IProblemSearchType {
    id: number;
    name: string;
    orderBy: number;
    contest: IContestType;
}

interface IContestDetailsSubmissionType {
    id: number;
    name: string;
}

interface IContestDetailsResponseType {
    id: number;
    name: string;
    description: string;
    type: ContestVariation;
    problems: IProblemType[];
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

interface IContestDetailsSliceType {
    id: number;
    name?: string;
    description?: string;
    problems?: IProblemType[];
    canViewCompeteResults?: boolean;
    canViewPracticeResults?: boolean;
    isOnlineExam?: boolean;
    canBeCompeted?: boolean;
    canBePracticed?: boolean;
    isAdminOrLecturerInContest?: boolean;
    allowedSubmissionTypes?: IContestDetailsSubmissionType[];
    competeParticipantsCount?: number;
    practiceParticipantsCount?: number;
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

interface IUserParticipationResult {
    practicePoints: number;
    competePoints: number;
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
    numberOfProblems: number;
    practiceResults: number;
    competeResults: number;
    competeMaximumPoints: number;
    practiceMaximumPoints: number;
    userParticipationResult?: IUserParticipationResult;
    createdOn: Date;
    modifiedOn: Date | null;
    officialParticipants: number;
    requirePasswordForCompete: boolean;
    requirePasswordForPractice: boolean;
}

interface IIndexProblemsType {
    id: number;
    name: string;
    problemGroupId: number;
    groupType: string;
    contest: string;
    practiceTestsCount: number;
    competeTetstsCount: number;
    isDeleted: boolean;
    contestId: number;
    createdOn: Date;
    modifiedOn: Date;
}

interface IIndexContestCategoriesType {
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

interface IContestCategoryAdministration {
    id: number;
    name: string;
    parent: string | null;
    parentId: number | null;
    isDeleted: boolean;
    isVisible: boolean;
    orderBy: number;
    deletedOn: Date | null;
    modifiedOn: Date | null;
}

interface IRegisterUserForContestResponseType {
    id: number;
    name: string;
    isOfficial: boolean;
    requirePassword: boolean;
    shouldConfirmParticipation: boolean; // if practice this should be false
    isRegisteredSuccessfully: boolean;
    duration: number;
    numberOfProblems: number;
    categoryId: number;
}

interface ICompeteContestResponseType {
    isRegisteredParticipant: boolean; // if user has participant,
    isInvalidated: boolean;
    isActiveParticipant: boolean; // if participant is valid,
    participantId: number;
    lastSubmissionTime: Date;
    contestIsCompete: boolean;
    userSubmissionsTimeLimit: number;
    endDateTimeForParticipantOrContest: Date | null;
    participantsCount: number;
    contest: IContestType | null;
    shouldEnterPassword: boolean;
}

interface IPagedResultType<TItem> {
    totalItemsCount: number;
    itemsPerPage: number;
    pagesCount: number;
    pageNumber: number;
    items?: TItem[];
}

interface IUserType {
    id: string;
    userName: string;
    email: string;
    permissions: IUserPermissionsType;
    isInRole: boolean;
    isAdmin: boolean;
    isLecturer: boolean;
    isDeveloper: boolean;
    canAccessAdministration: boolean;
}

interface IProblemAdministration {
    id: number | undefined;
    name: string;
    maximumPoints: number;
    sourceCodeSizeLimit: number;
    orderBy: number;
    showResults: boolean;
    showDetailedFeedback: boolean;
    checkerId: string;
    problemGroupType: string;
    contestId: number;
    contestName: string;
    submissionTypes: Array<IProblemSubmissionType>;
    timeLimit: number;
    memoryLimit: number;
    contestType: ContestVariation;
    tests: File | null;
    problemGroupOrderBy: number;
    problemGroupId : number;
}

interface ISubmissionTypesInListModel {
    id: number;
    name: string;
    executionStrategyType: string;
    compilerType: string;
    allowBinaryFilesUpload: boolean;
    allowedFileExtensions: string;
}

interface ISubmissionTypeInSubmissionDocumentInListModel {
    submissionTypeId: number;
    submissionTypeName: string;
    submissionTypeDocumentId: number;
    submissionTypeDocumentTitle: string;
}

interface ISubmissionTypeInSubmissionDocumentInViewModel {
    submissionTypeDocumentId: number;
    submissionTypeDocumentTitle: string;
    submissionTypeDocumentContent: string;
    submissionTypes: Array<string>;
}

interface IProblemGroupDropdownModel {
    id: number;
    orderBy: number;
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
    visibleFrom: Date | null;
    newIpPassword: string | null;
    allowParallelSubmissionsInTasks: boolean;
    orderBy: number;
    allowedIps: string;
    numberOfProblemGroups: number;
    duration: string | undefined;
    canBeCompeted: boolean;
    officialParticipants: number;
}

interface ISubmissionsAdminGridViewType {
    id: number;
    processed: boolean;
    points: number;
    participantId: number;
    participantName: string;
    problemId: number;
    problemName: string;
    submissionTypeId: number;
    submissionTypeName: string;
    isDeleted: boolean;
}

interface ISubmissionForProcessingAdminGridViewType {
    id: number;
    processed: boolean;
    processing: boolean;
    serializedException: string;
    serializedExecutionDetails: string;
    serializedExecutionResult: string;
    submissionId: number;
    createdOn: Date;
    modifiedOn: Date;
}

interface IHasNameAndIdType {
    id: number;
    name: string;
}

interface IContestAutocomplete extends IHasNameAndIdType {}

interface ITestsUploadModel {
    problemId: number;
    tests: File | null;
    retestProblem: boolean;
    deleteOldTests: boolean;
}

interface ITestsDropdownData {
    id: number;
    name: string;
}

interface IFileModel {
    blob: Blob;
    filename: string;
}
interface IContestCategories {
    id: number;
    name: string;
}

interface IEnumType {
    enumValues?: Array<string>;
}

interface IFilterColumn {
    columnName: string;
    columnType: FilterColumnTypeEnum;
    enumValues?: Array<string> | null;
}

type ExceptionData = {
    name: string;
    message: string;
    status?: number;
}

interface IProblemGroupsData {
    id :number;
    contestId: number;
    contest: string;
    isDeleted: boolean;
    orderBy: number;
    type: string;
    createdOn: Date;
    modifiedOn: Date;
}
interface IProblemSubmissionType{
    id: number;
    name: string;
    solutionSkeleton: string | null;
    timeLimit: number | null;
    memoryLimit: number | null;
}

interface IIndexExamGroupsType {
    id: number;
    name: string;
    contest: string;
    externalAppId: string;
    externalExamGroupId: string;
}

interface IExamGroupAdministration {
    id: number;
    name: string;
    contestName: string;
    contestId: number | null;
    externalAppId: string;
    externalExamGroupId: number;
}

interface IUserInExamGroupModel {
    id: string;
    username: string;
}

interface IUserAutocomplete {
    id: string;
    userName: string;
}

interface IReplaceSubmissionTypeModel {
    submissionTypeToReplace: number;
    submissionTypeToReplaceWith?: number | null;
}

interface ISubmissionTypeAdministrationModel {
    id: number;
    name: string;
    executionStrategyType: string;
    compilerType: string;
    additionalCompilerArguments: string;
    description: string;
    allowedFileExtensions: string;
    isSelectedByDefault: boolean;
    allowBinaryFilesUpload: boolean;
    baseTimeUsedInMilliseconds?: number;
    baseMemoryUsedInBytes?: number;
    maxAllowedTimeLimitInMilliseconds?: number;
    maxAllowedMemoryLimitInBytes?: number;
}

interface ISubmissionTypeInSubmissionDocumentAdministrationModel {
    submissionTypeId: number;
    submissionTypeDocumentId: number;
}

interface ISubmissionTypeDocumentAdministrationModel {
    id: number;
    title: string;
    content: string;
    orderBy: number;
    submissionTypesInSubmissionDocuments: Array<ISubmissionTypeInSubmissionDocumentAdministrationModel>;
}

interface ICheckerInListModel {
    id: number;
    name: string;
    dllFile: string;
    className: string;
    parameter: string;
    isDeleted: boolean;
    createdOn: Date;
    modifiedOn: Date;
}

interface ICheckerAdministrationModel {
    id: number;
    name: string;
    dllFile: string | null;
    className: string | null;
    parameter: string | null;
    description: string | null;
}

interface IParticipantInListModel {
    id: number;
    userName: string;
    contestName: string;
    contestId: number;
    isOfficial: boolean;
    createdOn: Date;
    modifiedOn: Date;
}

interface IParticipantAdministrationModel {
    id: number;
    userName: string;
    contestId: number;
    contestName: string;
    userId: string;
    isOfficial: boolean;
}

interface IUserAutocompleteData {
id: string;
userName: string;
}

interface IRoleInListModel {
    id: string;
    name: string;
}

interface IRoleAdministrationModel {
    id: string | null;
    name: string;
}

interface IUserInListModel {
    id: string;
    userName: string;
    email: string;
    firstName: string;
    lastName: string;
    city: string;
    dateOfBirth: Date;
    age: number;
    createdOn: Date;
    modifiedOn: Date;
}

interface IUserSettingsAdministrationModel {
    firstName: string | null;
    lastName: string | null;
    city: string | null;
    dateOfBirth: Date | null;
    age: number;
    company: string | null;
    jobTitle: string | null;
    facultyNumber: number | null;
    educationalInstitution: string | null;
}

interface IUserAdministrationModel {
    id: string;
    userName: string;
    email: string;
    userSettings: IUserSettingsAdministrationModel;
    roles: Array<IUserRoleType>;
}

interface IContestActivity {
    canBeCompeted: boolean;
}

interface ISettingInListModel {
    id: number;
    name: string;
    value: string;
    type: string;
}

interface ISettingAdministrationModel {
    id: number | null;
    name: string;
    value: string;
    type: string;
}

interface IUSerSearchCardProps {
    id: string;
    name: string;
}

interface IMappingEntityId {
    firstEntityId: number;
    secondEntityId: number;
}

type AdjacencyList<K extends string | number, V> = {
    [key in K]: V;
};
interface IContestCategoryHierarchyEdit {
    id: number;
    parentId?: number;
}

// eslint-disable-next-line import/prefer-default-export
export type {
    IIndexContestsType,
    IProblemType,
    IProblemResourceType,
    IPublicSubmission,
    ISubmissionTypeType,
    IPagedResultType,
    IUserType,
    IUserProfileType,
    IUsersState,
    IUserResponseType,
    IContestDetailsResponseType,
    IContestsSortAndFilterOptions,
    IGetContestParticipationsForUserQueryParams,
    IContestCategory,
    IContestCategoryHierarchy,
    IGetAllAdminParams,
    IContestAdministration,
    IFilterColumn,
    ISubmissionsAdminGridViewType,
    ISubmissionForProcessingAdminGridViewType,
    IContestCategories,
    ExceptionData,
    IIndexProblemsType,
    IProblemAdministration,
    IProblemSubmissionType,
    ISubmissionTypeInProblem,
    ISubmissionTypeInDocument,
    IContestAutocomplete,
    IProblemGroupsData,
    IIndexContestCategoriesType,
    IContestCategoryAdministration,
    ITestsDropdownData,
    IProblemResouceInLinstModel,
    IProblemResourceAdministrationModel,
    ITestsUploadModel,
    IFileModel,
    IEnumType,
    IIndexExamGroupsType,
    IExamGroupAdministration,
    IUserInExamGroupModel,
    IHasNameAndIdType,
    IUserAutocomplete,
    ISubmissionTypesInListModel,
    ISubmissionTypeInSubmissionDocumentInListModel,
    ISubmissionTypeInSubmissionDocumentInViewModel,
    IReplaceSubmissionTypeModel,
    ISubmissionTypeAdministrationModel,
    ISubmissionTypeInSubmissionDocumentAdministrationModel,
    ISubmissionTypeDocumentAdministrationModel,
    ITestRunInListModel,
    IProblemGroupDropdownModel,
    ICheckerInListModel,
    ICheckerAdministrationModel,
    IParticipantAdministrationModel,
    IParticipantInListModel,
    IUserAutocompleteData,
    IRoleInListModel,
    IRoleAdministrationModel,
    IUserInListModel,
    IUserAdministrationModel,
    IContestActivity,
    ISettingInListModel,
    ISettingAdministrationModel,
    IRegisterUserForContestResponseType,
    ICompeteContestResponseType,
    ISearchSliceState,
    IUSerSearchCardProps,
    IProblemSearchType,
    IContestDetailsSliceType,
    IMappingEntityId,
    AdjacencyList,
    IContestCategoryHierarchyEdit,
};
