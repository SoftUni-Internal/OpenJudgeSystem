import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import isEmpty from 'lodash/isEmpty';
import isNil from 'lodash/isNil';

import { IUrlParam } from '../common/common-types';
import {
    DEFAULT_FILTER_TYPE,
    DEFAULT_SORT_FILTER_TYPE,
    DEFAULT_SORT_TYPE,
    DEFAULT_STATUS_FILTER_TYPE,
} from '../common/constants';
import generateSortingStrategy from '../common/contest-sorting-utils';
import { FilterSortType, FilterType, IContestParam, IFilter, ISort, ToggleParam } from '../common/contest-types';
import { filterByType, findFilterByTypeAndName } from '../common/filter-utils';
import { IIndexContestsType, IPagedResultType } from '../common/types';
import { IAllContestsUrlParams, IGetContestByProblemUrlParams } from '../common/url-types';
import { IHaveChildrenProps } from '../components/common/Props';
import { areStringEqual } from '../utils/compare-utils';

import { useUrlParams } from './common/use-url-params';
import { generateCategoryFilters, generateStatusFilters, generateStrategyFilters } from './contests/contest-filter-utils';
import { useContestCategories } from './use-contest-categories';
import { useContestStrategyFilters } from './use-contest-strategy-filters';
import { useHttp } from './use-http';
import { useLoading } from './use-loading';
import { usePages } from './use-pages';
import { useUrls } from './use-urls';

interface IContestsContext {
    state: {
        contests: IIndexContestsType[];
        possibleFilters: IFilter[];
        possibleSortingTypes: ISort[];
        filters: IFilter[];
        sortingTypes: ISort[];
        contest: IIndexContestsType | null;
        isLoaded: boolean;
    };
    actions: {
        reload: () => Promise<void>;
        clearFilters: () => void;
        clearSorts: () => void;
        toggleParam: (param: IFilter | ISort) => void;
        loadContestByProblemId: (problemId: number) => void;
        initiateGetAllContestsQuery: () => void;
    };
}

type IContestsProviderProps = IHaveChildrenProps

const defaultState = {
    state: {
        contests: [] as IIndexContestsType[],
        possibleFilters: [] as IFilter[],
        possibleSortingTypes: [] as ISort[],
    },
};

const ContestsContext = createContext<IContestsContext>(defaultState as IContestsContext);

const collectParams = <T extends FilterSortType>(
    params: IUrlParam[],
    possibleFilters: IContestParam<T>[],
    filterType: FilterType,
    defaultValue: any) => {
    const collectedFilters = params
        .map(({ key, value }) => findFilterByTypeAndName(possibleFilters, key, value))
        .filter((f) => !isNil(f)) as IContestParam<FilterSortType>[];

    if (isEmpty(filterByType(collectedFilters, filterType))) {
        const defaultStatusFilters = filterByType(possibleFilters, filterType)
            .filter(({ name }) => name === defaultValue);

        collectedFilters.push(...defaultStatusFilters);
    }

    return collectedFilters;
};

const ContestsProvider = ({ children }: IContestsProviderProps) => {
    const [ contests, setContests ] = useState(defaultState.state.contests);
    const [ getAllContestsUrlParams, setGetAllContestsUrlParams ] = useState<IAllContestsUrlParams | null>();
    const [ getContestByProblemUrlParams, setGetContestByProblemUrlParams ] = useState<IGetContestByProblemUrlParams | null>();
    const [ contest, setContest ] = useState<IIndexContestsType | null>(null);

    const {
        state: { params },
        actions: {
            setParam,
            unsetParam,
        },
    } = useUrlParams();
    const {
        state: { currentPage },
        changePage,
        populatePageInformation,
    } = usePages();
    const { getAllContestsUrl, getContestByProblemUrl } = useUrls();
    const { startLoading, stopLoading } = useLoading();

    const {
        get: getContests,
        data: contestsData,
        isSuccess,
    } = useHttp<
        IAllContestsUrlParams,
        IPagedResultType<IIndexContestsType>>({
            url: getAllContestsUrl,
            parameters: getAllContestsUrlParams,
        });

    const {
        get: getContestByProblemId,
        data: contestData,
    } = useHttp<
        IGetContestByProblemUrlParams,
        IIndexContestsType>({
            url: getContestByProblemUrl,
            parameters: getContestByProblemUrlParams,
        });

    const { state: { strategies, isLoaded: strategiesAreLoaded } } = useContestStrategyFilters();
    const { state: { categories, isLoaded: categoriesAreLoaded } } = useContestCategories();

    const possibleFilters = useMemo(
        () => strategiesAreLoaded && categoriesAreLoaded
            ? generateStatusFilters()
                .concat(generateCategoryFilters(categories))
                .concat(generateStrategyFilters(strategies)) as IFilter[]
            : [] as IFilter[],
        [ categories, categoriesAreLoaded, strategies, strategiesAreLoaded ],
    );

    const possibleSortingTypes = useMemo(
        () => generateSortingStrategy() as unknown as ISort[],
        [],
    );

    const filters = useMemo(
        () => collectParams(params, possibleFilters, DEFAULT_FILTER_TYPE, DEFAULT_STATUS_FILTER_TYPE),
        [ params, possibleFilters ],
    );

    const sortingTypes = useMemo(
        () => collectParams(params, possibleSortingTypes, DEFAULT_SORT_FILTER_TYPE, DEFAULT_SORT_TYPE),
        [ params, possibleSortingTypes ],
    );

    const clearSorts = useCallback(
        () => {
            const defaultSortFilterTypeId = possibleSortingTypes.filter((s) => s.name === DEFAULT_SORT_TYPE)[0]?.id;

            unsetParam(DEFAULT_SORT_FILTER_TYPE);
            setParam(DEFAULT_SORT_FILTER_TYPE, defaultSortFilterTypeId);
        },
        [ setParam, unsetParam, possibleSortingTypes ],
    );

    const clearFilters = useCallback(
        () => {
            unsetParam(FilterType.Status);
            unsetParam(FilterType.Strategy);
            unsetParam(FilterType.Category);

            const defaultFilterTypeId = possibleFilters
                .filter((f) => f.type === DEFAULT_FILTER_TYPE)
                .filter((sf) => sf.name === DEFAULT_STATUS_FILTER_TYPE)[0]?.id;

            setParam(DEFAULT_FILTER_TYPE, defaultFilterTypeId);
        },
        [ unsetParam, setParam, possibleFilters ],
    );

    const reload = useCallback(
        async () => {
            startLoading();
            await getContests();
            stopLoading();
        },
        [ getContests, startLoading, stopLoading ],
    );

    const toggleParam = useCallback<ToggleParam>((param) => {
        const { type, value } = param;
        const paramName = type.toString();

        const shouldRemoveParam = params.some(({
            key,
            value: paramValue,
        }) => areStringEqual(key, type, false) && areStringEqual(paramValue, value, false));

        if (!shouldRemoveParam) {
            setParam(paramName, value);
        }

        changePage(1);
    }, [ changePage, params, setParam ]);

    const initiateGetAllContestsQuery = useCallback(
        () => {
            if (isEmpty(possibleFilters)) {
                return;
            }

            setGetAllContestsUrlParams({
                filters: filters as IFilter[],
                sorting: sortingTypes as ISort[],
                page: currentPage,
            });
        },
        [ currentPage, filters, possibleFilters, sortingTypes ],
    );

    const loadContestByProblemId = useCallback((problemId: number) => {
        if (isNil(problemId)) {
            return;
        }

        setGetContestByProblemUrlParams({ problemId });
    }, []);

    useEffect(
        () => {
            if (isNil(getAllContestsUrlParams)) {
                return;
            }

            (async () => {
                await reload();
            })();
        },
        [ getAllContestsUrlParams, reload ],
    );

    useEffect(
        () => {
            if (isNil(getContestByProblemUrlParams)) {
                return;
            }

            (async () => {
                await getContestByProblemId();
            })();
        },
        [ getContestByProblemUrlParams, getContestByProblemId ],
    );

    useEffect(() => {
        if (isNil(contestData)) {
            return;
        }

        const contestResult = contestData as IIndexContestsType;
        setContest(contestResult);
    }, [ contestData ]);

    useEffect(
        () => {
            if (isNil(contestsData)) {
                return;
            }

            const contestsResult = contestsData as IPagedResultType<IIndexContestsType>;
            const newData = contestsResult.items as IIndexContestsType[];
            const {
                pageNumber,
                itemsPerPage,
                pagesCount,
                totalItemsCount,
            } = contestsResult || {};

            const newPagesInfo = {
                pageNumber,
                itemsPerPage,
                pagesCount,
                totalItemsCount,
            };

            setContests(newData);
            populatePageInformation(newPagesInfo);
        },
        [ contestsData, populatePageInformation ],
    );

    const value = useMemo(
        () => ({
            state: {
                contests,
                possibleFilters,
                possibleSortingTypes,
                filters,
                sortingTypes,
                contest,
                isLoaded: isSuccess,
            },
            actions: {
                reload,
                clearFilters,
                clearSorts,
                toggleParam,
                loadContestByProblemId,
                initiateGetAllContestsQuery,
            },
        }),
        [
            clearFilters,
            contests,
            filters,
            possibleFilters,
            reload,
            clearSorts,
            possibleSortingTypes,
            sortingTypes,
            toggleParam,
            loadContestByProblemId,
            contest,
            initiateGetAllContestsQuery,
            isSuccess,
        ],
    );

    return (
        <ContestsContext.Provider value={value}>
            {children}
        </ContestsContext.Provider>
    );
};

const useContests = () => useContext(ContestsContext);

export default ContestsProvider;

export {
    useContests,
};
