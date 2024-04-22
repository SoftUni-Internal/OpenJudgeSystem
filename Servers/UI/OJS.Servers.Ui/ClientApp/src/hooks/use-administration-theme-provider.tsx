import { FC, ReactNode } from 'react';
import { ThemeProvider } from '@emotion/react';
import { createTheme } from '@mui/material/styles';

import { ThemeMode } from '../common/enums';

interface IAdministrationThemeProviderProps {
    children: ReactNode;
    mode: ThemeMode;
}

const getColors = (mode: ThemeMode) => ({
    palette: {
        primary: { main: '#44a9f8' },
        secondary: {
            main: mode === ThemeMode.Light
                ? '#ffffff'
                : '#333333',
        },
        deleted: mode === ThemeMode.Light
            ? '#FFA1A1'
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

const AdministrationThemeProvider: FC<IAdministrationThemeProviderProps> = (props: IAdministrationThemeProviderProps) => {
    const { children, mode } = props;

    const drawerProps = {
        backgroundColor: getColors(mode).palette.secondary.main,
        color: getColors(mode).textColors.primary,
        borderBottomRightRadius: '16px',
        borderTopRightRadius: '16px',
        height: '90vh',
        margin: '2rem 0',
        '& *': { color: 'inherit' },
        paddingBottom: '0.3rem',
    };

    const theme = createTheme({
        palette: {
            mode,
            text: { primary: getColors(mode).textColors.primary },
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
            MuiDataGrid: { styleOverrides: { root: { borderRadius: '14px', borderWidth: '1px' } } },
            MuiButton: { styleOverrides: { root: { borderRadius: 8 } } },
            MuiCssBaseline: {
                styleOverrides:
                {
                    body: {
                        backgroundColor: getColors(mode).background.default,
                        color: getColors(mode).textColors.primary,
                        '.box-wrapper': { backgroundColor: getColors(mode).background.default },
                    },
                    a: {
                        color: 'inherit',
                        textDecoration: 'none',
                    },
                    '.is-deleted-record': { backgroundColor: getColors(mode).palette.deleted },
                    '.is-visible-record': { backgroundColor: getColors(mode).palette.visible },
                    '::-webkit-scrollbar': {
                        width: '10px',
                        backgroundColor: mode === ThemeMode.Light
                            ? '#F0F0F0'
                            : '#2C2C2C',
                    },
                    '::-webkit-scrollbar-track': {
                        boxShadow: 'inset 0 0 5px grey',
                        borderRadius: '10px',
                    },
                    '::-webkit-scrollbar-thumb': {
                        background: mode === ThemeMode.Light
                            ? '#7c7c7cbf '
                            : '#555',
                        borderRadius: '10px',
                        '&:hover': {
                            background: mode === ThemeMode.Light
                                ? '#ccc'
                                : '#777',
                        },
                    },
                },
            },
        },
    });

    return (
        <ThemeProvider theme={theme}>
            {children}
        </ThemeProvider>
    );
};

export {
    isDeletedClassName,
    isVisibleClassName,
    getColors,
};
export default AdministrationThemeProvider;
