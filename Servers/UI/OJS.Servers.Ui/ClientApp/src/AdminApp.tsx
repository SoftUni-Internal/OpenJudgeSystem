import { Provider } from 'react-redux';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import { PersistGate } from 'redux-persist/integration/react';
import AdministrationThemeProvider from 'src/hooks/use-administration-theme-provider';

import { NEW_ADMINISTRATION_PATH } from './common/urls/administration-urls';
import AdministrationPortal from './components/portals/administration/AdministrationPortal';
import NotFoundPage from './pages/not-found/NotFoundPage';
import store, { persistor, useAppSelector } from './redux/store';

const AdminRoutes = () => {
    const { internalUser: user } = useAppSelector((state) => state.authorization);

    if (!user.canAccessAdministration) {
        return (
            <NotFoundPage />
        );
    }

    return (
        <AdministrationThemeProvider>
            <Routes>
                <Route
                  path={`/${NEW_ADMINISTRATION_PATH}/*`}
                  element={<AdministrationPortal />}
                />
            </Routes>
        </AdministrationThemeProvider>
    );
};

const AdminApp = () => 
    <Provider store={store}>
        <PersistGate persistor={persistor}>
            <Router>
                <AdminRoutes />
            </Router>
        </PersistGate>
    </Provider>
;

export default AdminApp;
