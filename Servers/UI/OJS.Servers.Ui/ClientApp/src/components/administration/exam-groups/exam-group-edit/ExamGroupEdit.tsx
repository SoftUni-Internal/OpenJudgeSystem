/* eslint-disable @typescript-eslint/ban-types */
/* eslint-disable no-case-declarations */
/* eslint-disable no-undefined */
import { useEffect, useState } from 'react';
import { Autocomplete, FormControl, MenuItem, TextField, Typography } from '@mui/material';

import {
    IContestAutocomplete,
    IContestCategories,
    IExamGroupAdministration,
} from '../../../../common/types';
import useDelayedSuccessEffect from '../../../../hooks/common/use-delayed-success-effect';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { useGetContestAutocompleteQuery } from '../../../../redux/services/admin/contestsAdminService';
import {
    useCreateExamGroupMutation, useGetExamGroupByIdQuery,
    useUpdateExamGroupMutation,
} from '../../../../redux/services/admin/examGroupsAdminService';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import AdministrationFormButtons from '../../common/administration-form-buttons/AdministrationFormButtons';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';

interface IExamGroupEditProps {
    examGroupId: number | null;
    isEditMode?: boolean;
    getContestId?: Function;
    onSuccess?: Function;
    setParentSuccessMessage?: Function;
}

const ExamGroupEdit = (props:IExamGroupEditProps) => {
    const { examGroupId, isEditMode = true, getContestId, onSuccess, setParentSuccessMessage } = props;

    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ isValidForm, setIsValidForm ] = useState<boolean>(!!isEditMode);
    const [ contestsData, setContestsData ] = useState <Array<IContestAutocomplete>>([]);

    const [ examGroup, setExamGroup ] = useState<IExamGroupAdministration>({
        id: 0,
        name: '',
        contestName: '',
        contestId: null,
        externalAppId: '',
        externalExamGroupId: 0,
    });

    const [ contestSearchString, setContestSearchString ] = useState<string>(examGroup.contestName);

    const [ examGroupValidations, setExamGroupValidations ] = useState({
        isNameTouched: false,
        isNameValid: !!isEditMode,
    });

    const { data, isFetching, isLoading } = useGetExamGroupByIdQuery({ id: Number(examGroupId) }, { skip: !isEditMode });
    const { data: contestsForDropdown } = useGetContestAutocompleteQuery(contestSearchString);

    const [
        updateExamGroup, {
            data: updateData,
            isLoading: isUpdating,
            isSuccess:
                isSuccessfullyUpdated,
            error: updateError,
        } ] = useUpdateExamGroupMutation();

    const [
        createExamGroup, {
            data: createData,
            isSuccess: isSuccessfullyCreated,
            error: createError,
            isLoading: isCreating,
        } ] = useCreateExamGroupMutation();

    useDelayedSuccessEffect({ isSuccess: isSuccessfullyCreated, onSuccess });

    useSuccessMessageEffect({
        data: [
            { message: createData, shouldGet: isSuccessfullyCreated },
            { message: updateData, shouldGet: isSuccessfullyUpdated },
        ],
        setParentSuccessMessage,
        setSuccessMessage,
    });

    useEffect(
        () => {
            if (data) {
                setExamGroup(data);
                setContestSearchString(data.contestName);
                if (getContestId) {
                    getContestId(data.contestId);
                }
            }
        },
        [ data, getContestId ],
    );

    useEffect(() => {
        if (contestsForDropdown) {
            if (examGroup.contestId && !contestsForDropdown.find((x) => x.id === examGroup.contestId)) {
                setContestsData(contestsForDropdown.concat([
                    { id: examGroup.contestId, name: examGroup.contestName } as IContestAutocomplete,
                ]));
            } else {
                setContestsData(contestsForDropdown);
            }
        }
    }, [ contestsForDropdown, examGroup.contestId, examGroup.contestName ]);

    useEffect(() => {
        setErrorMessages([]);
        if (isSuccessfullyUpdated) {
            setErrorMessages([]);
        }
        if (isSuccessfullyCreated) {
            setErrorMessages([]);
        }
    }, [ isSuccessfullyUpdated, updateData, createData, isSuccessfullyCreated ]);

    useEffect(() => {
        getAndSetExceptionMessage([ updateError, createError ], setErrorMessages);
    }, [ createError, updateError ]);

    const validateForm = () => {
        const isValid = examGroupValidations.isNameValid;
        setIsValidForm(isValid);
    };

    const onChange = (e: any) => {
        // eslint-disable-next-line prefer-destructuring
        const { name, value } = e.target;
        let {
            name: examGroupName,
            contestName,
            contestId,
        } = examGroup;
        const currentExamGroupValidations = examGroupValidations;
        // eslint-disable-next-line default-case
        switch (name) {
        case 'name':
            examGroupName = value;
            currentExamGroupValidations.isNameTouched = true;
            currentExamGroupValidations.isNameValid = true;
            if (value.length < 2 || value.length > 600) {
                currentExamGroupValidations.isNameValid = false;
            }
            break;
        case 'contest':
            const selectedContest = contestsForDropdown?.find((c) => c.id === value);
            if (selectedContest) {
                contestId = selectedContest!.id;
                contestName = selectedContest!.name;
            }
            break;
        }
        setExamGroupValidations(currentExamGroupValidations);
        setExamGroup((prevState) => ({
            ...prevState,
            name: examGroupName,
            contestName,
            contestId,
        }));
        validateForm();
    };

    const handleAutocompleteChange = (name: string, newValue:IContestCategories) => {
        setExamGroup((prevState) => ({
            ...prevState,
            contestId: newValue.id ?? null,
            contestName: newValue.name ?? null,
        }));
    };

    const onInputChange = (event: any, newInputValue: string) => {
        if (!newInputValue) {
            return;
        }

        setContestSearchString(newInputValue);
    };

    const edit = () => {
        if (isValidForm) {
            updateExamGroup(examGroup);
        }
    };

    const create = () => {
        if (isValidForm) {
            createExamGroup(examGroup);
        }
    };

    if (isFetching || isLoading || isUpdating || isCreating) {
        return <SpinningLoader />;
    }
    return (

        <>
            {renderErrorMessagesAlert(errorMessages)}
            {renderSuccessfullAlert(successMessage)}
            <Typography className={formStyles.centralize} variant="h4">
                {examGroup.name || 'Exam group form'}
            </Typography>
            <form className={`${formStyles.form}`}>
                {isEditMode && (
                <TextField
                  className={formStyles.inputRow}
                  label="Exam Group Id"
                  variant="standard"
                  value={examGroup.id}
                  disabled
                />
                )}
                <TextField
                  className={formStyles.inputRow}
                  label="Name"
                  variant="standard"
                  name="name"
                  onChange={(e) => onChange(e)}
                  value={examGroup.name}
                  color={examGroupValidations.isNameValid && examGroupValidations.isNameTouched
                      ? 'success'
                      : 'primary'}
                  error={(examGroupValidations.isNameTouched && !examGroupValidations.isNameValid)}
                  helperText={(examGroupValidations.isNameTouched &&
                                !examGroupValidations.isNameValid) &&
                                'Exam Group name length must be between 2 and 600 characters long'}
                />
                <FormControl className={formStyles.inputRow} sx={{ margin: '20px 0' }}>
                    <Autocomplete
                      className={formStyles.inputRow}
                      onChange={(event, newValue) => handleAutocompleteChange('contest', newValue!)}
                      onInputChange={onInputChange}
                      options={contestsData}
                      inputValue={contestSearchString}
                      renderInput={(params) => <TextField {...params} label="Select Contest" key={params.id} />}
                      isOptionEqualToValue={(option, value) => option.id === value.id && option.name === value.name}
                      getOptionLabel={(option) => option?.name}
                      disableCloseOnSelect
                      disableClearable
                      renderOption={(properties, option) => (
                          <MenuItem {...properties} key={option.id} value={option.id}>
                              {option.name}
                          </MenuItem>
                      )}
                    />
                </FormControl>
                <AdministrationFormButtons
                  isEditMode={isEditMode}
                  onCreateClick={() => create()}
                  onEditClick={() => edit()}
                  disabled={!isValidForm}
                />
            </form>
        </>
    );
};

export default ExamGroupEdit;
