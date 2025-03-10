/* eslint-disable import/exports-last */
/* eslint-disable no-param-reassign */
/* eslint-disable prefer-destructuring */
/* eslint-disable import/group-exports */
import { createSlice, PayloadAction } from '@reduxjs/toolkit';

import { IUserType } from '../../common/types';

export interface IAuthorizationReduxState {
    internalUser: IUserType;
    defaultLoginErrorMessage: string;
    isLoggedIn: boolean;
    isGetUserInfoCompleted: boolean;
}

const initialState: IAuthorizationReduxState = {
    internalUser: {
        id: '',
        userName: '',
        email: '',
        permissions: { canAccessAdministration: false },
        isInRole: false,
        isAdmin: false,
        canAccessAdministration: false,
    } as IUserType,
    defaultLoginErrorMessage: 'Invalid username or password',
    isLoggedIn: false,
    isGetUserInfoCompleted: false,
};

export const authorizationSlice = createSlice({
    name: 'authorization',
    initialState,
    reducers: {
        setInternalUser: (state, action) => {
            state.internalUser = action.payload;
        },
        setIsGetUserInfoCompleted: (state, action) => {
            state.isGetUserInfoCompleted = action.payload;
        },
        setIsLoggedIn: (state, action: PayloadAction<boolean>) => {
            state.isLoggedIn = action.payload;
            state.internalUser.canAccessAdministration = state.isLoggedIn && (state.internalUser.isAdmin || state.internalUser.isLecturer);
        },
        resetInInternalUser: (state) => {
            state.internalUser = initialState.internalUser;
        },
    },
});

export const {
    setInternalUser,
    setIsLoggedIn,
    setIsGetUserInfoCompleted,
    resetInInternalUser,
} = authorizationSlice.actions;

export const authorizationSliceName = authorizationSlice.name;

export default authorizationSlice.reducer;
