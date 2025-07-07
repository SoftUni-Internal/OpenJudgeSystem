import React, { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import VisibilityIcon from '@mui/icons-material/Visibility';
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff';
import { Autocomplete, FormControl, FormGroup, IconButton, TextField, Tooltip, Typography } from '@mui/material';
import Quill from 'quill';
import { useAdministrationTheme } from 'src/hooks/use-administration-theme-provider';

import { ThemeMode } from '../../../../common/enums';
import { ORDER_BY, SUBMISSION_TYPES, TITLE } from '../../../../common/labels';
import { SUBMISSION_TYPE_DOCUMENT_FORM_NAME } from '../../../../common/messages';
import {
    ISubmissionTypeDocumentAdministrationModel,
    ISubmissionTypeInDocument,
    ISubmissionTypeInSubmissionDocumentAdministrationModel,
} from '../../../../common/types';
import {
    NEW_ADMINISTRATION_PATH,
    SUBMISSION_TYPE_DOCUMENTS_PATH,
} from '../../../../common/urls/administration-urls';
import useDelayedSuccessEffect from '../../../../hooks/common/use-delayed-success-effect';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import {
    useCreateSubmissionTypeDocumentMutation,
    useGetSubmissionTypeDocumentByIdQuery,
    useUpdateSubmissionTypeDocumentMutation,
} from '../../../../redux/services/admin/submissionTypeDocumentsAdminService';
import { useGetForDocumentQuery } from '../../../../redux/services/admin/submissionTypesAdminService';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import clearSuccessMessages from '../../../../utils/success-messages-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import AdministrationFormButtons from '../../common/administration-form-buttons/AdministrationFormButtons';

import 'quill/dist/quill.snow.css';
import '../../../../styles/_editor.scss';
import formStyles from '../../common/styles/FormStyles.module.scss';
import styles from './SubmissionTypeDocumentForm.module.scss';

interface ISubmissionTypeDocumentFormProps {
    isEditMode: boolean;
    id: number | null;
}
const SubmissionTypeDocumentForm = (props: ISubmissionTypeDocumentFormProps) => {
    const navigate = useNavigate();
    const { isEditMode, id } = props;
    const { themeMode } = useAdministrationTheme();
    const theme = themeMode === ThemeMode.Dark
        ? 'quill-dark-theme'
        : 'quill-light-theme';

    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ isDisabled, setIsDisabled ] = useState<boolean>(true);
    const [ submissionTypes, setSubmissionTypes ] = useState<Array<ISubmissionTypeInDocument>>([]);
    const [ selectedSubmissionTypes, setSelectedSubmissionTypes ] = useState<Array<ISubmissionTypeInDocument> | null>(null);
    const [ currentSubmissionTypeDocument, setCurrentSubmissionTypeDocument ] = useState<ISubmissionTypeDocumentAdministrationModel>({
        id: 0,
        title: '',
        content: '',
        orderBy: 0,
        submissionTypesInSubmissionDocuments: [],
    });
    const [ quill, setQuill ] = useState<Quill | null>(null);

    const [
        create,
        {
            data: createData,
            error: createError,
            isSuccess: isSuccessfullyCreated,
            isLoading: isCreating,
        },
    ] = useCreateSubmissionTypeDocumentMutation();

    const [
        update,
        {
            data: updateData,
            error: updateError,
            isSuccess: isSuccessfullyUpdated,
            isLoading: isUpdating,
        },
    ] = useUpdateSubmissionTypeDocumentMutation();

    const {
        data: submissionTypeDocument,
        error: submissionTypeDocumentError,
        isLoading: isSubmissionTypeDocumentLoading,
        isFetching: isSubmissionTypeDocumentFetching,
    } = useGetSubmissionTypeDocumentByIdQuery({ id: Number(id) }, { skip: !id || id === 0 });

    const { data: allSubmissionTypes } = useGetForDocumentQuery(null);

    useDelayedSuccessEffect({
        isSuccess: isSuccessfullyCreated,
        onSuccess: () => navigate(`/${NEW_ADMINISTRATION_PATH}/${SUBMISSION_TYPE_DOCUMENTS_PATH}`),
        timeoutDuration: 2000,
    });

    useSuccessMessageEffect({
        data: [
            { message: updateData, shouldGet: isSuccessfullyUpdated },
            { message: createData, shouldGet: isSuccessfullyCreated },
        ],
        setSuccessMessage,
        clearFlags: [ isCreating, isUpdating ],
    });

    const onChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, type, value } = e.target;
        setCurrentSubmissionTypeDocument((prevState) => ({
            ...prevState,
            [name]: type === 'number'
                ? value
                    ? Number(value)
                    : null
                : value,
        }));
    }, []);

    const updateSubmissionTypesInDocument = useCallback(() => {
        const newSubmissionTypes = selectedSubmissionTypes?.map((st) => ({
            submissionTypeId: st.id,
            submissionTypeDocumentId: currentSubmissionTypeDocument.id,
        })) as ISubmissionTypeInSubmissionDocumentAdministrationModel[];

        setCurrentSubmissionTypeDocument((prevState) => ({
            ...prevState,
            submissionTypesInSubmissionDocuments: newSubmissionTypes,
        }));
    }, [ currentSubmissionTypeDocument.id, selectedSubmissionTypes ]);

    const handleCreateClick = useCallback(() => {
        updateSubmissionTypesInDocument();
        create(currentSubmissionTypeDocument);
    }, [ create, updateSubmissionTypesInDocument, currentSubmissionTypeDocument ]);

    const handleEditClick = useCallback(() => {
        updateSubmissionTypesInDocument();
        update(currentSubmissionTypeDocument);
    }, [ update, updateSubmissionTypesInDocument, currentSubmissionTypeDocument ]);

    const setEditorRef = useCallback((node: HTMLDivElement | null) => {
        if (node !== null && !quill) {
            const quillInstance = new Quill(node, {
                modules: {
                    toolbar: [
                        [ { header: [ 1, 2, 3, false ] } ],
                        [ 'bold', 'italic', 'underline' ],
                        [ 'blockquote', 'code-block' ],
                        [ { list: 'ordered' }, { list: 'bullet' } ],
                        [ { indent: '-1' }, { indent: '+1' } ],
                        [ { size: [ 'small', false, 'large', 'huge' ] } ],
                        [ 'link' ],
                        [ 'clean' ],
                    ],
                },
                theme: 'snow',
            });

            quillInstance.on('text-change', () => {
                const newContent = quillInstance.root.innerHTML;
                setCurrentSubmissionTypeDocument((prev) => ({
                    ...prev,
                    content: newContent,
                }));
            });

            setQuill(quillInstance);
        }
    }, [ quill ]);

    useEffect(() => {
        if (quill) {
            if (isDisabled && isEditMode) {
                quill.disable();
            } else {
                quill.enable();
            }
        }
    }, [ isDisabled, isEditMode, quill ]);

    useEffect(() => {
        if (allSubmissionTypes && !selectedSubmissionTypes) {
            setSubmissionTypes(allSubmissionTypes);
        }
    }, [ allSubmissionTypes, selectedSubmissionTypes ]);

    useEffect(() => () => {
        if (quill) {
            quill.off('text-change');
        }
    }, [ quill ]);

    useEffect(() => {
        if (submissionTypeDocument) {
            setCurrentSubmissionTypeDocument(submissionTypeDocument);

            if (allSubmissionTypes && !selectedSubmissionTypes) {
                const selectedTypes = submissionTypeDocument.submissionTypesInSubmissionDocuments.map((curr) => ({
                    id: curr.submissionTypeId,
                    name: allSubmissionTypes.find((st) => st.id === curr.submissionTypeId)!.name,
                }));
                setSelectedSubmissionTypes(selectedTypes);
            }

            if (quill && submissionTypeDocument.content) {
                quill.root.innerHTML = submissionTypeDocument.content;
            }
        }
        // eslint-disable-next-line
    }, [ allSubmissionTypes, submissionTypeDocument, quill ]);

    useEffect(() => {
        getAndSetExceptionMessage([
            createError,
            updateError,
            submissionTypeDocumentError,
        ], setErrorMessages);

        clearSuccessMessages({ setSuccessMessage });
    }, [ updateError, createError, submissionTypeDocumentError ]);

    useEffect(() => {
        updateSubmissionTypesInDocument();
    }, [ selectedSubmissionTypes, updateSubmissionTypesInDocument ]);

    if (isSubmissionTypeDocumentLoading || isSubmissionTypeDocumentFetching) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            {renderErrorMessagesAlert(errorMessages)}
            <form className={formStyles.form}>
                <Typography variant="h3" align="center">
                    {SUBMISSION_TYPE_DOCUMENT_FORM_NAME}
                </Typography>
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      label={TITLE}
                      InputLabelProps={{ shrink: true }}
                      type="text"
                      value={currentSubmissionTypeDocument.title}
                      name="title"
                      onChange={onChange}
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    {isEditMode &&
                        <div className={styles.visibilityToggle}>
                            <Tooltip title={isDisabled
                                ? 'Enable editing'
                                : 'Disable editing'}
                            >
                                <IconButton onClick={() => setIsDisabled(!isDisabled)}>
                                    {isDisabled
                                        ? <VisibilityOffIcon sx={{ color: 'text.disabled' }} />

                                        : <VisibilityIcon sx={{ color: 'primary.main' }} />
                                        }
                                </IconButton>
                            </Tooltip>
                            <Typography
                              variant="body2"
                              color={isDisabled
                                  ? 'text.disabled'
                                  : 'primary'}
                              sx={{ ml: 1, transition: 'color 0.3s' }}
                            >
                                {isDisabled
                                    ? 'Editor is locked'
                                    : 'Editor is unlocked'}
                            </Typography>
                        </div>
                    }
                    <div className={theme}>
                        <div
                          ref={setEditorRef}
                          className={`quillEditor ${theme}`}
                        />
                    </div>
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      label={ORDER_BY}
                      InputLabelProps={{ shrink: true }}
                      type="number"
                      value={currentSubmissionTypeDocument.orderBy}
                      name="orderBy"
                      onChange={onChange}
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <FormGroup>
                        <Autocomplete
                          multiple
                          options={submissionTypes}
                          renderInput={(params) => <TextField {...params} label={SUBMISSION_TYPES} />}
                          onChange={(event, newValue) => setSelectedSubmissionTypes(newValue.length === 0
                              ? null
                              : newValue)}
                          value={selectedSubmissionTypes ?? []}
                          isOptionEqualToValue={(option, value) => option.id === value.id}
                          getOptionLabel={(option) => option?.name || ''}
                        />
                    </FormGroup>
                </FormControl>

                <AdministrationFormButtons
                  onCreateClick={handleCreateClick}
                  onEditClick={handleEditClick}
                  isEditMode={isEditMode}
                />
            </form>
        </>
    );
};

export default SubmissionTypeDocumentForm;
