import React, { useEffect, useState } from 'react';
import { Box, Checkbox, FormControl, FormControlLabel, FormGroup, FormLabel, InputLabel, MenuItem, Select, TextareaAutosize, TextField, Typography } from '@mui/material';
import isNaN from 'lodash/isNaN';

import { HIDE_INPUT, ID, INPUT, ORDER_BY, OUTPUT, TYPE } from '../../../../common/labels';
import { useCreateTestMutation, useGetTestByIdQuery, useUpdateTestMutation } from '../../../../redux/services/admin/testsAdminService';
import { getAndSetExceptionMessage, getAndSetSuccesfullMessages } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import AdministrationFormButtons from '../../common/administration-form-buttons/AdministrationFormButtons';
import { ITestAdministration, TestTypes } from '../types';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';

interface ITestFormProps {
    id?:number;
    problemName?: string;
    isEditMode?: boolean;

    problemId?: number;
}
const TestForm = (props: ITestFormProps) => {
    const { id = 0, isEditMode = true, problemName = '', problemId = 0 } = props;

    const [ exceptionMessages, setExceptionMessages ] = useState<Array<string>>([]);
    const [ successfullMessage, setSuccessfullMessage ] = useState<string | null>(null);
    const [ test, setTest ] = useState<ITestAdministration>({
        id,
        input: '',
        output: '',
        orderBy: 0,
        retestProblem: false,
        type: Object.keys(TestTypes).filter((key) => isNaN(Number(key)))[0],
        hideInput: false,
        problemId,
        problemName,
    });

    const { data: testData, error: getTestError, isLoading: isGettingData } = useGetTestByIdQuery(id, { skip: !isEditMode });
    const [
        editTest,
        { data: editData, error: editError, isLoading: isEditing, isSuccess: isSuccessfullyEdited },
    ] = useUpdateTestMutation();
    const [
        createTest,
        { data: createData, error: createError, isLoading: isCreating, isSuccess: isSuccessfullyCreated },
    ] = useCreateTestMutation();

    useEffect(() => {
        if (testData) {
            setTest(testData);
        }
    }, [ testData ]);

    useEffect(() => {
        getAndSetExceptionMessage([ getTestError, editError, createError ], setExceptionMessages);
    }, [ getTestError, editError, createError ]);

    useEffect(() => {
        const message = getAndSetSuccesfullMessages([
            { message: editData, shouldGet: isSuccessfullyEdited },
            { message: createData, shouldGet: isSuccessfullyCreated },
        ]);
        setSuccessfullMessage(message);
    }, [ editData, createData, isSuccessfullyEdited, isSuccessfullyCreated ]);

    const onChange = (e: any) => {
        const { target } = e;
        const { name, type, value, checked } = target;
        setTest((prevState) => ({
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

    if (isGettingData || isEditing || isCreating) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderSuccessfullAlert(successfullMessage)}
            {renderErrorMessagesAlert(exceptionMessages)}
            <Typography className={formStyles.centralize} variant="h4">Test administration form</Typography>
            <form className={formStyles.form}>
                <Box className={formStyles.inputRow}>
                    <FormGroup className={formStyles.inputRow}>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={ID}
                              value={test?.id}
                              InputLabelProps={{ shrink: true }}
                              type="text"
                              disabled
                            />
                        </FormControl>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label="Problem Name"
                              value={test?.problemName}
                              InputLabelProps={{ shrink: true }}
                              type="text"
                              disabled
                            />
                        </FormControl>
                        <FormControl className={formStyles.spacing}>
                            <FormLabel>{INPUT}</FormLabel>
                            <TextareaAutosize
                              minRows={15}
                              maxRows={15}
                              value={test.input}
                              name="input"
                              onChange={(e) => onChange(e)}
                            />
                        </FormControl>
                        <FormControl className={formStyles.spacing}>
                            <FormLabel>{OUTPUT}</FormLabel>
                            <TextareaAutosize
                              minRows={15}
                              maxRows={15}
                              value={test.output}
                              name="output"
                              onChange={(e) => onChange(e)}
                            />
                        </FormControl>
                        <FormControl className={formStyles.spacing}>
                            <TextField
                              variant="standard"
                              label={ORDER_BY}
                              value={test?.orderBy}
                              InputLabelProps={{ shrink: true }}
                              type="number"
                              name="orderBy"
                              onChange={(e) => onChange(e)}
                            />
                        </FormControl>
                        <FormControl
                          className={formStyles.spacing}
                        >
                            <InputLabel id="test-type">{TYPE}</InputLabel>
                            <Select
                              sx={{ width: '100%' }}
                              variant="standard"
                              value={test.type}
                              name="type"
                              labelId="test-type"
                              onChange={(e) => onChange(e)}
                              onBlur={(e) => onChange(e)}
                            >
                                {Object.keys(TestTypes).filter((key) => isNaN(Number(key))).map((key) => (
                                    <MenuItem key={key} value={key}>
                                        {key}
                                    </MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                        {isEditMode && (
                        <FormControl className={formStyles.spacing}>
                            <FormControlLabel
                              control={<Checkbox checked={test.retestProblem ?? false} />}
                              label="Retest Problem"
                              name="retestProblem"
                              onChange={(e) => onChange(e)}
                            />
                        </FormControl>
                        )}
                        <FormControl className={formStyles.spacing}>
                            <FormControlLabel
                              control={<Checkbox checked={test.hideInput} />}
                              label={HIDE_INPUT}
                              name="hideInput"
                              onChange={(e) => onChange(e)}
                            />
                        </FormControl>
                    </FormGroup>
                </Box>
                <AdministrationFormButtons
                  isEditMode={isEditMode}
                  onCreateClick={() => createTest(test)}
                  onEditClick={() => editTest(test)}
                />
            </form>
        </>
    );
};
export default TestForm;
