/* eslint-disable max-len */
/* eslint-disable @typescript-eslint/no-use-before-define */
/* eslint-disable import/group-exports */
import React, { Dispatch, SetStateAction, useCallback, useEffect, useMemo, useState } from 'react';
import { NavigateOptions, URLSearchParamsInit } from 'react-router-dom';
// eslint-disable-next-line import/no-extraneous-dependencies
import CloseIcon from '@mui/icons-material/Close';
import DeleteIcon from '@mui/icons-material/Delete';
import FilterAltIcon from '@mui/icons-material/FilterAlt';
import { Box, Button, Popper, TextField } from '@mui/material';
import { GridColDef } from '@mui/x-data-grid';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { IDictionary } from 'src/common/common-types';
import { FilterColumnTypeEnum } from 'src/common/enums';
import { IEnumType, IFilterColumn } from 'src/common/types';
import { IGetSubmissionsUrlParams } from 'src/common/url-types';
import Dropdown from 'src/components/guidelines/dropdown/Dropdown';
import concatClassNames from 'src/utils/class-names';
import { getDateAsLocal } from 'src/utils/dates';

import useTheme from '../../hooks/use-theme';

import styles from './Filter.module.scss';

interface IFiltersColumnOperators {
    name: string;
    value: string;
    id: string;
}

interface IFilterProps {
    filterColumn: IFilterColumn;
    searchParams?: URLSearchParams;
    setSearchParams?: (params: URLSearchParamsInit, navigateOpts?: NavigateOptions) => void;
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
    availableColumns: IFilterColumn[];
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
        // Commented out because it does not work
        // { name: 'Equals', id: 'Equals', value: 'equals' },
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

const Filter = (props: IFilterProps) => {
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
        if (!setQueryParams || !setSearchParams) {
            return;
        }

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
            page: 1,
        }));

        const newParams = new URLSearchParams(searchParams);
        newParams.set('filter', newFilterValue || '');
        newParams.set('page', '1');

        setSearchParams(Object.fromEntries(newParams));
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
        const newFilter: IFilter = {
            id: filterColumn.id,
            column: filterColumn.name,
            operator: '',
            value: '',
            availableOperators: DROPDOWN_OPERATORS[filterColumn.columnType] || [],
            availableColumns: [ filterColumn ],
            inputType: filterColumn.columnType,
        };

        setAllFilters((prevFilters) => {
            const updatedFilters = { ...prevFilters };
            delete updatedFilters[filterColumn.name];

            updateQueryParams(updatedFilters);

            return { ...updatedFilters, [newFilter.column]: [ newFilter ] };
        });
    };

    const removeSingleFilter = (idx: number) => {
        setAllFilters((prevFilters) => {
            const filtersForColumn = prevFilters[filterColumn.name] || [];

            let updatedFilters: IDictionary<Array<IFilter>>;
            if (filtersForColumn.length <= 1) {
                // eslint-disable-next-line @typescript-eslint/no-unused-vars
                const { [filterColumn.name]: _, ...remainingFilters } = prevFilters;
                updatedFilters = remainingFilters;
            } else {
                updatedFilters = {
                    ...prevFilters,
                    [filterColumn.name]: filtersForColumn.filter((_, index) => index !== idx),
                };
            }

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
                      popper: {
                          placement: 'bottom-start',
                          disablePortal: true,
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

            <Dropdown<IFilterColumn>
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

const mapGridColumnsToAdministrationFilterProps =
    (dataColumns: Array<GridColDef & IEnumType>): IFilterColumn[] => dataColumns.map((column) => {
        const mappedEnumType = mapStringToFilterColumnTypeEnum(column.type || '');
        return {
            id: column.headerName?.replace(/\s/g, '') ?? '',
            name: column.headerName?.replace(/\s/g, '') ?? '',
            columnType: mappedEnumType,
            enumValues: mappedEnumType === FilterColumnTypeEnum.ENUM
                ? column.enumValues?.map((value) => ({ id: value, name: value }))
                : null,
        } as IFilterColumn;
    });

const mapUrlToFilters = (urlSearchParams: URLSearchParams | undefined, columns: Array<IFilterColumn>): IDictionary<Array<IFilter>> => {
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

        if (!urlSelectedFilters[column.name]) {
            urlSelectedFilters[column.name] = [];
        }

        urlSelectedFilters[column.name].push(filter);
    });

    return urlSelectedFilters;
};

const applyDefaultQueryValues = (
    searchParams: URLSearchParams,
    page?: number,
    itemsPerPage: number = 6,
): IGetSubmissionsUrlParams => ({
    page: page ?? searchParams.get('page')
        ? parseInt(searchParams.get('page')!, 10)
        : 1,
    itemsPerPage,
    filter: '',
    sorting: '',
} as IGetSubmissionsUrlParams);

const handlePageChange = (
    setQueryParams: Dispatch<React.SetStateAction<IGetSubmissionsUrlParams>>,
    setSearchParams: (params: URLSearchParamsInit, navigateOpts?: NavigateOptions) => void,
    newPage: number,
) => {
    setQueryParams((prev) => {
        const updatedParams = { ...prev, page: newPage };

        const newParams = new URLSearchParams();
        Object.entries(updatedParams).forEach(([ key, value ]) => {
            if (value !== undefined && value !== null) {
                newParams.set(key, value.toString());
            }
        });

        setSearchParams(newParams);
        return updatedParams;
    });
};

export {
    type IFilter,
    mapGridColumnsToAdministrationFilterProps,
    mapUrlToFilters,
    applyDefaultQueryValues,
    handlePageChange,
};

export default Filter;
