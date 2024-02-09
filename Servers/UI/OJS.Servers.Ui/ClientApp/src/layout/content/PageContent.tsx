import React, { FC } from 'react';
import { useSelector } from 'react-redux';
import { Route, Routes } from 'react-router-dom';

import { CONTEST_CATEGORIES_PATH, CONTESTS_PATH, NEW_ADMINISTRATION_PATH, PROBLEM_GROUPS_PATH, PROBLEMS_PATH, SUBMISSION_TYPES_PATH, SUBMISSIONS_FOR_PROCESSING_PATH, SUBMISSIONS_PATH, TESTS_PATH } from '../../common/urls';
import AdministrationContestPage from '../../components/administration/Contests/AdministrationContestPage';
import AdministrationProblem from '../../components/administration/Problems/AdministrationProblem';
import AdministrationPage from '../../pages/administration/AdministrationPage';
import ContestEditPage from '../../pages/administration/ContestEditPage';
import ContestProblemsPage from '../../pages/administration/ContestProblemsPage';
import SubmissionRetestPage from '../../pages/administration/SubmissionRetestPage';
import TestEditPage from '../../pages/administration/TestEditPage';
import Administration from '../../pages/administration-new/Administration';
import AdministrationContestCategories
    from '../../pages/administration-new/categoriesContest/AdministrationContestCategories';
import AdministrationContestsPage from '../../pages/administration-new/contests/AdministrationContests';
import AdministrationProblemGroupsPage from '../../pages/administration-new/problemGroups/AdministrationProblemGroupsPage';
import AdministrationProblemsPage from '../../pages/administration-new/problems/AdministrationProblemsPage';
import { AdministrationSubmissionsPage } from '../../pages/administration-new/submissions/AdminSubmissionsGrid';
import AdminSubmissionForProcessingDetails
    from '../../pages/administration-new/submissions-for-processing/AdminSubmissionForProcessingDetails';
import {
    AdministrationSubmissionsForProcessingPage,
} from '../../pages/administration-new/submissions-for-processing/AdminSubmissionsForProcessingGrid';
import ContestDetailsPage from '../../pages/contest/ContestDetailsPage';
import ContestPage from '../../pages/contest/ContestPage';
import ContestResultsPage from '../../pages/contest-results/ContestResultsPage';
import ContestsPage from '../../pages/contests/ContestsPage';
import HomePage from '../../pages/home/HomePage';
import LoginPage from '../../pages/login/LoginPage';
import LogoutPage from '../../pages/logout/LogoutPage';
import NotFoundPage from '../../pages/not-found/NotFoundPage';
import ProfilePage from '../../pages/profile/ProfilePage';
import RegisterPage from '../../pages/register/RegisterPage';
import SearchPage from '../../pages/search/SearchPage';
import withAdministrationNav from '../../pages/shared/set-admin-navigation';
import { asPage } from '../../pages/shared/set-page-params';
import { withTitle } from '../../pages/shared/set-page-title';
import SubmissionDetailsPage from '../../pages/submission-details/SubmissionDetailsPage';
import SubmissionsPage from '../../pages/submissions/SubmissionsPage';
import { IAuthorizationReduxState } from '../../redux/features/authorizationSlice';

import styles from './PageContent.module.scss';

const routes = [
    {
        path: '/login',
        Element: LoginPage,
        title: 'Login',
    },
    {
        path: '/register',
        Element: RegisterPage,
    },
    {
        path: '/logout',
        Element: LogoutPage,
    },
    {
        path: '/',
        Element: HomePage,
    },
    {
        path: '/profile',
        Element: ProfilePage,
    },
    {
        path: '/profile/:username',
        Element: ProfilePage,
    },
    {
        path: '/submissions',
        Element: SubmissionsPage,
        title: 'Submissions',
    },
    {
        path: '/submissions/:submissionId/details',
        Element: SubmissionDetailsPage,
    },
    {
        title: 'Contests',
        path: '/contests',
        Element: ContestsPage,
    },
    {
        path: '/contests/:contestId/:participationType',
        Element: ContestPage,
    },
    {
        path: '/contests/:contestId',
        Element: ContestDetailsPage,
    },
    {
        path: '/contests/:contestId/:participationType/results/:resultType',
        Element: ContestResultsPage,
    },
    {
        path: '/Submissions/Retest/:submissionId',
        Element: SubmissionRetestPage,
    },
    {
        path: '/Contest/Problems/:contestId',
        Element: ContestProblemsPage,
    },
    {
        path: '/Contest/Edit/:contestId',
        Element: ContestEditPage,
    },
    {
        path: '/Tests/Edit/:testId',
        Element: TestEditPage,
    },
    {
        path: '/search',
        Element: SearchPage,
    },
    {
        path: '*',
        Element: NotFoundPage,
    },
];

const adminRoutes = [
    {
        path: `/${NEW_ADMINISTRATION_PATH}`,
        Element: AdministrationContestsPage,
        title: 'Administration',
    },
    {
        path: `/${NEW_ADMINISTRATION_PATH}/${CONTESTS_PATH}`,
        Element: AdministrationContestsPage,
    },
    {
        path: `/${NEW_ADMINISTRATION_PATH}/${CONTESTS_PATH}/:id`,
        Element: AdministrationContestPage,
    },
    {
        path: `/${NEW_ADMINISTRATION_PATH}/${CONTEST_CATEGORIES_PATH}`,
        Element: AdministrationContestCategories,
    },
    {
        path: `/${NEW_ADMINISTRATION_PATH}/${SUBMISSIONS_PATH}`,
        Element: AdministrationSubmissionsPage,
    },
    {
        path: `/${NEW_ADMINISTRATION_PATH}/${SUBMISSIONS_FOR_PROCESSING_PATH}`,
        Element: AdministrationSubmissionsForProcessingPage,
    },
    {
        path: `/${NEW_ADMINISTRATION_PATH}/${SUBMISSIONS_FOR_PROCESSING_PATH}/:id`,
        Element: AdminSubmissionForProcessingDetails,
    },
    {
        path: `/${NEW_ADMINISTRATION_PATH}/${TESTS_PATH}`,
        Element: Administration,
    },
    {
        path: `/${NEW_ADMINISTRATION_PATH}/${PROBLEMS_PATH}`,
        Element: AdministrationProblemsPage,
    },
    {
        path: `/${NEW_ADMINISTRATION_PATH}/${PROBLEMS_PATH}/:id`,
        Element: AdministrationProblem,
    },
    {
        path: `/${NEW_ADMINISTRATION_PATH}/${PROBLEM_GROUPS_PATH}`,
        Element: AdministrationProblemGroupsPage,
    },
    {
        path: `/${NEW_ADMINISTRATION_PATH}/${SUBMISSION_TYPES_PATH}`,
        Element: Administration,
    },
    {
        path: '/administration',
        Element: AdministrationPage,
        title: 'Administration',
    },
];

const PageContent = () => {
    const { internalUser: user } =
    useSelector((state: {authorization: IAuthorizationReduxState}) => state.authorization);

    const renderRoute = (path: string, Element: FC, title: string | undefined, isAdminRoute: boolean) => {
        let WrappedElement = asPage(withTitle(Element, title));
        if (isAdminRoute) {
            WrappedElement = withAdministrationNav(Element);
        }
        return (
            <Route key={path} path={path} element={<WrappedElement />} />
        );
    };

    return (
        <main className={styles.main}>
            <Routes>
                {routes.map(({ path, Element, title }) => renderRoute(path, Element, title, false))}
                {user.canAccessAdministration && adminRoutes.map(({ path, Element, title }) => renderRoute(path, Element, title, true))}
            </Routes>
        </main>
    );
};

export default PageContent;
