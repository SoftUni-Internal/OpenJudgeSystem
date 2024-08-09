/* eslint-disable @typescript-eslint/ban-types */
import { useEffect, useState } from 'react';
import { Autocomplete, debounce, FormControl, FormGroup, InputLabel, MenuItem, Select, TextField, Typography } from '@mui/material';
import isNaN from 'lodash/isNaN';

import { ProblemGroupTypes } from '../../../../common/enums';
import { ID, ORDER_BY, TYPE } from '../../../../common/labels';
import { IContestAutocomplete } from '../../../../common/types';
import useDelayedSuccessEffect from '../../../../hooks/common/use-delayed-success-effect';
import useDisableMouseWheelOnNumberInputs from '../../../../hooks/common/use-disable-mouse-wheel-on-number-inputs';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { useGetContestAutocompleteQuery } from '../../../../redux/services/admin/contestsAdminService';
import { useCreateProblemGroupMutation, useGetProblemGroupByIdQuery, useUpdateProblemGroupMutation } from '../../../../redux/services/admin/problemGroupsAdminService';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import AdministrationFormButtons from '../../common/administration-form-buttons/AdministrationFormButtons';
import { IProblemGroupAdministrationModel } from '../types';

// The classes are used in multiple files. But not all of them are used in single file
// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';

interface IProblemFormProps {
    id?:number;
    isEditMode?: boolean;
    onSuccess?: Function;
    setParentSuccessMessage?: Function;
}

const ProblemGroupForm = (props: IProblemFormProps) => {
    const { id = null, isEditMode = true, onSuccess, setParentSuccessMessage } = props;
    const [ currentProblemGroup, setCurrentProblemGroup ] = useState<IProblemGroupAdministrationModel>({
        id: 0,
        orderBy: 0,
        type: '',
        contest: {
            id: 0,
            name: '',
        } as IContestAutocomplete,
    });
    const [ contestsData, setContestsData ] = useState <Array<IContestAutocomplete>>([]);
    const [ contestSearchString, setContestSearchString ] = useState<string>('');
    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);

    const { data: contestsAutocompleteData, error: getContestDataError } = useGetContestAutocompleteQuery(contestSearchString);
    const {
        data: problemGroupData,
        error: getProblemGroupError,
        isLoading: isGettingProblemGroupData,
    } = useGetProblemGroupByIdQuery(id!, { skip: !isEditMode || !id });
    const [
        updateProblemGroup,
        {
            data: updateData,
            error: updateError,
            isLoading: isUpdating,
            isSuccess: isSuccessfullyUpdated,
        } ] = useUpdateProblemGroupMutation();

    const [
        createProblemGroup,
        {
            data: createData,
            isLoading: isCreating,
            error: createError,
            isSuccess: isSuccessfullyCreated,
        } ] = useCreateProblemGroupMutation();

    useDisableMouseWheelOnNumberInputs();

    useDelayedSuccessEffect({ isSuccess: isSuccessfullyUpdated || isSuccessfullyCreated, onSuccess });

    useSuccessMessageEffect({
        data: [
            { message: createData, shouldGet: isSuccessfullyCreated },
            { message: updateData, shouldGet: isSuccessfullyUpdated },
        ],
        setParentSuccessMessage,
        setSuccessMessage,
    });

    useEffect(() => {
        if (contestsAutocompleteData) {
            if (problemGroupData) {
                const actualContestData = [ problemGroupData?.contest ] as Array<IContestAutocomplete>;
                setContestsData(contestsAutocompleteData.concat(actualContestData));
            }
            setContestsData(contestsAutocompleteData);
        }
    }, [ contestsAutocompleteData, problemGroupData ]);

    useEffect(() => {
        if (problemGroupData) {
            setCurrentProblemGroup(problemGroupData);
        }
    }, [ problemGroupData ]);

    useEffect(() => {
        getAndSetExceptionMessage([ getContestDataError, createError, updateError, getProblemGroupError ], setErrorMessages);
        setSuccessMessage(null);
    }, [ updateError, createError, getContestDataError, getProblemGroupError ]);

    const onAutocompleteChange = debounce((e: any) => {
        if (!e) {
            return;
        }
        setContestSearchString(e.target.value || '');
    }, 300);

    const onChange = (e: any) => {
        const { target } = e;
        const { name, type, value, checked } = target;
        setCurrentProblemGroup((prevState) => ({
            ...prevState,
            [name]: type === 'checkbox'
                ? checked
                : type === 'number'
                    ? value === ''
                        ? ''
                        : Number(value)
                    : value,
        }));
    };

    const onSelect = (contest: IContestAutocomplete) => {
        setCurrentProblemGroup((prevState) => ({
            ...prevState,
            contest,
        }));
    };

    if (isGettingProblemGroupData || isUpdating || isCreating) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderErrorMessagesAlert(errorMessages)}
            {renderSuccessfullAlert(successMessage)}
            <form className={formStyles.form}>
                <Typography variant="h4" className="centralize">
                    Problem Group Administration Form
                </Typography>
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      label={ID}
                      value={currentProblemGroup?.id}
                      InputLabelProps={{ shrink: true }}
                      type="text"
                      disabled
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      label={ORDER_BY}
                      value={currentProblemGroup?.orderBy}
                      InputLabelProps={{ shrink: true }}
                      type="number"
                      name="orderBy"
                      onChange={(e) => onChange(e)}
                    />
                </FormControl>
                <FormGroup className={formStyles.inputRow}>
                    <InputLabel id="problemGroupType">{TYPE}</InputLabel>
                    <Select
                      onChange={(e) => onChange(e)}
                      onBlur={(e) => onChange(e)}
                      labelId="problemGroupType"
                      value={currentProblemGroup.type || 'None'}
                      name="type"
                    >
                        {Object.keys(ProblemGroupTypes).filter((key) => isNaN(Number(key))).map((key) => (
                            <MenuItem key={key} value={key}>
                                {key}
                            </MenuItem>
                        ))}
                    </Select>
                </FormGroup>
                <FormControl className={formStyles.inputRow}>
                    <Autocomplete
                      options={contestsData!}
                      renderInput={(params) => <TextField {...params} label="Select Contest" key={params.id} />}
                      onChange={(event, newValue) => onSelect(newValue!)}
                      onInputChange={(event) => onAutocompleteChange(event)}
                      value={currentProblemGroup.contest.id
                          ? currentProblemGroup.contest
                          : null}
                      isOptionEqualToValue={(option, value) => option.id === value.id && option.name === value.name}
                      getOptionLabel={(option) => option?.name}
                      renderOption={(properties, option) => (
                          <MenuItem {...properties} key={option.id} value={option.id}>
                              {option.name}
                          </MenuItem>
                      )}
                    />
                </FormControl>
                <AdministrationFormButtons
                  isEditMode={isEditMode}
                  onCreateClick={() => createProblemGroup(currentProblemGroup)}
                  onEditClick={() => updateProblemGroup(currentProblemGroup)}
                />
            </form>
        </>

    );
};

export default ProblemGroupForm;
