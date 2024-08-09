/* eslint-disable @typescript-eslint/ban-types */
import React, { useEffect, useState } from 'react';
import { Box, Checkbox, FormControl, FormControlLabel, FormGroup, FormLabel, InputLabel, MenuItem, Select, TextareaAutosize, TextField, Typography } from '@mui/material';
import isNaN from 'lodash/isNaN';

import {
    HIDE_INPUT,
    ID,
    INPUT,
    ORDER_BY_DECIMAL,
    OUTPUT,
    TYPE,
} from '../../../../common/labels';
import useDelayedSuccessEffect from '../../../../hooks/common/use-delayed-success-effect';
import useDisableMouseWheelOnNumberInputs from '../../../../hooks/common/use-disable-mouse-wheel-on-number-inputs';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { useCreateTestMutation, useGetTestByIdQuery, useUpdateTestMutation } from '../../../../redux/services/admin/testsAdminService';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
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
    onSuccess?: Function;
    setParentSuccessMessage?: Function;
}
const TestForm = (props: ITestFormProps) => {
    const { id = 0, isEditMode = true, problemName = '', problemId = 0, onSuccess, setParentSuccessMessage } = props;

    const [ exceptionMessages, setExceptionMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
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
        updateTest,
        { data: updateData, error: updateError, isLoading: isUpdating, isSuccess: isSuccessfullyUpdated },
    ] = useUpdateTestMutation();
    const [
        createTest,
        { data: createData, error: createError, isLoading: isCreating, isSuccess: isSuccessfullyCreated },
    ] = useCreateTestMutation();

    useDisableMouseWheelOnNumberInputs();

    useDelayedSuccessEffect({ isSuccess: isSuccessfullyCreated, onSuccess });

    useSuccessMessageEffect({
        data: [
            { message: createData, shouldGet: isSuccessfullyCreated },
            { message: updateData, shouldGet: isSuccessfullyUpdated },
        ],
        setParentSuccessMessage,
        setSuccessMessage,
    });

    useEffect(() => {
        if (testData) {
            setTest(testData);
        }
    }, [ testData ]);

    useEffect(() => {
        getAndSetExceptionMessage([ getTestError, updateError, createError ], setExceptionMessages);
    }, [ getTestError, updateError, createError ]);

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

    if (isGettingData || isUpdating || isCreating) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
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
                              label={ORDER_BY_DECIMAL}
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
                  onEditClick={() => updateTest(test)}
                />
            </form>
        </>
    );
};
export default TestForm;
