
import React from 'react';
import { createFilterOptions, debounce } from '@mui/material';
import { GridColDef } from '@mui/x-data-grid';

import { IHasNameAndIdType } from '../../../common/types';

type AdministrationGridColDef = {
    hidden?: boolean;
} & GridColDef;

const handleDateTimePickerChange = (name: string, newValue: any, onChange: Function) => {
    const event = {
        target: {
            name,
            value: newValue
                ? newValue.toString()
                : null,
        },
    };

    onChange(event);
};

/**
 * Allows search by id when added to autocomplete {filterOptions} property
 */
const autocompleteNameIdFormatFilterOptions = createFilterOptions({
    stringify: (option: IHasNameAndIdType) => {
        const { name, id } = option;

        return `${name} ${id}`;
    },
});

const handleAutocompleteChange = <T extends { [key: string]: any }>(
    name: string,
    newValue: T,
    propertyToSet: string,
    onChange: (event: { target: { name: string; value: any } }) => void,
) => {
    const value = newValue[propertyToSet];
    if (value !== undefined) {
        const event = {
            target: {
                name,
                value,
            },
        };
        onChange(event);
    }
};

const onAutocompleteInputChange = debounce((e: React.ChangeEvent<HTMLInputElement>, setStateFunc: React.SetStateAction<any>) => {
    setStateFunc(e.target.value);
}, 300);

export type { AdministrationGridColDef };

export {
    autocompleteNameIdFormatFilterOptions,
    handleDateTimePickerChange,
    handleAutocompleteChange,
    onAutocompleteInputChange,
};
