import { createSlice } from '@reduxjs/toolkit';

import { IAdministrationFilter } from '../../../pages/administration-new/administration-filters/AdministrationFilters';
import { IAdministrationSorter } from '../../../pages/administration-new/administration-sorting/AdministrationSorting';

interface IContestsAdminSlice {
    [key: string]: null | {
        selectedFilters: IAdministrationFilter[];
        selectedSorters: IAdministrationSorter[];
    };
}

const initialState: IContestsAdminSlice = {};

// eslint-disable-next-line import/group-exports
export const contestsAdminSlice = createSlice({
    name: 'adminContests',
    initialState,
    reducers: {
        setAdminContestsFilters: (state, action) => {
            // eslint-disable-next-line prefer-destructuring
            const { key, filters } = action.payload;

            if (state[key]) {
                // eslint-disable-next-line no-param-reassign,@typescript-eslint/ban-ts-comment
                // @ts-ignore
                // eslint-disable-next-line no-param-reassign
                state[key].selectedFilters = filters;
            } else {
                // eslint-disable-next-line no-param-reassign
                state[key] = { selectedFilters: filters, selectedSorters: [] };
            }
        },
        setAdminContestsSorters: (state, action) => {
            // eslint-disable-next-line prefer-destructuring
            const { key, sorters } = action.payload;

            if (state[key]) {
                // eslint-disable-next-line no-param-reassign,@typescript-eslint/ban-ts-comment
                // @ts-ignore
                // eslint-disable-next-line no-param-reassign
                state[key].selectedSorters = sorters;
            } else {
                // eslint-disable-next-line no-param-reassign
                state[key] = { selectedFilters: [], selectedSorters: sorters };
            }
        },
    },
});

// eslint-disable-next-line prefer-destructuring,import/group-exports
export const {
    setAdminContestsFilters,
    setAdminContestsSorters,
} = contestsAdminSlice.actions;

export default contestsAdminSlice.reducer;