import React, { useCallback, useEffect } from 'react';
import { Outlet, ScrollRestoration } from 'react-router-dom';
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterMoment } from '@mui/x-date-pickers/AdapterMoment';
import usePageViewTracking from 'src/hooks/use-page-view-tracking';

import MuiUiThemeProvider from '../../../hooks/use-mui-ui-theme';
import useTheme from '../../../hooks/use-theme';
import PageFooter from '../../../layout/footer/PageFooter';
import PageHeader from '../../../layout/header/PageHeader';
import SearchBar from '../../search/search-bar/SearchBar';

import styles from './ClientPortal.module.scss';

const ClientPortal = () => {
    const { themeColors, getColorClassName } = useTheme();
    const backgroundColorClassName = getColorClassName(themeColors.baseColor400);

    const { isDarkMode } = useTheme(); // Get the current theme mode

    const toggleGlobalStyles = useCallback(() => {
        const rootElement = document.documentElement; // This refers to the `html` element
        const darkScrollbarClass = 'darkScrollbar';
        const lightScrollbarClass = 'lightScrollbar';

        // Inputs autofill
        const darkAutofillClass = 'darkAutofill';
        const lightAutofillClass = 'lightAutofill';

        if (isDarkMode) {
            rootElement.classList.add(darkScrollbarClass);
            rootElement.classList.remove(lightScrollbarClass);

            rootElement.classList.add(darkAutofillClass);
            rootElement.classList.remove(lightAutofillClass);
        } else {
            rootElement.classList.add(lightScrollbarClass);
            rootElement.classList.remove(darkScrollbarClass);

            rootElement.classList.add(lightAutofillClass);
            rootElement.classList.remove(darkAutofillClass);
        }

        // Cleanup in case the effect is run multiple times (e.g., toggling theme back and forth)
        return () => {
            rootElement.classList.remove(darkScrollbarClass, lightScrollbarClass);
            rootElement.classList.remove(darkAutofillClass, lightAutofillClass);
        };
    }, [ isDarkMode ]);

    usePageViewTracking();

    useEffect(() => {
        toggleGlobalStyles();
    }, [ isDarkMode, toggleGlobalStyles ]);

    return (
        <MuiUiThemeProvider>
            <LocalizationProvider dateAdapter={AdapterMoment} adapterLocale="bg">
                <PageHeader />
                <SearchBar />
                <main className={`${styles.main} ${backgroundColorClassName}`}>
                    <Outlet />
                </main>
                <PageFooter />
                <ScrollRestoration />
            </LocalizationProvider>
        </MuiUiThemeProvider>
    );
};

export default ClientPortal;
