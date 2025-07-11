import { createSlice } from '@reduxjs/toolkit';

import { ThemeMode } from '../../common/enums';

interface IThemeState {
    mode: ThemeMode;
}

const initialState: IThemeState = { mode: ThemeMode.Dark };


export const themeSlice = createSlice({
    name: 'theme',
    initialState,
    reducers: {
        toggleTheme: (state) => {

            // eslint-disable-next-line @typescript-eslint/no-unused-expressions
            state.mode === ThemeMode.Light

                ? state.mode = ThemeMode.Dark

                : state.mode = ThemeMode.Light;
        },
    },
});


export const { toggleTheme } = themeSlice.actions;

export default themeSlice.reducer;
