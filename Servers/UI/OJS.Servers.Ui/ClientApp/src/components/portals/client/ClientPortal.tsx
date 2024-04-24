import React from 'react';
import { Route, Routes } from 'react-router-dom';

import MuiUiThemeProvider from '../../../hooks/use-mui-ui-theme';
import useTheme from '../../../hooks/use-theme';
import PageFooter from '../../../layout/footer/PageFooter';
import PageHeader from '../../../layout/header/PageHeader';
import SearchBar from '../../../layout/search-bar/SearchBar';
import ContestEditPage from '../../../pages/administration/ContestEditPage';
import ContestProblemsPage from '../../../pages/administration/ContestProblemsPage';
import SubmissionRetestPage from '../../../pages/administration/SubmissionRetestPage';
import TestEditPage from '../../../pages/administration/TestEditPage';
import LoginPage from '../../../pages/auth/login/LoginPage';
import LogoutPage from '../../../pages/auth/logout/LogoutPage';
import RegisterPage from '../../../pages/auth/register/RegisterPage';
import ContestResultsPage from '../../../pages/contest-results/ContestResultsPage';
import ContestDetailsPage from '../../../pages/contests/contest-details/ContestDetailsPage';
import ContestRegister from '../../../pages/contests/contest-register/ContestRegister';
import ContestSolutionSubmitPage from '../../../pages/contests/contest-solution-submit/ContestSolutionSubmitPage';
import ContestsPage from '../../../pages/contests/ContestsPage';
import HomePage from '../../../pages/home/HomePage';
import NotFoundPage from '../../../pages/not-found/NotFoundPage';
import ProfilePage from '../../../pages/profile/ProfilePage';
import SearchPage from '../../../pages/search/SearchPage';
import { asPage } from '../../../pages/shared/set-page-params';
import SubmissionDetailsPage from '../../../pages/submissions/submission-details/SubmissionDetailsPage';
import SubmissionsPage from '../../../pages/submissions/SubmissionsPage';

import styles from '../../../layout/content/PageContent.module.scss';

const ClientPortal = () => {
    const routes = [
        // Static Routes
        {
            path: '/',
            Element: HomePage,
        },
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
            path: '/search',
            Element: SearchPage,
        },
        {
            path: '/submissions',
            Element: SubmissionsPage,
            title: 'Submissions',
        },
        {
            title: '/contests',
            path: 'contests',
            Element: ContestsPage,
        },
        // Profile routes
        {
            path: '/profile/:username?',
            Element: ProfilePage,
        },
        // Submissions routes,
        {
            path: '/submissions/:submissionId/details',
            Element: SubmissionDetailsPage,
        },
        {
            path: '/submissions/retest/:submissionId',
            Element: SubmissionRetestPage,
        },
        {
            path: '/tests/edit/:testId',
            Element: TestEditPage,
        },
        // Contest Routes
        {
            path: '/contests/:contestId',
            Element: ContestDetailsPage,
        },
        {
            path: '/contests/register/:contestId/:participationType',
            Element: ContestRegister,
        },
        {
            path: '/contest/problems/:contestId',
            Element: ContestProblemsPage,
        },
        {
            path: '/contest/edit/:contestId',
            Element: ContestEditPage,
        },
        {
            path: '/contests/:contestId/:participationType',
            Element: ContestSolutionSubmitPage,
        },
        {
            path: '/contests/:contestId/:participationType/results/:resultType',
            Element: ContestResultsPage,
        },
        // Catch-All Route
        {
            path: '*',
            Element: NotFoundPage,
        },
    ];

    const { themeColors, getColorClassName } = useTheme();
    const backgroundColorClassName = getColorClassName(themeColors.baseColor400);

    return (
        <MuiUiThemeProvider>
            <PageHeader />
            <SearchBar />
            <main className={`${styles.main} ${backgroundColorClassName}`}>
                <Routes>
                    {routes.map(({ path, Element }) => <Route key={path} path={path} element={React.createElement(asPage(Element))} />)}
                </Routes>
            </main>
            <PageFooter />
        </MuiUiThemeProvider>

    );
};
export default ClientPortal;
