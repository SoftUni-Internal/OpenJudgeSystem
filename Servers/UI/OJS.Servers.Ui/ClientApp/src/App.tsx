import React from 'react';
import { Provider } from 'react-redux';
import { BrowserRouter as Router } from 'react-router-dom';
import { PersistGate } from 'redux-persist/integration/react';

import InitProviders, { ProviderType } from './components/common/InitProviders';
import HashUrlParamProvider from './hooks/common/use-hash-url-params';
import RouteUrlParamsProvider from './hooks/common/use-route-url-params';
import UrlParamsProvider from './hooks/common/use-url-params';
import CurrentContestResultsProvider from './hooks/contests/use-current-contest-results';
import ProblemSubmissionsProvider from './hooks/submissions/use-problem-submissions';
import PublicSubmissionsProvider from './hooks/submissions/use-public-submissions';
import SubmissionsProvider from './hooks/submissions/use-submissions';
import ContestCategoriesProvider from './hooks/use-contest-categories';
import CategoriesBreadcrumbProvider from './hooks/use-contest-categories-breadcrumb';
import ContestStrategyFiltersProvider from './hooks/use-contest-strategy-filters';
import CurrentContestsProvider from './hooks/use-current-contest';
import HomeContestsProvider from './hooks/use-home-contests';
import NotificationsProvider from './hooks/use-notifications';
import PageWithTitleProvider from './hooks/use-page-titles';
import PageProvider from './hooks/use-pages';
import ParticipationsProvider from './hooks/use-participations';
import ProblemsProvider from './hooks/use-problems';
import SearchProvider from './hooks/use-search';
import ServicesProvider from './hooks/use-services';
import UsersProvider from './hooks/use-users';
import PageContent from './layout/content/PageContent';
import PageFooter from './layout/footer/PageFooter';
import PageHeader from './layout/header/PageHeader';
import SearchBar from './layout/search-bar/SearchBar';
import store, { persistor } from './redux/store';

import './styles/global.scss';

const App = () => {
    const providers = [
        HashUrlParamProvider,
        UrlParamsProvider,
        RouteUrlParamsProvider,
        ServicesProvider,
        PageProvider,
        NotificationsProvider,
        PageWithTitleProvider,
        UsersProvider,
        ContestCategoriesProvider,
        ContestStrategyFiltersProvider,
        CategoriesBreadcrumbProvider,
        HomeContestsProvider,
        ParticipationsProvider,
        CurrentContestsProvider,
        CurrentContestResultsProvider,
        ProblemSubmissionsProvider,
        ProblemsProvider,
        SubmissionsProvider,
        PublicSubmissionsProvider,
        SearchProvider,
    ] as ProviderType[];

    return (
        <Provider store={store}>
            <PersistGate persistor={persistor}>
                <Router>
                    <InitProviders providers={providers}>
                        <PageHeader />
                        <SearchBar />
                        <PageContent />
                        <PageFooter />
                    </InitProviders>
                </Router>
            </PersistGate>
        </Provider>
    );
};

export default App;
