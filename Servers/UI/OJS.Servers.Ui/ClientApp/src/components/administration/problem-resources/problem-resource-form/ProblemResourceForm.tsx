/* eslint-disable @typescript-eslint/ban-types */
import { useEffect, useState } from 'react';
import { Divider, FormControl, FormGroup, InputLabel, MenuItem, Select, TextField, Typography } from '@mui/material';
import isNaN from 'lodash/isNaN';

import { ProblemResourceType } from '../../../../common/enums';
import { ID, LINK, NAME, ORDER_BY, TYPE } from '../../../../common/labels';
import { IProblemResourceAdministrationModel } from '../../../../common/types';
import useDelayedSuccessEffect from '../../../../hooks/common/use-delayed-success-effect';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { useCreateProblemResourceMutation, useDownloadResourceQuery, useGetProblemResourceByIdQuery, useUpdateProblemResourceMutation } from '../../../../redux/services/admin/problemResourcesAdminService';
import downloadFile from '../../../../utils/file-download-utils';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import AdministrationFormButtons from '../../common/administration-form-buttons/AdministrationFormButtons';
import FileUpload from '../../common/file-upload/FileUpload';

/*
The rule is disabled because the formStyles are common scss file.
It is used in most of the forms in the administration.
*/
// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';

interface IProblemResourceFormProps {
    id: number;
    isEditMode?: boolean;
    problemId? : number;
    onSuccess?: Function;
    setParentSuccessMessage?: Function;
}
const ProblemResourceForm = (props :IProblemResourceFormProps) => {
    const { id, isEditMode = true, problemId = 0, onSuccess, setParentSuccessMessage } = props;

    const [ currentResource, setCurrentResource ] = useState<IProblemResourceAdministrationModel>({
        id: 0,
        link: '',
        name: '',
        orderBy: 0,
        type: 'ProblemDescription',
        file: null,
        hasFile: false,
        problemId: 0,
    });
    const [ skipDownload, setSkipDownload ] = useState<boolean>(true);
    const [ exceptionMessages, setExceptionMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);

    const { data: resourceData, error: resourceError, isLoading: isGetting } = useGetProblemResourceByIdQuery(id, { skip: !isEditMode });
    const [
        create,
        {
            data: createData,
            error: createErrorData,
            isSuccess: isSuccessfullyCreated,
            isLoading: isCreating,
        },
    ] = useCreateProblemResourceMutation();

    const [
        update,
        {
            data: updateData,
            error: updateErrorData,
            isSuccess: isSuccessfullyUpdated,
            isLoading: isUpdating,
        } ] = useUpdateProblemResourceMutation();

    const {
        data: downloadData,
        isLoading: isDownloadingFiles,
        error: downloadError,
    } = useDownloadResourceQuery(Number(id), { skip: skipDownload });

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
        if (resourceData) {
            setCurrentResource(resourceData);
        }
    }, [ resourceData ]);

    useEffect(() => {
        getAndSetExceptionMessage([ downloadError, createErrorData, updateErrorData, resourceError ], setExceptionMessages);
    }, [ createErrorData, downloadError, resourceError, updateErrorData ]);

    useEffect(() => {
        if (downloadData?.blob) {
            downloadFile(downloadData.blob, downloadData.filename);
        }
    }, [ downloadData ]);

    const handleFileUpload = (e: any) => {
        let { file } = currentResource;
        file = e.target.files[0];
        setCurrentResource((prevState) => ({
            ...prevState,
            file,
        }));
    };

    const handleFileClearance = () => {
        let { file } = currentResource;
        file = null;
        setCurrentResource((prevState) => ({
            ...prevState,
            file,
        }));
    };
    const onChange = (e: any) => {
        const { target } = e;
        const { name, type, value, checked } = target;
        setCurrentResource((prevState) => ({
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

    const submitForm = () => {
        const formData = new FormData();
        formData.append('name', currentResource.name);
        formData.append('id', currentResource.id?.toString() ?? '');
        formData.append('orderBy', currentResource.orderBy?.toString() || '');
        formData.append('link', currentResource.link || '');
        formData.append('type', currentResource.type);

        if (currentResource.file) {
            formData.append('file', currentResource.file);
        }
        if (isEditMode) {
            formData.append('problemId', currentResource.problemId?.toString() || '');

            update(formData);
        } else {
            formData.append('problemId', problemId?.toString() || '');

            create(formData);
        }
    };

    if (isDownloadingFiles || isUpdating || isCreating || isGetting) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            {renderErrorMessagesAlert(exceptionMessages)}
            <form className={formStyles.form}>
                <Typography variant="h4" className="centralize">
                    Problem Resource Administration Form
                </Typography>
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      label={ID}
                      value={id}
                      InputLabelProps={{ shrink: true }}
                      type="text"
                      disabled
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      label={NAME}
                      name="name"
                      value={currentResource?.name}
                      InputLabelProps={{ shrink: true }}
                      type="text"
                      onChange={(e) => onChange(e)}
                    />
                </FormControl>
                <FormGroup className={formStyles.inputRow}>
                    <InputLabel id="problemGroupType">{TYPE}</InputLabel>
                    <Select
                      onChange={(e) => onChange(e)}
                      onBlur={(e) => onChange(e)}
                      labelId="problemGroupType"
                      value={currentResource.type || 'None'}
                      name="type"
                    >
                        {Object.keys(ProblemResourceType).filter((key) => isNaN(Number(key))).map((key, i: number) => (
                            <MenuItem key={i} value={key}>
                                {key}
                            </MenuItem>
                        ))}
                    </Select>
                </FormGroup>
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      label={LINK}
                      value={currentResource.link}
                      InputLabelProps={{ shrink: true }}
                      type="text"
                      name="link"
                      onChange={(e) => onChange(e)}
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <TextField
                      variant="standard"
                      label={ORDER_BY}
                      value={currentResource.orderBy}
                      InputLabelProps={{ shrink: true }}
                      type="text"
                      name="orderBy"
                      onChange={(e) => onChange(e)}
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <Typography variant="h4">File</Typography>
                    <Divider />
                    <FileUpload
                      handleFileUpload={handleFileUpload}
                      propName="file"
                      setSkipDownload={setSkipDownload}
                      uploadButtonName={currentResource.file?.name}
                      showDownloadButton={currentResource.hasFile}
                      onClearSelectionClicked={handleFileClearance}
                      disableClearButton={currentResource.file === null}
                    />
                </FormControl>
                <AdministrationFormButtons
                  isEditMode={isEditMode}
                  onCreateClick={() => submitForm()}
                  onEditClick={() => submitForm()}
                />
            </form>
        </>
    );
};
export default ProblemResourceForm;
