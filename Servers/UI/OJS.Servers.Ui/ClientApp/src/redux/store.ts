import { combineReducers, configureStore } from '@reduxjs/toolkit';
// eslint-disable-next-line import/no-extraneous-dependencies
import { persistReducer, persistStore } from 'redux-persist';
// eslint-disable-next-line import/no-extraneous-dependencies
import storage from 'redux-persist/lib/storage';

// features
import { contestCategoriesAdminSlice } from './features/admin/contestCategoriesAdminSlice';
import { contestsAdminSlice } from './features/admin/contestsAdminSlice';
import { problemGroupsAdminSlice } from './features/admin/problemGroupsSlice';
import { problemsAdminSlice } from './features/admin/problemsAdminSlice';
import { submissionsAdminSlice } from './features/admin/submissionsAdminSlice';
import { submissionsForProcessingAdminSlice } from './features/admin/submissionsForProcessingAdminSlice';
import { authorizationSlide } from './features/authorizationSlice';
import { submissionDetailsSlice } from './features/submissionDetailsSlice';
import checkerAdminService from './services/admin/checkersAdminService';
import contestCategoriesAdminService from './services/admin/contestCategoriesAdminService';
// services
import contestsAdminService from './services/admin/contestsAdminService';
import participantsAdminService from './services/admin/participantsAdminService';
import problemGroupsAdminService from './services/admin/problemGroupsAdminService';
// services
import problemsAdminService from './services/admin/problemsAdminService';
import submissionsAdminService from './services/admin/submissionsAdminService';
import SubmissionsForProcessingAdminService from './services/admin/submissionsForProcessingAdminService';
import submissionTypesAdminService from './services/admin/submissionTypesAdminService';
// features
import authorizationService from './services/authorizationService';
import submissionDetailsService from './services/submissionDetailsService';

const rootReducer = combineReducers({
    // reducers
    [submissionDetailsSlice.name]: submissionDetailsSlice.reducer,
    [authorizationSlide.name]: authorizationSlide.reducer,
    [contestsAdminSlice.name]: contestsAdminSlice.reducer,
    [submissionsAdminSlice.name]: submissionsAdminSlice.reducer,
    [submissionsForProcessingAdminSlice.name]: submissionsForProcessingAdminSlice.reducer,
    [problemsAdminSlice.name]: problemsAdminSlice.reducer,
    [problemGroupsAdminSlice.name]: problemGroupsAdminSlice.reducer,
    [contestCategoriesAdminSlice.name]: contestCategoriesAdminSlice.reducer,

    // services
    [submissionDetailsService.reducerPath]: submissionDetailsService.reducer,
    [authorizationService.reducerPath]: authorizationService.reducer,

    [contestsAdminService.reducerPath]: contestsAdminService.reducer,
    [submissionsAdminService.reducerPath]: submissionsAdminService.reducer,
    [SubmissionsForProcessingAdminService.reducerPath]: SubmissionsForProcessingAdminService.reducer,
    [participantsAdminService.reducerPath]: participantsAdminService.reducer,
    [problemsAdminService.reducerPath]: problemsAdminService.reducer,
    [contestCategoriesAdminService.reducerPath]: contestCategoriesAdminService.reducer,
    [submissionTypesAdminService.reducerPath]: submissionTypesAdminService.reducer,
    [problemGroupsAdminService.reducerPath]: problemGroupsAdminService.reducer,
    [checkerAdminService.reducerPath]: checkerAdminService.reducer,
});

const persistConfig = (reducersToPersist: string[]) => ({
    key: 'root',
    storage,
    whitelist: reducersToPersist,
});

// list reducers with data to be persisted here
const reducersToPersist = [
    contestsAdminSlice.name,
    authorizationSlide.name,
    problemsAdminSlice.name,
    problemGroupsAdminSlice.name,
    contestCategoriesAdminSlice.name,
];

const persistRootReducer = persistReducer(persistConfig([ ...reducersToPersist ]), rootReducer);

const store = configureStore({
    reducer: persistRootReducer,
    middleware: (getDefaultMiddleware) => getDefaultMiddleware({ serializableCheck: false }).concat([
        submissionDetailsService.middleware,
        contestsAdminService.middleware,
        participantsAdminService.middleware,
        problemGroupsAdminService.middleware,
        contestCategoriesAdminService.middleware,
        authorizationService.middleware,
        problemsAdminService.middleware,
        submissionsAdminService.middleware,
        submissionsForProcessingAdminService.middleware,
        submissionTypesAdminService.middleware,
        checkerAdminService.middleware,
    ]),
});

export const persistor = persistStore(store);

export default store;
