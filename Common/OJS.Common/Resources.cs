namespace OJS.Common;

public static class Resources
{
    public static class ContestsGeneral
    {
        public const string BinaryFilesNotAllowed = "This submission type does not allow sending binary files";
        public const string Cancel = "Cancel";
        public const string CategoryNotFound = "This category does not exist!";
        public const string Compete = "Compete";
        public const string Practice = "Practice";
        public const string Contest = "Contest";
        public const string ContestCannotBeCompeted = "This contest cannot be competed!";
        public const string ContestCannotBePracticed = "This contest cannot be practiced!";
        public const string ContestNotFound = "Invalid contest id was provided!";
        public const string ContestResultsNotAvailable = "This contest results are not available!";
        public const string InvalidExtention = "Invalid file extension";
        public const string InvalidRequest = "Invalid request!";
        public const string InvalidSelection = "Invalid selection";
        public const string MainCategories = "Main categories";
        public const string ProblemNotAssignedToUser = "The problem is not part of your problems for the exam";
        public const string ProblemNotFound = "The problem was not found!";
        public const string ProblemResultsNotAvailable = "You cannot view the results for this problem!";
        public const string ReportExcelFormat = "Ranking for {0} {1}{2}";
        public const string ResourceCannotBeDownloaded = "This resource cannot be downloaded!";
        public const string SolutionUploaded = "Solution uploaded.";
        public const string SubmissionNotFound = "Invalid submission requested!";
        public const string SubmissionNotMadeByUser = "This submission was not made by you!";
        public const string SubmissionTooLong = "The submitted code is too long!";
        public const string SubmissionTypeNotFound = "Wrong submission type!";
        public const string SubmissionWasSentTooSoon = "Submission was sent too soon!";
        public const string TextUploadNotAllowed = "This submission type does not allow sending text";
        public const string UploadFile = "Please upload file.";
        public const string UserHasNotProcessedSubmissionForProblem = "You have unprocessed submission for this problem. Please wait until the submission is processed.";
        public const string UserIsNotRegisteredForExam = "You are not registered for this exam!";
        public const string Yes = "Yes";
    }

    public static class ParticipantsBusiness
    {
        public const string ParticipantDoesNotExist = "Participant does not exist!";
        public const string ContestDurationNotSet = "The contest does not have duration set!";
        public const string ParticipantParticipationTimeNotSet = "Participant does not have participation time set!";
        public const string ParticipationTimeReduceBelowDurationWarning = "Participant time cannot be reduced, because it will be below the contest duration!";
    }

    public static class ProblemGroupsBusiness
    {
        public const string CannotCopyProblemGroupsIntoActiveContest = "Cannot copy problem group into an active contest";
        public const string CannotCopyProblemGroupsIntoSameContest = "Cannot copy problem groups into the same contest";
        public const string CannotDeleteProblemGroupWithProblems = "Cannot delete problem group with problems";
    }

    public static class ProblemsBusiness
    {
        public const string CannotCopyProblemsIntoActiveContest = "Cannot copy problems into an active contest";
        public const string CannotCopyProblemsIntoSameContest = "Cannot copy problems into the same contest";
    }

    public static class AdministrationGeneral
    {
        public const string Add = "Add";
        public const string BackToNavigation = "Back to navigation";
        public const string Cancel = "Cancel";
        public const string Change = "Change";
        public const string ChooseFile = "Choose File";
        public const string Create = "Create";
        public const string CreatedOn = "Created on";
        public const string Delete = "Delete";
        public const string DeletePrompt = "Do you want to delete this item?";
        public const string Edit = "Edit";
        public const string ExportToExcel = "Export To Excel";
        public const string GroupByMessage = "Drag and drop here the title of the column you wish to group by";
        public const string ModifiedOn = "Modified on";
        public const string Name = "Name";
        public const string NoPrivilegesMessage = "You don't have privileges for this action!";
        public const string NoPermissionsForContest = "You don't have permissions for the contest!";
        public const string Remove = "Remove";
        public const string CannotEditSubmissionForProcessing = "Submissions for processing cannot be edited!";
    }

    public static class ContestsControllers
    {
        public const string ActiveContestCannotEditDurationType = "Contest is active and Duration and Type cannot be edited!";
        public const string ActiveContestForbiddenForDeletion = "Contest is active and cannot be deleted!";
        public const string ActiveContestForbiddenForTransfer = "Contest is active and participants cannot be transferred";
        public const string AddedTimeToParticipantsOnline = "Successfully added {0} minutes to the times of all selected active participants in the contest {1}";
        public const string AddedTimeToSingleParticipantOnline = "Successfully added {0} minutes to the time of the participant with username: {1}, in the contest {2}";
        public const string ContestAdded = "Contest added successfully";
        public const string ContestEdited = "Contest edited successfully";
        public const string ContestNotFound = "Contest not found";
        public const string ContestNotValid = "Contest is not valid";
        public const string ContestStartDateBeforeEnd = "Contest start date must be before the contest end date";
        public const string DurationInvalidFormat = "Duration must be in format \"hh:mm\"";
        public const string FailedToUpdateParticipantsDuration = "<br />WARNING: The following users' contest duration was not updated because their contest duration would have been reduced below the base contest duration: {0}";
        public const string NoActiveContests = "No active contests";
        public const string NoFutureContests = "No future contests";
        public const string NoLatestContests = "No latest contests";
        public const string NoQuestionById = "Question could not be found by given Id";
        public const string ParticipantNotInContest = "The participant is not in the contest!";
        public const string ParticipantsTransferred = "Participants transferred successfully";
        public const string PracticeStartDateBeforeEnd = "Practice start date must be before the practice end date";
        public const string ProblemGroupsCountLimit = "Problem groups count cannot be more than {0}";
        public const string ReportZipFormat = "{1} submissions for {0}.zip";
        public const string RequiredFieldForOnline = "The field is required for Online Contest";
        public const string RequiredContestEndTimeField = "Contest EndTime cannot be null!";
        public const string SelectOneSubmissionType = "Choose at least one submission type!";
        public const string SubtractedTimeFromParticipantsOnline = "Successfully subtracted {0} minutes from the times of all selected active participants in the contest {1}";
        public const string SubtractedTimeFromSingleParticipantOnline = "Successfully subtracted {0} minutes from the time of the participant with username: {1}, in the contest {2}";
        public const string CategoryNotSelected = "Category not selected!";
    }

    public static class ContestCategoriesController
    {
        public const string RequiredName = "Name cannot be null!";
        public const string ContestCategoryOrderByCanNotBeNegative =
            "The order by of the contest category cannot be a negative number!";
    }

    public static class ExamGroupsController
    {
        public const string CannotAddUsers = "You cannot add users to exam group that is not attached to a contest.";
        public const string CannotAttachContest = "You don't have privileges to attach this contest to an exam group!";
        public const string CannotDeleteGroupWithActiveContest = "You cannot delete exam group which has active contest attached to it.";
        public const string CannotRemoveUsers = "You cannot remove users from exam group that is not attached to a contest.";
        public const string UsersAdded = "Users were added successfully to the exam group.";
    }

    public static class ProblemsController
    {
        public const string ActiveContestProblemsPermittedForDeletion = "The contest is active and problems cannot be deleted!";
        public const string CopyProblemSuccessMessage = "Successfully copied the problem \"{0}\" into the contest \"{1}\"";
        public const string InvalidContest = "Invalid contest";
        public const string InvalidProblem = "Invalid problem";
        public const string InvalidProblemGroup = "Invalid problem group";
        public const string InvalidTests = "Invalid tests";
        public const string InvalidUser = "Invalid user";
        public const string MustBeZipFile = "Uploaded file must be in .ZIP format.";
        public const string ProblemAdded = "Problem added successfully.";
        public const string ProblemDeleted = "Problem deleted successfully.";
        public const string ProblemEdited = "Problem edited successfully.";
        public const string ProblemNotFound = "The problem was not found!";
        public const string ProblemRetested = "Problem retested successfully.";
        public const string ProblemsDeleted = "Problems deleted successfully.";
        public const string ResourcesNotComplete = "Resources must be fully entered.";
        public const string SelectOneSubmissionType = "Choose at least one submission type!";
        public const string TestsCannotBeImprorted = "The tests cannot be imported: {0}";
    }

    public static class ProblemsRetest
    {
        public const string Retest = "Retest";
        public const string RetestConfirmationMessage = "Are you sure you want to Retest <strong>{0} submissions</strong> from the problem <strong>{1}</strong>?";
    }

    public static class ProblemGroupsControllers
    {
        public const string ActiveContestCannotAddProblemGroup = "The contest is аctive and you cannot add problem groups";
        public const string ActiveContestCannotDeleteProblemGroup = "The contest is аctive and you cannot delete problem groups";
        public const string CanCreateOnlyInContestWithRandomTasks = "You can create problem groups only in a contest with random tasks";
        public const string CanEditOrderByOnlyInContestWithRandomTasks = "You can edit problem groups order only in a contest with random tasks.";
        public const string ContestDoesNotExist = "The selected contest does not exist";
        public const string ContestRequired = "Contest is required";
        public const string CopyAllProblemGroupsSuccessMessage = "Successfully copied all problem groups from the contest \"{0}\" into the contest \"{1}\"";
    }

    public static class TestsControllers
    {
        public const string NoEmptyFile = "File must not be empty";
        public const string MustBeZip = "File must be a zip";
        public const string ZipDamaged = "Zip is damaged";
        public const string InvalidTests = "Invalid tests";
        public const string TestsAddedToProblem = "{0} tests added to problem successfully.";
    }

    public static class ProblemsDeleteAll
    {
        public const string Confirm = "Confirm";
        public const string ConfirmMessage = "Do you want to delete all problems for";
        public const string PageTitle = "Delete all problems";
    }

    public static class CopyProblem
    {
        public const string BulkCopyContestLabel = "Search for the contest in which you want to copy all the problems";
        public const string ContestLabel = "Search for the contest in which you want to copy the problem in";
        public const string ContestRequired = "Contest is required";
        public const string ProblemGroupLabel = "Problem group:";
        public const string ProblemGroupPlaceholder = "Copy into a new problem group";
    }

    public static class Partials
    {
        public const string Back = "Back";
        public const string ChooseContest = "Choose contest...";
        public const string ChooseResource = "Choose resource";
        public const string Copy = "Copy";
        public const string DeleteCurrentQuestions = "Delete current questions";
        public const string Details = "Details";
        public const string File = "File";
        public const string Name = "Name";
        public const string Type = "Type";
    }

    public static class ProblemResourcesController
    {
        public const string FileRequired = "The file is required!";
        public const string OnlyLinkAllowed = "The resource should not have file, but only link";
        public const string OnlyFileAllowed = "The resource should not have link, but only file";
        public const string InvalidResource = "Resource is invalid!";
        public const string LinkNotEmpty = "The link must not be empty";
        public const string ProblemNotFound = "Problem not found.";
        public const string ResourceNotFound = "Resource not found.";
    }

    public static class SubmissionsController
    {
        public const string SubmissionIsAddedInQueueForProcessing = "The submission is added in the queue for processing!";
        public const string SubmissionCanNotBeProcessed = "Submission can't be processed!";
        public const string SubmissionIsProcessing = "Submission is processing, please wait to be processed for retesting!";
        public const string SubmissionNotFound = "Submission was not found!";
        public const string SubmissionNotFileUpload = "The submission does not have an attachment!";
    }
}