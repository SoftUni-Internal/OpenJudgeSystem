# Administration System - Complete Field Reference

## Table of Contents
1. [Contest Administration](#contest-administration)
2. [Problem Administration](#problem-administration)
3. [Problem Group Administration](#problem-group-administration)
4. [Test Administration](#test-administration)
5. [Problem Resources](#problem-resources)
6. [Exam Groups](#exam-groups)
7. [Contest Categories](#contest-categories)
8. [Checkers](#checkers)
9. [Submission Types](#submission-types)

---

## Contest Administration

### Overview
Contests are the top-level entities that contain problems organized into problem groups. Each contest has specific time windows, access controls, and behavior settings.

### All Contest Fields

| Field | Type | Required | Default | Description | User Impact |
|-------|------|----------|---------|-------------|-------------|
| **Id** | int | Auto | - | Unique identifier | System-generated, read-only |
| **Name** | string | ✅ Yes | - | Contest name (4-100 chars) | Displayed to users everywhere |
| **Type** | enum | ✅ Yes | Exercise | Contest type (see types below) | Determines contest behavior |
| **CategoryId** | int | ✅ Yes | - | Parent category | Organizes contests hierarchically |
| **Description** | string | No | null | Contest description | Shown on contest details page |
| **StartTime** | DateTime? | No | null | Official start time (UTC) | When users can compete officially |
| **EndTime** | DateTime? | No | null | Official end time (UTC) | When official competition ends |
| **PracticeStartTime** | DateTime? | No | null | Practice start time (UTC) | When users can practice |
| **PracticeEndTime** | DateTime? | No | null | Practice end time (UTC) | When practice ends |
| **ContestPassword** | string? | No | null | Official mode password (max 20 chars) | Required to compete if set |
| **PracticePassword** | string? | No | null | Practice mode password (max 20 chars) | Required to practice if set |
| **IsVisible** | bool | ✅ Yes | false | Immediate visibility flag | If true, contest is visible immediately |
| **VisibleFrom** | DateTime? | No | null | Scheduled visibility time | Contest becomes visible at this time |
| **Duration** | TimeSpan? | No | null | Individual participation duration | For online exams: user's time window |
| **LimitBetweenSubmissions** | int | ✅ Yes | 0 | Seconds between submissions | Throttles submission rate (0 = no limit) |
| **AutoChangeLimitBetweenSubmissions** | bool | ✅ Yes | false | Auto-adjust submission limit | System adjusts based on worker load |
| **NewIpPassword** | string? | No | null | Password to add new IPs (max 20 chars) | Allows users to add their IP to allowed list |
| **AllowedIps** | string | No | "" | Semicolon-separated IP list | Restricts access to specific IPs |
| **AllowParallelSubmissionsInTasks** | bool | ✅ Yes | true | Allow concurrent submissions | If false, users can't submit to multiple problems simultaneously |
| **OrderBy** | double | ✅ Yes | 0 | Display order | Controls contest ordering in lists |

### Contest Types

```csharp
public enum ContestType
{
    Exercise = 0,                           // Regular practice contest
    OnsitePracticalExam = 1,                // On-site exam (fixed problems)
    OnlinePracticalExam = 2,                // Online exam (individual time windows, random tasks, exam groups)
    Lab = 3,                                // Laboratory assignment
    OnsitePracticalExamWithRandomTasks = 4  // On-site exam with random task assignment
}
```

### Field Details & Business Logic

#### Name
- **Validation**: 4-100 characters
- **Impact**: Displayed in contest lists, breadcrumbs, page titles
- **Best Practice**: Use clear, descriptive names (e.g., "Data Structures - Exam 2024")

#### Type
- **Impact on User**:
  - **Exercise**: Standard contest, no special restrictions
  - **OnsitePracticalExam**: Fixed problem set, typically with IP restrictions
  - **OnlinePracticalExam**: 
    - Individual time windows (starts when user registers)
    - Requires exam group membership for official participation
    - Random task assignment
    - Requires participation confirmation
  - **Lab**: Similar to Exercise, used for lab assignments
  - **OnsitePracticalExamWithRandomTasks**: On-site exam with random problems per user

#### StartTime / EndTime
- **Null Behavior**:
  - If `StartTime` is null: Contest **cannot be competed**
  - If `EndTime` is null but `StartTime` is set: Contest can be competed **forever** (after start)
- **Impact**: Determines `CanBeCompeted` status
- **For Online Exams**: These are contest-wide windows; individual users get personal windows based on `Duration`

#### PracticeStartTime / PracticeEndTime
- **Null Behavior**: Same as official times
- **Impact**: Determines `CanBePracticed` status
- **Independent**: Practice mode is completely separate from official mode

#### ContestPassword / PracticePassword
- **When Required**: Only for new or invalidated participants
- **Impact**: 
  - Blocks access until correct password is provided
  - Changing password invalidates all existing participants in that mode
- **Bypass**: Admins and lecturers don't need passwords

#### IsVisible / VisibleFrom
- **Logic**: Contest is visible if `IsVisible == true` OR `VisibleFrom <= CurrentTime`
- **Impact**: 
  - Invisible contests return 404 for regular users
  - Admins and lecturers can always see all contests
- **Use Case**: Schedule contest visibility in advance with `VisibleFrom`

#### Duration
- **Only for Online Exams**: Defines individual participation window length
- **Example**: Duration = 3 hours
  - User registers at 10:00 AM
  - Their window: 10:00 AM - 1:00 PM
  - They can only submit during this window
- **For Other Types**: Ignored (uses contest-wide StartTime/EndTime)

#### LimitBetweenSubmissions
- **Unit**: Seconds
- **0 = No Limit**: Users can submit as fast as they want
- **> 0**: Users must wait this many seconds between submissions
- **Impact**: 
  - Prevents spam submissions
  - Reduces worker load
  - User sees countdown timer if they try to submit too soon
- **Auto-Adjustment**: If `AutoChangeLimitBetweenSubmissions` is true, system adjusts this value

#### AutoChangeLimitBetweenSubmissions
- **Purpose**: Automatically adjust `LimitBetweenSubmissions` based on system load
- **How It Works**:
  - Background service monitors worker busy ratio and queue length
  - If workers are busy (>70%): Increases limit (slows down submissions)
  - If workers are idle (<30%): Decreases limit (allows faster submissions)
  - Maximum limit: 300 seconds (5 minutes)
- **Settings** (configurable in database):
  ```csharp
  MaxLimitBetweenSubmissionsInSeconds = 300
  BusyRatioMaxFactor = 1.5
  BusyRatioModerateThreshold = 0.3  // <30% busy = decrease limit
  BusyRatioCriticalThreshold = 0.7  // >70% busy = increase limit
  QueueLenghtMaxFactor = 4
  ```
- **Impact**: Balances user experience with system capacity

#### NewIpPassword / AllowedIps
- **Purpose**: Restrict contest access to specific locations (e.g., computer labs)
- **AllowedIps Format**: Semicolon-separated list (e.g., `192.168.1.100;192.168.1.101`)
- **NewIpPassword**: Allows users to add their IP if not in the list
- **Flow**:
  1. User tries to access contest from IP `10.0.0.50`
  2. IP not in allowed list → Access denied
  3. User provides `NewIpPassword`
  4. If correct: IP `10.0.0.50` is added to allowed list with `IsOriginallyAllowed = false`
  5. User can now access contest
- **Bypass**: Admins and lecturers can access from any IP

#### AllowParallelSubmissionsInTasks
- **true** (default): User can submit to Problem A while Problem B submission is processing
- **false**: User must wait for current submission to finish before submitting to another problem
- **Impact**: Prevents users from overwhelming the system with parallel submissions

#### OrderBy
- **Purpose**: Controls display order in contest lists
- **Lower values appear first**
- **Can be decimal** (e.g., 1.5, 2.0, 2.5)
- **Impact**: Only affects UI ordering, no functional impact

### Computed/Read-Only Fields

| Field | Type | Description |
|-------|------|-------------|
| **IsOnlineExam** | bool | `Type == OnlinePracticalExam` |
| **IsWithRandomTasks** | bool | `Type == OnlinePracticalExam OR OnsitePracticalExamWithRandomTasks` |
| **IsOnsiteExam** | bool | `Type == OnsitePracticalExam` |
| **HasContestPassword** | bool | `ContestPassword != null` |
| **HasPracticePassword** | bool | `PracticePassword != null` |
| **ResultsArePubliclyVisible** | bool | Contest is visible, not deleted, and `EndTime` has passed |
| **OfficialParticipants** | int | Count of participants with `IsOfficial = true` |
| **NumberOfProblemGroups** | int | Count of non-deleted problem groups |

### Business Rules

1. **Password Change Invalidates Participants**:
   ```csharp
   // When ContestPassword changes: All official participants are invalidated
   // When PracticePassword changes: All practice participants are invalidated
   // Invalidated participants must re-register (provide password again)
   ```

2. **Online Exam Restrictions**:
   - Official participation requires exam group membership
   - Requires participation confirmation before starting
   - Creates individual time window on registration
   - Assigns random problems (if not admin/lecturer)

3. **Visibility Hierarchy**:
   - Contest visibility AND category visibility must both be true
   - Admins/lecturers bypass visibility checks

4. **Time Window Flexibility**:
   - Null `EndTime` = infinite duration (after start)
   - Null `StartTime` = cannot participate
   - Null `PracticeEndTime` = practice forever (after practice start)

---

## Problem Administration

### Overview
Problems are the individual tasks within a contest. Each problem belongs to a problem group and has execution limits, tests, and resources.

### All Problem Fields

| Field | Type | Required | Default | Description | User Impact |
|-------|------|----------|---------|-------------|-------------|
| **Id** | int | Auto | - | Unique identifier | System-generated |
| **Name** | string | ✅ Yes | - | Problem name (1-50 chars) | Displayed in problem lists |
| **ProblemGroupId** | int | ✅ Yes | - | Parent problem group | Organizes problems within contest |
| **ContestId** | int | ✅ Yes | - | Parent contest (via group) | Determines contest context |
| **MaximumPoints** | short | ✅ Yes | 100 | Maximum score (0-32767) | User's maximum achievable score |
| **TimeLimit** | int | ✅ Yes | 1000 | Execution time limit (ms) | Submission fails if exceeded |
| **MemoryLimit** | int | ✅ Yes | 16777216 | Memory limit (bytes) | Submission fails if exceeded (default: 16MB) |
| **SourceCodeSizeLimit** | int? | No | null | Source code size limit (bytes) | Submission rejected if code too large |
| **CheckerId** | int? | No | null | Checker for output validation | Determines how outputs are compared |
| **DefaultSubmissionTypeId** | int? | No | null | Default programming language | Pre-selected language in UI |
| **ShowDetailedFeedback** | bool | ✅ Yes | false | Show test inputs/outputs | Users see test data for trial tests |
| **SolutionSkeleton** | byte[]? | No | null | Code template (zipped) | Pre-filled code in editor |
| **AdditionalFiles** | byte[]? | No | null | Problem dependencies (zipped) | Compiled/executed with user code |
| **OrderBy** | double | ✅ Yes | 0 | Display order | Controls problem ordering |

### Field Details & Business Logic

#### Name
- **Validation**: 1-50 characters
- **Impact**: Displayed in problem navigation, breadcrumbs, submission lists
- **Best Practice**: Short, descriptive names (e.g., "Sum of Two Numbers", "Binary Search Tree")

#### MaximumPoints
- **Range**: 0-32767 (short)
- **Impact**:
  - User's score is calculated as: `(PassedTests / TotalTests) * MaximumPoints`
  - Total contest score = sum of all problem scores
  - Leaderboard ranking based on total score
- **Common Values**: 100 (standard), 50 (easier), 200 (harder)

#### TimeLimit
- **Unit**: Milliseconds
- **Default**: 1000ms (1 second)
- **Impact**:
  - Submission execution is terminated if it exceeds this limit
  - User sees "Time Limit Exceeded" error
  - Different strategies may have base time limits that are added to this
- **Common Values**:
  - Simple problems: 100-500ms
  - Medium problems: 1000-2000ms
  - Complex problems: 3000-5000ms
  - HTML/CSS: 15000ms (15 seconds)

#### MemoryLimit
- **Unit**: Bytes
- **Default**: 16777216 bytes (16 MB)
- **Impact**:
  - Submission execution is terminated if it exceeds this limit
  - User sees "Memory Limit Exceeded" error
- **Common Values**:
  - Standard: 16 MB
  - Large data structures: 64 MB (67108864 bytes)
  - Very large: 256 MB (268435456 bytes)

#### SourceCodeSizeLimit
- **Unit**: Bytes
- **Null = No Limit**
- **Impact**:
  - Submission is rejected before execution if code is too large
  - User sees validation error immediately
- **Use Case**: Prevent users from submitting huge files

#### CheckerId
- **Null = Default Checker**: Uses exact string comparison (trim whitespace)
- **Custom Checker**: Uses specified checker logic
- **Impact**: Determines how user output is compared to expected output
- **Common Checkers**:
  - **Trim**: Exact match after trimming whitespace (most common)
  - **CaseInsensitive**: Ignores case differences
  - **FloatingPoint**: Compares floating-point numbers with tolerance
  - **Custom**: C# code checker for complex validation logic

#### DefaultSubmissionTypeId
- **Null = No Default**: User must select language manually
- **Set = Pre-selected**: Language is pre-selected in dropdown
- **Impact**: Improves UX by pre-selecting the expected language
- **Example**: For C# problems, set to C# submission type

#### ShowDetailedFeedback
- **false** (default): Users only see pass/fail for each test
- **true**: Users see:
  - Test input (for trial tests only, unless `HideInput = true`)
  - Expected output
  - Actual output
  - Execution time
  - Memory used
- **Impact**:
  - Helps users debug their solutions
  - Should be **false** for competitive contests (to prevent reverse-engineering)
  - Should be **true** for learning/practice problems

#### SolutionSkeleton
- **Format**: Zipped byte array containing code template
- **Impact**:
  - Pre-fills the code editor with template code
  - User modifies the template instead of starting from scratch
- **Use Case**:
  - Provide class structure for OOP problems
  - Give function signatures
  - Include imports/using statements
- **Example**:
  ```csharp
  // SolutionSkeleton for C# problem
  using System;

  public class Solution
  {
      public static void Main()
      {
          // Your code here
      }
  }
  ```

#### AdditionalFiles
- **Format**: Zipped byte array containing files
- **Impact**:
  - Files are extracted and compiled/executed with user code
  - User code can reference these files
- **Use Cases**:
  - Unit test frameworks (e.g., NUnit, JUnit)
  - Mock objects
  - Data files (JSON, XML, CSV)
  - Helper classes
- **Example**: For unit testing problems, include test class that calls user's code

#### OrderBy
- **Purpose**: Controls display order in problem lists
- **Lower values appear first**
- **Impact**: Only affects UI ordering

### Problem Group Type Impact

Problems inherit behavior from their problem group's type:

| Group Type | Impact on Problems |
|------------|-------------------|
| **None** | Standard problems, included in scoring |
| **ExcludedFromHomework** | Problems are visible but excluded from homework scoring |

### Computed/Read-Only Fields

| Field | Type | Description |
|-------|------|-------------|
| **TestsCount** | int | Total number of tests |
| **TrialTestsCount** | int | Number of trial tests (practice tests) |
| **CompeteTestsCount** | int | Number of compete tests (standard tests) |
| **SubmissionTypesCount** | int | Number of allowed submission types (languages) |

### Business Rules

1. **Problem Creation**:
   - If `ProblemGroupId` is not provided or is default (0), a new problem group is created automatically
   - Problem inherits `ContestId` from its problem group

2. **Problem Copying**:
   - Can copy problem to another contest
   - Copies all tests, resources, and submission types
   - **Cannot copy to active contests** (validation error)

3. **Problem Deletion**:
   - Soft delete (sets `IsDeleted = true`)
   - Cascades to tests and resources (they are also soft-deleted)
   - Cannot delete from active contests

4. **Validation**:
   - `TimeLimit` must be > 0
   - `MemoryLimit` must be > 0
   - `MaximumPoints` must be >= 0
   - `Name` must be unique within the problem group

---

## Problem Group Administration

### Overview
Problem groups organize problems within a contest. They control ordering and can exclude problems from homework scoring.

### All Problem Group Fields

| Field | Type | Required | Default | Description | User Impact |
|-------|------|----------|---------|-------------|-------------|
| **Id** | int | Auto | - | Unique identifier | System-generated |
| **ContestId** | int | ✅ Yes | - | Parent contest | Determines contest context |
| **OrderBy** | double | ✅ Yes | 0 | Display order | Controls group ordering |
| **Type** | enum? | No | null | Group type | Affects problem behavior |

### Problem Group Types

```csharp
public enum ProblemGroupType
{
    None = 0,                    // Standard group (default)
    ExcludedFromHomework = 1     // Problems excluded from homework scoring
}
```

### Field Details & Business Logic

#### Type
- **null or None**: Standard problem group
  - Problems are included in all scoring
  - Normal behavior
- **ExcludedFromHomework**:
  - Problems are visible and can be solved
  - **Excluded from homework scoring calculations**
  - Use case: Bonus problems, optional challenges

#### OrderBy
- **Purpose**: Controls display order of problem groups
- **Lower values appear first**
- **Impact**: Determines order of problem sections in contest

### Business Rules

1. **Creation**:
   - Can only create in contests with random tasks (`IsWithRandomTasks = true`)
   - For other contest types, problem groups are created automatically when creating problems

2. **Editing**:
   - Cannot edit problem groups in active contests
   - Changing `OrderBy` triggers re-evaluation of all problems and groups

3. **Deletion**:
   - Cannot delete from active contests
   - Soft delete (sets `IsDeleted = true`)
   - Cascades to all problems in the group

4. **Order Re-evaluation**:
   - After any change, system re-evaluates `OrderBy` for all problems and groups
   - Ensures consistent ordering

---

## Test Administration

### Overview
Tests are the input/output pairs used to validate submissions. Each test belongs to a problem and has a specific type.

### All Test Fields

| Field | Type | Required | Default | Description | User Impact |
|-------|------|----------|---------|-------------|-------------|
| **Id** | int | Auto | - | Unique identifier | System-generated |
| **ProblemId** | int | ✅ Yes | - | Parent problem | Determines which problem this tests |
| **InputData** | byte[] | ✅ Yes | - | Test input (zipped) | Passed to user's program as stdin |
| **OutputData** | byte[] | ✅ Yes | - | Expected output (zipped) | Compared with user's output |
| **IsTrialTest** | bool | ✅ Yes | false | Is this a trial test? | Visible to users before submission |
| **IsOpenTest** | bool | ✅ Yes | false | Is this an open test? | Visible to users after submission |
| **HideInput** | bool | ✅ Yes | false | Hide input from users? | Prevents users from seeing input |
| **OrderBy** | double | ✅ Yes | 0 | Execution order | Tests run in this order |

### Test Types

Tests are categorized by their `IsTrialTest` and `IsOpenTest` flags:

| IsTrialTest | IsOpenTest | Type | Frontend Label | Description |
|-------------|------------|------|----------------|-------------|
| false | false | **Standard** | "Compete" | Hidden tests for scoring |
| true | false | **Trial** | "Practice" | Visible before submission |
| false | true | **Open** | "Open" | Visible after submission |

### Field Details & Business Logic

#### InputData / OutputData
- **Format**: Zipped byte arrays (compressed to save database space)
- **Access**: Decompressed to strings when needed
- **Impact**:
  - `InputData` is passed to user's program via stdin
  - `OutputData` is compared with user's program output
- **Size**: Can be very large (e.g., 10MB+ for data structure problems)

#### IsTrialTest (Trial Tests)
- **Purpose**: Practice tests that users can see **before** submitting
- **Visibility**:
  - Input: Visible (unless `HideInput = true`)
  - Output: Visible
- **Impact**:
  - Users can test their code locally with these inputs
  - Not counted in final score (in some configurations)
  - Helps users understand the problem
- **Best Practice**: Include 2-3 trial tests with simple cases

#### IsOpenTest (Open Tests)
- **Purpose**: Tests visible **after** user submits
- **Visibility**:
  - Input: Visible after submission
  - Output: Visible after submission
- **Impact**:
  - Users can see what they got wrong
  - Helps debugging
- **Use Case**: Educational contests where learning is prioritized

#### Standard Tests (IsTrialTest = false, IsOpenTest = false)
- **Purpose**: Hidden tests for scoring
- **Visibility**: Never visible to users (unless admin/lecturer)
- **Impact**:
  - Determines user's score
  - Prevents users from hardcoding solutions
- **Best Practice**: Include edge cases, large inputs, corner cases

#### HideInput
- **Purpose**: Hide test input even for trial/open tests
- **Impact**:
  - Users see "Input is hidden" instead of actual input
  - Output is still visible (for trial/open tests)
- **Use Case**:
  - Prevent users from reverse-engineering the problem
  - Hide sensitive data

#### ShowDetailedFeedback Interaction
The `Problem.ShowDetailedFeedback` flag controls what users see:

| ShowDetailedFeedback | Test Type | User Sees |
|---------------------|-----------|-----------|
| false | Standard | ✅ Pass/Fail only |
| false | Trial | ✅ Pass/Fail + Input (if not hidden) + Expected Output |
| false | Open | ✅ Pass/Fail + Input + Expected Output (after submission) |
| true | Standard | ✅ Pass/Fail + Actual Output + Expected Output + Time + Memory |
| true | Trial | ✅ Pass/Fail + Input + Actual Output + Expected Output + Time + Memory |
| true | Open | ✅ Pass/Fail + Input + Actual Output + Expected Output + Time + Memory |

#### OrderBy
- **Purpose**: Determines execution order
- **Impact**:
  - Tests run in ascending `OrderBy` order
  - If a test fails, remaining tests may still run (depends on strategy)
- **Best Practice**: Order from simple to complex (fast fail on basic cases)

### Business Rules

1. **Test Type Validation**:
   - A test cannot be both trial and open (`IsTrialTest = true AND IsOpenTest = true` is invalid)
   - At least one of the three types must be set

2. **Scoring**:
   - User's score = `(PassedTests / TotalTests) * Problem.MaximumPoints`
   - Only standard tests count for scoring (in most configurations)

3. **Execution**:
   - Tests run in `OrderBy` order
   - Each test has time and memory limits from the problem
   - Checker validates output against expected output

4. **Retest Problem Flag**:
   - When a test is modified, can trigger re-evaluation of all submissions
   - Useful when fixing incorrect test data

---

## Problem Resources

### Overview
Problem resources are files or links attached to problems (e.g., problem descriptions, author solutions, additional materials).

### All Problem Resource Fields

| Field | Type | Required | Default | Description | User Impact |
|-------|------|----------|---------|-------------|-------------|
| **Id** | int | Auto | - | Unique identifier | System-generated |
| **ProblemId** | int | ✅ Yes | - | Parent problem | Determines which problem this belongs to |
| **Name** | string | ✅ Yes | - | Resource name | Displayed to users |
| **Type** | enum | ✅ Yes | - | Resource type | Determines resource purpose |
| **File** | byte[]? | Conditional | null | File content | Downloaded by users |
| **Link** | string? | Conditional | null | External URL | Opened by users |
| **FileExtension** | string? | No | null | File extension (e.g., "pdf") | Used for download filename |
| **OrderBy** | double | ✅ Yes | 0 | Display order | Controls resource ordering |

### Problem Resource Types

```csharp
public enum ProblemResourceType
{
    ProblemDescription = 0,  // Problem statement (PDF, DOCX, etc.)
    AuthorsSolution = 1,     // Author's solution code
    Other = 2                // Any other resource
}
```

### Field Details & Business Logic

#### Type
- **ProblemDescription**:
  - Main problem statement
  - Usually PDF or DOCX
  - Displayed prominently in UI
- **AuthorsSolution**:
  - Reference solution by problem author
  - Usually source code file
  - May be hidden from regular users
- **Other**:
  - Any additional materials
  - Examples: diagrams, data files, hints

#### File vs Link
- **Mutually Exclusive**: A resource must have **either** a file **or** a link, not both
- **File**:
  - Stored in database as byte array
  - Downloaded when user clicks
  - Filename: `Resource-{Id}-{Name}.{FileExtension}`
- **Link**:
  - External URL (e.g., Google Drive, YouTube)
  - Opened in new tab when user clicks

#### Validation Rules
1. **Create**: Must have either file or link
2. **Update**: Cannot have both file and link
3. **File**: If provided, `FileExtension` is auto-assigned from filename

### Business Rules

1. **Access Control**:
   - Admins: Can see all resources
   - Lecturers: Can see resources for their contests
   - Regular Users: Can see resources for visible contests they have access to

2. **Deletion**:
   - Soft delete (sets `IsDeleted = true`)
   - Cannot delete from active contests

---

## Exam Groups

### Overview
Exam groups are pre-registration groups for online exams. Users must be in an exam group to participate officially in online exams.

### All Exam Group Fields

| Field | Type | Required | Default | Description | User Impact |
|-------|------|----------|---------|-------------|-------------|
| **Id** | int | Auto | - | Unique identifier | System-generated |
| **Name** | string | ✅ Yes | - | Group name | Displayed to users |
| **ContestId** | int? | No | null | Associated contest | Links group to specific contest |
| **ExternalExamGroupId** | int? | No | null | External system ID | Integration with external systems |
| **ExternalAppId** | string? | No | null | External app identifier | Integration with external systems |

### Field Details & Business Logic

#### Name
- **Purpose**: Identifies the exam group (e.g., "Group A", "Morning Session")
- **Impact**: Displayed in exam group selection dropdown

#### ContestId
- **Null = Reusable**: Group can be used for multiple contests
- **Set = Contest-Specific**: Group is tied to a specific contest
- **Impact**: Determines which contests the group can be used for

#### ExternalExamGroupId / ExternalAppId
- **Purpose**: Integration with external systems (e.g., university student management systems)
- **Impact**: Allows automatic enrollment based on external data

### Business Rules

1. **Online Exam Requirement**:
   - For `OnlinePracticalExam` contests, users must be in an exam group to participate officially
   - Practice mode doesn't require exam group membership

2. **User Assignment**:
   - Users are assigned to exam groups via `UserInExamGroup` table
   - One user can be in multiple exam groups

3. **Access Control**:
   - Only users in the exam group can register for official participation
   - Admins/lecturers bypass this requirement

---

## Contest Categories

### Overview
Contest categories organize contests hierarchically. Categories can have parent categories, creating a tree structure.

### All Contest Category Fields

| Field | Type | Required | Default | Description | User Impact |
|-------|------|----------|---------|-------------|-------------|
| **Id** | int | Auto | - | Unique identifier | System-generated |
| **Name** | string | ✅ Yes | - | Category name (6-100 chars) | Displayed in navigation, breadcrumbs |
| **ParentId** | int? | No | null | Parent category ID | Creates hierarchy |
| **IsVisible** | bool | ✅ Yes | false | Category visibility | Hides category and all children |
| **AllowMentor** | bool | ✅ Yes | false | Enable AI mentor for contests | Users can access AI mentor |
| **OrderBy** | double | ✅ Yes | 0 | Display order | Controls category ordering |

### Field Details & Business Logic

#### Name
- **Validation**: 6-100 characters
- **Impact**: Displayed in category navigation, contest breadcrumbs, page titles
- **Best Practice**: Use clear, hierarchical names (e.g., "Algorithms", "Data Structures", "2024 Spring Semester")

#### ParentId
- **Null = Root Category**: Top-level category
- **Set = Child Category**: Category is nested under parent
- **Impact**:
  - Creates hierarchical navigation
  - Visibility is inherited (if parent is hidden, children are hidden)
- **Example Hierarchy**:
  ```
  Computer Science (ParentId = null)
  ├── Algorithms (ParentId = Computer Science.Id)
  │   ├── Sorting (ParentId = Algorithms.Id)
  │   └── Searching (ParentId = Algorithms.Id)
  └── Data Structures (ParentId = Computer Science.Id)
  ```

#### IsVisible
- **Hierarchical Visibility**:
  - If category is hidden (`IsVisible = false`), **all child categories and contests are hidden**
  - If parent is hidden, child is hidden even if `IsVisible = true`
- **Visibility Check Logic**:
  ```csharp
  // From ContestCategoriesBusinessService.cs
  private async Task<bool> IsCategoryVisible(ContestCategoryServiceModel category)
  {
      if (!category.IsVisible)
          return false;

      // Check all parents up the hierarchy
      var parentId = category.ParentId;
      while (parentId != null)
      {
          var parent = await GetParent(parentId);
          if (parent is { IsVisible: false })
              return false;
          parentId = parent?.ParentId;
      }

      return true;
  }
  ```
- **Impact**:
  - Hidden categories return 404 for regular users
  - Admins/lecturers can see all categories
- **Use Case**: Hide entire sections (e.g., "2023 Archive") while keeping data

#### AllowMentor
- **Purpose**: Enable AI-powered mentor assistant for contests in this category
- **Impact**:
  - If `true`: Users see "Ask Mentor" button in contest problems
  - If `false`: Mentor feature is disabled
  - Inherited by all contests in the category
- **Mentor Features**:
  - AI-powered hints and explanations
  - Code review and suggestions
  - Problem clarification
  - Quota-limited per user
- **Use Case**: Enable for learning contests, disable for competitive exams

#### OrderBy
- **Purpose**: Controls display order of categories
- **Lower values appear first**
- **Impact**: Only affects UI ordering

### Business Rules

1. **Hierarchical Visibility**:
   - Category visibility is checked recursively up the parent chain
   - All parents must be visible for category to be visible

2. **Deletion**:
   - Soft delete (sets `IsDeleted = true`)
   - Cannot delete if category has contests or child categories
   - Must delete/move children first

3. **Access Control**:
   - Admins: Can see and manage all categories
   - Lecturers: Can see categories they are assigned to
   - Regular Users: Can only see visible categories (with visible parents)

4. **Validation**:
   - `Name` must be 6-100 characters
   - `ParentId` must reference an existing category (if set)
   - Cannot set self as parent (circular reference)

---

## Checkers

### Overview
Checkers are validation logic used to compare user output with expected output. They determine if a submission is correct.

### All Checker Fields

| Field | Type | Required | Default | Description | User Impact |
|-------|------|----------|---------|-------------|-------------|
| **Id** | int | Auto | - | Unique identifier | System-generated |
| **Name** | string | ✅ Yes | - | Checker name (3-100 chars) | Displayed in problem settings |
| **Description** | string? | No | null | Checker description | Explains checker behavior |
| **DllFile** | string? | No | null | DLL filename | For custom C# checkers |
| **ClassName** | string? | No | null | Class name in DLL | For custom C# checkers |
| **Parameter** | string? | No | null | Checker parameter | Passed to checker logic |

### Built-in Checkers

| Checker Name | Description | Use Case |
|--------------|-------------|----------|
| **Trim** | Exact match after trimming whitespace | Most common, default checker |
| **CaseInsensitive** | Ignores case differences | Text processing problems |
| **FloatingPoint** | Compares floats with tolerance | Math/geometry problems |
| **Sort** | Compares sorted arrays | Problems with multiple valid orderings |
| **Precision** | Compares with specific decimal precision | Scientific calculations |

### Custom Checkers

Custom checkers are C# classes that implement `IChecker` interface:

```csharp
public interface IChecker
{
    CheckerResult Check(string inputData, string receivedOutput, string expectedOutput, bool isTrialTest);
    void SetParameter(string parameter);
}
```

**Example Custom Checker**:
```csharp
public class CustomChecker : Checker
{
    public override CheckerResult Check(string inputData, string receivedOutput, string expectedOutput, bool isTrialTest)
    {
        // Custom validation logic
        var userLines = receivedOutput.Split('\n');
        var expectedLines = expectedOutput.Split('\n');

        if (userLines.Length != expectedLines.Length)
            return new CheckerResult { IsCorrect = false, ResultType = CheckerResultType.WrongAnswer };

        // ... more logic

        return new CheckerResult { IsCorrect = true, ResultType = CheckerResultType.Ok };
    }
}
```

### Field Details & Business Logic

#### DllFile / ClassName
- **For Custom Checkers**: Specify DLL and class name
- **Format**:
  - `DllFile`: "OJS.Workers.Checkers.dll"
  - `ClassName`: "CustomChecker"
- **Loading**: Checker is loaded dynamically at runtime
- **Impact**: Allows complex validation logic beyond simple string comparison

#### Parameter
- **Purpose**: Pass configuration to checker
- **Examples**:
  - FloatingPoint checker: `Parameter = "0.001"` (tolerance)
  - Precision checker: `Parameter = "3"` (decimal places)
- **Impact**: Customizes checker behavior per problem

### Business Rules

1. **Default Checker**:
   - If `CheckerId` is null on a problem, uses "Trim" checker
   - Trims leading/trailing whitespace and compares strings

2. **Checker Execution**:
   - Called for each test after user code executes
   - Receives: input data, user output, expected output, isTrialTest flag
   - Returns: `CheckerResult` with `IsCorrect` and `ResultType`

3. **Result Types**:
   ```csharp
   public enum CheckerResultType
   {
       Ok,                    // Correct answer
       WrongAnswer,           // Incorrect answer
       InvalidNumberOfLines,  // Output has wrong number of lines
       InvalidLength,         // Output has wrong length
       InvalidFormat          // Output format is invalid
   }
   ```

---

## Submission Types

### Overview
Submission types define the programming languages and execution strategies available for problems.

### All Submission Type Fields

| Field | Type | Required | Default | Description | User Impact |
|-------|------|----------|---------|-------------|-------------|
| **Id** | int | Auto | - | Unique identifier | System-generated |
| **Name** | string | ✅ Yes | - | Display name (3-100 chars) | Shown in language dropdown |
| **ExecutionStrategyType** | enum | ✅ Yes | - | Execution strategy | Determines how code is executed |
| **CompilerType** | enum | ✅ Yes | - | Compiler type | Determines how code is compiled |
| **AdditionalCompilerArguments** | string? | No | null | Extra compiler flags | Passed to compiler |
| **Description** | string? | No | null | Description | Shown to users |
| **AllowBinaryFilesUpload** | bool | ✅ Yes | false | Allow file uploads | Users can upload files instead of code |
| **AllowedFileExtensions** | string? | No | null | Allowed file extensions | Comma-separated list (e.g., "zip,rar") |
| **BaseTimeUsedInMilliseconds** | int? | No | null | Base time overhead | Added to problem time limit |
| **BaseMemoryUsedInBytes** | int? | No | null | Base memory overhead | Added to problem memory limit |
| **MaxAllowedTimeLimitInMilliseconds** | int? | No | null | Maximum time limit | Caps problem time limit |
| **MaxAllowedMemoryLimitInBytes** | int? | No | null | Maximum memory limit | Caps problem memory limit |

### Common Submission Types

| Name | Compiler | Strategy | Description |
|------|----------|----------|-------------|
| **C# .NET 6** | CSharpDotNetCore | DotNetCore6CompileExecuteAndCheck | C# code execution |
| **C# .NET 6 Unit Tests** | CSharpDotNetCore | DotNetCore6UnitTestsExecutionStrategy | C# with NUnit tests |
| **Java 21** | Java | Java21PreprocessCompileExecuteAndCheck | Java code execution |
| **Java 21 Unit Tests** | Java | Java21UnitTestsExecutionStrategy | Java with JUnit tests |
| **JavaScript (Node.js 20)** | None | NodeJsV20PreprocessExecuteAndCheck | JavaScript execution |
| **Python 3** | None | PythonExecuteAndCheck | Python code execution |
| **C++ (GCC)** | CPlusPlusGcc | CPlusPlusCompileExecuteAndCheckExecutionStrategy | C++ compilation and execution |
| **TypeScript** | TypeScriptCompiler | NodeJsV20PreprocessExecuteAndCheck | TypeScript compilation to JS |
| **MySQL** | None | MySqlPrepareDatabaseAndRunQueries | SQL query execution |
| **PostgreSQL** | None | PostgreSqlPrepareDatabaseAndRunQueries | SQL query execution |

### Field Details & Business Logic

#### ExecutionStrategyType
- **Purpose**: Defines how user code is executed
- **Examples**:
  - `CompileExecuteAndCheck`: Compile → Execute → Check output
  - `UnitTestsExecutionStrategy`: Compile → Run unit tests → Check results
  - `ZipFileCompileExecuteAndCheck`: Extract ZIP → Compile → Execute → Check
- **Impact**: Determines entire execution pipeline

#### CompilerType
- **Purpose**: Specifies which compiler to use
- **Examples**:
  - `CSharpDotNetCore`: Uses `dotnet` compiler
  - `CPlusPlusGcc`: Uses `g++` compiler
  - `Java`: Uses `javac` compiler
  - `None`: No compilation (interpreted languages)
- **Impact**: Determines compilation command and arguments

#### AllowBinaryFilesUpload
- **false** (default): Users submit code in text editor
- **true**: Users can upload files (ZIP, JAR, etc.)
- **Impact**:
  - Changes UI to file upload instead of code editor
  - Useful for project-based problems

#### AllowedFileExtensions
- **Null = Text Only**: Users can only submit code in text editor
- **Set = File Upload**: Users must upload files with specified extensions
- **Format**: Comma-separated (e.g., `"zip,rar,7z"`)
- **Impact**:
  - If set, text editor is disabled
  - Only files with these extensions are accepted
- **Example**: For Java ZIP problems: `AllowedFileExtensions = "zip"`

#### BaseTimeUsedInMilliseconds / BaseMemoryUsedInBytes
- **Purpose**: Account for language/framework overhead
- **Impact**:
  - Added to problem's time/memory limits
  - Example: C# has higher base time due to JIT compilation
- **Calculation**:
  ```csharp
  ActualTimeLimit = Problem.TimeLimit + SubmissionType.BaseTimeUsedInMilliseconds
  ActualMemoryLimit = Problem.MemoryLimit + SubmissionType.BaseMemoryUsedInBytes
  ```

#### MaxAllowedTimeLimitInMilliseconds / MaxAllowedMemoryLimitInBytes
- **Purpose**: Cap maximum limits for this language
- **Impact**:
  - If problem's limit exceeds this, it's clamped to max
  - Prevents unreasonable limits for specific languages
- **Example**: JavaScript might have `MaxAllowedTimeLimit = 5000ms` to prevent long-running scripts

### Business Rules

1. **Problem Assignment**:
   - Problems can allow multiple submission types via `SubmissionTypeInProblem` table
   - Users select from allowed submission types in dropdown

2. **File Extension Logic**:
   - If `AllowedFileExtensions` is set: File upload mode
   - If `AllowedFileExtensions` is null: Text editor mode
   - Cannot have both

3. **Execution**:
   - Worker service receives submission with `ExecutionStrategyType` and `CompilerType`
   - Selects appropriate strategy and compiler
   - Executes code with calculated time/memory limits

---

## Participant Administration

### Overview
Participants represent user enrollments in contests. Each participant has a mode (official/practice) and state (valid/invalidated).

### All Participant Fields

| Field | Type | Required | Default | Description | User Impact |
|-------|------|----------|---------|-------------|-------------|
| **Id** | int | Auto | - | Unique identifier | System-generated |
| **ContestId** | int | ✅ Yes | - | Contest ID | Which contest user is enrolled in |
| **UserId** | string | ✅ Yes | - | User ID | Which user is enrolled |
| **IsOfficial** | bool | ✅ Yes | - | Official vs Practice mode | Determines participation mode |
| **IsInvalidated** | bool | ✅ Yes | false | Participant invalidated? | Blocks access until re-registration |
| **ParticipationStartTime** | DateTime? | No | null | Individual start time | For online exams only |
| **ParticipationEndTime** | DateTime? | No | null | Individual end time | For online exams only |
| **LastSubmissionTime** | DateTime? | No | null | Last submission timestamp | For submission throttling |
| **TotalScoreSnapshot** | int | Auto | 0 | Cached total score | Performance optimization |
| **TotalScoreSnapshotModifiedOn** | DateTime? | Auto | null | Score cache timestamp | When score was last updated |

### Field Details & Business Logic

#### IsOfficial
- **true**: Official/Compete mode
  - Counts toward leaderboard
  - Subject to time windows (`StartTime`/`EndTime`)
  - Requires contest password (if set)
  - For online exams: Requires exam group membership
- **false**: Practice mode
  - Does not count toward leaderboard
  - Subject to practice time windows (`PracticeStartTime`/`PracticeEndTime`)
  - Requires practice password (if set)
  - No exam group requirement

#### IsInvalidated
- **Purpose**: Temporarily block participant access
- **When Set to true**:
  - Contest password is changed
  - Admin manually invalidates participant
  - Participant violates rules
- **Impact**:
  - User cannot access contest
  - User must re-register (provide password again)
  - Previous submissions are preserved
- **Reset**: Set to `false` when user re-registers with correct password

#### ParticipationStartTime / ParticipationEndTime
- **Only for Online Exams** (`Contest.Type == OnlinePracticalExam`)
- **Set on Registration**:
  ```csharp
  participant.ParticipationStartTime = DateTime.UtcNow;
  participant.ParticipationEndTime = DateTime.UtcNow + Contest.Duration;
  ```
- **Impact**:
  - User can only submit during this window
  - Window is independent of contest-wide times
  - Allows flexible exam scheduling
- **Example**:
  - Contest: 9:00 AM - 5:00 PM, Duration = 3 hours
  - User A registers at 9:00 AM → Window: 9:00 AM - 12:00 PM
  - User B registers at 2:00 PM → Window: 2:00 PM - 5:00 PM

#### LastSubmissionTime
- **Purpose**: Track last submission for throttling
- **Updated**: Every time user submits
- **Used For**: `LimitBetweenSubmissions` enforcement
- **Logic**:
  ```csharp
  var timeSinceLastSubmission = DateTime.UtcNow - participant.LastSubmissionTime;
  if (timeSinceLastSubmission < contest.LimitBetweenSubmissions)
  {
      var remainingTime = contest.LimitBetweenSubmissions - timeSinceLastSubmission;
      return Error($"Please wait {remainingTime} seconds before submitting again");
  }
  ```

#### TotalScoreSnapshot
- **Purpose**: Cache total score for performance
- **Calculation**: Sum of best scores for each problem
- **Updated**:
  - Periodically by background job
  - On-demand when viewing leaderboard
- **Impact**: Leaderboard queries are fast (no need to calculate scores on every request)

### Business Rules

1. **Unique Constraint**:
   - One participant per (ContestId, UserId, IsOfficial) combination
   - User can have both official and practice participants in same contest

2. **Password Change Invalidation**:
   ```csharp
   // When contest password changes
   if (contestPasswordChanged)
   {
       // Invalidate all official participants
       participants.Where(p => p.IsOfficial).ForEach(p => p.IsInvalidated = true);
   }

   if (practicePasswordChanged)
   {
       // Invalidate all practice participants
       participants.Where(p => !p.IsOfficial).ForEach(p => p.IsInvalidated = true);
   }
   ```

3. **Random Problem Assignment** (for online exams):
   - When creating official participant for online exam
   - System assigns random problems from each problem group
   - Stored in `ProblemForParticipant` table
   - Admins/lecturers see all problems (no random assignment)

4. **Participation Time Adjustment**:
   - Admins can manually adjust `ParticipationEndTime`
   - Use case: Give extra time to specific students
   - Only for online exams with individual time windows

---

## Summary: How Fields Affect User Experience

### Contest-Level Impact

| Field | User Sees/Experiences |
|-------|----------------------|
| **Name** | Contest title everywhere |
| **Type** | Different enrollment flows, random problems, time windows |
| **StartTime/EndTime** | "Contest is active" or "Contest has ended" |
| **PracticeStartTime/PracticeEndTime** | "Practice mode available" |
| **ContestPassword** | Password prompt before competing |
| **IsVisible/VisibleFrom** | Contest appears in lists or returns 404 |
| **Duration** | Individual 3-hour window (online exams) |
| **LimitBetweenSubmissions** | "Please wait 30 seconds before next submission" |
| **AllowedIps** | "Access denied from this location" |
| **AllowParallelSubmissionsInTasks** | Can/cannot submit to multiple problems simultaneously |

### Problem-Level Impact

| Field | User Sees/Experiences |
|-------|----------------------|
| **Name** | Problem title in navigation |
| **MaximumPoints** | "This problem is worth 100 points" |
| **TimeLimit** | "Time Limit Exceeded" error if code is too slow |
| **MemoryLimit** | "Memory Limit Exceeded" error if code uses too much RAM |
| **ShowDetailedFeedback** | Sees test inputs/outputs or just pass/fail |
| **SolutionSkeleton** | Code editor pre-filled with template |
| **DefaultSubmissionTypeId** | Language pre-selected in dropdown |

### Test-Level Impact

| Field | User Sees/Experiences |
|-------|----------------------|
| **IsTrialTest** | Can see input/output before submitting |
| **IsOpenTest** | Can see input/output after submitting |
| **HideInput** | "Input is hidden" instead of actual input |
| **Standard Tests** | Never sees these tests, only pass/fail |

### Category-Level Impact

| Field | User Sees/Experiences |
|-------|----------------------|
| **IsVisible** | Category and all contests appear or return 404 |
| **AllowMentor** | "Ask Mentor" button available in problems |
| **ParentId** | Hierarchical navigation breadcrumbs |

### Participant-Level Impact

| Field | User Sees/Experiences |
|-------|----------------------|
| **IsOfficial** | Competes for leaderboard vs practices |
| **IsInvalidated** | "You must re-register for this contest" |
| **ParticipationStartTime/EndTime** | "Your exam window: 10:00 AM - 1:00 PM" |
| **LastSubmissionTime** | Countdown timer before next submission allowed |

---

## Administration Best Practices

### Contest Setup

1. **Time Windows**:
   - Set `StartTime`/`EndTime` for official competition
   - Set `PracticeStartTime`/`PracticeEndTime` for practice (can overlap or be separate)
   - For online exams: Set `Duration` (e.g., 3 hours) instead of fixed end time

2. **Passwords**:
   - Use `ContestPassword` for official mode security
   - Use `PracticePassword` if you want to control practice access
   - **Warning**: Changing password invalidates all participants!

3. **Submission Throttling**:
   - Start with `LimitBetweenSubmissions = 30` (30 seconds)
   - Enable `AutoChangeLimitBetweenSubmissions = true` for automatic adjustment
   - System will increase limit if workers are busy, decrease if idle

4. **IP Restrictions**:
   - For on-site exams: Add lab IPs to `AllowedIps`
   - Set `NewIpPassword` to allow students to add their IPs if needed

### Problem Setup

1. **Limits**:
   - **TimeLimit**: Start with 1000ms, adjust based on expected solution complexity
   - **MemoryLimit**: Default 16MB is usually sufficient, increase for data structure problems
   - **SourceCodeSizeLimit**: Usually not needed, set only if you want to prevent large files

2. **Feedback**:
   - **Learning/Practice**: `ShowDetailedFeedback = true` (helps students debug)
   - **Competitive**: `ShowDetailedFeedback = false` (prevents reverse-engineering)

3. **Tests**:
   - Include 2-3 **trial tests** with simple cases (users can test locally)
   - Include 10-20 **standard tests** with edge cases, large inputs, corner cases
   - Optionally include **open tests** for educational contests

4. **Checkers**:
   - Most problems: Use default "Trim" checker
   - Floating-point: Use "FloatingPoint" checker with appropriate tolerance
   - Complex validation: Create custom C# checker

### Category Setup

1. **Hierarchy**:
   - Create logical hierarchy (e.g., "2024" → "Spring Semester" → "Algorithms")
   - Use `OrderBy` to control display order

2. **Visibility**:
   - Hide entire sections with `IsVisible = false`
   - Remember: Hiding parent hides all children

3. **Mentor**:
   - Enable `AllowMentor = true` for learning-focused categories
   - Disable for competitive exams to prevent AI assistance

---

**End of Administration Documentation**


