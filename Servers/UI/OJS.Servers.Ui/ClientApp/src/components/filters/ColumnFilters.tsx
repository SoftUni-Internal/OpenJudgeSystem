/* eslint-disable max-len */
/* eslint-disable @typescript-eslint/no-use-before-define */
/* eslint-disable import/group-exports */
import React, { Dispatch, SetStateAction, useCallback, useEffect, useMemo, useState } from 'react';
import { SetURLSearchParams } from 'react-router-dom';
// eslint-disable-next-line import/no-extraneous-dependencies
import CloseIcon from '@mui/icons-material/Close';
import DeleteIcon from '@mui/icons-material/Delete';
import FilterAltIcon from '@mui/icons-material/FilterAlt';
import { Box, Button, Popper, TextField } from '@mui/material';
import { GridColDef } from '@mui/x-data-grid';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { IDictionary } from 'src/common/common-types';
import { FilterColumnTypeEnum } from 'src/common/enums';
import { IEnumType, ISingleColumnFilter } from 'src/common/types';
import { IGetSubmissionsUrlParams } from 'src/common/url-types';
import Dropdown from 'src/components/guidelines/dropdown/Dropdown';
import { defaultFilterToAdd } from 'src/pages/administration-new/AdministrationGridView';
import concatClassNames from 'src/utils/class-names';
import { DEFAULT_ITEMS_PER_PAGE } from 'src/utils/constants';
import { getDateAsLocal } from 'src/utils/dates';

import useTheme from '../../hooks/use-theme';

import styles from './ColumnFilters.module.scss';

interface IFiltersColumnOperators {
    name: string;
    value: string;
    id: string;
}

interface IFilterProps {
    filterColumn: ISingleColumnFilter;
    searchParams?: URLSearchParams;
    setSearchParams?: SetURLSearchParams;
    allFilters: IDictionary<Array<IFilter>>;
    setAllFilters: Dispatch<SetStateAction<IDictionary<Array<IFilter>>>>;
    withSearchParams?: boolean;
    setQueryParams?: Dispatch<React.SetStateAction<IGetSubmissionsUrlParams>>;
    openFilter: string | null;
    onToggleFilter: (columnName: string | null) => void;
}

interface IFilter {
    id: string;
    column: string;
    operator: string;
    value: string;
    inputType: FilterColumnTypeEnum;
    availableOperators?: IFiltersColumnOperators[];
    availableColumns: ISingleColumnFilter[];
}

const DROPDOWN_OPERATORS = {
    [FilterColumnTypeEnum.STRING]: [
        { name: 'Contains', id: 'Contains', value: 'contains' },
        { name: 'Equals', id: 'Equals', value: 'equals' },
        { name: 'Starts with', id: 'Starts with', value: 'startswith' },
        { name: 'Ends with', id: 'Ends with', value: 'endswith' },
    ],
    [FilterColumnTypeEnum.ENUM]: [
        { name: 'Equals', id: 'Equals', value: 'equals' },
    ],
    [FilterColumnTypeEnum.BOOL]: [
        { name: 'Equals', id: 'Equals', value: 'equals' },
    ],
    [FilterColumnTypeEnum.NUMBER]: [
        { name: 'Equals', id: 'Equals', value: 'equals' },
        { name: 'Greater Than', id: 'Greater Than', value: 'greaterthan' },
        { name: 'Less Than', id: 'Less Than', value: 'lessthan' },
        { name: 'Less Than Or Equal', id: 'Less Than Or Equal', value: 'lessthanorequal' },
        { name: 'Greater Than Or Equal', id: 'Greater Than Or Equal', value: 'greaterthanorequal' },
    ],
    [FilterColumnTypeEnum.DATE]: [
        // Commented because it is not working
        { name: 'Equals', id: 'Equals', value: 'equals' },
        { name: 'Greater Than', id: 'Greater Than', value: 'greaterthan' },
        { name: 'Less Than', id: 'Less Than', value: 'lessthan' },
        { name: 'Less Than Or Equal', id: 'Less Than Or Equal', value: 'lessthanorequal' },
        { name: 'Greater Than Or Equal', id: 'Greater Than Or Equal', value: 'greaterthanorequal' },
    ],
};

const BOOL_DROPDOWN_VALUES = [
    { name: 'True', id: 'True', value: 'true' },
    { name: 'False', id: 'False', value: 'false' },
];

const mapStringToFilterColumnTypeEnum = (type: string) => {
    if (type === 'number') {
        return FilterColumnTypeEnum.NUMBER;
    }

    if (type === 'boolean') {
        return FilterColumnTypeEnum.BOOL;
    }

    if (type === 'date') {
        return FilterColumnTypeEnum.DATE;
    }

    if (type === 'enum') {
        return FilterColumnTypeEnum.ENUM;
    }

    return FilterColumnTypeEnum.STRING;
};

const filterSeparator = '&&;';
const filterParamsSeparator = '~';
const sorterSeparator = '&';
const sorterParamSeparator = '=';

const ColumnFilters = (props: IFilterProps) => {
    const { isDarkMode } = useTheme();
    const {
        filterColumn,
        withSearchParams = true,
        allFilters,
        setAllFilters,
        setQueryParams,
        searchParams,
        setSearchParams,
        openFilter,
        onToggleFilter,
    } = props;

    const defaultFilter = useMemo<IFilter>(() => ({
        id: filterColumn.id,
        column: filterColumn.name,
        operator: '',
        value: '',
        availableOperators: DROPDOWN_OPERATORS[filterColumn.columnType] || [],
        availableColumns: [ filterColumn ],
        inputType: filterColumn.columnType,
    }), [ filterColumn ]);

    const isOpen = useMemo(() => openFilter === filterColumn.name, [ filterColumn.name, openFilter ]);
    const selectedFilters = useMemo(() => allFilters[filterColumn.name] || [], [ allFilters, filterColumn.name ]);

    const [ filterAnchor, setFilterAnchor ] = useState<null | HTMLElement>(null);

    const setSelectedFilters = useCallback(
        (updatedFilter: IFilter, index?: number) => {
            setAllFilters((prevFilters) => {
                const existingFilters = prevFilters[filterColumn.name] || [];

                let updatedFilters;
                if (index !== undefined) {
                    updatedFilters = existingFilters.map((filter, idx) => idx === index
                        ? { ...filter, ...updatedFilter }
                        : filter);
                } else {
                    updatedFilters = [ updatedFilter, ...existingFilters ];
                }

                return {
                    ...prevFilters,
                    [filterColumn.name]: updatedFilters,
                };
            });
        },
        [ setAllFilters, filterColumn.name ],
    );

    useEffect(() => {
        if (withSearchParams && filterColumn.name &&
            !selectedFilters.some((filter) => filter.column === filterColumn.name &&
            filter.operator === defaultFilter.operator &&
            filter.value === defaultFilter.value)) {
            setSelectedFilters(defaultFilter);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    useEffect(() => {
        if (!searchParams || !setSearchParams || !setQueryParams) {
            return;
        }

        const formatFilterToString = (filter: IFilter) => filter.column && filter.operator && filter.value
            ? `${filter.id}${filterParamsSeparator}${filter.operator}${filterParamsSeparator}${filter.value}`.toLowerCase()
            : null;

        const filtersFormattedArray = Object.values(allFilters)
            .flat()
            .map(formatFilterToString)
            .filter(Boolean) as string[];

        const newFilterValue = filtersFormattedArray.join(filterSeparator);

        if (!filtersFormattedArray.length) {
            if (searchParams.has('filter')) {
                searchParams.delete('filter');
                setSearchParams(searchParams);
            }
        } else if (searchParams.get('filter') !== newFilterValue) {
            searchParams.set('filter', newFilterValue);
            setSearchParams(searchParams);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [ allFilters ]);

    const handleOpenClick = (event: React.MouseEvent<HTMLElement>) => {
        if (isOpen) {
            onToggleFilter(null);
            setFilterAnchor(null);
        } else {
            onToggleFilter(filterColumn.name);
            setFilterAnchor(event.currentTarget);
        }
    };

    const handleCloseClick = () => {
        onToggleFilter(null);
        setFilterAnchor(null);
    };

    const updateQueryParams = (filters?: IDictionary<Array<IFilter>>) => {
        if (!setQueryParams) {
            return;
        }

        // Use the passed filters if available, otherwise default to allFilters
        const filtersToUse = filters || allFilters;

        const formatFilterToString = (filter: IFilter) => filter.column && filter.operator && filter.value
            ? `${filter.id}${filterParamsSeparator}${filter.operator}${filterParamsSeparator}${filter.value}`.toLowerCase()
            : null;

        const filtersFormattedArray = Object.values(filtersToUse)
            .flat()
            .map(formatFilterToString)
            .filter(Boolean) as string[];

        const newFilterValue = filtersFormattedArray.join(filterSeparator);

        setQueryParams((currentParams) => ({
            ...currentParams,
            filter: newFilterValue || '',
        }));
    };

    const addFilter = () => {
        if (!setQueryParams) {
            return;
        }

        const newFilter: IFilter = {
            id: filterColumn.id,
            column: filterColumn.name,
            operator: '',
            value: '',
            availableOperators: DROPDOWN_OPERATORS[filterColumn.columnType] || [],
            availableColumns: [ filterColumn ],
            inputType: filterColumn.columnType,
        };

        setSelectedFilters(newFilter);

        updateQueryParams(allFilters);
    };

    const removeAllFilters = () => {
        setAllFilters((prevFilters) => {
            const updatedFilters = { ...prevFilters };
            delete updatedFilters[filterColumn.name];

            // Call updateQueryParams with the latest filters
            updateQueryParams(updatedFilters);

            return updatedFilters;
        });
    };

    const removeSingleFilter = (idx: number) => {
        setAllFilters((prevFilters) => {
            const filtersForColumn = prevFilters[filterColumn.name] || [];

            let updatedFilters: IDictionary<Array<IFilter>>;
            if (filtersForColumn.length <= 1) {
                // If only one filter exists, remove the entire key
                const { [filterColumn.name]: _, ...remainingFilters } = prevFilters;
                updatedFilters = remainingFilters;
            } else {
                // Remove only the selected filter
                updatedFilters = {
                    ...prevFilters,
                    [filterColumn.name]: filtersForColumn.filter((_, index) => index !== idx),
                };
            }

            // Call updateQueryParams with the latest filters
            updateQueryParams(updatedFilters);

            return updatedFilters;
        });
    };

    const updateFilterColumnData = (indexToUpdate: number, value: any, updateProperty: string) => {
        const filter = selectedFilters[indexToUpdate];

        if (!filter) { return; }

        setSelectedFilters({ ...filter, [updateProperty]: value }, indexToUpdate);
    };

    const renderInputField = (idx: number) => {
        const selectedFilter = selectedFilters[idx];

        const filterType = filterColumn.columnType;

        if (filterType === FilterColumnTypeEnum.BOOL) {
            return (
                <Dropdown
                  dropdownItems={BOOL_DROPDOWN_VALUES}
                  value={BOOL_DROPDOWN_VALUES.find((v) => v.name.toLowerCase() === (selectedFilter?.value ?? 'True')) ?? { id: '', name: '', value: '' }}
                  handleDropdownItemClick={(newValue) => updateFilterColumnData(idx, newValue?.value || '', 'value')}
                  minWidth={250}
                />
            );
        }

        if (filterType === FilterColumnTypeEnum.ENUM) {
            return (
                <Dropdown
                  dropdownItems={filterColumn.enumValues ?? []}
                  value={filterColumn.enumValues?.find((v) => v.name === selectedFilter?.value) ?? { id: '', name: '' }}
                  handleDropdownItemClick={(newValue) => updateFilterColumnData(idx, newValue?.name || '', 'value')}
                  minWidth={250}
                />
            );
        }

        if (filterType === FilterColumnTypeEnum.DATE) {
            return (
                <DateTimePicker
                  orientation="landscape"
                  value={getDateAsLocal(selectedFilter?.value)}
                  closeOnSelect={false}
                  onAccept={(newValue) => {
                      if (newValue) {
                          const formattedDate = newValue.toISOString();
                          updateFilterColumnData(idx, formattedDate, 'value');
                      }
                  }}
                  ampm={false}
                  views={[ 'year', 'month', 'day', 'hours', 'minutes', 'seconds' ]}
                  format="YYYY-MM-DD HH:mm:ss"
                  disabled={!selectedFilter.operator || idx > 0}
                  slots={{ textField: TextField }}
                  slotProps={{
                      textField: {
                          variant: 'standard',
                          InputProps: {
                              sx: {
                                  '& .MuiInputAdornment-root .MuiIconButton-root': { color: '#42abf8' },
                                  '& .MuiInputAdornment-root .MuiSvgIcon-root': { color: '#42abf8' },
                              },
                          },
                      },
                      desktopPaper: {
                          sx: {
                              backgroundColor: isDarkMode
                                  ? '#212328'
                                  : '#ffffff',
                              border: '2px solid #42abf8',
                              '& .MuiPickersDay-root': {
                                  color: isDarkMode
                                      ? '#f3f1f1'
                                      : '#687487',
                              },
                              '& .MuiPickersDay-root.Mui-selected': {
                                  backgroundColor: '#42abf8',
                                  color: '#ffffff',
                              },
                              '& .MuiDayCalendar-weekDayLabel': {
                                  color: isDarkMode
                                      ? '#f3f1f1'
                                      : '#687487',
                              },
                              '& .MuiPickersDay-today': { border: '1px solid #42abf8' },
                              '& .MuiTypography-root': {
                                  color: isDarkMode
                                      ? '#f3f1f1'
                                      : '#687487',
                              },
                              '& .MuiPickersCalendarHeader-label': {
                                  color: isDarkMode
                                      ? '#f3f1f1'
                                      : '#687487',
                              },
                              '& .MuiPickersArrowSwitcher-button': {
                                  color: isDarkMode
                                      ? '#f3f1f1'
                                      : '#687487',
                              },
                              '& .MuiClock-pin': { backgroundColor: '#42abf8' },
                              '& .MuiClockPointer-root': { backgroundColor: '#42abf8' },
                              '& .MuiClockPointer-thumb': {
                                  backgroundColor: '#42abf8',
                                  borderColor: '#42abf8',
                              },
                              '& .MuiClockNumber-root': {
                                  color: isDarkMode
                                      ? '#f3f1f1'
                                      : '#687487',
                              },
                              '& .MuiClockNumber-root.Mui-selected': {
                                  backgroundColor: '#42abf8',
                                  color: '#ffffff',
                              },
                              '& .MuiTabs-indicator': { backgroundColor: '#42abf8' },
                              '& .MuiTab-root': {
                                  color: isDarkMode
                                      ? '#f3f1f1'
                                      : '#687487',
                              },
                              '& .MuiTab-root.Mui-selected': { color: '#42abf8' },
                              '& .MuiPickersLayout-actionBar': { '& .MuiButton-root': { color: '#42abf8' } },
                          },
                      },
                      mobilePaper: {
                          sx: {
                              backgroundColor: isDarkMode
                                  ? '#212328'
                                  : '#ffffff',
                              border: '2px solid #42abf8',
                          },
                      },
                  }}
                />
            );
        }

        return (
            <TextField
              value={selectedFilter?.value}
              variant="standard"
              type={selectedFilter.inputType}
              onChange={(e) => updateFilterColumnData(idx, e.target.value, 'value')}
              disabled={!selectedFilter.operator || idx > 0}
            />
        );
    };

    const renderFilter = (idx: number) => (
        <Box style={{ display: 'flex', margin: '5px 0' }} key={`admin-filter-${idx}`} className={styles.fieldsContainer}>
            <div className={styles.title}>
                Filter #
                {selectedFilters.length - idx}
                <DeleteIcon
                  color="error"
                  onClick={() => removeSingleFilter(idx)}
                  className={concatClassNames(idx !== 0
                      ? ''
                      : styles.hidden, styles.removeFilterButton)}
                />
            </div>

            <Dropdown<ISingleColumnFilter>
              dropdownItems={selectedFilters[idx].availableColumns}
              value={filterColumn}
              minWidth={250}
              isDisabled
            />

            <Dropdown<IFiltersColumnOperators>
              dropdownItems={selectedFilters[idx]?.availableOperators ?? []}
              value={selectedFilters[idx].availableOperators?.find((op) => op.value === selectedFilters[idx].operator) ?? { id: '', name: '', value: '' }}
              handleDropdownItemClick={(newValue) => updateFilterColumnData(idx, newValue?.value || '', 'operator')}
              isDisabled={idx > 0}
              minWidth={250}
            />

            {renderInputField(idx)}
            {idx < selectedFilters.length - 1 && <hr className={styles.divider} />}
        </Box>
    );

    return (
        <Box>
            <Box sx={{ display: 'inline-flex', alignItems: 'center', cursor: 'pointer' }} onClick={handleOpenClick}>
                <FilterAltIcon sx={{ margin: '10px 0' }} />
                {' '}
                {selectedFilters.length > 1 && `(${selectedFilters.length - 1})`}
            </Box>
            <Popper
              className={`${styles.popupContainer} ${isDarkMode
                  ? styles.darkTheme
                  : styles.lightTheme} ${isOpen
                  ? styles.open
                  : ''}`}
              open={isOpen}
              anchorEl={filterAnchor}
              placement="bottom-start"
              modifiers={[
                  {
                      name: 'preventOverflow',
                      options: {
                          boundary: 'window',
                          padding: 10,
                      },
                  },
                  {
                      name: 'flip',
                      options: { fallbackPlacements: [ 'top-start', 'bottom-end' ] },
                  },
                  {
                      name: 'shift',
                      options: {
                          boundary: 'viewport',
                          padding: 5,
                      },
                  },
              ]}
            >
                <Box>
                    <CloseIcon className={styles.closeIcon} onClick={handleCloseClick} />
                    <Box style={{ display: 'flex', flexDirection: 'column' }}>
                        {selectedFilters.map((filter, idx) => renderFilter(idx))}
                    </Box>
                    <Box className={styles.buttonsSection}>
                        <Button onClick={removeAllFilters} className={styles.removeAllFilters}>Remove All</Button>
                        <Button
                          onClick={addFilter}
                          disabled={selectedFilters.length
                              ? !selectedFilters[0].value
                              : false}
                          className={styles.addFilter}
                        >
                            Add filter
                        </Button>
                    </Box>
                </Box>
            </Popper>

        </Box>
    );
};

/**
 * Maps columns from the grid to an array with property name, type and enum values (when there is a column of type 'enum').
 * @param {GridColDef& IEnumType} dataColumns The grid columns that will be mapped.
 * @returns {Array<IFilter>} Return the grid col definitions mapped to an easy to use object.
 */
const mapGridColumnsToAdministrationFilterProps =
    (dataColumns: Array<GridColDef & IEnumType>): ISingleColumnFilter[] => dataColumns.map((column) => {
        const mappedEnumType = mapStringToFilterColumnTypeEnum(column.type || '');
        return {
            id: column.headerName?.replace(/\s/g, '') ?? '',
            name: column.headerName?.replace(/\s/g, '') ?? '',
            columnType: mappedEnumType,
            enumValues: mappedEnumType === FilterColumnTypeEnum.ENUM
                ? column.enumValues?.map((value) => ({ id: value, name: value }))
                : null,
        } as ISingleColumnFilter;
    });

/**
 * Maps the filter parameter from the search bar to Array<IFilter> that will later be united with the default filter and will set the initial default filters.
 * @param {URLSearchParams} urlSearchParams Search params that will be mapped to IFilter.
 * @param {Array<string>} columns The sorting columns as an Array<ISingleColumnFilter>
 * @returns {Array<IFilter>} Return the filters from the search bar mapped as Array<IFilter>
 */
const mapUrlToFilters = (urlSearchParams: URLSearchParams | undefined, columns: Array<ISingleColumnFilter>): IDictionary<Array<IFilter>> => {
    if (!urlSearchParams) {
        return {};
    }

    const urlSelectedFilters: IDictionary<Array<IFilter>> = {};

    const filterParams = urlSearchParams.get('filter') ?? '';

    const urlParams = filterParams.split(filterSeparator).filter((param) => param);
    urlParams.forEach((param) => {
        const paramChunks = param.split(filterParamsSeparator).filter((chunk) => chunk);

        const columnValue = paramChunks[0];
        const operator = paramChunks[1];
        const value = paramChunks[2];

        const column = columns.find((c) => c.id.toLowerCase() === columnValue) || {
            name: '',
            id: '',
            columnType: FilterColumnTypeEnum.STRING,
        };

        const availableColumns = columns.filter((c) => !(urlSelectedFilters[column.name] || []).some((f) => f.column === c.name));
        const availableOperators = column?.columnType
            ? DROPDOWN_OPERATORS[column.columnType]
            : [];

        const filter: IFilter = {
            id: column?.id || '',
            column: column?.name || '',
            operator,
            value,
            availableOperators,
            availableColumns: [ ...availableColumns, { ...column } ],
            inputType: column?.columnType || FilterColumnTypeEnum.STRING,
        };

        // Add filter to the dictionary under its column name
        if (!urlSelectedFilters[column.name]) {
            urlSelectedFilters[column.name] = [];
        }

        urlSelectedFilters[column.name].push(filter);
    });

    return urlSelectedFilters;
};

/**
 * Sets the initial Selected filters.
 * @param {GridColDef[]} gridColDef All filterable columns for the certain grid.
 * @param {URLSearchParams} searchParams Search params that will be mapped to IFilter.
 * @param {string} filterToAdd The default filter that must be added.
 * @returns {Array<IFilter>} Return the initial selected filters that will appear in the search bar.
 */
const addDefaultFilter = (
    gridColDef: GridColDef[],
    searchParams: URLSearchParams,
    filterToAdd: string = defaultFilterToAdd,
): IDictionary<Array<IFilter>> => {
    const columns = mapGridColumnsToAdministrationFilterProps(gridColDef);
    const filters = mapUrlToFilters(searchParams, columns); // Now a dictionary

    const paramChunks = filterToAdd.split(filterParamsSeparator);
    const columnValue = paramChunks[0];
    const operator = paramChunks[1];
    const value = paramChunks[2];

    if (!columns.some((x) => x.name.toLowerCase() === columnValue)) {
        return filters; // No matching column found, return existing filters
    }

    const column = columns.find((c) => c.name.toLowerCase() === columnValue) || {
        id: columnValue, // Ensure a valid `id`
        name: columnValue,
        columnType: FilterColumnTypeEnum.STRING,
    };

    const availableColumns = columns.filter((c) => !Object.values(filters).flat().some((f) => f.column === c.name));
    const availableOperators = column?.columnType
        ? DROPDOWN_OPERATORS[column.columnType]
        : [];

    const newFilter: IFilter = {
        id: column.id,
        column: column.name,
        operator,
        value,
        availableOperators,
        availableColumns: [ ...availableColumns, column ], // Ensure `column` is always valid
        inputType: column.columnType,
    };

    // Initialize the key if it doesn't exist
    if (!filters[column.name]) {
        filters[column.name] = [];
    }

    // Ensure we don't duplicate the default filter
    const existingFilters = filters[column.name];
    if (!existingFilters.some((f) => f.column.toLowerCase() === columnValue.toLowerCase())) {
        filters[column.name] = [ newFilter, ...existingFilters ];
    }

    return { ...filters }; // Ensure immutability by returning a new object reference
};

const applyQueryParam = (
    separator: string,
    paramSeparator: string,
    param: string | null,
    skipDefault: boolean,
    defaultParamValue: string,
) => {
    if (!defaultParamValue) {
        return param || '';
    }

    const defaultParamArray = defaultParamValue.split(separator);
    const valueToReturn = param
        ? param.split(separator)
        : [];

    if (!param) {
        return skipDefault
            ? ''
            : defaultParamValue;
    }

    // Ensure default values are added only if they don't exist in `param`
    defaultParamArray.forEach((defaultValue) => {
        const prop = defaultValue.split(paramSeparator)[0];

        if (!valueToReturn.some((existing) => existing.startsWith(prop))) {
            valueToReturn.push(defaultValue);
        }
    });

    return valueToReturn.join(separator);
};

const applyDefaultFilterToQueryString = (
    defaultFilter: string,
    defaultSorter: string,
    searchParams?: URLSearchParams,
    skipDefault: boolean = false,
    page: number = 1,
    itemsPerPage: number = 6,
): IGetSubmissionsUrlParams => {
    let filter = searchParams?.get('filter') || '';
    let sorting = searchParams?.get('sorting') || '';

    filter = applyQueryParam(filterSeparator, filterParamsSeparator, filter, skipDefault, defaultFilter);
    sorting = applyQueryParam(sorterSeparator, sorterParamSeparator, sorting, skipDefault, defaultSorter);

    return {
        page,
        itemsPerPage,
        filter,
        sorting,
    } as IGetSubmissionsUrlParams;
};

export {
    type IFilter,
    mapGridColumnsToAdministrationFilterProps,
    addDefaultFilter,
    mapUrlToFilters,
    applyDefaultFilterToQueryString,
};

export default ColumnFilters;
