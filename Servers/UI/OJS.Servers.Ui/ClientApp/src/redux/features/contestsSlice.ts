/* eslint-disable no-param-reassign */
/* eslint-disable prefer-destructuring */
/* eslint-disable import/group-exports */

import { createSlice, PayloadAction } from '@reduxjs/toolkit';

import { ContestBreadcrumb, IContestStrategyFilter } from '../../common/contest-types';
import {
    IContestCategory,
    IContestDetailsResponseType,
    IContestDetailsSliceType,
    IIndexContestsType, IPagedResultType,
    IProblemType,
} from '../../common/types';

interface IContestState {
    contests: IPagedResultType<IIndexContestsType> | null;
    contestsCacheIsReset: boolean;
    selectedCategory: IContestCategory | null;
    selectedStrategy: IContestStrategyFilter | null;
    breadcrumbItems: Array<ContestBreadcrumb>;
    contestDetails: IContestDetailsSliceType | null;
    contestCategories: Array<IContestCategory>;
    selectedContestDetailsProblem: IProblemType | null;
}

const initialState: IContestState = {
    contests: null,
    contestsCacheIsReset: false,
    selectedCategory: null,
    selectedStrategy: null,
    breadcrumbItems: [],
    contestDetails: null,
    contestCategories: [],
    selectedContestDetailsProblem: null,
};

// eslint-disable-next-line import/group-exports
export const contestSlice = createSlice({
    name: 'contests',
    initialState,
    reducers: {
        setContests: (state, action: PayloadAction<IPagedResultType<IIndexContestsType> | null>) => {
            state.contests = action.payload;
        },
        setContestsCacheIsReset: (state, action: PayloadAction<boolean>) => {
            state.contestsCacheIsReset = action.payload;
        },
        setContestCategory: (state, action: PayloadAction<IContestCategory | null>) => {
            state.selectedCategory = action.payload;
        },
        setContestStrategy: (state, action: PayloadAction<IContestStrategyFilter | null>) => {
            state.selectedStrategy = action.payload;
        },
        updateContestCategoryBreadcrumbItem: (state, action: PayloadAction<{ elements: Array<ContestBreadcrumb> | undefined}>) => {
            const { elements } = action.payload;
            if (!elements) {
                return;
            }
            state.breadcrumbItems = [ ...elements ];
        },
        clearContestCategoryBreadcrumbItems: (state) => {
            state.breadcrumbItems = [];
        },
        setContestDetailsIdAndCategoryId: (state, action: PayloadAction<{
            id: number; name: string; categoryId: number; isOnlineExam?: boolean;
        }>) => {
            const { id, name, categoryId, isOnlineExam } = action.payload;
            state.contestDetails = { id, name, categoryId, isOnlineExam };
        },
        setSelectedContestDetailsProblem: (state, action: PayloadAction<{ selectedProblem: IProblemType | null }>) => {
            const { selectedProblem } = action.payload;
            state.selectedContestDetailsProblem = selectedProblem;
        },
        setContestDetails: (state, action: PayloadAction<{ contest: IContestDetailsResponseType | null }>) => {
            const { contest } = action.payload;
            state.contestDetails = contest;
        },
        setContestCategories: (state, action: PayloadAction<{ contestCategories: Array<IContestCategory> }>) => {
            const { contestCategories } = action.payload;
            state.contestCategories = contestCategories;
        },
    },
});

export const {
    setContests,
    setContestsCacheIsReset,
    setContestDetails,
    setContestCategory,
    setContestStrategy,
    updateContestCategoryBreadcrumbItem,
    clearContestCategoryBreadcrumbItems,
    setSelectedContestDetailsProblem,
    setContestCategories,
    setContestDetailsIdAndCategoryId,
} = contestSlice.actions;

export default contestSlice.reducer;
