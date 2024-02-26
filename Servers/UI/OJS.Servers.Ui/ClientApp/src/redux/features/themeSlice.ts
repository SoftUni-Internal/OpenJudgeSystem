import { createSlice } from '@reduxjs/toolkit';

import { ThemeMode } from '../../common/enums';

interface IThemeState {
    mode: ThemeMode;
}

const initialState: IThemeState = { mode: ThemeMode.Dark };

// eslint-disable-next-line import/group-exports
export const themeSlice = createSlice({
    name: 'theme',
    initialState,
    reducers: {
        toggleTheme: (state) => {
            // eslint-disable-next-line no-unused-expressions
            state.mode === ThemeMode.Light
                // eslint-disable-next-line no-param-reassign,prefer-destructuring
                ? state.mode = ThemeMode.Dark
                // eslint-disable-next-line no-param-reassign,prefer-destructuring
                : state.mode = ThemeMode.Light;
        },
    },
});

// eslint-disable-next-line prefer-destructuring,import/group-exports
export const { toggleTheme } = themeSlice.actions;

export default themeSlice.reducer;
