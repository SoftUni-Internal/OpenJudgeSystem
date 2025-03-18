/* eslint-disable max-len */
/* eslint-disable @typescript-eslint/no-use-before-define */
/* eslint-disable import/group-exports */
import React, { Dispatch, SetStateAction, useCallback, useEffect, useMemo, useState } from 'react';
import { NavigateOptions, URLSearchParamsInit } from 'react-router-dom';
// eslint-disable-next-line import/no-extraneous-dependencies
import CloseIcon from '@mui/icons-material/Close';
import DeleteIcon from '@mui/icons-material/Delete';
import FilterAltIcon from '@mui/icons-material/FilterAlt';
import { Box, Button, Popper, TextField, TextFieldProps, Typography } from '@mui/material';
import { GridColDef } from '@mui/x-data-grid';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { IDictionary } from 'src/common/common-types';
import { FilterColumnTypeEnum } from 'src/common/enums';
import { IEnumType, IFilterColumn } from 'src/common/types';
import { IGetSubmissionsUrlParams } from 'src/common/url-types';
import Dropdown from 'src/components/guidelines/dropdown/Dropdown';
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

    const renderTextField = (params: TextFieldProps, commonTextFieldSx: object) => (
        <TextField
          {...params}
          variant="standard"
          sx={commonTextFieldSx}
        />
    );

    const renderInputField = (idx: number) => {
        const selectedFilter = selectedFilters[idx];
        const commonTextFieldSx = {
            width: '100%',
            '& .MuiInputBase-root': {
                fontSize: '0.9rem',
                width: '100%',
            },
            '& .MuiInputLabel-root': { fontSize: '0.9rem' },
        };

        const filterType = filterColumn.columnType;

        if (filterType === FilterColumnTypeEnum.BOOL) {
            return (
                <Dropdown
                  dropdownItems={BOOL_DROPDOWN_VALUES}
                  value={BOOL_DROPDOWN_VALUES.find((v) => v.name.toLowerCase() === (selectedFilter?.value ?? 'True')) ?? { id: '', name: '', value: '' }}
                  handleDropdownItemClick={(newValue) => updateFilterColumnData(idx, newValue?.value || '', 'value')}
                  minWidth={0}
                  placeholder="Select value"
                />
            );
        }

        if (filterType === FilterColumnTypeEnum.ENUM) {
            return (
                <Dropdown
                  dropdownItems={filterColumn.enumValues ?? []}
                  value={filterColumn.enumValues?.find((v) => v.name === selectedFilter?.value) ?? { id: '', name: '' }}
                  handleDropdownItemClick={(newValue) => updateFilterColumnData(idx, newValue?.name || '', 'value')}
                  minWidth={0}
                  placeholder="Select value"
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
                          setTimeout(() => {
                              const formattedDate = newValue.toISOString();
                              updateFilterColumnData(idx, formattedDate, 'value');
                          }, 300);
                      }
                  }}
                  ampm={false}
                  views={[ 'year', 'month', 'day', 'hours', 'minutes', 'seconds' ]}
                  format="YYYY-MM-DD HH:mm:ss"
                  disabled={!selectedFilter.operator || idx > 0}
                  slots={{ textField: (params) => renderTextField(params, commonTextFieldSx) }}
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
                              position: 'fixed',
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
                              borderRadius: '12px',
                              '& .MuiPickersLayout-root': { borderRadius: '12px' },
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
              sx={commonTextFieldSx}
              placeholder={`Enter ${filterColumn.name}`}
            />
        );
    };

    const renderFilter = (idx: number) => (
        <Box
          sx={{
              backgroundColor: isDarkMode
                  ? 'rgba(243, 241, 241, 0.03)'
                  : 'rgba(62, 76, 93, 0.03)',
              borderRadius: '0.5rem',
              padding: '1rem',
              marginBottom: '0.75rem',
              border: `1px solid ${isDarkMode
                  ? 'rgba(243, 241, 241, 0.1)'
                  : 'rgba(62, 76, 93, 0.1)'}`,
              position: 'relative',
          }}
          key={`admin-filter-${idx}`}
          className={styles.fieldsContainer}
        >
            <Box sx={{
                display: 'grid',
                gridTemplateColumns: {
                    xs: '1fr',
                    sm: '1fr 1fr',
                    md: '1fr 1fr 1fr 2.5rem',
                },
                gap: '0.75rem',
                alignItems: 'center',
                '& > *': {
                    minWidth: 'unset',
                    width: '100%',
                },
            }}
            >
                <TextField
                  value={filterColumn.name}
                  variant="standard"
                  disabled
                  sx={{
                      '& .MuiInputBase-root': {
                          fontSize: '0.875rem',
                          color: isDarkMode
                              ? '#f3f1f1'
                              : '#3e4c5d',
                          '&.Mui-disabled': {
                              color: isDarkMode
                                  ? '#f3f1f1'
                                  : '#3e4c5d',
                          },
                      },
                  }}
                />

                <Dropdown<IFiltersColumnOperators>
                  dropdownItems={selectedFilters[idx]?.availableOperators ?? []}
                  value={selectedFilters[idx].availableOperators?.find((op) => op.value === selectedFilters[idx].operator) ?? { id: '', name: '', value: '' }}
                  handleDropdownItemClick={(newValue) => updateFilterColumnData(idx, newValue?.value || '', 'operator')}
                  isDisabled={idx > 0}
                  minWidth={0}
                  placeholder="Select operator"
                />

                {renderInputField(idx)}

                <Box sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    height: '100%',
                    width: '2.5rem !important',
                }}
                >
                    {idx !== 0 && (
                        <DeleteIcon
                          sx={{
                              color: '#ef5350',
                              cursor: 'pointer',
                              fontSize: '1.25rem',
                              '&:hover': { color: '#f44336' },
                          }}
                          onClick={() => removeSingleFilter(idx)}
                        />
                    )}
                </Box>
            </Box>
        </Box>
    );

    return (
        <Box>
            <Box
              sx={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  cursor: 'pointer',
                  padding: '0.375rem 0.75rem',
                  borderRadius: '0.375rem',
                  transition: 'all 0.2s ease',
                  border: '2px solid transparent',
                  backgroundColor: 'transparent',
                  '&:hover': {
                      backgroundColor: 'rgba(255, 255, 255, 0.05)',
                      border: '2px solid transparent',
                      transform: 'translateY(-1px)',
                  },
              }}
              onClick={handleOpenClick}
            >
                <FilterAltIcon
                  sx={{
                      margin: '0.25rem',
                      fontSize: '1.25rem',
                      color: isDarkMode
                          ? selectedFilters.length > 1
                              ? '#42abf8'
                              : '#f3f1f1'
                          : '#f3f1f1',
                      transition: 'all 0.2s ease',
                      transform: isOpen
                          ? 'rotate(180deg)'
                          : 'rotate(0)',
                  }}
                />
                {selectedFilters.length > 1 && (
                    <Box
                      sx={{
                          backgroundColor: isDarkMode
                              ? '#42abf8'
                              : 'white',
                          color: isDarkMode
                              ? 'white'
                              : '#42abf8',
                          border: isDarkMode
                              ? 'none'
                              : '1px solid #42abf8',
                          borderRadius: '0.75rem',
                          padding: '0.125rem 0.5rem',
                          fontSize: '0.75rem',
                          marginLeft: '0.25rem',
                          minWidth: '1.25rem',
                          textAlign: 'center',
                          fontWeight: 600,
                      }}
                    >
                        {selectedFilters.length - 1}
                    </Box>
                )}
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
                      name: 'computeStyles',
                      options: {
                          adaptive: false,
                          gpuAcceleration: false,
                      },
                  },
                  {
                      name: 'preventOverflow',
                      options: {
                          boundary: 'window',
                          padding: '1rem',
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
                          padding: '1rem',
                      },
                  },
              ]}
              transition={false}
              sx={{
                  maxWidth: {
                      xs: 'calc(100vw - 2rem)',
                      sm: '31.25rem',
                      md: '37.5rem',
                  },
                  width: '100%',
              }}
            >
                <Box sx={{
                    width: '100%',
                    backgroundColor: isDarkMode
                        ? '#212328'
                        : '#ffffff',
                    borderRadius: '0.5rem',
                    boxShadow: '0 0.25rem 1.25rem rgba(0, 0, 0, 0.15)',
                    border: `1px solid ${isDarkMode
                        ? 'rgba(243, 241, 241, 0.1)'
                        : 'rgba(62, 76, 93, 0.1)'}`,
                }}
                >
                    <Box sx={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'center',
                        padding: '0.75rem 1rem',
                        borderBottom: `1px solid ${isDarkMode
                            ? 'rgba(243, 241, 241, 0.1)'
                            : 'rgba(62, 76, 93, 0.1)'}`,
                    }}
                    >
                        <Typography
                          variant="subtitle1"
                          sx={{
                              fontWeight: 600,
                              color: isDarkMode
                                  ? '#f3f1f1'
                                  : '#3e4c5d',
                              fontSize: '0.875rem',
                          }}
                        >
                            {filterColumn.name}
                            {' '}
                            Filters
                        </Typography>
                        <CloseIcon
                          className={styles.closeIcon}
                          onClick={handleCloseClick}
                          sx={{
                              cursor: 'pointer',
                              fontSize: '1.25rem',
                          }}
                        />
                    </Box>
                    <Box sx={{
                        padding: '1rem',
                        display: 'flex',
                        flexDirection: 'column',
                        gap: '0.5rem',
                        maxHeight: '60vh',
                        overflowY: 'auto',
                    }}
                    >
                        {selectedFilters.map((filter, idx) => renderFilter(idx))}
                    </Box>
                    <Box
                      className={styles.buttonsSection}
                      sx={{
                          display: 'flex',
                          justifyContent: 'space-between',
                          padding: '0.75rem 1rem',
                          borderTop: `1px solid ${isDarkMode
                              ? 'rgba(243, 241, 241, 0.1)'
                              : 'rgba(62, 76, 93, 0.1)'}`,
                          gap: '0.5rem',
                      }}
                    >
                        <Button
                          onClick={removeAllFilters}
                          className={styles.removeAllFilters}
                          startIcon={<DeleteIcon />}
                          sx={{
                              color: '#ef5350',
                              fontSize: '0.875rem',
                              '&:hover': { backgroundColor: 'rgba(239, 83, 80, 0.08)' },
                          }}
                        >
                            Remove All
                        </Button>
                        <Button
                          onClick={addFilter}
                          disabled={selectedFilters.length
                              ? !selectedFilters[0].value
                              : false}
                          className={styles.addFilter}
                          sx={{
                              backgroundColor: '#42abf8',
                              color: 'white',
                              fontSize: '0.875rem',
                              '&:hover': { backgroundColor: '#3b9ae0' },
                              '&:disabled': {
                                  backgroundColor: isDarkMode
                                      ? 'rgba(243, 241, 241, 0.1)'
                                      : 'rgba(62, 76, 93, 0.1)',
                                  color: isDarkMode
                                      ? '#f3f1f1'
                                      : '#3e4c5d',
                              },
                          }}
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
