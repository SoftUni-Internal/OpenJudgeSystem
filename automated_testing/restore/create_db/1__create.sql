USE [master]
GO
/****** Object:  Database [OpenJudgeSystem]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE DATABASE [OpenJudgeSystem]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'OpenJudgeSystem', FILENAME = N'/var/opt/mssql/data/OpenJudgeSystem.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'OpenJudgeSystem_log', FILENAME = N'/var/opt/mssql/data/OpenJudgeSystem_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [OpenJudgeSystem] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [OpenJudgeSystem].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [OpenJudgeSystem] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET ARITHABORT OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [OpenJudgeSystem] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [OpenJudgeSystem] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET QUOTED_IDENTIFIER ON 
GO
ALTER DATABASE [OpenJudgeSystem] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET  ENABLE_BROKER 
GO
ALTER DATABASE [OpenJudgeSystem] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [OpenJudgeSystem] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [OpenJudgeSystem] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET RECOVERY FULL 
GO
ALTER DATABASE [OpenJudgeSystem] SET  MULTI_USER 
GO
ALTER DATABASE [OpenJudgeSystem] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [OpenJudgeSystem] SET DB_CHAINING OFF 
GO
ALTER DATABASE [OpenJudgeSystem] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [OpenJudgeSystem] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [OpenJudgeSystem] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [OpenJudgeSystem] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'OpenJudgeSystem', N'ON'
GO
ALTER DATABASE [OpenJudgeSystem] SET QUERY_STORE = OFF
GO
USE [OpenJudgeSystem]
GO
/****** Object:  Schema [HangFire]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE SCHEMA [HangFire]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AccessLogs]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccessLogs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[IpAddress] [nvarchar](max) NOT NULL,
	[RequestType] [nvarchar](max) NOT NULL,
	[Url] [nvarchar](max) NOT NULL,
	[PostParams] [nvarchar](max) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_AccessLogs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](450) NOT NULL,
	[ProviderKey] [nvarchar](450) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](450) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](450) NOT NULL,
	[Email] [nvarchar](256) NOT NULL,
	[FirstName] [nvarchar](30) NULL,
	[LastName] [nvarchar](30) NULL,
	[City] [nvarchar](200) NULL,
	[EducationalInstitution] [nvarchar](max) NULL,
	[FacultyNumber] [nvarchar](30) NULL,
	[DateOfBirth] [datetime2](7) NULL,
	[Company] [nvarchar](200) NULL,
	[JobTitle] [nvarchar](100) NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[UserName] [varchar](50) NOT NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [nvarchar](450) NOT NULL,
	[LoginProvider] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](450) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Checkers]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Checkers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[DllFile] [nvarchar](max) NULL,
	[ClassName] [nvarchar](max) NULL,
	[Parameter] [nvarchar](max) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Checkers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ContestCategories]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContestCategories](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[OrderBy] [float] NOT NULL,
	[ParentId] [int] NULL,
	[IsVisible] [bit] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_ContestCategories] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ContestIps]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContestIps](
	[ContestId] [int] NOT NULL,
	[IpId] [int] NOT NULL,
	[IsOriginallyAllowed] [bit] NOT NULL,
 CONSTRAINT [PK_ContestIps] PRIMARY KEY CLUSTERED 
(
	[ContestId] ASC,
	[IpId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ContestQuestionAnswers]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContestQuestionAnswers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[QuestionId] [int] NOT NULL,
	[Text] [nvarchar](100) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_ContestQuestionAnswers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ContestQuestions]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContestQuestions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContestId] [int] NOT NULL,
	[Text] [nvarchar](100) NULL,
	[AskOfficialParticipants] [bit] NOT NULL,
	[AskPracticeParticipants] [bit] NOT NULL,
	[Type] [int] NOT NULL,
	[RegularExpressionValidation] [nvarchar](max) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_ContestQuestions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Contests]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Contests](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NULL,
	[IsVisible] [bit] NOT NULL,
	[AutoChangeTestsFeedbackVisibility] [bit] NOT NULL,
	[CategoryId] [int] NULL,
	[Type] [int] NOT NULL,
	[Duration] [time](7) NULL,
	[StartTime] [datetime2](7) NULL,
	[EndTime] [datetime2](7) NULL,
	[ContestPassword] [nvarchar](20) NULL,
	[PracticePassword] [nvarchar](20) NULL,
	[NewIpPassword] [nvarchar](20) NULL,
	[PracticeStartTime] [datetime2](7) NULL,
	[PracticeEndTime] [datetime2](7) NULL,
	[LimitBetweenSubmissions] [int] NOT NULL,
	[OrderBy] [float] NOT NULL,
	[NumberOfProblemGroups] [smallint] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Contests] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Events]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Events](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[StartTime] [datetime2](7) NOT NULL,
	[EndTime] [datetime2](7) NULL,
	[Url] [nvarchar](max) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExamGroups]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExamGroups](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ExternalExamGroupId] [int] NULL,
	[ExternalAppId] [nvarchar](max) NULL,
	[Name] [nvarchar](600) NOT NULL,
	[ContestId] [int] NULL,
 CONSTRAINT [PK_ExamGroups] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FeedbackReports]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FeedbackReports](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
	[Content] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[IsFixed] [bit] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_FeedbackReports] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Ips]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ips](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Value] [nvarchar](45) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Ips] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LecturersInContestCategories]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LecturersInContestCategories](
	[LecturerId] [nvarchar](450) NOT NULL,
	[ContestCategoryId] [int] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_LecturersInContestCategories] PRIMARY KEY CLUSTERED 
(
	[LecturerId] ASC,
	[ContestCategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LecturersInContests]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LecturersInContests](
	[LecturerId] [nvarchar](450) NOT NULL,
	[ContestId] [int] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_LecturersInContests] PRIMARY KEY CLUSTERED 
(
	[LecturerId] ASC,
	[ContestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ParticipantAnswers]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ParticipantAnswers](
	[ParticipantId] [int] NOT NULL,
	[ContestQuestionId] [int] NOT NULL,
	[Answer] [nvarchar](max) NULL,
 CONSTRAINT [PK_ParticipantAnswers] PRIMARY KEY CLUSTERED 
(
	[ParticipantId] ASC,
	[ContestQuestionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Participants]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Participants](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContestId] [int] NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ParticipationStartTime] [datetime2](7) NULL,
	[ParticipationEndTime] [datetime2](7) NULL,
	[IsOfficial] [bit] NOT NULL,
	[IsInvalidated] [bit] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Participants] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ParticipantScores]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ParticipantScores](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProblemId] [int] NOT NULL,
	[ParticipantId] [int] NOT NULL,
	[SubmissionId] [int] NULL,
	[ParticipantName] [nvarchar](max) NOT NULL,
	[Points] [int] NOT NULL,
	[IsOfficial] [bit] NOT NULL,
 CONSTRAINT [PK_ParticipantScores] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemGroups]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemGroups](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContestId] [int] NOT NULL,
	[OrderBy] [float] NOT NULL,
	[Type] [int] NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_ProblemGroups] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemResources]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemResources](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProblemId] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Type] [int] NOT NULL,
	[File] [varbinary](max) NULL,
	[FileExtension] [nvarchar](4) NULL,
	[Link] [nvarchar](max) NULL,
	[OrderBy] [float] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_ProblemResources] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Problems]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Problems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProblemGroupId] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[MaximumPoints] [smallint] NOT NULL,
	[TimeLimit] [int] NOT NULL,
	[MemoryLimit] [int] NOT NULL,
	[SourceCodeSizeLimit] [int] NULL,
	[CheckerId] [int] NULL,
	[OrderBy] [float] NOT NULL,
	[SolutionSkeleton] [varbinary](max) NULL,
	[AdditionalFiles] [varbinary](max) NULL,
	[ShowResults] [bit] NOT NULL,
	[ShowDetailedFeedback] [bit] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Problems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProblemsForParticipants]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProblemsForParticipants](
	[ProblemId] [int] NOT NULL,
	[ParticipantId] [int] NOT NULL,
 CONSTRAINT [PK_ProblemsForParticipants] PRIMARY KEY CLUSTERED 
(
	[ProblemId] ASC,
	[ParticipantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Settings]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Settings](
	[Name] [nvarchar](450) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SourceCodes]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SourceCodes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AuthorId] [nvarchar](450) NOT NULL,
	[ProblemId] [int] NULL,
	[Content] [varbinary](max) NOT NULL,
	[IsPublic] [bit] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_SourceCodes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Submissions]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Submissions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ParticipantId] [int] NULL,
	[ProblemId] [int] NULL,
	[SubmissionTypeId] [int] NULL,
	[Content] [varbinary](max) NOT NULL,
	[FileExtension] [nvarchar](max) NULL,
	[SolutionSkeleton] [varbinary](max) NULL,
	[IpAddress] [varchar](45) NULL,
	[IsCompiledSuccessfully] [bit] NOT NULL,
	[CompilerComment] [nvarchar](max) NULL,
	[IsPublic] [bit] NULL,
	[TestRunsCache] [nvarchar](max) NULL,
	[Processed] [bit] NOT NULL,
	[ProcessingComment] [nvarchar](max) NULL,
	[Points] [int] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Submissions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SubmissionsForProcessing]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubmissionsForProcessing](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubmissionId] [int] NOT NULL,
	[Processing] [bit] NOT NULL,
	[Processed] [bit] NOT NULL,
 CONSTRAINT [PK_SubmissionsForProcessing] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SubmissionTypeProblems]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubmissionTypeProblems](
	[SubmissionTypeId] [int] NOT NULL,
	[ProblemId] [int] NOT NULL,
 CONSTRAINT [PK_SubmissionTypeProblems] PRIMARY KEY CLUSTERED 
(
	[SubmissionTypeId] ASC,
	[ProblemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SubmissionTypes]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubmissionTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[IsSelectedByDefault] [bit] NOT NULL,
	[ExecutionStrategyType] [int] NOT NULL,
	[CompilerType] [int] NOT NULL,
	[AdditionalCompilerArguments] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[AllowBinaryFilesUpload] [bit] NOT NULL,
	[AllowedFileExtensions] [nvarchar](max) NULL,
 CONSTRAINT [PK_SubmissionTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TagProblems]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TagProblems](
	[TagId] [int] NOT NULL,
	[ProblemId] [int] NOT NULL,
 CONSTRAINT [PK_TagProblems] PRIMARY KEY CLUSTERED 
(
	[TagId] ASC,
	[ProblemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tags](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ForegroundColor] [nvarchar](max) NULL,
	[BackgroundColor] [nvarchar](max) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TestRuns]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TestRuns](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubmissionId] [int] NOT NULL,
	[TestId] [int] NOT NULL,
	[TimeUsed] [int] NOT NULL,
	[MemoryUsed] [bigint] NOT NULL,
	[ResultType] [int] NOT NULL,
	[ExecutionComment] [nvarchar](max) NULL,
	[CheckerComment] [nvarchar](max) NULL,
	[ExpectedOutputFragment] [nvarchar](max) NULL,
	[UserOutputFragment] [nvarchar](max) NULL,
 CONSTRAINT [PK_TestRuns] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tests]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tests](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProblemId] [int] NOT NULL,
	[InputData] [varbinary](max) NOT NULL,
	[OutputData] [varbinary](max) NOT NULL,
	[IsTrialTest] [bit] NOT NULL,
	[IsOpenTest] [bit] NOT NULL,
	[HideInput] [bit] NOT NULL,
	[OrderBy] [float] NOT NULL,
 CONSTRAINT [PK_Tests] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UsersInExamGroups]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UsersInExamGroups](
	[UserId] [nvarchar](450) NOT NULL,
	[ExamGroupId] [int] NOT NULL,
 CONSTRAINT [PK_UsersInExamGroups] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[ExamGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[AggregatedCounter]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[AggregatedCounter](
	[Key] [nvarchar](100) NOT NULL,
	[Value] [bigint] NOT NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_CounterAggregated] PRIMARY KEY CLUSTERED 
(
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[Counter]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Counter](
	[Key] [nvarchar](100) NOT NULL,
	[Value] [int] NOT NULL,
	[ExpireAt] [datetime] NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [CX_HangFire_Counter]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE CLUSTERED INDEX [CX_HangFire_Counter] ON [HangFire].[Counter]
(
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[Hash]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Hash](
	[Key] [nvarchar](100) NOT NULL,
	[Field] [nvarchar](100) NOT NULL,
	[Value] [nvarchar](max) NULL,
	[ExpireAt] [datetime2](7) NULL,
 CONSTRAINT [PK_HangFire_Hash] PRIMARY KEY CLUSTERED 
(
	[Key] ASC,
	[Field] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[Job]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Job](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[StateId] [bigint] NULL,
	[StateName] [nvarchar](20) NULL,
	[InvocationData] [nvarchar](max) NOT NULL,
	[Arguments] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_Job] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[JobParameter]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[JobParameter](
	[JobId] [bigint] NOT NULL,
	[Name] [nvarchar](40) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_HangFire_JobParameter] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[JobQueue]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[JobQueue](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[JobId] [bigint] NOT NULL,
	[Queue] [nvarchar](50) NOT NULL,
	[FetchedAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_JobQueue] PRIMARY KEY CLUSTERED 
(
	[Queue] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[List]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[List](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](100) NOT NULL,
	[Value] [nvarchar](max) NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_List] PRIMARY KEY CLUSTERED 
(
	[Key] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[Schema]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Schema](
	[Version] [int] NOT NULL,
 CONSTRAINT [PK_HangFire_Schema] PRIMARY KEY CLUSTERED 
(
	[Version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[Server]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Server](
	[Id] [nvarchar](200) NOT NULL,
	[Data] [nvarchar](max) NULL,
	[LastHeartbeat] [datetime] NOT NULL,
 CONSTRAINT [PK_HangFire_Server] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[Set]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Set](
	[Key] [nvarchar](100) NOT NULL,
	[Score] [float] NOT NULL,
	[Value] [nvarchar](256) NOT NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_Set] PRIMARY KEY CLUSTERED 
(
	[Key] ASC,
	[Value] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[State]    Script Date: 30.6.2022 г. 17:15:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[State](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[JobId] [bigint] NOT NULL,
	[Name] [nvarchar](20) NOT NULL,
	[Reason] [nvarchar](100) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[Data] [nvarchar](max) NULL,
 CONSTRAINT [PK_HangFire_State] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AccessLogs_UserId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_AccessLogs_UserId] ON [dbo].[AccessLogs]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetRoleClaims_RoleId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [RoleNameIndex]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
(
	[NormalizedName] ASC
)
WHERE ([NormalizedName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserClaims_UserId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserLogins_UserId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserRoles_RoleId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [EmailIndex]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [EmailIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedEmail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UserNameIndex]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedUserName] ASC
)
WHERE ([NormalizedUserName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ContestCategories_ParentId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_ContestCategories_ParentId] ON [dbo].[ContestCategories]
(
	[ParentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ContestIps_IpId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_ContestIps_IpId] ON [dbo].[ContestIps]
(
	[IpId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ContestQuestionAnswers_QuestionId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_ContestQuestionAnswers_QuestionId] ON [dbo].[ContestQuestionAnswers]
(
	[QuestionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ContestQuestions_ContestId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_ContestQuestions_ContestId] ON [dbo].[ContestQuestions]
(
	[ContestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Contests_CategoryId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_Contests_CategoryId] ON [dbo].[Contests]
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ExamGroups_ContestId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_ExamGroups_ContestId] ON [dbo].[ExamGroups]
(
	[ContestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_FeedbackReports_UserId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_FeedbackReports_UserId] ON [dbo].[FeedbackReports]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Ips_Value]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Ips_Value] ON [dbo].[Ips]
(
	[Value] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_LecturersInContestCategories_ContestCategoryId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_LecturersInContestCategories_ContestCategoryId] ON [dbo].[LecturersInContestCategories]
(
	[ContestCategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_LecturersInContests_ContestId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_LecturersInContests_ContestId] ON [dbo].[LecturersInContests]
(
	[ContestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ParticipantAnswers_ContestQuestionId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_ParticipantAnswers_ContestQuestionId] ON [dbo].[ParticipantAnswers]
(
	[ContestQuestionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Participants_ContestId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_Participants_ContestId] ON [dbo].[Participants]
(
	[ContestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Participants_UserId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_Participants_UserId] ON [dbo].[Participants]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ParticipantScores_ParticipantId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_ParticipantScores_ParticipantId] ON [dbo].[ParticipantScores]
(
	[ParticipantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ParticipantScores_ProblemId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_ParticipantScores_ProblemId] ON [dbo].[ParticipantScores]
(
	[ProblemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ParticipantScores_SubmissionId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_ParticipantScores_SubmissionId] ON [dbo].[ParticipantScores]
(
	[SubmissionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProblemGroups_ContestId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_ProblemGroups_ContestId] ON [dbo].[ProblemGroups]
(
	[ContestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProblemResources_ProblemId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_ProblemResources_ProblemId] ON [dbo].[ProblemResources]
(
	[ProblemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Problems_CheckerId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_Problems_CheckerId] ON [dbo].[Problems]
(
	[CheckerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Problems_ProblemGroupId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_Problems_ProblemGroupId] ON [dbo].[Problems]
(
	[ProblemGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ProblemsForParticipants_ParticipantId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_ProblemsForParticipants_ParticipantId] ON [dbo].[ProblemsForParticipants]
(
	[ParticipantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_SourceCodes_AuthorId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_SourceCodes_AuthorId] ON [dbo].[SourceCodes]
(
	[AuthorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_SourceCodes_ProblemId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_SourceCodes_ProblemId] ON [dbo].[SourceCodes]
(
	[ProblemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Submissions_ParticipantId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_Submissions_ParticipantId] ON [dbo].[Submissions]
(
	[ParticipantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Submissions_ProblemId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_Submissions_ProblemId] ON [dbo].[Submissions]
(
	[ProblemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Submissions_SubmissionTypeId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_Submissions_SubmissionTypeId] ON [dbo].[Submissions]
(
	[SubmissionTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_SubmissionTypeProblems_ProblemId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_SubmissionTypeProblems_ProblemId] ON [dbo].[SubmissionTypeProblems]
(
	[ProblemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_TagProblems_ProblemId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_TagProblems_ProblemId] ON [dbo].[TagProblems]
(
	[ProblemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_TestRuns_SubmissionId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_TestRuns_SubmissionId] ON [dbo].[TestRuns]
(
	[SubmissionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_TestRuns_TestId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_TestRuns_TestId] ON [dbo].[TestRuns]
(
	[TestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Tests_ProblemId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_Tests_ProblemId] ON [dbo].[Tests]
(
	[ProblemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_UsersInExamGroups_ExamGroupId]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_UsersInExamGroups_ExamGroupId] ON [dbo].[UsersInExamGroups]
(
	[ExamGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HangFire_AggregatedCounter_ExpireAt]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_AggregatedCounter_ExpireAt] ON [HangFire].[AggregatedCounter]
(
	[ExpireAt] ASC
)
WHERE ([ExpireAt] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HangFire_Hash_ExpireAt]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_Hash_ExpireAt] ON [HangFire].[Hash]
(
	[ExpireAt] ASC
)
WHERE ([ExpireAt] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HangFire_Job_ExpireAt]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_Job_ExpireAt] ON [HangFire].[Job]
(
	[ExpireAt] ASC
)
INCLUDE([StateName]) 
WHERE ([ExpireAt] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_HangFire_Job_StateName]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_Job_StateName] ON [HangFire].[Job]
(
	[StateName] ASC
)
WHERE ([StateName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HangFire_List_ExpireAt]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_List_ExpireAt] ON [HangFire].[List]
(
	[ExpireAt] ASC
)
WHERE ([ExpireAt] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HangFire_Server_LastHeartbeat]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_Server_LastHeartbeat] ON [HangFire].[Server]
(
	[LastHeartbeat] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HangFire_Set_ExpireAt]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_Set_ExpireAt] ON [HangFire].[Set]
(
	[ExpireAt] ASC
)
WHERE ([ExpireAt] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_HangFire_Set_Score]    Script Date: 30.6.2022 г. 17:15:08 ******/
CREATE NONCLUSTERED INDEX [IX_HangFire_Set_Score] ON [HangFire].[Set]
(
	[Key] ASC,
	[Score] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AccessLogs]  WITH CHECK ADD  CONSTRAINT [FK_AccessLogs_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AccessLogs] CHECK CONSTRAINT [FK_AccessLogs_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[ContestCategories]  WITH CHECK ADD  CONSTRAINT [FK_ContestCategories_ContestCategories_ParentId] FOREIGN KEY([ParentId])
REFERENCES [dbo].[ContestCategories] ([Id])
GO
ALTER TABLE [dbo].[ContestCategories] CHECK CONSTRAINT [FK_ContestCategories_ContestCategories_ParentId]
GO
ALTER TABLE [dbo].[ContestIps]  WITH CHECK ADD  CONSTRAINT [FK_ContestIps_Contests_ContestId] FOREIGN KEY([ContestId])
REFERENCES [dbo].[Contests] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ContestIps] CHECK CONSTRAINT [FK_ContestIps_Contests_ContestId]
GO
ALTER TABLE [dbo].[ContestIps]  WITH CHECK ADD  CONSTRAINT [FK_ContestIps_Ips_IpId] FOREIGN KEY([IpId])
REFERENCES [dbo].[Ips] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ContestIps] CHECK CONSTRAINT [FK_ContestIps_Ips_IpId]
GO
ALTER TABLE [dbo].[ContestQuestionAnswers]  WITH CHECK ADD  CONSTRAINT [FK_ContestQuestionAnswers_ContestQuestions_QuestionId] FOREIGN KEY([QuestionId])
REFERENCES [dbo].[ContestQuestions] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ContestQuestionAnswers] CHECK CONSTRAINT [FK_ContestQuestionAnswers_ContestQuestions_QuestionId]
GO
ALTER TABLE [dbo].[ContestQuestions]  WITH CHECK ADD  CONSTRAINT [FK_ContestQuestions_Contests_ContestId] FOREIGN KEY([ContestId])
REFERENCES [dbo].[Contests] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ContestQuestions] CHECK CONSTRAINT [FK_ContestQuestions_Contests_ContestId]
GO
ALTER TABLE [dbo].[Contests]  WITH CHECK ADD  CONSTRAINT [FK_Contests_ContestCategories_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[ContestCategories] ([Id])
GO
ALTER TABLE [dbo].[Contests] CHECK CONSTRAINT [FK_Contests_ContestCategories_CategoryId]
GO
ALTER TABLE [dbo].[ExamGroups]  WITH CHECK ADD  CONSTRAINT [FK_ExamGroups_Contests_ContestId] FOREIGN KEY([ContestId])
REFERENCES [dbo].[Contests] ([Id])
GO
ALTER TABLE [dbo].[ExamGroups] CHECK CONSTRAINT [FK_ExamGroups_Contests_ContestId]
GO
ALTER TABLE [dbo].[FeedbackReports]  WITH CHECK ADD  CONSTRAINT [FK_FeedbackReports_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FeedbackReports] CHECK CONSTRAINT [FK_FeedbackReports_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[LecturersInContestCategories]  WITH CHECK ADD  CONSTRAINT [FK_LecturersInContestCategories_AspNetUsers_LecturerId] FOREIGN KEY([LecturerId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LecturersInContestCategories] CHECK CONSTRAINT [FK_LecturersInContestCategories_AspNetUsers_LecturerId]
GO
ALTER TABLE [dbo].[LecturersInContestCategories]  WITH CHECK ADD  CONSTRAINT [FK_LecturersInContestCategories_ContestCategories_ContestCategoryId] FOREIGN KEY([ContestCategoryId])
REFERENCES [dbo].[ContestCategories] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LecturersInContestCategories] CHECK CONSTRAINT [FK_LecturersInContestCategories_ContestCategories_ContestCategoryId]
GO
ALTER TABLE [dbo].[LecturersInContests]  WITH CHECK ADD  CONSTRAINT [FK_LecturersInContests_AspNetUsers_LecturerId] FOREIGN KEY([LecturerId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LecturersInContests] CHECK CONSTRAINT [FK_LecturersInContests_AspNetUsers_LecturerId]
GO
ALTER TABLE [dbo].[LecturersInContests]  WITH CHECK ADD  CONSTRAINT [FK_LecturersInContests_Contests_ContestId] FOREIGN KEY([ContestId])
REFERENCES [dbo].[Contests] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LecturersInContests] CHECK CONSTRAINT [FK_LecturersInContests_Contests_ContestId]
GO
ALTER TABLE [dbo].[ParticipantAnswers]  WITH CHECK ADD  CONSTRAINT [FK_ParticipantAnswers_ContestQuestions_ContestQuestionId] FOREIGN KEY([ContestQuestionId])
REFERENCES [dbo].[ContestQuestions] ([Id])
GO
ALTER TABLE [dbo].[ParticipantAnswers] CHECK CONSTRAINT [FK_ParticipantAnswers_ContestQuestions_ContestQuestionId]
GO
ALTER TABLE [dbo].[ParticipantAnswers]  WITH CHECK ADD  CONSTRAINT [FK_ParticipantAnswers_Participants_ParticipantId] FOREIGN KEY([ParticipantId])
REFERENCES [dbo].[Participants] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ParticipantAnswers] CHECK CONSTRAINT [FK_ParticipantAnswers_Participants_ParticipantId]
GO
ALTER TABLE [dbo].[Participants]  WITH CHECK ADD  CONSTRAINT [FK_Participants_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Participants] CHECK CONSTRAINT [FK_Participants_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[Participants]  WITH CHECK ADD  CONSTRAINT [FK_Participants_Contests_ContestId] FOREIGN KEY([ContestId])
REFERENCES [dbo].[Contests] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Participants] CHECK CONSTRAINT [FK_Participants_Contests_ContestId]
GO
ALTER TABLE [dbo].[ParticipantScores]  WITH CHECK ADD  CONSTRAINT [FK_ParticipantScores_Participants_ParticipantId] FOREIGN KEY([ParticipantId])
REFERENCES [dbo].[Participants] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ParticipantScores] CHECK CONSTRAINT [FK_ParticipantScores_Participants_ParticipantId]
GO
ALTER TABLE [dbo].[ParticipantScores]  WITH CHECK ADD  CONSTRAINT [FK_ParticipantScores_Problems_ProblemId] FOREIGN KEY([ProblemId])
REFERENCES [dbo].[Problems] ([Id])
GO
ALTER TABLE [dbo].[ParticipantScores] CHECK CONSTRAINT [FK_ParticipantScores_Problems_ProblemId]
GO
ALTER TABLE [dbo].[ParticipantScores]  WITH CHECK ADD  CONSTRAINT [FK_ParticipantScores_Submissions_SubmissionId] FOREIGN KEY([SubmissionId])
REFERENCES [dbo].[Submissions] ([Id])
GO
ALTER TABLE [dbo].[ParticipantScores] CHECK CONSTRAINT [FK_ParticipantScores_Submissions_SubmissionId]
GO
ALTER TABLE [dbo].[ProblemGroups]  WITH CHECK ADD  CONSTRAINT [FK_ProblemGroups_Contests_ContestId] FOREIGN KEY([ContestId])
REFERENCES [dbo].[Contests] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProblemGroups] CHECK CONSTRAINT [FK_ProblemGroups_Contests_ContestId]
GO
ALTER TABLE [dbo].[ProblemResources]  WITH CHECK ADD  CONSTRAINT [FK_ProblemResources_Problems_ProblemId] FOREIGN KEY([ProblemId])
REFERENCES [dbo].[Problems] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProblemResources] CHECK CONSTRAINT [FK_ProblemResources_Problems_ProblemId]
GO
ALTER TABLE [dbo].[Problems]  WITH CHECK ADD  CONSTRAINT [FK_Problems_Checkers_CheckerId] FOREIGN KEY([CheckerId])
REFERENCES [dbo].[Checkers] ([Id])
GO
ALTER TABLE [dbo].[Problems] CHECK CONSTRAINT [FK_Problems_Checkers_CheckerId]
GO
ALTER TABLE [dbo].[Problems]  WITH CHECK ADD  CONSTRAINT [FK_Problems_ProblemGroups_ProblemGroupId] FOREIGN KEY([ProblemGroupId])
REFERENCES [dbo].[ProblemGroups] ([Id])
GO
ALTER TABLE [dbo].[Problems] CHECK CONSTRAINT [FK_Problems_ProblemGroups_ProblemGroupId]
GO
ALTER TABLE [dbo].[ProblemsForParticipants]  WITH CHECK ADD  CONSTRAINT [FK_ProblemsForParticipants_Participants_ParticipantId] FOREIGN KEY([ParticipantId])
REFERENCES [dbo].[Participants] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProblemsForParticipants] CHECK CONSTRAINT [FK_ProblemsForParticipants_Participants_ParticipantId]
GO
ALTER TABLE [dbo].[ProblemsForParticipants]  WITH CHECK ADD  CONSTRAINT [FK_ProblemsForParticipants_Problems_ProblemId] FOREIGN KEY([ProblemId])
REFERENCES [dbo].[Problems] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProblemsForParticipants] CHECK CONSTRAINT [FK_ProblemsForParticipants_Problems_ProblemId]
GO
ALTER TABLE [dbo].[SourceCodes]  WITH CHECK ADD  CONSTRAINT [FK_SourceCodes_AspNetUsers_AuthorId] FOREIGN KEY([AuthorId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SourceCodes] CHECK CONSTRAINT [FK_SourceCodes_AspNetUsers_AuthorId]
GO
ALTER TABLE [dbo].[SourceCodes]  WITH CHECK ADD  CONSTRAINT [FK_SourceCodes_Problems_ProblemId] FOREIGN KEY([ProblemId])
REFERENCES [dbo].[Problems] ([Id])
GO
ALTER TABLE [dbo].[SourceCodes] CHECK CONSTRAINT [FK_SourceCodes_Problems_ProblemId]
GO
ALTER TABLE [dbo].[Submissions]  WITH CHECK ADD  CONSTRAINT [FK_Submissions_Participants_ParticipantId] FOREIGN KEY([ParticipantId])
REFERENCES [dbo].[Participants] ([Id])
GO
ALTER TABLE [dbo].[Submissions] CHECK CONSTRAINT [FK_Submissions_Participants_ParticipantId]
GO
ALTER TABLE [dbo].[Submissions]  WITH CHECK ADD  CONSTRAINT [FK_Submissions_Problems_ProblemId] FOREIGN KEY([ProblemId])
REFERENCES [dbo].[Problems] ([Id])
GO
ALTER TABLE [dbo].[Submissions] CHECK CONSTRAINT [FK_Submissions_Problems_ProblemId]
GO
ALTER TABLE [dbo].[Submissions]  WITH CHECK ADD  CONSTRAINT [FK_Submissions_SubmissionTypes_SubmissionTypeId] FOREIGN KEY([SubmissionTypeId])
REFERENCES [dbo].[SubmissionTypes] ([Id])
GO
ALTER TABLE [dbo].[Submissions] CHECK CONSTRAINT [FK_Submissions_SubmissionTypes_SubmissionTypeId]
GO
ALTER TABLE [dbo].[SubmissionTypeProblems]  WITH CHECK ADD  CONSTRAINT [FK_SubmissionTypeProblems_Problems_ProblemId] FOREIGN KEY([ProblemId])
REFERENCES [dbo].[Problems] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SubmissionTypeProblems] CHECK CONSTRAINT [FK_SubmissionTypeProblems_Problems_ProblemId]
GO
ALTER TABLE [dbo].[SubmissionTypeProblems]  WITH CHECK ADD  CONSTRAINT [FK_SubmissionTypeProblems_SubmissionTypes_SubmissionTypeId] FOREIGN KEY([SubmissionTypeId])
REFERENCES [dbo].[SubmissionTypes] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SubmissionTypeProblems] CHECK CONSTRAINT [FK_SubmissionTypeProblems_SubmissionTypes_SubmissionTypeId]
GO
ALTER TABLE [dbo].[TagProblems]  WITH CHECK ADD  CONSTRAINT [FK_TagProblems_Problems_ProblemId] FOREIGN KEY([ProblemId])
REFERENCES [dbo].[Problems] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TagProblems] CHECK CONSTRAINT [FK_TagProblems_Problems_ProblemId]
GO
ALTER TABLE [dbo].[TagProblems]  WITH CHECK ADD  CONSTRAINT [FK_TagProblems_Tags_TagId] FOREIGN KEY([TagId])
REFERENCES [dbo].[Tags] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TagProblems] CHECK CONSTRAINT [FK_TagProblems_Tags_TagId]
GO
ALTER TABLE [dbo].[TestRuns]  WITH CHECK ADD  CONSTRAINT [FK_TestRuns_Submissions_SubmissionId] FOREIGN KEY([SubmissionId])
REFERENCES [dbo].[Submissions] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TestRuns] CHECK CONSTRAINT [FK_TestRuns_Submissions_SubmissionId]
GO
ALTER TABLE [dbo].[TestRuns]  WITH CHECK ADD  CONSTRAINT [FK_TestRuns_Tests_TestId] FOREIGN KEY([TestId])
REFERENCES [dbo].[Tests] ([Id])
GO
ALTER TABLE [dbo].[TestRuns] CHECK CONSTRAINT [FK_TestRuns_Tests_TestId]
GO
ALTER TABLE [dbo].[Tests]  WITH CHECK ADD  CONSTRAINT [FK_Tests_Problems_ProblemId] FOREIGN KEY([ProblemId])
REFERENCES [dbo].[Problems] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Tests] CHECK CONSTRAINT [FK_Tests_Problems_ProblemId]
GO
ALTER TABLE [dbo].[UsersInExamGroups]  WITH CHECK ADD  CONSTRAINT [FK_UsersInExamGroups_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UsersInExamGroups] CHECK CONSTRAINT [FK_UsersInExamGroups_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[UsersInExamGroups]  WITH CHECK ADD  CONSTRAINT [FK_UsersInExamGroups_ExamGroups_ExamGroupId] FOREIGN KEY([ExamGroupId])
REFERENCES [dbo].[ExamGroups] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UsersInExamGroups] CHECK CONSTRAINT [FK_UsersInExamGroups_ExamGroups_ExamGroupId]
GO
ALTER TABLE [HangFire].[JobParameter]  WITH CHECK ADD  CONSTRAINT [FK_HangFire_JobParameter_Job] FOREIGN KEY([JobId])
REFERENCES [HangFire].[Job] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [HangFire].[JobParameter] CHECK CONSTRAINT [FK_HangFire_JobParameter_Job]
GO
ALTER TABLE [HangFire].[State]  WITH CHECK ADD  CONSTRAINT [FK_HangFire_State_Job] FOREIGN KEY([JobId])
REFERENCES [HangFire].[Job] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [HangFire].[State] CHECK CONSTRAINT [FK_HangFire_State_Job]
GO
USE [master]
GO
ALTER DATABASE [OpenJudgeSystem] SET  READ_WRITE 
GO
