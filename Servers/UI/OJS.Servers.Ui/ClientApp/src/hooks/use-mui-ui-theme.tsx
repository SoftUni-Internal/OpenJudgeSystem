import { FC, ReactNode } from 'react';
import { ThemeProvider } from '@emotion/react';
import { createTheme } from '@mui/material/styles';
import type {} from '@mui/x-data-grid/themeAugmentation';

import useTheme from './use-theme';

interface IMuiUiThemeProviderProps {
    children: ReactNode;
}

const MuiUiThemeProvider: FC<IMuiUiThemeProviderProps> = ({ children }) => {
    const { themeColors } = useTheme();
    const theme = createTheme({
        palette: {
            primary: {
                main: themeColors.baseColor100,
                100: themeColors.baseColor100,
                200: themeColors.baseColor200,
                300: themeColors.baseColor300,
                400: themeColors.baseColor400,
                500: themeColors.baseColor500,
            },
            text: { primary: themeColors.textColor },
        },
        components: {
            MuiMenuItem: {
                styleOverrides: {
                    root: {
                        color: themeColors.textColor,
                        backgroundColor: themeColors.baseColor100,
                        '&:hover': { backgroundColor: themeColors.baseColor200 },
                        '&.Mui-selected': { backgroundColor: themeColors.baseColor300 },
                        '&.Mui-selected:hover': { backgroundColor: themeColors.baseColor400 },
                    },
                },
            },
            MuiSelect: {
                styleOverrides: {
                    root: { border: '2px solid #44a9f8' }, // Need to extract color in the theme for this
                    icon: { root: { fill: themeColors.textColor } },

                },

            },
            MuiOutlinedInput: { styleOverrides: { root: { '&.Mui-focused .MuiOutlinedInput-notchedOutline': { borderWidth: 0 } } } },
            MuiPagination: {
                styleOverrides: {
                    ul: {
                        '& .Mui-selected': { backgroundColor: '#44a9f8' },
                        '& .MuiPaginationItem-root': {
                            color: themeColors.textColor,
                            borderRadius: '10%',
                            '&:hover': {
                                backgroundColor: '#44a9f8', //  backgroundColor: mode === 'dark' ? '#44a9f8' : '#cbcbcb',
                                color: themeColors.textColor,
                            },
                        },
                        '& .MuiPaginationItem-icon': { color: themeColors.textColor },
                    },
                },
            },
            MuiToggleButton: {
                styleOverrides: {
                    root: {
                        width: 50,
                        backgroundColor: 'white',
                        transition: 'background-color 0.1s ease-in-out',
                        '&.Mui-selected': {
                            backgroundColor: '#42a8f8',
                            color: 'white',
                        },
                        '&.Mui-selected:hover': { backgroundColor: '#e0457a' },
                    },
                },
            },
            MuiDataGrid: {
                styleOverrides: {
                    columnHeaderTitle: {
                        whiteSpace: 'break-spaces',
                        lineHeight: 1,
                        textAlign: 'center',
                    },
                },
            },
            MuiModal: { styleOverrides: { root: { '& .MuiBackdrop-root': { backgroundColor: 'transparent' } } } },
        },
    });

    return (
        <ThemeProvider theme={theme}>
            {children}
        </ThemeProvider>
    );
};
export default MuiUiThemeProvider;
