enum ThemeMode {
    Light = 'light',
    Dark = 'dark',
}

enum FilterColumnTypeEnum {
    STRING = 'text',
    BOOL = 'boolean',
    NUMBER = 'number',
    DATE = 'date',
    ENUM = 'enum',
}

enum SortingEnum {
    ASC = 'ASC',
    DESC = 'DESC',
}

enum ProblemGroupTypes {
    None = 0,
    ExcludedFromHomework = 1,
}

enum SubmissionStatus {
    All = 1,
    Processing = 2,
    Pending = 3,
}

enum ProblemResourceType
{
    ProblemDescription = 1,
    AuthorsSolution = 2,
    Other = 3,
}

enum SettingTypeEnums {
    Numeric = 1,
    ShortString = 2,
    LongString= 3,
    DateTime= 4,
    Boolean= 5,
}

enum CheckboxSearchValues {
    contests = 'Contests',
    problems = 'Problems',
    users = 'Users',
}

enum RolesEnum {
    Lecturer = 'Lecturer',
    Developer = 'Developer',
    Administrator = 'Administrator',
}

enum SubmissionStrategyType {
    NotFound = 0,
    CompileExecuteAndCheck = 1,
    NodeJsPreprocessExecuteAndCheck = 2,
    JavaPreprocessCompileExecuteAndCheck = 4,
    // PhpCgiExecuteAndCheck = 5,
    // PhpCliExecuteAndCheck = 6,
    CheckOnly = 7,
    JavaZipFileCompileExecuteAndCheck = 8,
    PythonExecuteAndCheck = 9,
    NodeJsPreprocessExecuteAndRunUnitTestsWithMocha = 11,
    NodeJsPreprocessExecuteAndRunJsDomUnitTests = 12,
    MySqlPrepareDatabaseAndRunQueries = 16,
    MySqlRunQueriesAndCheckDatabase = 17,
    MySqlRunSkeletonRunQueriesAndCheckDatabase = 18,
    NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy = 19,
    // NodeJsZipPreprocessExecuteAndRunUnitTestsWithDomAndMocha = 20,
    // NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy = 21,
    NodeJsZipExecuteHtmlAndCssStrategy = 22,
    // CSharpUnitTestsExecutionStrategy = 23,
    // CSharpProjectTestsExecutionStrategy = 24,
    JavaProjectTestsExecutionStrategy = 25,
    CPlusPlusZipFileExecutionStrategy = 26,
    JavaUnitTestsExecutionStrategy = 27,
    CPlusPlusCompileExecuteAndCheckExecutionStrategy = 29,
    JavaSpringAndHibernateProjectExecutionStrategy = 30,
    // RubyExecutionStrategy = 32,
    DotNetCoreProjectExecutionStrategy = 33,
    DotNetCoreProjectTestsExecutionStrategy = 35,
    DotNetCoreCompileExecuteAndCheck = 37,
    DotNetCoreUnitTestsExecutionStrategy = 38,
    DoNothing = 40,
    PythonUnitTests = 41,
    PythonCodeExecuteAgainstUnitTests = 42,
    PythonProjectTests = 43,
    PythonProjectUnitTests = 44,
    SqlServerSingleDatabasePrepareDatabaseAndRunQueries = 45,
    SqlServerSingleDatabaseRunQueriesAndCheckDatabase = 46,
    SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabase = 47,
    RunSpaAndExecuteMochaTestsExecutionStrategy = 48,
    GolangCompileExecuteAndCheck = 49,
    DotNetCore6ProjectTestsExecutionStrategy = 50,
    DotNetCore5ProjectTestsExecutionStrategy = 51,
    DotNetCore5CompileExecuteAndCheck = 52,
    DotNetCore6CompileExecuteAndCheck = 53,
    DotNetCore5UnitTestsExecutionStrategy = 54,
    DotNetCore6UnitTestsExecutionStrategy = 55,
    DotNetCore5ProjectExecutionStrategy = 56,
    DotNetCore6ProjectExecutionStrategy = 57,
    PostgreSqlPrepareDatabaseAndRunQueries = 58,
    PostgreSqlRunQueriesAndCheckDatabase = 59,
    PostgreSqlRunSkeletonRunQueriesAndCheckDatabase = 60,
    PythonDjangoOrmExecutionStrategy = 61,
    Java21ProjectTestsExecutionStrategy = 62,
    Java21ZipFileCompileExecuteAndCheck = 63,
    Java21UnitTestsExecutionStrategy = 64,
    Java21SpringAndHibernateProjectExecution = 65,
    Java21PreprocessCompileExecuteAndCheck = 66,
    RunSpaAndExecuteMochaTestsExecutionStrategySeparateTests = 67,
    NodeJsV20PreprocessExecuteAndCheck = 68,
    NodeJsV20PreprocessExecuteAndRunUnitTestsWithMocha = 69,
    NodeJsV20PreprocessExecuteAndRunJsDomUnitTests = 70,
    NodeJsV20PreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy = 71,
    NodeJsV20ZipExecuteHtmlAndCssStrategy = 72,
    NodeJsV20RunSpaAndExecuteMochaTestsExecutionStrategySeparateTests = 73,
    TypeScriptV20PreprocessExecuteAndCheck = 74,
}

 enum CompilerType {
    // Commented out compilers are deprecated, but left here to preserve order,
    // as modifying values will require database migration.
    None = 0,
    // CSharp = 1,
    // MsBuild = 2,
    CPlusPlusGcc = 3,
    Java = 4,
    JavaZip = 5,
    // MsBuildLibrary = 6,
    CPlusPlusZip = 7,
    JavaInPlaceCompiler = 8,
    DotNetCompiler = 9,
    CSharpDotNetCore = 10,
    // SolidityCompiler = 11,
    GolangCompiler = 12,
}

enum ChatMessageRole {
    System = 0,
    User = 1,
    Assistant = 2,
    Tool = 3,
    Function = 4,
    Information = 5,
}

enum ExceptionType {
    Solution = 0,
    Configuration = 1,
    Strategy = 2,
    Remote = 3,
    Other = 4,
}

export {
    ThemeMode,
    FilterColumnTypeEnum,
    SortingEnum,
    ProblemGroupTypes,
    ProblemResourceType,
    SubmissionStatus,
    SettingTypeEnums,
    RolesEnum,
    SubmissionStrategyType,
    CompilerType,
    CheckboxSearchValues,
    ChatMessageRole,
    ExceptionType,
};
