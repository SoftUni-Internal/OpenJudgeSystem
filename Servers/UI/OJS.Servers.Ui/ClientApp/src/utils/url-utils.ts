/* eslint-disable max-len */
import { Dispatch, SetStateAction, useEffect } from 'react';
import { NavigateOptions, URLSearchParamsInit, useSearchParams } from 'react-router-dom';
import { ActionCreatorWithPayload } from '@reduxjs/toolkit';
import { IGetSubmissionsUrlParams } from 'src/common/url-types';
import { useAppDispatch } from 'src/redux/store';

type SetQueryParams = Dispatch<SetStateAction<IGetSubmissionsUrlParams>>;

const pageParam = 'page';
const defaultPageParamValue = '1';

/**
 * Syncs query parameters from the URL to a local React state.
 * Automatically extracts `page`, `filter`, and `sorting` params and sets them to the given state setter.
 * The param 'itemsPerPage` has not been added, otherwise if the user changes it from the url, the value will be
 * sent to the backend and a lot more data could be fetched, increasing the database load.
 *
 * NOTE: If these filters are made available for regular users, the change of `filter` and `sorting` through the url should be revised.
 *
 * @param searchParams - Current URLSearchParams from React Router.
 * @param setQueryParams - React state setter for the query parameters.
 */
const useSyncQueryParamsFromUrl = (
    searchParams: URLSearchParams,
    setQueryParams: SetQueryParams,
): void => {
    useEffect(() => {
        const page = parseInt(searchParams.get(pageParam) || defaultPageParamValue, 10);
        const filter = searchParams.get('filter');
        const sorting = searchParams.get('sorting');

        setQueryParams((prev) => ({
            ...prev,
            ...Number.isFinite(page) && page > 0
                ? { page }
                : {},
            ...filter
                ? { filter }
                : {},
            ...sorting
                ? { sorting }
                : {},
        }));
    }, [ searchParams, setQueryParams ]);
};

// A function for updating a single param from the URL.
const updateSingleParam = (
    searchParams: URLSearchParams,
    setSearchParams: (newParams: URLSearchParamsInit, navigateOpts?: NavigateOptions) => void,
    key: string,
    value: string | undefined,
) => {
    const newParams = new URLSearchParams(searchParams.toString());

    if (value) {
        newParams.set(key, value);
    } else {
        newParams.delete(key);
    }

    newParams.set(pageParam, defaultPageParamValue);
    setSearchParams(newParams, { replace: false });
};

// A function for updating multiple params from the URL.
const batchUrlUpdates = (
    searchParams: URLSearchParams,
    updates: Array<{ key: string; value: string | undefined }>,
    setSearchParams: (newParams: URLSearchParamsInit, navigateOpts?: NavigateOptions) => void,
) => {
    const newParams = new URLSearchParams(searchParams.toString());

    updates.forEach(({ key, value }) => {
        if (value !== undefined) {
            newParams.set(key, value);
        } else {
            newParams.delete(key);
        }
    });

    newParams.set(pageParam, defaultPageParamValue);
    /*
        When we do batch updates, we want all params to be set at once ( a single browser history entry ),
        this is why 'replace' is set to 'true'.
     */
    setSearchParams(newParams, { replace: true });
};

type DropdownUrlSyncHook<T> = {
    searchParams: URLSearchParams;
    updateUrl: (item: T | undefined) => void;
    batchUpdateUrl?: (updates: Array<{ key: string; value: string | undefined }>) => void;
};

/**
 * Hook for syncing dropdown selection with URL and Redux state.
 * Automatically updates the Redux state based on the selected ID from the URL.
 * Also provides a function to update the URL from dropdown selection.
 *
 * @template T - Must have an `id: string | number` field.
 * @param key - The query param key used in the URL.
 * @param items - Available items for selection.
 * @param setReduxState - Redux action creator that accepts the matched item.
 * @param batchMode - Whether to return batch URL update functionality.
 * @returns Object containing searchParams, updateUrl, and optionally batchUpdateUrl.
 */
const useDropdownUrlSync = <T extends { id: number | string }>(
    key: string,
    items: T[] | undefined,
    setReduxState: ActionCreatorWithPayload<T | null>,
    batchMode = false,
): DropdownUrlSyncHook<T> => {
    const dispatch = useAppDispatch();
    const [ searchParams, setSearchParams ] = useSearchParams();
    const selectedId = searchParams.get(key);

    useEffect(() => {
        if (!items) {
            return;
        }

        const match = items.find((item) => item.id.toString() === selectedId);
        dispatch(setReduxState(match ?? null));
    }, [ selectedId, items, dispatch, setReduxState ]);

    const updateUrl = (item: T | undefined) => {
        updateSingleParam(searchParams, setSearchParams, key, item?.id?.toString());
    };

    const batchUpdateUrl = (updates: Array<{ key: string; value: string | undefined }>) => {
        batchUrlUpdates(searchParams, updates, setSearchParams);
    };

    return batchMode
        ? { searchParams, updateUrl, batchUpdateUrl }
        : { searchParams, updateUrl };
};

type DropdownUrlStateSyncHook<T> = {
    updateUrl: (item: T | undefined) => void;
    batchUpdateUrl?: (updates: Array<{ key: string; value: string | undefined }>) => void;
};

/**
 * Hook for syncing dropdown selection with URL and local React state.
 * Automatically updates local state based on the selected ID from the URL.
 * Also provides a function to update the URL from dropdown selection.
 *
 * @template T - Must have an `id: string | number` field.
 * @param key - The query param key used in the URL.
 * @param items - Available items for selection.
 * @param setState - Local state setter for the matched item.
 * @param setSearchParams - React Router's setSearchParams function.
 * @param searchParams - Current URLSearchParams.
 * @param batchMode - Whether to return batch URL update functionality.
 * @returns Object containing updateUrl, and optionally batchUpdateUrl.
 */
const useDropdownUrlStateSync = <T extends { id: number | string }>(
    key: string,
    items: T[] | undefined,
    setState: (val: T | undefined) => void,
    setSearchParams: (newParams: URLSearchParamsInit, navigateOpts?: NavigateOptions) => void,
    searchParams: URLSearchParams,
    batchMode = false,
): DropdownUrlStateSyncHook<T> => {
    const selectedId = searchParams.get(key);

    useEffect(() => {
        if (!items) {
            return;
        }

        const match = items.find((i) => i.id.toString() === selectedId);
        setState(match);
    }, [ selectedId, items, setState ]);

    const updateUrl = (item: T | undefined) => {
        updateSingleParam(searchParams, setSearchParams, key, item?.id?.toString());
    };

    const batchUpdateUrl = (updates: Array<{ key: string; value: string | undefined }>) => {
        batchUrlUpdates(searchParams, updates, setSearchParams);
    };

    return batchMode
        ? { updateUrl, batchUpdateUrl }
        : { updateUrl };
};

export {
    useSyncQueryParamsFromUrl,
    useDropdownUrlSync,
    useDropdownUrlStateSync,
};
