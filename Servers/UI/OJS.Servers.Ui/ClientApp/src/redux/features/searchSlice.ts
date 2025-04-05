/* eslint-disable simple-import-sort/imports */
import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { CheckboxSearchValues } from '../../common/enums';
import { ISearchSliceState } from '../../common/types';

const initialState: ISearchSliceState = {
    isVisible: false,
    searchValue: '',
    selectedTerms: [
        CheckboxSearchValues.contests,
        CheckboxSearchValues.problems,
        CheckboxSearchValues.users,
    ],
};

const searchSlice = createSlice({
    name: 'search',
    initialState,
    reducers: {
        setIsVisible: (state: ISearchSliceState, action: PayloadAction<boolean>) => {
            state.isVisible = action.payload;
        },
        setSearchValue: (state: ISearchSliceState, action: PayloadAction<string>) => {
            state.searchValue = action.payload;
        },
        setSelectedTerms: (state: ISearchSliceState, action: PayloadAction<Array<CheckboxSearchValues>>) => {
            state.selectedTerms = action.payload;
        },
        setSelectedTermsDefaultValue: (state: ISearchSliceState) => {
            state.selectedTerms = initialState.selectedTerms;
        },
    },
});

export const {
    setIsVisible,
    setSearchValue,
    setSelectedTerms,
} = searchSlice.actions;

export default searchSlice;
