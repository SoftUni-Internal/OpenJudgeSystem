/* eslint-disable @typescript-eslint/ban-types */
import React, { useEffect, useState } from 'react';
import { Autocomplete, Box, Checkbox, FormControl, FormControlLabel, MenuItem, TextField, Typography } from '@mui/material';

import {
    IContestCategories,
    IContestCategoryAdministration,
} from '../../../common/types';
import useDelayedSuccessEffect from '../../../hooks/common/use-delayed-success-effect';
import useDisableMouseWheelOnNumberInputs from '../../../hooks/common/use-disable-mouse-wheel-on-number-inputs';
import useSuccessMessageEffect from '../../../hooks/common/use-success-message-effect';
import {
    useCreateContestCategoryMutation,
    useGetCategoriesQuery,
    useGetContestCategoryByIdQuery, useUpdateContestCategoryByIdMutation,
} from '../../../redux/services/admin/contestCategoriesAdminService';
import { getAndSetExceptionMessage } from '../../../utils/messages-utils';
import { renderErrorMessagesAlert } from '../../../utils/render-utils';
import SpinningLoader from '../../guidelines/spinning-loader/SpinningLoader';
import AdministrationFormButtons from '../common/administration-form-buttons/AdministrationFormButtons';

import styles from './CategoryEdit.module.scss';

interface IContestCategoryEditProps {
    contestCategoryId: number | null;
    isEditMode?: boolean;
    setParentSuccessMessage: Function;
    onSuccess: Function;
}

const initialState : IContestCategoryAdministration = {
    id: 0,
    name: '',
    parent: null,
    parentId: null,
    isDeleted: false,
    isVisible: false,
    orderBy: 0,
    deletedOn: null,
    modifiedOn: null,
};

const ContestCategoryEdit = (props:IContestCategoryEditProps) => {
    const { contestCategoryId, isEditMode = true, onSuccess, setParentSuccessMessage } = props;

    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ isValidForm, setIsValidForm ] = useState<boolean>(!!isEditMode);

    const [ contestCategory, setContestCategory ] = useState<IContestCategoryAdministration>(initialState);

    const [ contestCategoryValidations, setContestCategoryValidations ] = useState({
        isNameTouched: false,
        isNameValid: !!isEditMode,
        isOrderByTouched: false,
        isOrderByValid: true,
    });

    const { data, isFetching, isLoading } = useGetContestCategoryByIdQuery({ id: Number(contestCategoryId) }, { skip: !isEditMode });
    const { isFetching: isGettingCategories, data: contestCategories } = useGetCategoriesQuery(null);

    const [
        updateContestCategory, {
            data: updateData,
            isLoading: isUpdating,
            isSuccess: isSuccessfullyUpdated,
            error: updateError,
        } ] = useUpdateContestCategoryByIdMutation();

    const [
        createContestCategory, {
            data: createData,
            isSuccess: isSuccessfullyCreated,
            error: createError,
            isLoading: isCreating,
        } ] = useCreateContestCategoryMutation();

    useDisableMouseWheelOnNumberInputs();

    useDelayedSuccessEffect({ isSuccess: isSuccessfullyUpdated || isSuccessfullyCreated, onSuccess });

    useSuccessMessageEffect({
        data: [
            { message: createData, shouldGet: isSuccessfullyCreated },
            { message: updateData, shouldGet: isSuccessfullyUpdated },
        ],
        setParentSuccessMessage,
    });

    useEffect(
        () => {
            if (data) {
                setContestCategory(data);
            }
        },
        [ data ],
    );

    useEffect(() => {
        if (isSuccessfullyUpdated) {
            setErrorMessages([]);
        }

        if (isSuccessfullyCreated) {
            setErrorMessages([]);
        }
    }, [ isSuccessfullyCreated, isSuccessfullyUpdated ]);

    useEffect(() => {
        getAndSetExceptionMessage([ createError, updateError ], setErrorMessages);
    }, [ createError, updateError ]);

    useEffect(() => () => {
        setContestCategory(initialState);
    }, []);
    const validateForm = () => {
        const isValid = contestCategoryValidations.isNameValid &&
            contestCategoryValidations.isOrderByValid;

        setIsValidForm(isValid);
    };

    const onChange = (e: any) => {
        const { name, value, checked } = e.target;
        let {
            name: contestCategoryName,
            parentId,
            parent,
            isVisible,
            orderBy,
        } = contestCategory;
        const contestCategoryValidations1 = contestCategoryValidations;
        // eslint-disable-next-line default-case
        switch (name) {
        case 'name':
            contestCategoryName = value;
            contestCategoryValidations1.isNameTouched = true;
            contestCategoryValidations1.isNameValid = true;
            if (value.length < 2 || value.length > 100) {
                contestCategoryValidations1.isNameValid = false;
            }
            break;
        case 'orderBy':
            contestCategoryValidations1.isOrderByTouched = true;
            contestCategoryValidations1.isOrderByValid = true;
            orderBy = value;
            if (value < 0) {
                contestCategoryValidations1.isOrderByValid = false;
            }
            break;
        case 'isVisible':
            isVisible = checked;
            break;
        case 'parent': {
            const category = contestCategories?.find((cc) => cc.id === value);
            if (category) {
                const { id, name: parentName } = category;
                parentId = id;
                parent = parentName;
            } else {
                parentId = null;
                parent = null;
            }
            break;
        }
        }
        setContestCategoryValidations(contestCategoryValidations1);
        setContestCategory((prevState) => ({
            ...prevState,
            name: contestCategoryName,
            parent,
            parentId,
            isVisible,
            orderBy,
        }));
        validateForm();
    };

    const handleAutocompleteChange = (name: string, newValue:IContestCategories) => {
        const event = {
            target: {
                name,
                value: newValue?.id,
            },
        };
        onChange(event);
    };

    const edit = () => {
        if (isValidForm) {
            updateContestCategory(contestCategory);
        }
    };

    const create = () => {
        if (isValidForm) {
            createContestCategory(contestCategory);
        }
    };

    return (
        isFetching || isLoading || isGettingCategories || isCreating || isUpdating
            ? <SpinningLoader />
            : (
                <div className={`${styles.flex}`}>
                    {renderErrorMessagesAlert(errorMessages)}
                    <Typography className={styles.centralize} variant="h4">
                        {isEditMode
                            ? contestCategory.name
                            : 'Contest Category form'}
                    </Typography>
                    <form className={`${styles.form}`}>
                        <Box>
                            <TextField
                              className={styles.inputRow}
                              label="Name"
                              variant="standard"
                              name="name"
                              onChange={(e) => onChange(e)}
                              value={contestCategory.name}
                              color={contestCategoryValidations.isNameValid && contestCategoryValidations.isNameTouched
                                  ? 'success'
                                  : 'primary'}
                              error={(contestCategoryValidations.isNameTouched && !contestCategoryValidations.isNameValid)}
                                    // eslint-disable-next-line max-len
                              helperText={(contestCategoryValidations.isNameTouched && !contestCategoryValidations.isNameValid) && 'Category name length must be between 2 and 100 characters long'}
                            />
                            <TextField
                              className={styles.inputRow}
                              type="number"
                              label="Order By"
                              variant="standard"
                              value={contestCategory.orderBy}
                              onChange={(e) => onChange(e)}
                              InputLabelProps={{ shrink: true }}
                              name="orderBy"
                              color={contestCategoryValidations.isOrderByValid && contestCategoryValidations.isOrderByTouched
                                  ? 'success'
                                  : 'primary'}
                              error={(contestCategoryValidations.isOrderByTouched && !contestCategoryValidations.isOrderByValid)}
                                    // eslint-disable-next-line max-len
                              helperText={(contestCategoryValidations.isOrderByTouched && !contestCategoryValidations.isOrderByValid) && 'Order by cannot be less than 0'}
                            />
                            <FormControlLabel
                              sx={{ marginTop: '1rem' }}
                              control={<Checkbox checked={contestCategory.isVisible} />}
                              label="IsVisible"
                              name="isVisible"
                              onChange={(e) => onChange(e)}
                            />
                            <FormControl className={styles.textArea} sx={{ margin: '15px 0' }}>
                                <Autocomplete
                                  sx={{ width: '100%' }}
                                  className={styles.inputRow}
                                  onChange={(event, newValue) => handleAutocompleteChange('parent', newValue!)}
                                  value={contestCategories?.find((category) => category.id === contestCategory.parentId) ?? null}
                                  options={contestCategories!}
                                  renderInput={(params) => <TextField {...params} label="Parent Category" key={params.id} />}
                                  getOptionLabel={(option) => option?.name}
                                  renderOption={(properties, option) => (
                                      <MenuItem {...properties} key={option.id} value={option.id}>
                                          {option.name}
                                      </MenuItem>
                                  )}
                                />
                            </FormControl>
                        </Box>
                    </form>
                    <AdministrationFormButtons
                      isEditMode={isEditMode}
                      onCreateClick={() => create()}
                      onEditClick={() => edit()}
                      disabled={!isValidForm}
                    />
                </div>
            )
    );
};

export default ContestCategoryEdit;
