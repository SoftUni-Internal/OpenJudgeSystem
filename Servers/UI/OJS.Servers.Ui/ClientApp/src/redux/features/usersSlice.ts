import { createSlice, PayloadAction } from '@reduxjs/toolkit';

import { usersSliceName } from '../../common/reduxNames';
import { IUserProfileType, IUsersState } from '../../common/types';

const initialState: IUsersState = { profile: null };

const usersSlice = createSlice({
    name: usersSliceName,
    initialState,
    reducers: {
        setProfile: (state: IUsersState, action: PayloadAction<IUserProfileType | null>) => {
             
            state.profile = action.payload;
        },
    },
});

const { setProfile } = usersSlice.actions;

export {
    setProfile,
};

export default usersSlice;
