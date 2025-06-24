import { Provider } from 'react-redux';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { HelmetProvider } from '@dr.pogodin/react-helmet';
import { PersistGate } from 'redux-persist/integration/react';

import ClientPortal from './components/portals/client/ClientPortal';
import SubmissionRetestPage from './pages/administration/SubmissionRetestPage';
import LoginPage from './pages/auth/login/LoginPage';
import LogoutPage from './pages/auth/logout/LogoutPage';
import RegisterPage from './pages/auth/register/RegisterPage';
import ContestResultsPage from './pages/contest-results/ContestResultsPage';
import ContestDetailsPage from './pages/contests/contest-details/ContestDetailsPage';
import ContestRegister from './pages/contests/contest-register/ContestRegister';
import ContestSolutionSubmitPage from './pages/contests/contest-solution-submit/ContestSolutionSubmitPage';
import ContestsPage from './pages/contests/ContestsPage';
import ErrorPage from './pages/error/ErrorPage';
import HomeInfoPage from './pages/home/HomeInfoPage';
import HomePage from './pages/home/HomePage';
import NotFoundPage from './pages/not-found/NotFoundPage';
import ProfilePage from './pages/profile/ProfilePage';
import SearchPage from './pages/search/SearchPage';
import SubmissionDetailsPage from './pages/submissions/submission-details/SubmissionDetailsPage';
import SubmissionsPage from './pages/submissions/SubmissionsPage';
import store, { persistor } from './redux/store';
import {
    validateIntegerParam,
    validateParticipationType,
    validateResultType,
} from './utils/router-utils';

const App = () => {
    const router = createBrowserRouter([
        // Static Routes
        {
            path: '/',
            element: <ClientPortal />,
            children: [
                {
                    path: '/',
                    element: <HomePage />,
                    errorElement: <ErrorPage />,
                    children: [
                        {
                            path: '/',
                            element: <HomeInfoPage />,
                        },
                        {
                            path: 'contests',
                            element: <ContestsPage />,
                        },
                        {
                            path: 'contests/by-category/:slug/:categoryId',
                            element: <ContestsPage />,
                            loader: ({ params }) => {
                                validateIntegerParam(params.categoryId);
                                return null;
                            },
                        },
                        {
                            path: 'contests/by-category/:categoryId', // without slug (have to be last)
                            element: <ContestsPage />,
                            loader: ({ params }) => {
                                validateIntegerParam(params.categoryId);
                                return null;
                            },
                        },
                    ],
                },
                {
                    path: 'login',
                    element: <LoginPage />,
                },
                {
                    path: 'register',
                    element: <RegisterPage />,
                },
                {
                    path: 'logout',
                    element: <LogoutPage />,
                },
                {
                    path: 'search',
                    element: <SearchPage />,
                },
                {
                    path: 'submissions',
                    element: <SubmissionsPage />,
                },
                // Profile routes
                {
                    path: 'profile/:username?',
                    element: <ProfilePage />,
                },
                // Submissions routes,
                {
                    path: 'submissions/:submissionId/details',
                    element: <SubmissionDetailsPage />,
                    errorElement: <ErrorPage />,
                    loader: ({ params }) => {
                        validateIntegerParam(params.submissionId);
                        return null;
                    },
                },
                {
                    path: 'submissions/retest/:submissionId',
                    element: <SubmissionRetestPage />,
                    errorElement: <ErrorPage />,
                    loader: ({ params }) => {
                        validateIntegerParam(params.submissionId);
                        return null;
                    },
                },
                // Contest Routes
                {
                    path: 'contests/:slug/:contestId',
                    element: <ContestDetailsPage />,
                    errorElement: <ErrorPage />,
                    loader: ({ params }) => {
                        validateIntegerParam(params.contestId);
                        return null;
                    },
                },
                {
                    path: 'contests/:slug/:contestId/:participationType',
                    element: <ContestSolutionSubmitPage />,
                    errorElement: <ErrorPage />,
                    loader: ({ params }) => {
                        validateIntegerParam(params.contestId);
                        validateParticipationType(params.participationType);
                        return null;
                    },
                },
                {
                    path: 'contests/:slug/:contestId/:participationType/register',
                    element: <ContestRegister />,
                    errorElement: <ErrorPage />,
                    loader: ({ params }) => {
                        validateIntegerParam(params.contestId);
                        validateParticipationType(params.participationType);
                        return null;
                    },
                },
                {
                    path: 'contests/:slug/:contestId/:participationType/results/:resultType',
                    element: <ContestResultsPage />,
                    errorElement: <ErrorPage />,
                    loader: ({ params }) => {
                        validateIntegerParam(params.contestId);
                        validateParticipationType(params.participationType);
                        validateResultType(params.resultType);
                        return null;
                    },
                },
                // Contest Routes without slug (have to be last)
                {
                    path: 'contests/:contestId',
                    element: <ContestDetailsPage />,
                    errorElement: <ErrorPage />,
                    loader: ({ params }) => {
                        validateIntegerParam(params.contestId);
                        return null;
                    },
                },
                // static routes for participationType (compete/practice) without slug,
                // because /contests/:slug/:contestId takes precedence if
                // :participationType is dynamic for these paths
                {
                    path: 'contests/:contestId/compete',
                    element: <ContestSolutionSubmitPage />,
                    errorElement: <ErrorPage />,
                    loader: ({ params }) => {
                        validateIntegerParam(params.contestId);
                        return null;
                    },
                },
                {
                    path: 'contests/:contestId/practice',
                    element: <ContestSolutionSubmitPage />,
                    errorElement: <ErrorPage />,
                    loader: ({ params }) => {
                        validateIntegerParam(params.contestId);
                        return null;
                    },
                },
                {
                    path: 'contests/:contestId/:participationType/register',
                    element: <ContestRegister />,
                    errorElement: <ErrorPage />,
                    loader: ({ params }) => {
                        validateIntegerParam(params.contestId);
                        validateParticipationType(params.participationType);
                        return null;
                    },
                },
                {
                    path: 'contests/:contestId/:participationType/results/:resultType',
                    element: <ContestResultsPage />,
                    errorElement: <ErrorPage />,
                    loader: ({ params }) => {
                        validateIntegerParam(params.contestId);
                        validateParticipationType(params.participationType);
                        validateResultType(params.resultType);
                        return null;
                    },
                },
                // Catch-All Route
                {
                    path: '*',
                    element: <NotFoundPage />,
                },
            ],
        },
        // Catch-All Route
        {
            path: '*',
            element: <NotFoundPage />,
        },
    ]);

    return (
        <Provider store={store}>
            <PersistGate persistor={persistor}>
                <HelmetProvider>
                    <RouterProvider router={router} />
                </HelmetProvider>
            </PersistGate>
        </Provider>
    );
};

export default App;
