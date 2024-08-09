import React, { useCallback, useState } from 'react';
import { Button, FormControl, InputLabel, MenuItem, Select, Typography } from '@mui/material';
import isNil from 'lodash/isNil';

import { IReplaceSubmissionTypeModel } from '../../../common/types';
import AdministrationModal
    from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import FormActionButton from '../../../components/administration/form-action-button/FormActionButton';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import {
    useGetForProblemQuery, useReplaceSubmissionTypeMutation,
} from '../../../redux/services/admin/submissionTypesAdminService';
import isNilOrEmpty from '../../../utils/check-utils';
import concatClassNames from '../../../utils/class-names';
import { getErrorMessage } from '../../../utils/http-utils';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../../components/administration/common/styles/FormStyles.module.scss';
import styles from './AdministrationReplaceDeleteSubmissionTypesPage.module.scss';

const AdministrationReplaceDeleteSubmissionTypesPage = () => {
    const [ validations, setValidations ] = useState({
        isSubmissionTypeToReplaceTouched: false,
        isSubmissionTypeToReplaceValid: false,
        isReplaceWithTouched: false,
        isReplaceWithValid: false,
    });

    const [ submissionTypeToReplace, setSubmissionTypeToReplace ] = useState<string | null>();
    const [ replaceWith, setReplaceWith ] = useState<string | null>();
    const [ submissionTypeIdToReplace, setSubmissionTypeIdToReplace ] = useState<number | null>();
    const [ replaceWithId, setReplaceWithId ] = useState<number | null>();
    const [ openConfirmModal, setOpenConfirmModal ] = useState<boolean>(false);

    const SUBMISSION_TYPE_TO_REPLACE_FIELD_NAME = 'submission-type-to-replace';
    const REPLACE_WITH_FIELD_NAME = 'replace-with';

    const {
        data: submissionTypesData,
        isLoading: areSubmissionTypesLoading,
    } = useGetForProblemQuery(null);

    const [
        replaceSubmissionType,
        {
            data: replaceResult,
            error,
            isUninitialized,
            isSuccess: isSuccessfullyReplaced,
            isLoading: isReplaceLoading,
        },
    ] = useReplaceSubmissionTypeMutation();

    const findSubmissionTypeId = (nameValue: string) => submissionTypesData!
        .find((st) => st.name === nameValue)!.id;

    const onChange = (e: any) => {
        const { name, value } = e.target;
        const currentValidations = validations;
        // eslint-disable-next-line default-case
        switch (name) {
        case SUBMISSION_TYPE_TO_REPLACE_FIELD_NAME: {
            if (isNil(value)) {
                currentValidations.isSubmissionTypeToReplaceValid = false;
                break;
            }

            setSubmissionTypeToReplace(value);
            setSubmissionTypeIdToReplace(findSubmissionTypeId(value));
            currentValidations.isSubmissionTypeToReplaceValid = true;
            break;
        }
        case REPLACE_WITH_FIELD_NAME: {
            setReplaceWith(value);
            setReplaceWithId(findSubmissionTypeId(value));
            break;
        }
        }
        setValidations(currentValidations);
    };

    const clearReplaceWith = useCallback(() => {
        setReplaceWith(null);
        setReplaceWithId(null);
    }, []);

    const renderReplaceSubmissionTypeText = useCallback(
        () => (
            <Typography>
                <p>
                    {'You are about to replace submission type '}
                    <span className={styles.blueText}>{submissionTypeToReplace}</span>
                    {' with '}
                    <span className={styles.blueText}>{replaceWith}</span>
                    . This action will
                    <span className={styles.redText}> delete</span>
                    {' the '}
                    <span className={styles.blueText}>
                        {submissionTypeToReplace}
                        {' '}
                    </span>
                    submission type.
                </p>
                <p>
                    Problems using only this submission type will remain with
                    <span className={styles.redText}> 0</span>
                    {' '}
                    submission types attached.
                    All submissions associated will be updated with the new one (
                    <span className={styles.blueText}>{replaceWith}</span>
                    ) but they will
                    {' '}
                    <span className={styles.redText}> not be retested</span>
                    .
                </p>
                <p>Changes might need to be made to the problems, tests or submissions in order to execute correctly.</p>
            </Typography>
        ),
        [ replaceWith, submissionTypeToReplace ],
    );

    const renderReplaceSubmissionTypeWithDeletionText = useCallback(() => (
        <Typography>
            <p>
                {'You are about to delete the '}
                <span className={styles.blueText}>{submissionTypeToReplace}</span>
                {' submission type. '}
                Problems using only this submission type will remain with
                <span className={styles.redText}> 0</span>
                {' '}
                submission types attached.
            </p>
            <p>
                All submissions associated will be
                <span className={styles.redText}> deleted</span>
                .
            </p>
        </Typography>
    ), [ submissionTypeToReplace ]);

    const submit = () => {
        replaceSubmissionType({
            submissionTypeToReplace: submissionTypeIdToReplace,
            submissionTypeToReplaceWith: replaceWithId,
        } as IReplaceSubmissionTypeModel);
    };

    const renderSubmissionTypesMenuItems = useCallback(
        () => !isNil(submissionTypesData) && submissionTypesData.map((st) => (
            <MenuItem key={st.id} value={st.name}>
                {st.id}
                :
                {' '}
                {st.name}
            </MenuItem>
        )),
        [ submissionTypesData ],
    );

    return (
        <>
            <h1>Replace/delete submission types</h1>
            <div className={styles.pageContentContainer}>
                {
                    !areSubmissionTypesLoading && (
                    <form className={concatClassNames(formStyles.form, styles.replaceSubmissionTypesForm)}>
                        <FormControl
                          className={formStyles.inputRow}
                        >
                            <InputLabel id={SUBMISSION_TYPE_TO_REPLACE_FIELD_NAME}>Submission type</InputLabel>
                            <Select
                              sx={{ width: '100%' }}
                              variant="standard"
                              value={isNilOrEmpty(submissionTypeToReplace)
                                  ? ''
                                  : submissionTypeToReplace}
                              className={formStyles.inputRow}
                              name={SUBMISSION_TYPE_TO_REPLACE_FIELD_NAME}
                              labelId={SUBMISSION_TYPE_TO_REPLACE_FIELD_NAME}
                              onChange={(e) => onChange(e)}
                              onBlur={(e) => onChange(e)}
                              color={validations.isSubmissionTypeToReplaceValid && validations.isSubmissionTypeToReplaceTouched
                                  ? 'success'
                                  : 'primary'}
                              error={(validations.isSubmissionTypeToReplaceTouched && !validations.isSubmissionTypeToReplaceValid)}
                            >
                                {renderSubmissionTypesMenuItems()}
                                helperText=
                                {(validations.isSubmissionTypeToReplaceTouched &&
                                        !validations.isSubmissionTypeToReplaceValid) &&
                                    'Invalid submission type to replace'}
                            </Select>
                        </FormControl>
                        <FormControl
                          className={formStyles.inputRow}
                        >
                            <InputLabel id={REPLACE_WITH_FIELD_NAME}>Replace with</InputLabel>
                            <Button onClick={clearReplaceWith} className={styles.clearBtn}>
                                Clear
                            </Button>
                            <Select
                              sx={{ width: '100%' }}
                              variant="standard"
                              value={isNilOrEmpty(replaceWith)
                                  ? ''
                                  : replaceWith}
                              className={formStyles.inputRow}
                              name={REPLACE_WITH_FIELD_NAME}
                              labelId={REPLACE_WITH_FIELD_NAME}
                              onChange={(e) => onChange(e)}
                              onBlur={(e) => onChange(e)}
                              color={validations.isReplaceWithValid && validations.isReplaceWithTouched
                                  ? 'success'
                                  : 'primary'}
                              error={(validations.isReplaceWithTouched && !validations.isReplaceWithValid)}
                            >
                                {renderSubmissionTypesMenuItems()}
                                helperText=
                                {(validations.isSubmissionTypeToReplaceTouched &&
                                        !validations.isSubmissionTypeToReplaceValid) &&
                                    'Invalid submission type to replace'}
                            </Select>
                        </FormControl>
                        <AdministrationModal
                          className={styles.confirmModal}
                          key={5}
                          index={1}
                          open={openConfirmModal}
                          onClose={() => setOpenConfirmModal(false)}
                        >
                            {isNilOrEmpty(replaceWith)
                                ? renderReplaceSubmissionTypeWithDeletionText()
                                : renderReplaceSubmissionTypeText()}
                            <FormActionButton
                              name="Confirm"
                              onClick={() => {
                                  submit();
                                  setOpenConfirmModal(false);
                              }}
                              className={styles.confirmModalBtn}
                            />
                        </AdministrationModal>
                        <FormActionButton
                          name="Find And Replace All"
                          onClick={() => setOpenConfirmModal(true)}
                          disabled={!validations.isSubmissionTypeToReplaceValid}
                        />
                    </form>
                    )
}
                {
                    !isUninitialized && (
                        <div className={styles.resultContainer}>
                            <h2>Result</h2>
                            {isReplaceLoading && <SpinningLoader />}
                            {!isReplaceLoading && isSuccessfullyReplaced && (<p>{replaceResult}</p>)}
                            {!isReplaceLoading && !isNilOrEmpty(error) && (
                            <p className={styles.redText}>{getErrorMessage(error)}</p>)}
                        </div>
                    )
                }
            </div>
        </>
    );
};

export default AdministrationReplaceDeleteSubmissionTypesPage;
