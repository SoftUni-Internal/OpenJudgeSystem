# Contest System - Complete Documentation

## Table of Contents
1. [Contest Types](#contest-types)
2. [User Access Levels](#user-access-levels)
3. [Contest Visibility](#contest-visibility)
4. [Participation Modes](#participation-modes)
5. [Enrollment Rules](#enrollment-rules)
6. [Time-Based Access](#time-based-access)
7. [Password Protection](#password-protection)
8. [Online Exams Special Handling](#online-exams-special-handling)
9. [Exam Groups](#exam-groups)
10. [IP Restrictions](#ip-restrictions)
11. [Participant States](#participant-states)
12. [Random Task Assignment](#random-task-assignment)

---

## Contest Types

The system supports **5 contest types** (defined in `ContestType` enum):

| Type | Value | Description | Special Features |
|------|-------|-------------|------------------|
| **Exercise** | 0 | Regular practice contest | None |
| **OnsitePracticalExam** | 1 | On-site exam without random tasks | Fixed problem set |
| **OnlinePracticalExam** | 2 | Online exam with time-limited participation | Duration-based windows, random tasks, exam groups |
| **Lab** | 3 | Laboratory assignment | None |
| **OnsitePracticalExamWithRandomTasks** | 4 | On-site exam with random task assignment | Random tasks per participant |

### Contest Type Properties

```csharp
// Computed properties on Contest model
IsOnlineExam = Type == OnlinePracticalExam
IsWithRandomTasks = Type == OnlinePracticalExam || Type == OnsitePracticalExamWithRandomTasks
IsOnsiteExam = Type == OnsitePracticalExam
```

---

## User Access Levels

The system recognizes **4 user access levels**:

### 1. **Anonymous Users**
- Cannot participate in contests
- Cannot view contest details
- No access to any contest functionality

### 2. **Authenticated Regular Users**
- Can view visible contests
- Can enroll in contests (subject to restrictions)
- Can participate in official and practice modes
- Subject to all access restrictions (time windows, passwords, exam groups, IP restrictions)

### 3. **Lecturers in Contest**
- Have special permissions for specific contests
- **Bypass ALL restrictions**:
  - Time windows (can compete/practice anytime)
  - Passwords (no password required)
  - Exam groups (no exam group membership required)
  - IP restrictions (can access from any IP)
- Can manage contest participants
- Can view participant details and submissions

### 4. **Administrators**
- Have **global permissions** across all contests
- **Bypass ALL restrictions** (same as lecturers)
- Can create, edit, and delete contests
- Can manage exam groups
- Can invalidate participants

**Permission Check Logic:**
```csharp
// From ContestParticipationValidationService.cs
if (userIsAdminOrLecturerInContest)
{
    return ValidationResult.Valid(); // Bypass all checks
}
```

---

## Contest Visibility

Contests have **two visibility mechanisms**:

### 1. IsVisible Flag
- `IsVisible = true`: Contest is immediately visible
- `IsVisible = false`: Contest is hidden until `VisibleFrom` date

### 2. VisibleFrom DateTime
- If `VisibleFrom <= CurrentTime`: Contest becomes visible
- Allows scheduling contest visibility in advance

### Visibility Logic
```csharp
// From ContestsActivityService.cs
var contestIsVisible = contest.IsVisible || contest.VisibleFrom <= DateTime.UtcNow;
```

### Access Rules
- **Regular users**: Can only see contests where `contestIsVisible == true` AND `ContestCategory.IsVisible == true`
- **Admins/Lecturers**: Can see ALL contests regardless of visibility

---

## Participation Modes

Each contest supports **two independent participation modes**:

### 1. **Official Mode (Compete)**
- Competitive participation
- Counts towards rankings and results
- Uses `ContestPassword` for protection
- Time window: `StartTime` to `EndTime`
- For online exams: Creates individual participation window (`ParticipationStartTime` to `ParticipationEndTime`)

### 2. **Practice Mode**
- Non-competitive participation
- Does not count towards rankings
- Uses `PracticePassword` for protection
- Time window: `PracticeStartTime` to `PracticeEndTime`
- No individual participation windows (uses contest-wide times)

### Key Differences

| Feature | Official Mode | Practice Mode |
|---------|---------------|---------------|
| Password | `ContestPassword` | `PracticePassword` |
| Start Time | `StartTime` | `PracticeStartTime` |
| End Time | `EndTime` | `PracticeEndTime` |
| Participant Flag | `IsOfficial = true` | `IsOfficial = false` |
| Individual Time Window | Yes (for online exams) | No |
| Random Tasks | Yes (if `IsWithRandomTasks`) | No |
| Exam Group Required | Yes (for online exams) | No |

---

## Enrollment Rules

### When Can a User Enroll?

A user can enroll in a contest when **ALL** of the following conditions are met:

#### 1. **User is Authenticated**
```csharp
// From CompeteController.cs - [Authorize] attribute required
```

#### 2. **Contest is Visible**
```csharp
contestIsVisible = contest.IsVisible || contest.VisibleFrom <= DateTime.UtcNow
```

#### 3. **Contest Category is Visible**
```csharp
contestCategory.IsVisible == true
```
*(Bypassed for admins/lecturers)*

#### 4. **Contest Can Be Competed/Practiced**
- For official: `CanBeCompeted == true`
- For practice: `CanBePracticed == true`

*(Bypassed for admins/lecturers)*

#### 5. **Time Window is Active**
See [Time-Based Access](#time-based-access) section

#### 6. **Password is Correct** (if required)
See [Password Protection](#password-protection) section

#### 7. **User is in Exam Group** (for online exams in official mode)
See [Exam Groups](#exam-groups) section

#### 8. **User Confirms Participation** (for online exams in official mode)
See [Online Exams Special Handling](#online-exams-special-handling) section

---

## Time-Based Access

### Time Window Validation Logic

```csharp
// From ContestsActivityService.cs
private bool TimeRangeAllowsParticipation(DateTime? startTime, DateTime? endTime)
{
    var utcNow = this.dates.GetUtcNow();
    return startTime <= utcNow && (endTime == null || utcNow <= endTime);
}
```

### Rules:
1. **If `startTime` is null**: Participation is **NOT POSSIBLE**
2. **If `startTime` is set but `endTime` is null**: Participation is **ALLOWED FOREVER** (after start time)
3. **If both are set**: Participation is allowed only between `startTime` and `endTime`

### Official Mode Time Windows

#### For Regular Contests (Non-Online Exams)
- Uses contest-wide time window: `Contest.StartTime` to `Contest.EndTime`
- All participants share the same time window

#### For Online Exams
- Each participant gets an **individual time window**
- Window starts when participant registers: `ParticipationStartTime = DateTime.UtcNow`
- Window ends after contest duration: `ParticipationEndTime = ParticipationStartTime + Contest.Duration`
- Example: If contest duration is 3 hours and user registers at 10:00 AM, their window is 10:00 AM - 1:00 PM

```csharp
// From ParticipantsBusinessService.cs
if (contest.IsOnlineExam)
{
    var utcNow = DateTime.SpecifyKind(this.datesService.GetUtcNow(), DateTimeKind.Unspecified);
    participant.ParticipationStartTime = utcNow;
    participant.ParticipationEndTime = utcNow + contest.Duration;
}
```

### Practice Mode Time Windows
- Always uses contest-wide time window: `Contest.PracticeStartTime` to `Contest.PracticeEndTime`
- No individual windows (even for online exams)

### CanBeCompeted Logic

```csharp
// From ContestsActivityService.cs
private bool CanBeCompeted(IContestForActivityServiceModel contest)
{
    var contestIsVisible = contest.IsVisible || contest.VisibleFrom <= this.dates.GetUtcNow();

    return contestIsVisible &&
           !contest.IsDeleted &&
           this.TimeRangeAllowsParticipation(contest.StartTime, contest.EndTime);
}
```

**Contest can be competed when:**
1. Contest is visible (or VisibleFrom has passed)
2. Contest is not deleted
3. Current time is within `StartTime` to `EndTime` window

**Special case for participants with active individual windows:**
- If participant has `ParticipationStartTime` and `ParticipationEndTime` set
- `CanBeCompeted` is determined by their individual window, not contest-wide window
- This allows online exam participants to continue even after contest `EndTime` has passed

### CanBePracticed Logic

```csharp
// From ContestsActivityService.cs
private bool CanBePracticed(IContestForActivityServiceModel contest)
{
    var contestIsVisible = contest.IsVisible || contest.VisibleFrom <= this.dates.GetUtcNow();

    return contestIsVisible &&
           !contest.IsDeleted &&
           this.TimeRangeAllowsParticipation(contest.PracticeStartTime, contest.PracticeEndTime);
}
```

**Contest can be practiced when:**
1. Contest is visible (or VisibleFrom has passed)
2. Contest is not deleted
3. Current time is within `PracticeStartTime` to `PracticeEndTime` window

---

## Password Protection

Contests can be protected with passwords for both official and practice modes.

### Password Fields

| Field | Purpose | When Required |
|-------|---------|---------------|
| `ContestPassword` | Protects official participation | When set (not null) |
| `PracticePassword` | Protects practice participation | When set (not null) |
| `NewIpPassword` | Allows new IPs to be added to allowed list | When IP restrictions are enabled |

### Password Requirement Logic

```csharp
// From ContestsBusinessService.cs
private static bool ShouldRequirePassword(
    bool hasContestPassword,
    bool hasPracticePassword,
    IParticipantForActivityServiceModel? participant,
    bool official)
{
    return participant is not { IsInvalidated: false }
           && ((official && hasContestPassword) || (!official && hasPracticePassword));
}
```

**Password is required when:**
1. Participant doesn't exist OR participant is invalidated
2. AND password is set for the participation mode (official or practice)

**Password is NOT required when:**
- User is admin or lecturer in contest
- Participant already exists and is not invalidated (already registered)

### Password Validation

```csharp
// From ContestsBusinessService.cs
var isPasswordValid = isOfficial
    ? contest.ContestPassword == password
    : contest.PracticePassword == password;

if (!isPasswordValid)
{
    return ServiceResult<VoidResult>.Error("Invalid password");
}
```

---

## Online Exams Special Handling

Online exams (`ContestType.OnlinePracticalExam`) have **unique behavior**:

### 1. **Individual Participation Windows**

Each participant gets their own time window:
- **Start**: When they register (click "Start Contest")
- **End**: Start time + Contest.Duration
- **Example**: 3-hour exam, user starts at 2:00 PM → window is 2:00 PM - 5:00 PM

### 2. **Participation Confirmation Required**

```csharp
// From ContestsBusinessService.cs
private static bool ShouldConfirmParticipation(
    IParticipantForActivityServiceModel? participant,
    bool official,
    bool contestIsOnlineExam,
    bool userIsAdminOrLecturerInContest)
{
    return contestIsOnlineExam &&
           official &&
           (participant == null || participant.IsInvalidated) &&
           !userIsAdminOrLecturerInContest;
}
```

**Confirmation is required when:**
- Contest is an online exam
- Participation is official (not practice)
- Participant doesn't exist OR is invalidated
- User is NOT admin/lecturer

**Purpose**: Ensures user understands that starting the exam begins their time window immediately.

### 3. **Exam Group Membership Required**

```csharp
// From ContestParticipationValidationService.cs
if (contest.IsOnlineExam &&
    official &&
    !userIsAdminOrLecturerInContest &&
    !await this.contestsData.IsUserInExamGroupByContestAndUser(contest.Id, user.Id))
{
    return ValidationResult.AccessDenied(ValidationMessages.Participant.NotRegisteredForExam);
}
```

**For official participation in online exams:**
- User MUST be in an exam group associated with the contest
- Admins and lecturers bypass this requirement
- Practice mode does NOT require exam group membership

### 4. **Random Task Assignment**

Online exams automatically assign random tasks to participants (see [Random Task Assignment](#random-task-assignment)).

### 5. **Contest Activity Check**

```csharp
// From ContestsActivityService.cs
public async Task<bool> IsContestActive(IContestForActivityServiceModel contest)
    => this.CanBeCompeted(contest) ||
       (contest.Type == ContestType.OnlinePracticalExam &&
            await this.participantsCommonData
                .GetAllByContestAndIsOfficial(contest.Id, true)
                .AnyAsync(p =>
                    p.ParticipationEndTime.HasValue &&
                    p.ParticipationEndTime.Value >= this.dates.GetUtcNow()));
```

**Online exam is considered active when:**
- Contest can be competed (within contest time window), OR
- At least one official participant has an active individual participation window

This allows the system to keep the contest "active" for participants who started late.

---

## Exam Groups

Exam groups are used to **control access to online exams**.

### Data Model

```csharp
// From ExamGroup.cs
public class ExamGroup
{
    public int Id { get; set; }
    public int? ExternalExamGroupId { get; set; }  // For integration with external systems (e.g., SULS)
    public string? ExternalAppId { get; set; }
    public string Name { get; set; }
    public int? ContestId { get; set; }
    public virtual Contest? Contest { get; set; }
    public virtual ICollection<UserInExamGroup> UsersInExamGroups { get; set; }
}

// From UserInExamGroup.cs
public class UserInExamGroup
{
    public string UserId { get; set; }
    public virtual UserProfile User { get; set; }
    public int ExamGroupId { get; set; }
    public virtual ExamGroup ExamGroup { get; set; }
}
```

### Purpose

1. **Pre-registration for online exams**: Only users in exam groups can participate officially
2. **External system integration**: Can sync with external platforms (e.g., university systems)
3. **Access control**: Ensures only authorized students can take the exam

### Enrollment Process

1. **Admin/Lecturer creates exam group** for a contest
2. **Admin/Lecturer adds users to exam group** (manually or via bulk import)
3. **Users in exam group can participate** in the online exam officially
4. **Users NOT in exam group** receive error: "Not registered for exam"

### Management

- **Who can manage**: Admins and lecturers in the contest
- **Operations**:
  - Create exam group for contest
  - Add single user to exam group
  - Add multiple users to exam group (bulk import via usernames)
  - Remove user from exam group
  - Delete exam group

### Validation

```csharp
// From ContestParticipationValidationService.cs (line 66-72)
if (contest.IsOnlineExam &&
    official &&
    !userIsAdminOrLecturerInContest &&
    !await this.contestsData.IsUserInExamGroupByContestAndUser(contest.Id, user.Id))
{
    return ValidationResult.AccessDenied(ValidationMessages.Participant.NotRegisteredForExam);
}
```

---

## IP Restrictions

Contests can restrict access to **specific IP addresses**.

### Data Model

```csharp
// From Contest.cs
public virtual ICollection<IpInContest> IpsInContests { get; set; }
public string? NewIpPassword { get; set; }  // Password to add new IPs
```

### Purpose

1. **On-site exam security**: Ensure participants are physically present in exam room
2. **Lab restrictions**: Limit access to specific computer labs
3. **Network security**: Prevent remote access to sensitive contests

### How It Works

1. **Admin/Lecturer configures allowed IPs** for contest (semicolon-separated list)
   - Example: `192.168.1.100;192.168.1.101;192.168.1.102`

2. **System checks participant's IP** when accessing contest

3. **If IP is not in allowed list**:
   - User can provide `NewIpPassword`
   - If password is correct, their IP is added to allowed list with `IsOriginallyAllowed = false`
   - If password is incorrect, access is denied

### IP Management

```csharp
// From ContestsBusinessService.cs
private async Task AddIpsToContest(Contest contest, string? mergedIps)
{
    if (!string.IsNullOrWhiteSpace(mergedIps))
    {
        var ipValues = mergedIps.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var ipValue in ipValues)
        {
            var ip = await this.ipsData.GetByValue(ipValue) ?? new Ip { Value = ipValue };
            contest.IpsInContests.Add(new IpInContest { Ip = ip, IsOriginallyAllowed = true });
        }
    }
}
```

### Bypass Rules

- **Admins**: Can access from any IP
- **Lecturers in contest**: Can access from any IP
- **Regular users**: Must be in allowed IP list OR provide correct `NewIpPassword`

---

## Participant States

Each participant has **two boolean flags** that control their status:

### 1. IsOfficial

- `true`: Official (competitive) participation
- `false`: Practice (non-competitive) participation

**Note**: A user can have **TWO separate participant records** for the same contest:
- One with `IsOfficial = true` (for competing)
- One with `IsOfficial = false` (for practicing)

### 2. IsInvalidated

- `true`: Participant is invalidated (cannot participate)
- `false`: Participant is valid (can participate)

### Invalidation

**When does invalidation happen?**

1. **Password change**: When contest/practice password is changed, all participants in that mode are invalidated
   ```csharp
   // From ContestsBusinessService.cs
   await this.InvalidateParticipants(originalContestPassword, originalPracticePassword, model);
   ```

2. **Manual invalidation**: Admin/lecturer can manually invalidate participants

**Effect of invalidation:**
- Participant must re-register (provide password again if required)
- For online exams: New participation window is created upon re-registration
- Previous submissions remain intact

### Participant Activity Model

```csharp
// From ParticipantActivityServiceModel.cs
public class ParticipantActivityServiceModel
{
    public bool HasParticipationTimeLeft { get; set; }
    public bool IsInvalidated { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
```

**Participant can participate when:**
- `HasParticipationTimeLeft == true` (current time within their window)
- `IsInvalidated == false`

---

## Random Task Assignment

Contests with `IsWithRandomTasks == true` assign **random problems** to each participant.

### Which Contest Types Have Random Tasks?

1. **OnlinePracticalExam** (type 2)
2. **OnsitePracticalExamWithRandomTasks** (type 4)

### How It Works

```csharp
// From ParticipantsBusinessService.cs
if (!isAdminOrLecturerInContest && contest.IsWithRandomTasks)
{
    var problemGroups = await this.problemGroupsData
        .GetAllByContest(contest.Id)
        .Include(pg => pg.Problems)
        .ToListAsync();

    AssignRandomProblemsToParticipant(participant, problemGroups);
}
```

### Assignment Logic

1. **For each problem group** in the contest:
   - Select a random problem from the group
   - Assign it to the participant

2. **Result**: Each participant sees a different subset of problems

3. **Purpose**:
   - Prevent cheating in online exams
   - Ensure fairness (each participant gets similar difficulty)

### Bypass Rules

- **Admins**: See ALL problems (no random assignment)
- **Lecturers in contest**: See ALL problems (no random assignment)
- **Regular users in official mode**: Get random problems
- **Practice mode**: No random assignment (all users see all problems)

---

## Complete Enrollment Flow

### Official Mode Enrollment

```
1. User clicks "Compete" on contest
   ↓
2. System checks: Is user authenticated?
   → NO: Redirect to login
   → YES: Continue
   ↓
3. System checks: Is contest visible?
   → NO: Return 404 Not Found
   → YES: Continue
   ↓
4. System checks: Is user admin or lecturer in contest?
   → YES: Allow access (bypass all checks)
   → NO: Continue
   ↓
5. System checks: Can contest be competed? (CanBeCompeted)
   → NO: Return "Contest cannot be competed at this time"
   → YES: Continue
   ↓
6. System checks: Is this an online exam?
   → YES: Check if user is in exam group
      → NO: Return "Not registered for exam"
      → YES: Continue
   → NO: Continue
   ↓
7. System checks: Does participant already exist and is valid?
   → YES: Start contest (skip password/confirmation)
   → NO: Continue
   ↓
8. System checks: Is password required?
   → YES: Show password form
      → User enters password
      → Validate password
      → INVALID: Return error
      → VALID: Continue
   → NO: Continue
   ↓
9. System checks: Is this an online exam requiring confirmation?
   → YES: Show confirmation dialog
      → User confirms
      → Continue
   → NO: Continue
   ↓
10. Create participant record:
    - Set IsOfficial = true
    - If online exam: Set ParticipationStartTime and ParticipationEndTime
    - If random tasks: Assign random problems
    ↓
11. Redirect to contest page
```

### Practice Mode Enrollment

```
1. User clicks "Practice" on contest
   ↓
2-4. Same as official mode (authentication, visibility, admin/lecturer check)
   ↓
5. System checks: Can contest be practiced? (CanBePracticed)
   → NO: Return "Contest cannot be practiced at this time"
   → YES: Continue
   ↓
6. (Skip exam group check - not required for practice)
   ↓
7. System checks: Does participant already exist and is valid?
   → YES: Start contest (skip password)
   → NO: Continue
   ↓
8. System checks: Is practice password required?
   → YES: Show password form
      → User enters password
      → Validate password
      → INVALID: Return error
      → VALID: Continue
   → NO: Continue
   ↓
9. (Skip confirmation - not required for practice)
   ↓
10. Create participant record:
    - Set IsOfficial = false
    - No individual time window (use contest-wide PracticeStartTime/PracticeEndTime)
    - No random task assignment
    ↓
11. Redirect to contest page
```

---

## Summary: Access Control Matrix

| User Type | Visibility | Time Windows | Passwords | Exam Groups | IP Restrictions | Random Tasks |
|-----------|------------|--------------|-----------|-------------|-----------------|--------------|
| **Anonymous** | ❌ No access | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Regular User (Official)** | ✅ If visible | ✅ Required | ✅ Required | ✅ Required (online exams) | ✅ Required | ✅ Assigned |
| **Regular User (Practice)** | ✅ If visible | ✅ Required | ✅ Required | ❌ Not required | ✅ Required | ❌ Not assigned |
| **Lecturer in Contest** | ✅ Always | ⚠️ Bypassed | ⚠️ Bypassed | ⚠️ Bypassed | ⚠️ Bypassed | ❌ Sees all |
| **Administrator** | ✅ Always | ⚠️ Bypassed | ⚠️ Bypassed | ⚠️ Bypassed | ⚠️ Bypassed | ❌ Sees all |

**Legend:**
- ✅ = Check is enforced
- ❌ = Check is not applicable
- ⚠️ = Check is bypassed (always passes)

---

## Key Takeaways

1. **Two participation modes**: Official (competitive) and Practice (non-competitive) are completely independent
2. **Admins and lecturers bypass everything**: They have unrestricted access to all contests
3. **Online exams are special**: Individual time windows, exam groups, confirmation required, random tasks
4. **Time windows are flexible**: Can be set to allow participation forever (null end time)
5. **Passwords are optional**: Only required if set, and only for new/invalidated participants
6. **Exam groups control access**: Only for official participation in online exams
7. **IP restrictions add security**: For on-site exams and lab restrictions
8. **Invalidation forces re-registration**: Useful when passwords change or participants need to be reset
9. **Random tasks prevent cheating**: Each participant gets different problems in online exams
10. **Visibility is two-tiered**: Contest visibility AND category visibility must both be true (for regular users)


