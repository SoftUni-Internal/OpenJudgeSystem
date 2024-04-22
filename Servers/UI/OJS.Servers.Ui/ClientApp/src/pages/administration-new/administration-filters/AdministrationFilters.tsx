/* eslint-disable max-len */
/* eslint-disable @typescript-eslint/no-use-before-define */
/* eslint-disable import/group-exports */
import React, { Dispatch, SetStateAction, useEffect, useMemo, useState } from 'react';
import { SetURLSearchParams } from 'react-router-dom';
// eslint-disable-next-line import/no-extraneous-dependencies
import { Unstable_Popup as BasePopup } from '@mui/base/Unstable_Popup';
import CloseIcon from '@mui/icons-material/Close';
import DeleteIcon from '@mui/icons-material/Delete';
import { Box, Button, FormControl, InputLabel, MenuItem, Select, TextField } from '@mui/material';
import { GridColDef } from '@mui/x-data-grid';
import { DateTimePicker } from '@mui/x-date-pickers';
import debounce from 'lodash/debounce';

import { FilterColumnTypeEnum, SortingEnum } from '../../../common/enums';
import { IEnumType, IFilterColumn, IGetAllAdminParams } from '../../../common/types';
import { getColors } from '../../../hooks/use-administration-theme-provider';
import { useAppSelector } from '../../../redux/store';
import { getDateAsLocal } from '../../../utils/administration/administration-dates';
import concatClassNames from '../../../utils/class-names';
import { DEFAULT_ITEMS_PER_PAGE } from '../../../utils/constants';
import { defaultFilterToAdd } from '../AdministrationGridView';

import styles from './AdministrationFilters.module.scss';

interface IFiltersColumnOperators {
    name: string;
    value: string;
}

interface IAdministrationFilterProps {
    filterColumns: IFilterColumn[];
    searchParams?: URLSearchParams;
    setSearchParams?: SetURLSearchParams;
    selectedFilters: Array<IAdministrationFilter>;
    setSelectedFilters?: Dispatch<SetStateAction<IAdministrationFilter[]>>;
    withSearchParams?: boolean;
    sortingColumns: string[];
    selectedSorters: Array<IAdministrationSorter>;
    setSelectedSorters?: Dispatch<SetStateAction<IAdministrationSorter[]>>;
    setQueryParams?: Dispatch<React.SetStateAction<IGetAllAdminParams>>;
}

interface IAdministrationFilter {
    column: string;
    operator: string;
    value: string;
    inputType: FilterColumnTypeEnum;
    availableOperators?: IFiltersColumnOperators[];
    availableColumns: IFilterColumn[];
}

const DROPDOWN_OPERATORS = {
    [FilterColumnTypeEnum.STRING]: [
        { name: 'Contains', value: 'contains' },
        { name: 'Equals', value: 'equals' },
        { name: 'Starts with', value: 'startswith' },
        { name: 'Ends with', value: 'endswith' },
    ],
    [FilterColumnTypeEnum.ENUM]: [
        { name: 'Equals', value: 'equals' },
    ],
    [FilterColumnTypeEnum.BOOL]: [
        { name: 'Equals', value: 'equals' },
    ],
    [FilterColumnTypeEnum.NUMBER]: [
        { name: 'Equals', value: 'equals' },
        { name: 'Greater Than', value: 'greaterthan' },
        { name: 'Less Than', value: 'lessthan' },
        { name: 'Less Than Or Equal', value: 'lessthanorequal' },
        { name: 'Greater Than Or Equal', value: 'greaterthanorequal' },
    ],
    [FilterColumnTypeEnum.DATE]: [
        // Commented because it is not working
        // { name: 'Equals', value: 'equals' },
        { name: 'Greater Than', value: 'greaterthan' },
        { name: 'Less Than', value: 'lessthan' },
        { name: 'Less Than Or Equal', value: 'lessthanorequal' },
        { name: 'Greater Than Or Equal', value: 'greaterthanorequal' },
    ],
};

const BOOL_DROPDOWN_VALUES = [
    { name: 'True', value: 'true' },
    { name: 'False', value: 'false' },
];

const mapStringToFilterColumnTypeEnum = (type: string) => {
    if (type === 'number') {
        return FilterColumnTypeEnum.NUMBER;
    } if (type === 'boolean') {
        return FilterColumnTypeEnum.BOOL;
    } if (type === 'date') {
        return FilterColumnTypeEnum.DATE;
    }
    if (type === 'enum') {
        return FilterColumnTypeEnum.ENUM;
    }
    return FilterColumnTypeEnum.STRING;
};

interface IAdministrationSorter {
    columnName: string;
    orderBy: SortingEnum;
    availableColumns: string[];
}

const orderByOptions = [
    { name: 'Ascending', value: SortingEnum.ASC },
    { name: 'Descending', value: SortingEnum.DESC },
];

const filterSeparator = '&&;';
const filterParamsSeparator = '~';
const sorterSeparator = '&';
const sorterParamSeparator = '=';

const AdministrationFilters = (props: IAdministrationFilterProps) => {
    const themeMode = useAppSelector((x) => x.theme.administrationMode);
    const {
        filterColumns,
        withSearchParams = true,
        selectedFilters,
        setSelectedFilters,
        setQueryParams,
        searchParams,
        sortingColumns,
        selectedSorters,
        setSelectedSorters,
        setSearchParams,
    } = props;

    const defaultFilter = useMemo<IAdministrationFilter>(() => ({
        column: '',
        operator: '',
        value: '',
        availableOperators: [],
        availableColumns: filterColumns,
        inputType: FilterColumnTypeEnum.STRING,
    }), [ filterColumns ]);

    const defaultSorter = useMemo<IAdministrationSorter>(() => ({
        columnName: '',
        orderBy: SortingEnum.ASC,
        availableColumns: sortingColumns,
    }), [ sortingColumns ]);

    useEffect(() => {
        if (selectedFilters.length <= 0 && setSelectedFilters) {
            setSelectedFilters([ defaultFilter ]);
        }

        if (selectedSorters.length <= 0 && setSelectedSorters) {
            setSelectedSorters([ defaultSorter ]);
        }
    }, [
        defaultFilter,
        selectedFilters,
        setSelectedFilters,
        setSelectedSorters,
        selectedSorters,
        defaultSorter,
    ]);

    const [ filterAnchor, setFilterAnchor ] = useState<null | HTMLElement>(null);
    const [ sortersAnchor, setSortersAnchor ] = useState<null | HTMLElement>(null);
    const filterOpen = Boolean(filterAnchor);
    const openSorters = Boolean(sortersAnchor);

    useEffect(() => {
        if (!searchParams || !setSearchParams) {
            return;
        }

        const formatFilterToString = (filter: IAdministrationFilter) => {
            if (!filter.column || !filter.operator || !filter.value) {
                return;
            }

            // eslint-disable-next-line consistent-return, max-len
            return `${filter.column}${filterParamsSeparator}${filter.operator}${filterParamsSeparator}${filter.value}`.toLowerCase();
        };

        const filtersFormattedArray = selectedFilters.map(formatFilterToString).filter((filter) => filter);
        if (!filtersFormattedArray.length) {
            searchParams.delete('filter');
            if (withSearchParams) {
                setSearchParams(searchParams);
            }

            if (setQueryParams) {
                setQueryParams((currentParams) => ({
                    ...currentParams,
                    filter: filtersFormattedArray.join(filterSeparator) ?? '',
                }));
            }

            return;
        }

        const delayedSetOfSearch = debounce(() => {
            searchParams.set('filter', filtersFormattedArray.join(filterSeparator));
            if (withSearchParams) {
                setSearchParams(searchParams);
            }
        }, 300);

        delayedSetOfSearch();
        const filtersToSet = filtersFormattedArray.join(filterSeparator);

        searchParams.set('filter', filtersToSet);
        if (withSearchParams) {
            setSearchParams(searchParams);
        }
        if (setQueryParams) {
            setQueryParams((currentParams) => ({
                ...currentParams,
                filter: filtersToSet,
            }));
        }

        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [ selectedFilters ]);

    useEffect(() => {
        const formatSorterToString = (sorter: IAdministrationSorter) => {
            if (!sorter?.columnName) {
                return;
            }

            // eslint-disable-next-line consistent-return
            return `${sorter.columnName.replace(/\s/g, '').toLowerCase()}=${sorter.orderBy}`;
        };

        const sorterFormattedArray = selectedSorters.map(formatSorterToString).filter((sorter) => sorter);
        if (!sorterFormattedArray.length && searchParams && setSearchParams) {
            searchParams.delete('sorting');
            if (withSearchParams) {
                setSearchParams(searchParams);
            }

            if (setQueryParams) {
                setQueryParams((currentParams) => ({
                    ...currentParams,
                    sorting: sorterFormattedArray.join(sorterSeparator) ?? '',
                }));
            }

            return;
        }

        if (searchParams && setSearchParams && withSearchParams) {
            searchParams.set('sorting', sorterFormattedArray.join('&'));
            setSearchParams(searchParams);
        }

        if (setQueryParams) {
            setQueryParams((currentParams) => ({
                ...currentParams,
                sorting: sorterFormattedArray.join(sorterSeparator) ?? '',
            }));
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [ selectedSorters ]);

    const handleOpenClick = (event: React.MouseEvent<HTMLElement>) => {
        if (!filterAnchor) {
            setSortersAnchor(null);
        }

        setFilterAnchor(filterAnchor
            ? null
            : event.currentTarget);
    };

    const addFilter = () => {
        const availableColumns = filterColumns.filter((column) => !selectedFilters.some((f) => f.column === column.columnName));
        const newFiltersArray = [ { ...defaultFilter, availableColumns }, ...selectedFilters.map((filter) => ({
            ...filter,
            availableColumns: [ ...availableColumns, { columnName: filter.column, columnType: filter.inputType } ],
        })) ];

        if (setSelectedFilters) {
            setSelectedFilters(newFiltersArray);
        }
    };

    const removeAllFilters = () => {
        if (searchParams && setSearchParams && withSearchParams) {
            searchParams.delete('filter');
        }
        if (setSelectedFilters) {
            setSelectedFilters([ defaultFilter ]);
        }
    };

    const removeSingleFilter = (idx: number) => {
        const deletedFilter = selectedFilters[idx];
        const newFiltersArray = [ ...selectedFilters.map((filter) => ({
            ...filter,
            availableColumns: [ ...filter.availableColumns, { columnName: deletedFilter.column, columnType: deletedFilter.inputType } ],
        })) ];
        newFiltersArray.splice(idx, 1);

        if (setSelectedFilters) {
            setSelectedFilters(newFiltersArray);
        }

        if (newFiltersArray.length === 1 && searchParams && setSearchParams) {
            searchParams.delete('filter');
            if (withSearchParams) {
                setSearchParams(searchParams);
            }
        }
    };

    const handleDateTimePickerChange = (indexToUpdate: number, value: any, updateProperty: string) => {
        updateFilterColumnData(indexToUpdate, { target: { value } }, updateProperty);
    };
    const getColumnTypeByName = (columnName: string) => filterColumns.find((column) => column.columnName === columnName)?.columnType;

    /**
 * Updates selected filters when there is a change.
 * @param {number} dataColumns The index of the filter that must be updated.
 * @param {any} target The target of the event which value will be taken.
 * @param {string} updateProperty Property to be updated (property name, operator type or value)
 */
    const updateFilterColumnData = (indexToUpdate: number, { target }: any, updateProperty: string) => {
        const { value } = target;
        const newFiltersArray = [ ...selectedFilters ].map((element, idx) => {
            if (idx === indexToUpdate) {
                if (updateProperty === 'column') {
                    const columnType = getColumnTypeByName(value);
                    const columnOperators = columnType
                        ? DROPDOWN_OPERATORS[columnType]
                        : [];
                    return {
                        column: value,
                        operator: '',
                        value: '',
                        inputType: columnType || FilterColumnTypeEnum.STRING,
                        availableOperators: columnOperators,
                        availableColumns: element.availableColumns,
                    };
                }
                return { ...element, [updateProperty]: value };
            }
            return element;
        });

        if (setSelectedFilters) {
            setSelectedFilters(newFiltersArray);
        }
    };

    const renderInputField = (idx: number) => {
        const selectedFilter = selectedFilters[idx];
        if (selectedFilter.inputType === FilterColumnTypeEnum.BOOL) {
            return (
                <FormControl sx={{ width: '140px', marginRight: '10px' }} variant="standard">
                    <InputLabel id="value-select-label" shrink>Value</InputLabel>
                    <Select
                      labelId="value-select-label"
                      value={selectedFilters[idx]?.value}
                      label="Value"
                      onChange={(e) => updateFilterColumnData(idx, e, 'value')}
                      disabled={!selectedFilters[idx].operator || idx > 0}
                    >

                        { BOOL_DROPDOWN_VALUES.map((column) => (
                            <MenuItem
                              key={`s-c-${column.value}`}
                              value={column.value}
                            >
                                {column.name}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
            );
        }
        if (selectedFilter.inputType === FilterColumnTypeEnum.ENUM) {
            const column = filterColumns.filter((c) => c.columnType === selectedFilter.inputType &&
            selectedFilter.column === c.columnName)[0];

            return (
                <FormControl sx={{ width: '140px', marginRight: '10px' }} variant="standard">
                    <InputLabel id="value-select-label" shrink>Value</InputLabel>
                    <Select
                      labelId="value-select-label"
                      value={selectedFilters[idx]?.value}
                      label="Value"
                      onChange={(e) => updateFilterColumnData(idx, e, 'value')}
                      disabled={!selectedFilters[idx].operator || idx > 0}
                    >

                        { column.enumValues?.map((value) => (
                            <MenuItem
                              key={`s-c-${value}`}
                              value={value}
                            >
                                {value}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
            );
        }
        return (

            selectedFilter.inputType === FilterColumnTypeEnum.DATE
                ? (
                    <DateTimePicker
                      orientation="landscape"
                      label={FilterColumnTypeEnum.DATE}
                      value={getDateAsLocal(selectedFilters[idx]?.value)}
                      onChange={(newValue) => handleDateTimePickerChange(idx, newValue, 'value')}
                      disabled={!selectedFilters[idx].operator || idx > 0}
                    />
                )
                : (
                    <TextField
                      label="Value"
                      variant="standard"
                      type={selectedFilter.inputType}
                      value={selectedFilters[idx]?.value}
                      onChange={(e) => updateFilterColumnData(idx, e, 'value')}
                      disabled={!selectedFilters[idx].operator || idx > 0}
                    />
                )
        );
    };

    const handleOpenSorters = (event: React.MouseEvent<HTMLElement>) => {
        if (!sortersAnchor) {
            setFilterAnchor(null);
        }

        setSortersAnchor(sortersAnchor
            ? null
            : event.currentTarget);
    };

    const addSorter = () => {
        const availableColumns = sortingColumns.filter((column) => !selectedSorters.some((s) => s.columnName === column));
        const newSortersArray = [ { ...defaultSorter, availableColumns }, ...selectedSorters.map((sorter) => ({
            ...sorter,
            availableColumns: [ ...availableColumns, sorter.columnName ],
        })) ];

        if (setSelectedSorters) {
            setSelectedSorters(newSortersArray);
        }
    };

    const removeAllSorters = () => {
        if (searchParams && setSearchParams && withSearchParams) {
            searchParams.delete('sorting');
            setSearchParams(searchParams);
        }

        if (setSelectedSorters) {
            setSelectedSorters([ defaultSorter ]);
        }
    };

    const removeSingleSorter = (idx: number) => {
        const deletedSorter = selectedSorters[idx];
        const newSortersArray = [ ...selectedSorters.map((sorter) => ({
            ...sorter,
            availableColumns: [ ...sorter.availableColumns, deletedSorter.columnName ],
        })) ];
        newSortersArray.splice(idx, 1);

        if (setSelectedSorters) {
            setSelectedSorters(newSortersArray);
        }

        if (newSortersArray.length === 1) {
            if (searchParams && setSearchParams && withSearchParams) {
                searchParams.delete('sorting');
                setSearchParams(searchParams);
            }
        }
    };

    /**
 * Updates selected sorters when there is a change.
 * @param {number} dataColumns The index of the sorter that must be updated.
 * @param {any} target The target of the event which value will be taken.
 * @param {string} updateProperty Property to be updated (property name or value)
 */
    const updateSorterColumnData = (indexToUpdate: number, { target }: any, updateProperty: string) => {
        const { value } = target;

        const newSortersArray = [ ...selectedSorters ].map((element, idx) => {
            if (idx === indexToUpdate) {
                return { ...element, [updateProperty]: value };
            }
            return element;
        });

        if (setSelectedSorters) {
            setSelectedSorters(newSortersArray);
        }
    };

    const renderFilter = (idx: number) => (
        <Box style={{ display: 'flex', margin: '5px 0' }} key={`admin-filter-${idx}`}>
            <CloseIcon className={styles.closeIcon} onClick={() => setFilterAnchor(null)} />
            { idx !== 0 && (
                <DeleteIcon color="error" className={styles.removeFilterButton} onClick={() => removeSingleFilter(idx)} />
            )}
            <FormControl sx={{ width: '140px', marginRight: '10px' }} variant="standard">
                <InputLabel id="column-select-label">Column</InputLabel>
                <Select
                  labelId="column-select-label"
                  value={selectedFilters[idx]?.column}
                  label="Column"
                  onChange={(e) => updateFilterColumnData(idx, e, 'column')}
                  disabled={idx > 0}
                >
                    { selectedFilters[idx]?.availableColumns?.map((column) => (
                        <MenuItem
                          key={`s-c-${column.columnName}`}
                          value={column.columnName}
                        >
                            {column.columnName}
                        </MenuItem>
                    ))}
                </Select>
            </FormControl>
            <FormControl sx={{ width: '140px', marginRight: '10px' }} variant="standard">
                <InputLabel id="column-operator-label">Operator</InputLabel>
                <Select
                  labelId="column-operator-label"
                  value={selectedFilters[idx]?.operator}
                  label="Operator"
                  onChange={(e) => updateFilterColumnData(idx, e, 'operator')}
                  disabled={!selectedFilters[idx].column || idx > 0}
                >
                    { selectedFilters[idx].availableOperators?.map((operator) => (
                        <MenuItem key={`s-o-${operator.value}`} value={operator.value}>{operator.name}</MenuItem>)) }
                </Select>
            </FormControl>
            {renderInputField(idx)}
        </Box>
    );

    const renderSorter = (idx: number) => (
        <Box className={styles.sortWrapper} key={`a-s-w-${idx}`}>
            <CloseIcon className={styles.closeIcon} onClick={() => setSortersAnchor(null)} />
            { idx !== 0 && (
                <DeleteIcon color="error" className={styles.removeSorterButton} onClick={() => removeSingleSorter(idx)} />
            )}
            <FormControl sx={{ width: '140px', marginRight: '10px' }} variant="standard">
                <InputLabel id="column-sorting-label">Sort by</InputLabel>
                <Select
                  labelId="column-sorting-label"
                  value={selectedSorters[idx]?.columnName}
                  label="Sort By"
                  onChange={(e) => updateSorterColumnData(idx, e, 'columnName')}
                  disabled={idx > 0}
                >
                    { selectedSorters[idx].availableColumns.map((sortOption) => (
                        <MenuItem key={`a-s-e-${sortOption}`} value={sortOption}>{sortOption}</MenuItem>)) }
                </Select>
            </FormControl>
            <FormControl sx={{ width: '140px', marginRight: '10px' }} variant="standard">
                <InputLabel id="column-order-label">Order By</InputLabel>
                <Select
                  labelId="column-order-label"
                  value={selectedSorters[idx]?.orderBy}
                  label="Order By"
                  onChange={(e) => updateSorterColumnData(idx, e, 'orderBy')}
                  disabled={!selectedSorters[idx]?.columnName || idx > 0}
                >
                    { orderByOptions.map((orderByOption) => (
                        <MenuItem key={`s-o-o-${orderByOption.name}`} value={orderByOption.value}>{orderByOption.name}</MenuItem>)) }
                </Select>
            </FormControl>
        </Box>
    );
    return (
        <>
            <Box>
                <Button onClick={handleOpenClick} style={{ margin: '10px 0' }}>
                    {filterOpen
                        ? 'close'
                        : 'open'}
                    {' '}
                    filters
                    <span style={{ marginLeft: '10px', color: getColors(themeMode).textColors.primary }}>
                        (
                        {selectedFilters.filter((f) => f.value).length}
                        ) Active
                    </span>
                </Button>
                <BasePopup anchor={filterAnchor} open={filterOpen}>
                    <Box className={concatClassNames(styles.administrationFilters, 'box-wrapper')}>
                        <Box style={{ display: 'flex', flexDirection: 'column' }}>
                            {selectedFilters.map((filter, idx) => renderFilter(idx))}
                        </Box>
                        <Box className={styles.buttonsSection}>
                            <Button
                              onClick={addFilter}
                              disabled={selectedFilters.length
                                  ? !selectedFilters[0].value
                                  : false}
                            >
                                Add filter
                            </Button>
                            <Button onClick={removeAllFilters}>Remove All</Button>
                        </Box>
                    </Box>
                </BasePopup>
            </Box>
            <Box>
                <Button onClick={handleOpenSorters}>
                    {openSorters
                        ? 'close'
                        : 'open'}
                    {' '}
                    sorters
                    <Box style={{ marginLeft: '10px', color: getColors(themeMode).textColors.primary }}>
                        (
                        {selectedSorters.filter((s) => s.columnName).length}
                        ) Active
                    </Box>
                </Button>
                <BasePopup anchor={sortersAnchor} open={openSorters}>
                    <Box className={concatClassNames(styles.administrationSorters, 'box-wrapper')}>
                        <Box style={{ display: 'flex', flexDirection: 'column' }}>
                            {selectedSorters.map((sorter, idx) => renderSorter(idx))}
                        </Box>
                        <Box className={styles.buttonsSection}>
                            <Button
                              onClick={addSorter}
                              disabled={selectedSorters.length
                                  ? !selectedSorters[0].columnName
                                  : true}
                            >
                                Add Sorter
                            </Button>
                            <Button onClick={removeAllSorters}>Remove All</Button>
                        </Box>
                    </Box>
                </BasePopup>
            </Box>
        </>
    );
};

/**
 * Maps columns from the grid to an array with property name, type and enum values (when there is a column of type 'enum').
 * @param {GridColDef& IEnumType} dataColumns The grid columns that will be mapped.
 * @returns {Array<IAdministrationFilter>} Return the grid col definitions mapped to an easy to use object.
 */

const mapGridColumnsToAdministrationFilterProps =
(dataColumns: Array<GridColDef& IEnumType>): IFilterColumn[] => dataColumns.map((column) => {
    const mappedEnumType = mapStringToFilterColumnTypeEnum(column.type || '');
    return {
        columnName: column.headerName?.replace(/\s/g, '') ?? '',
        columnType: mappedEnumType,
        enumValues: mappedEnumType === FilterColumnTypeEnum.ENUM
            ? column.enumValues
            : null,
    };
});

/**
 * Maps the filter parameter from the search bar to Array<IAdministrationFilter> that will later be united with the default filter and will set the initial default filters.
 * @param {URLSearchParams} searchParams Search params that will be mapped to IAdministrationFilter.
 * @param {Array<string>} columns The sorting columns as a  Array<IFilterColumn>
 * @returns {Array<IAdministrationFilter>} Return the filters from the search bar mapped as Array<IAdministrationFilter>
 */
const mapUrlToFilters = (urlSearchParams: URLSearchParams | undefined, columns: Array<IFilterColumn>): IAdministrationFilter[] => {
    if (!urlSearchParams) {
        return [];
    }

    const urlSelectedFilters: Array<IAdministrationFilter> = [];

    const filterParams = urlSearchParams.get('filter') ?? '';

    const urlParams = filterParams.split(filterSeparator).filter((param) => param);
    urlParams.forEach((param) => {
        const paramChunks = param.split(filterParamsSeparator).filter((chunk) => chunk);

        const columnValue = paramChunks[0];
        const operator = paramChunks[1];
        const value = paramChunks[2];

        const column = columns.find((c) => c.columnName.toLowerCase() === columnValue) ||
        { columnName: '', columnType: FilterColumnTypeEnum.STRING };
        const availableColumns = columns.filter((c) => !urlSelectedFilters.some((f: { column: string }) => f.column === c.columnName));
        const availableOperators = column?.columnType
            ? DROPDOWN_OPERATORS[column.columnType]
            : [];

        const filter = {
            column: column?.columnName || '',
            operator,
            value,
            availableOperators,
            availableColumns: [ ...availableColumns, { ...column } ],
            inputType: column?.columnType || FilterColumnTypeEnum.STRING,
        };

        urlSelectedFilters.push(filter);
    });

    return urlSelectedFilters;
};

/**
 * Sets the initial Selected Sorters.
 * @param {URLSearchParams} searchParams Search params that will be mapped to IAdministrationSorter.
 * @param {Array<string>} columns The sorting columns as a string []
 * @returns {Array<IAdministrationSorter>} Return the initial selected sorters that will appear in the search bar.
 */
const mapUrlToSorters = (
    searchParams: URLSearchParams | undefined,
    columns:Array<string>,
    defaultSorter?: string,
): IAdministrationSorter[] => {
    if (!searchParams) {
        return [];
    }
    const urlSelectedSorters: IAdministrationSorter[] = [];

    const sorterParams = searchParams.get('sorting') ?? '';
    const urlParams = sorterParams.split(sorterSeparator).filter((param) => param);

    if (defaultSorter) {
        const sorterProperty = defaultSorter.split('=')[0];
        const indexOfSorter = columns.findIndex((x) => x.toLowerCase() === sorterProperty.toLowerCase());
        if ((!sorterParams || !urlParams.find((x) => x.includes(sorterProperty))) && indexOfSorter >= 0) {
            urlParams.push(defaultSorter);
        }
    }

    urlParams.forEach((param: string) => {
        const paramChunks = param.split('=').filter((chunk) => chunk);

        const sorterColumn = paramChunks[0];
        const sorterOrderBy = paramChunks[1];

        const columnName = columns.find((c) => c.toLowerCase() === sorterColumn) || '';
        const orderBy = sorterOrderBy === 'ASC'
            ? SortingEnum.ASC
            : SortingEnum.DESC;
        const availableColumns = columns.filter((column) => !urlSelectedSorters.some((s) => s.columnName === column));

        const sorter: IAdministrationSorter = {
            columnName,
            orderBy,
            availableColumns: [ ...availableColumns, columnName ],
        };

        urlSelectedSorters.push(sorter);
    });

    return urlSelectedSorters;
};

/**
 * Sets the initial Selected filters.
 * @param {GridColDef[]} gridColDef All filterable columns for the certain grid.
 * @param {URLSearchParams} searchParams Search params that will be mapped to IAdministrationFilter.
 * @param {string} filterToAdd The default filter that must be added.
 * @returns {Array<IAdministrationFilter>} Return the initial selected filters that will appear in the search bar.
 */

const addDefaultFilter = (
    gridColDef: GridColDef[],
    searchParams:URLSearchParams,
    filterToAdd: string = defaultFilterToAdd,
): Array<IAdministrationFilter> => {
    const columns = mapGridColumnsToAdministrationFilterProps(gridColDef);

    const filters = mapUrlToFilters(searchParams, columns);
    const paramChunks = filterToAdd.split(filterParamsSeparator);

    const columnValue = paramChunks[0];
    const operator = paramChunks[1];
    const value = paramChunks[2];

    if (!columns.find((x) => x.columnName.toLowerCase() === columnValue)) {
        return filters;
    }

    const column = columns.find((c) => c.columnName.toLowerCase() === columnValue) ||
    { columnName: '', columnType: FilterColumnTypeEnum.STRING };
    const availableColumns = columns.filter((c) => !filters.some((f: { column: string }) => f.column === c.columnName));
    const availableOperators = column?.columnType
        ? DROPDOWN_OPERATORS[column.columnType]
        : [];

    const filter = {
        column: column?.columnName || '',
        operator,
        value,
        availableOperators,
        availableColumns: [ ...availableColumns, { ...column } ],
        inputType: column?.columnType || FilterColumnTypeEnum.STRING,
    };

    const defFilter = {
        column: '',
        operator: '',
        value: '',
        availableOperators: [],
        availableColumns: columns,
        inputType: FilterColumnTypeEnum.STRING,
    };

    if (!filters.find((x) => x.column.toLowerCase() === columnValue.toLowerCase())) {
        filters.push(filter);
    }

    const newFiltersArray = [ { ...defFilter, availableColumns }, ...filters.map((f) => ({
        ...f,
        availableColumns: [ ...availableColumns, { columnName: f.column, columnType: f.inputType } ],
    })) ];

    return newFiltersArray;
};

/**
 * Applies the default filters and sorters to query string when component mounts.
 * @param {string} defaultFilter The default filter which will be applied.
 * @param {string} defaultSorter The default sorter which will be applied.
 * @param {URLSearchParams} searchParams Search parameters which are used to apply also other filters and sorters if present.
 * @param {boolean} skipDefault Flag which determines if the default filters and sorters must be applied or not.
 * @param {number} page The page which needs to be applied to the query string.
 * @param {number} itemsPerPage The quantity of records that must be fetched from the server.
 * @returns {IGetAllAdminParams}  The parameters for the query string.
 */
const applyDefaultFilterToQueryString = (
    defaultFilter: string,
    defaultSorter: string,
    searchParams? : URLSearchParams,
    skipDefault: boolean = false,
    page: number = 1,
    itemsPerPage: number = DEFAULT_ITEMS_PER_PAGE,
) => {
    let filter : string | null = '';
    let sorting : string | null = '';
    if (searchParams) {
        filter = searchParams.get('filter');
        sorting = searchParams.get('sorting');
    }
    filter = applyQueryParam(filterSeparator, filterParamsSeparator, filter, skipDefault, defaultFilter);

    sorting = applyQueryParam(sorterSeparator, sorterParamSeparator, sorting, skipDefault, defaultSorter);

    const queryParams = {
        page,
        itemsPerPage,
        filter,
        sorting,
    } as IGetAllAdminParams;

    return queryParams;
};

/**
 * Prepares parameter from the search parameters to be applied to query string and adds default value if any.
 * @param {string} separator The separator used to divide more than one values for parameter.
 * @param {string} paramSeparator The separator used to divide property operator and value in a single value of a parameter
 * @param {string} param The search param as string.
 * @param {boolean} skipDefault Flag which determines if the default filters and sorters must be applied or not.
 * @param {string} defaultParamValue Default value that must be applied to the parameter.
 * @returns {string}  Returns the query param as string.
 */

const applyQueryParam = (
    separator: string,
    paramSeparator: string,
    param: string | null,
    skipDefault: boolean,
    defaultParamValue: string,
) => {
    const defaultParamArray = defaultParamValue.split(separator);
    let valueToReturn = param || '';

    if (defaultParamValue === '') {
        return '';
    }

    if (!param) {
        if (skipDefault) {
            valueToReturn = '';
        } else {
            valueToReturn = defaultParamValue;
        }
    } else {
        defaultParamArray.forEach((currentValue, idx) => {
            const prop = currentValue.split(paramSeparator)[0];
            if (!param?.includes(prop)) {
                valueToReturn += `${idx > 0 && separator}${currentValue}`;
            }
        });
    }

    return valueToReturn;
};

const mapGridColumnsToAdministrationSortingProps =
    (dataColumns: GridColDef[]): string[] => dataColumns.map((column) => column.headerName?.replace(/\s/g, '') ?? '').filter((el) => el);

export {
    type IAdministrationFilter,
    type IAdministrationSorter,
    type IFiltersColumnOperators,
    mapGridColumnsToAdministrationFilterProps,
    mapGridColumnsToAdministrationSortingProps,
    mapUrlToFilters,
    mapUrlToSorters,
    addDefaultFilter,
    applyDefaultFilterToQueryString,
    filterSeparator,
    filterParamsSeparator,
};
export default AdministrationFilters;
