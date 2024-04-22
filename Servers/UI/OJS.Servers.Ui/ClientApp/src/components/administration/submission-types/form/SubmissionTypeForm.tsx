import { useEffect, useState } from 'react';
import { Checkbox, FormControl, FormControlLabel, FormLabel, InputLabel, MenuItem, Select, TextareaAutosize, TextField, Typography } from '@mui/material';

import { ADDITIONAL_COMPILER_ARGUMENTS, ALLOW_BINARY_FILES_UPLOAD, ALLOWED_FILE_EXTENSIONS, COMPILER, DESCRIPTION, EXECUTION_STRATEGY, ID, IS_SELECTED, NAME } from '../../../../common/labels';
import { SUBMISSION_TYPE_FILE_EXTENSION_PLACEHOLDER, SUBMISSION_TYPE_FORM_NAME } from '../../../../common/messages';
import { ISubmissionTypeAdministrationModel } from '../../../../common/types';
import { useCreateSubmissionTypeMutation, useGetByIdQuery, useGetCompilersQuery, useGetExecutionStrategiesQuery, useUpdateSubmissionTypeMutation } from '../../../../redux/services/admin/submissionTypesAdminService';
import { getAndSetExceptionMessage, getAndSetSuccesfullMessages } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import AdministrationFormButtons from '../../common/administration-form-buttons/AdministrationFormButtons';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';

interface ISubmissionTypesFormProps {
    id: number | null;
    isEditMode: boolean;
}
const SubmissionTypesForm = (props : ISubmissionTypesFormProps) => {
    const { id, isEditMode } = props;
    const [ compilersData, setCompilersData ] = useState<Array<string>>([]);
    const [ strategiesData, setStrategiesData ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);

    const [ currentSubmissionType, setCurrentSubmissionType ] = useState<ISubmissionTypeAdministrationModel>({
        name: '',
        additionalCompilerArguments: '',
        compilerType: '',
        description: '',
        executionStrategyType: '',
        id: 0,
        isSelectedByDefault: false,
        allowedFileExtensions: '',
        allowBinaryFilesUpload: false,

    });

    const {
        data: compilers,
        error: compilerGetError,
        isLoading: isGettingCompilers,
    } = useGetCompilersQuery(null);

    const {
        data: submissionType,
        isLoading: isGettingSubmissionType,
        error: gettingSubmissionTypeError,
    } = useGetByIdQuery(id || 0, { skip: !isEditMode });

    const {
        data: executionStrategies,
        error: strategiesError,
        isLoading: isGettingStrategies,
    } = useGetExecutionStrategiesQuery(null);

    const [
        create,
        {
            data: createData,
            error: createError,
            isSuccess: isSuccessfullyCreated,
            isLoading: isCreating,
        },
    ] = useCreateSubmissionTypeMutation();

    const [
        update,
        {
            data: updateData,
            error: updateError,
            isSuccess: isSuccessfullyUpdated,
            isLoading: isUpdating,
        },
    ] = useUpdateSubmissionTypeMutation();

    useEffect(() => {
        const message = getAndSetSuccesfullMessages([
            {
                message: updateData,
                shouldGet: isSuccessfullyUpdated,
            },
            {
                message: createData,
                shouldGet: isSuccessfullyCreated,
            },
        ]);

        if (message) {
            setSuccessMessage(message);
        }
    }, [ createData, isSuccessfullyCreated, isSuccessfullyUpdated, updateData ]);

    useEffect(() => {
        getAndSetExceptionMessage([
            compilerGetError, createError, updateError, gettingSubmissionTypeError, strategiesError,
        ], setErrorMessages);

        setSuccessMessage(null);
    }, [ updateError, createError, compilerGetError, gettingSubmissionTypeError, strategiesError ]);

    useEffect(() => {
        if (submissionType) {
            setCurrentSubmissionType(submissionType);
        }
    }, [ submissionType ]);

    useEffect(() => {
        if (compilers) {
            setCompilersData(compilers);
        }
    }, [ compilers ]);

    useEffect(() => {
        if (executionStrategies) {
            setStrategiesData(executionStrategies);
        }
    }, [ executionStrategies ]);

    const onChange = (e: any) => {
        const { target } = e;
        const { name, type, value, checked } = target;
        setCurrentSubmissionType((prevState) => ({
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

    if (isGettingCompilers || isGettingStrategies || isGettingSubmissionType || isCreating || isUpdating) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderErrorMessagesAlert(errorMessages)}
            {renderSuccessfullAlert(successMessage)}
            <form className={formStyles.form}>
                <Typography variant="h3">
                    {currentSubmissionType.name
                        ? currentSubmissionType.name
                        : SUBMISSION_TYPE_FORM_NAME}
                </Typography>
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      label={ID}
                      InputLabelProps={{ shrink: true }}
                      type="number"
                      value={id ?? 0}
                      disabled
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      InputLabelProps={{ shrink: true }}
                      type="string"
                      name="name"
                      label={NAME}
                      onChange={(e) => onChange(e)}
                      value={currentSubmissionType.name}
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <InputLabel id="execution-Strategy">{EXECUTION_STRATEGY}</InputLabel>
                    <Select
                      sx={{ width: '100%' }}
                      variant="standard"
                      value={currentSubmissionType.executionStrategyType}
                      name="executionStrategyType"
                      labelId="execution-Strategy"
                      onChange={(e) => onChange(e)}
                      onBlur={(e) => onChange(e)}
                    >
                        {strategiesData.map((c) => (
                            <MenuItem key={c} value={c}>
                                {c}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <InputLabel id="compiler">{COMPILER}</InputLabel>
                    <Select
                      sx={{ width: '100%' }}
                      variant="standard"
                      value={currentSubmissionType.compilerType}
                      name="compilerType"
                      labelId="compiler"
                      onChange={(e) => onChange(e)}
                      onBlur={(e) => onChange(e)}
                    >
                        {compilersData.map((c) => (
                            <MenuItem key={c} value={c}>
                                {c}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      InputLabelProps={{ shrink: true }}
                      type="string"
                      label={ADDITIONAL_COMPILER_ARGUMENTS}
                      name="additionalCompilerArguments"
                      onChange={(e) => onChange(e)}
                      value={currentSubmissionType.additionalCompilerArguments}
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <FormLabel>{DESCRIPTION}</FormLabel>
                    <TextareaAutosize
                      minRows={10}
                      maxRows={10}
                      name="description"
                      onChange={(e) => onChange(e)}
                      value={currentSubmissionType.description === null
                          ? ''
                          : currentSubmissionType.description}
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      InputLabelProps={{ shrink: true }}
                      type="string"
                      label={ALLOWED_FILE_EXTENSIONS}
                      name="allowedFileExtensions"
                      placeholder={SUBMISSION_TYPE_FILE_EXTENSION_PLACEHOLDER}
                      onChange={(e) => onChange(e)}
                      value={currentSubmissionType.allowedFileExtensions}
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <FormControlLabel
                      control={<Checkbox checked={currentSubmissionType.isSelectedByDefault} />}
                      label={IS_SELECTED}
                      name="isSelectedByDefault"
                      onChange={(e) => onChange(e)}
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <FormControlLabel
                      control={<Checkbox checked={currentSubmissionType.allowBinaryFilesUpload} />}
                      label={ALLOW_BINARY_FILES_UPLOAD}
                      name="allowBinaryFilesUpload"
                      onChange={(e) => onChange(e)}
                    />
                </FormControl>

                <AdministrationFormButtons
                  isEditMode={isEditMode}
                  onCreateClick={() => create(currentSubmissionType)}
                  onEditClick={() => update(currentSubmissionType)}
                />
            </form>
        </>
    );
};

export default SubmissionTypesForm;
