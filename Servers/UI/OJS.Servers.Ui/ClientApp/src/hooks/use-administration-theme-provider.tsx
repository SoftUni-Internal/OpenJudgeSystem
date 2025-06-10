import { createContext, FC, ReactNode, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { ThemeProvider } from '@emotion/react';
import { createTheme } from '@mui/material/styles';

import { ThemeMode } from '../common/enums';

interface IAdministrationThemeProviderProps {
    children: ReactNode;
}

interface IThemeContext {
    themeMode: ThemeMode;
    toggleThemeMode: (newThemeMode: ThemeMode) => void;
}

const ThemeContext = createContext<IThemeContext | undefined>(undefined);

const getColors = (mode: ThemeMode) => ({
    palette: {
        primary: { main: '#44a9f8' },
        secondary: {
            main: mode === ThemeMode.Light
                ? '#ffffff'
                : '#333333',
        },
        deleted: mode === ThemeMode.Light
            ? '#FF7A7A'
            : '#e04545',
        visible: mode === ThemeMode.Light
            ? '#c0c0c0'
            : '#636363',
    },
    textColors: {
        primary: mode === ThemeMode.Light
            ? '#14181c'
            : '#f3f1f1',
    },
    background: {
        default: mode === ThemeMode.Light
            ? '#F9F9F9'
            : '#212328',
    },
});

const isDeletedClassName = 'is-deleted-record';
const isVisibleClassName = 'is-visible-record';

const AdministrationThemeProvider: FC<IAdministrationThemeProviderProps> = ({ children }) => {
    const [ themeMode, setThemeMode ] = useState<ThemeMode>(ThemeMode.Dark);

    useEffect(() => {
        const storedMode = localStorage.getItem('administrationMode') as ThemeMode;
        if (storedMode) {
            setThemeMode(storedMode);
        }
    }, []);

    useEffect(() => {
        localStorage.setItem('administrationMode', themeMode);
    }, [ themeMode ]);

    const toggleThemeMode = useCallback((newThemeMode: ThemeMode) => {
        setThemeMode(newThemeMode);
    }, []);

    const contextValue = useMemo(() => ({ themeMode, toggleThemeMode }), [ themeMode, toggleThemeMode ]);

    const drawerProps = {
        backgroundColor: getColors(themeMode).palette.secondary.main,
        color: getColors(themeMode).textColors.primary,
        borderBottomRightRadius: '16px',
        borderTopRightRadius: '16px',
        height: '90vh',
        margin: '2rem 0',
        '& *': { color: 'inherit' },
        paddingBottom: '0.3rem',
    };

    const theme = createTheme({
        palette: {
            mode: themeMode,
            text: { primary: getColors(themeMode).textColors.primary },
        },
        components: {
            MuiDrawer: {
                styleOverrides: {
                    paper: drawerProps,
                    docked: {
                        ...drawerProps,
                        boxShadow: '3px 0 22px -4px rgb(22 0 0 / 75%)',
                    },
                },
            },
            MuiDataGrid: {
                styleOverrides: {
                    root: {
                        borderRadius: '14px',
                        borderWidth: '1px',
                        backgroundColor: getColors(themeMode).background.default,
                        '& .MuiDataGrid-cell': { backgroundColor: getColors(themeMode).background.default },
                        '& .MuiDataGrid-row': { backgroundColor: getColors(themeMode).background.default },
                        '& .MuiDataGrid-columnHeaders': {
                            backgroundColor: getColors(themeMode).background.default,
                            '& .MuiDataGrid-columnHeader': { backgroundColor: getColors(themeMode).background.default },
                            '& .MuiDataGrid-columnHeaderTitle': { color: getColors(themeMode).textColors.primary },
                        },
                    },
                },
            },
            MuiButton: { styleOverrides: { root: { borderRadius: 8 } } },
            MuiCssBaseline: {
                styleOverrides: {
                    body: {
                        backgroundColor: `${getColors(themeMode).background.default} !important`,
                        color: getColors(themeMode).textColors.primary,
                        '.box-wrapper': { backgroundColor: getColors(themeMode).background.default },
                    },
                    a: {
                        color: 'inherit',
                        textDecoration: 'none',
                    },
                    '.is-deleted-record': {
                        backgroundColor: `${getColors(themeMode).palette.deleted} !important`,
                        '&:hover': {
                            backgroundColor: `${
                                themeMode === ThemeMode.Light
                                    ? '#e04545'
                                    : '#FF7A7A'
                            } !important`,
                        },
                    },
                    '.is-visible-record': { backgroundColor: getColors(themeMode).palette.visible },
                    '::-webkit-scrollbar': {
                        width: '10px',
                        backgroundColor: themeMode === ThemeMode.Light
                            ? '#F0F0F0'
                            : '#2C2C2C',
                    },
                    '::-webkit-scrollbar-track': {
                        boxShadow: 'inset 0 0 5px grey',
                        borderRadius: '10px',
                    },
                    '::-webkit-scrollbar-thumb': {
                        background: themeMode === ThemeMode.Light
                            ? '#7c7c7cbf'
                            : '#555',
                        borderRadius: '10px',
                        '&:hover': {
                            background: themeMode === ThemeMode.Light
                                ? '#ccc'
                                : '#777',
                        },
                    },
                },
            },
        },
    });

    return (
        <ThemeContext.Provider value={contextValue}>
            <ThemeProvider theme={theme}>
                {children}
            </ThemeProvider>
        </ThemeContext.Provider>
    );
};

const useAdministrationTheme = () => {
    const context = useContext(ThemeContext);
    if (!context) {
        throw new Error('useAdministrationTheme must be used within a AdministrationThemeProvider');
    }
    return context;
};

export {
    isDeletedClassName,
    isVisibleClassName,
    getColors,
    AdministrationThemeProvider,
    useAdministrationTheme,
};

export default AdministrationThemeProvider;
