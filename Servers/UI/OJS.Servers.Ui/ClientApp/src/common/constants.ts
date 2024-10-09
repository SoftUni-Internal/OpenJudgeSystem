const FileValidationError = 'Invalid file extension.';
const PAGE_SIBLING_COUNT = 2;
const PAGE_BOUNDARY_COUNT = 0;
const EmptyUsernameErrorMessage = 'Username is required.';
const UsernameLengthErrorMessage = 'The username must be between 5 and 32 characters long.';
const UsernameFormatErrorMessage = 'The username can contain only Latin letters, numbers, and the symbols \'.\' or \'_\'. ' +
    'The username must start with a letter and end with a letter or a number.';
const EmptyPasswordErrorMessage = 'Please enter your password.';
const PasswordLengthErrorMessage = 'The password must be at least 6 characters long.';

const LECTURER = 'Lecturer';
const defaultPathIdentifier = 'api';

enum TestRunResultType {
    CorrectAnswer = 0,
    WrongAnswer = 1,
    TimeLimit = 2,
    MemoryLimit = 3,
    RunTimeError = 4,
}

enum ContestParticipationType {
    Compete = 'compete',
    Practice = 'practice',
}

enum ContestResultType {
    Simple = 'simple',
    Full = 'full',
}

export {
    TestRunResultType,
    ContestParticipationType,
    ContestResultType,
    FileValidationError,
    PAGE_SIBLING_COUNT,
    PAGE_BOUNDARY_COUNT,
    defaultPathIdentifier,
    EmptyUsernameErrorMessage,
    UsernameLengthErrorMessage,
    UsernameFormatErrorMessage,
    EmptyPasswordErrorMessage,
    PasswordLengthErrorMessage,
    LECTURER,
};
