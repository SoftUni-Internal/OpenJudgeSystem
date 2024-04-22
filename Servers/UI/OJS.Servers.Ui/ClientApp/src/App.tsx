import { Provider } from 'react-redux';
import { BrowserRouter as Router } from 'react-router-dom';
import { PersistGate } from 'redux-persist/integration/react';

import InitProviders, { ProviderType } from './components/common/InitProviders';
import HashUrlParamProvider from './hooks/common/use-hash-url-params';
import RouteUrlParamsProvider from './hooks/common/use-route-url-params';
import UrlParamsProvider from './hooks/common/use-url-params';
import ProblemSubmissionsProvider from './hooks/submissions/use-problem-submissions';
import ProfileSubmissionsProvider from './hooks/submissions/use-profile-submissions';
import SubmissionsProvider from './hooks/submissions/use-submissions';
import ContestCategoriesProvider from './hooks/use-contest-categories';
import CategoriesBreadcrumbProvider from './hooks/use-contest-categories-breadcrumb';
import ContestStrategyFiltersProvider from './hooks/use-contest-strategy-filters';
import CurrentContestsProvider from './hooks/use-current-contest';
import NotificationsProvider from './hooks/use-notifications';
import PageWithTitleProvider from './hooks/use-page-titles';
import PageProvider from './hooks/use-pages';
import ParticipationsProvider from './hooks/use-participations';
import ProblemsProvider from './hooks/use-problems';
import SearchProvider from './hooks/use-search';
import ServicesProvider from './hooks/use-services';
import PageContent from './layout/content/PageContent';
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
        ContestCategoriesProvider,
        ContestStrategyFiltersProvider,
        CategoriesBreadcrumbProvider,
        ParticipationsProvider,
        CurrentContestsProvider,
        ProblemSubmissionsProvider,
        ProblemsProvider,
        SubmissionsProvider,
        ProfileSubmissionsProvider,
        SearchProvider,
    ] as ProviderType[];

    return (
        <Provider store={store}>
            <PersistGate persistor={persistor}>
                <Router>
                    <InitProviders providers={providers}>
                        <PageContent />
                    </InitProviders>
                </Router>
            </PersistGate>
        </Provider>
    );
};

export default App;
