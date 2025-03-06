import React, { useState } from 'react';
import CloseIcon from '@mui/icons-material/Close';
import DeleteIcon from '@mui/icons-material/Delete';
import FilterAltIcon from '@mui/icons-material/FilterAlt';
import { Box, Button, IconButton, InputLabel, MenuItem, Popper, Select } from '@mui/material';
import { IFilterColumn } from 'src/common/types';
import FormControl from 'src/components/guidelines/forms/FormControl';
import useThemeMode from 'src/hooks/use-theme';

import { IAdministrationFilter } from '../ColumnFilters';

import styles from './FilterButton.module.scss';

interface IFilterButtonProps {
    selectedFilters: IAdministrationFilter[];
    setSelectedFilters: React.Dispatch<React.SetStateAction<IAdministrationFilter[]>>;
    filterColumns: IFilterColumn[];
    IAdm: string;
    searchParams?: URLSearchParams;
    setSearchParams?: (params: URLSearchParams) => void;
    withSearchParams?: boolean;
}

const FilterButton: React.FC<IFilterButtonProps> = ({
    selectedFilters,
    setSelectedFilters,
    filterColumns,
    searchParams,
    setSearchParams,
    withSearchParams = true,
    defaultFilter,
}) => {
    const [ anchorEl, setAnchorEl ] = useState<null | HTMLElement>(null);
    const filterOpen = Boolean(anchorEl);
    const { isDarkMode } = useThemeMode();

    const themeClass = isDarkMode
        ? styles.darkPopup
        : styles.lightPopup;

    const handleToggle = (event: React.MouseEvent<HTMLElement>) => {
        setAnchorEl(filterOpen
            ? null
            : event.currentTarget);
    };

    const updateFilterColumnData = (indexToUpdate: number, event: React.ChangeEvent<{ value: unknown }>, updateProperty: string) => {
        const newValue = event.target.value as string;
        const updatedFilters = selectedFilters.map((filter, idx) => idx === indexToUpdate
            ? { ...filter, [updateProperty]: newValue }
            : filter);

        setSelectedFilters(updatedFilters);
    };

    const removeSingleFilter = (idx: number) => {
        const updatedFilters = selectedFilters.filter((_, index) => index !== idx);
        setSelectedFilters(updatedFilters);

        if (updatedFilters.length === 0 && searchParams && setSearchParams && withSearchParams) {
            searchParams.delete('filter');
            setSearchParams(searchParams);
        }
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

    return (
        <Box>
            {/* Filter Button */}
            <IconButton onClick={handleToggle}>
                <FilterAltIcon />
            </IconButton>

            {/* Popper for Popup */}
            <Popper open={filterOpen} anchorEl={anchorEl} placement="bottom-start">
                <Box className={`${styles.filterPopup} ${themeClass}`}>
                    {/* Header with Close Button */}
                    <Box className={styles.closeButton}>
                        <IconButton size="small" onClick={() => setAnchorEl(null)}>
                            <CloseIcon fontSize="small" />
                        </IconButton>
                    </Box>

                    {/* Title */}
                    <Box className={styles.header}>
                        <strong>Filters</strong>
                    </Box>

                    {/* Render Filters */}
                    <Box className={styles.filtersList}>{selectedFilters.map((_, idx) => renderFilter(idx))}</Box>

                    {/* Actions */}
                    <Box className={styles.buttonsSection}>
                        <Button className={styles.addFilter} onClick={addFilter} disabled={selectedFilters.length && !selectedFilters[0].value}>
                            Add Filter
                        </Button>
                        <Button className={styles.removeAll} onClick={removeAllFilters}>
                            Remove All
                        </Button>
                    </Box>
                </Box>
            </Popper>
        </Box>
    );
};

export default FilterButton;
