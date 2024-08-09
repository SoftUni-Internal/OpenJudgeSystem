import { useEffect, useState } from 'react';
import { FaFileImport } from 'react-icons/fa';
import { MdDeleteForever } from 'react-icons/md';
import { RiFolderZipFill } from 'react-icons/ri';
import { Checkbox, FormControl, FormControlLabel, IconButton, Tooltip, Typography } from '@mui/material';

import { IGetAllAdminParams, ITestsUploadModel } from '../../../../common/types';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { applyDefaultFilterToQueryString } from '../../../../pages/administration-new/administration-filters/AdministrationFilters';
import AdministrationGridView from '../../../../pages/administration-new/AdministrationGridView';
import testsFilterableColums, { returnTestsNonFilterableColumns } from '../../../../pages/administration-new/tests/testsGridColumns';
import { useDeleteByProblemMutation, useExportZipQuery, useGetTestsByProblemIdQuery, useImportTestsMutation } from '../../../../redux/services/admin/testsAdminService';
import downloadFile from '../../../../utils/file-download-utils';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import ConfirmDialog from '../../../guidelines/dialog/ConfirmDialog';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import CreateButton from '../../common/create/CreateButton';
import FileUpload from '../../common/file-upload/FileUpload';
import AdministrationModal from '../../common/modals/administration-modal/AdministrationModal';
import SubmitSolution from '../../common/submit-solution/SubmitSolution';
import FormActionButton from '../../form-action-button/FormActionButton';
import TestForm from '../../tests/test-form/TestForm';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';

interface ITestsInProblemsViewProps {
    problemId: number;
    problemName: string;
    contestId: number;
    canBeCompeted: boolean;
}

const defaultTestsSorterToAdd = 'istrialtest=DESC&orderby=ASC';

const TestsInProblemView = (props: ITestsInProblemsViewProps) => {
    const { problemId, problemName, canBeCompeted, contestId } = props;
    const defaultStateForUploadTests = {
        deleteOldTests: true,
        retestProblem: false,
        tests: null,
        problemId,
    } as ITestsUploadModel;

    const [ testId, setTestId ] = useState<number | null>(null);
    const [ openEditTestModal, setOpenEditTestModal ] = useState(false);
    const [ openCreateModal, setOpenCreateModal ] = useState(false);
    const [ showDeleteAllConfirm, setShowDeleteAllConfirm ] = useState<boolean>(false);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ showImportModal, setShowImportModal ] = useState<boolean>(false);
    const [ shouldSkip, setShouldSkip ] = useState<boolean>(true);
    const [ testsToUpload, setTestsToUpload ] = useState<ITestsUploadModel>({ ...defaultStateForUploadTests });

    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString('', defaultTestsSorterToAdd));

    const {
        refetch: retakeTests,
        data: testsData,
        error,
        isLoading: isGettingData,
    } = useGetTestsByProblemIdQuery({ problemId, ...queryParams });

    const [ deleteByProblem,
        {
            data: deleteAllData,
            isSuccess: isSuccesfullyDeletedAll,
            isLoading: isDeletingAll,
            error: deleteAllError,
        } ] = useDeleteByProblemMutation();

    const [ importTests,
        {
            data: importTestsData,
            isSuccess: isSuccessfullyImported,
            isLoading: isImporting,
            error: importTestsError,
        } ] = useImportTestsMutation();

    const { refetch: reExportZip, data: zipData, isError: exportZipError } = useExportZipQuery(problemId, { skip: shouldSkip });

    useSuccessMessageEffect({
        data: [
            { message: deleteAllData, shouldGet: isSuccesfullyDeletedAll },
            { message: importTestsData, shouldGet: isSuccessfullyImported },
        ],
        setSuccessMessage,
    });

    useEffect(() => {
        if (isSuccesfullyDeletedAll) {
            retakeTests();
        }
    }, [ deleteAllData, isSuccesfullyDeletedAll, importTestsData, isSuccessfullyImported, retakeTests ]);

    useEffect(() => {
        getAndSetExceptionMessage([ deleteAllError, importTestsError, exportZipError ], setErrorMessages);
    }, [ deleteAllError, exportZipError, importTestsError ]);

    useEffect(() => {
        if (zipData) {
            downloadFile(zipData.blob, zipData.filename);
        }
    }, [ zipData ]);

    const onEditClick = (id: number) => {
        setOpenEditTestModal(true);
        setTestId(id);
    };

    const onFileUploadModelChange = (e: any) => {
        const { target } = e;
        const { name, type, value, checked } = target;
        setTestsToUpload((prevState) => ({
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

    const handleFileUpload = (e: any) => {
        setTestsToUpload((prevState) => ({
            ...prevState,
            tests: e.target.files[0],
        }));
    };

    const exportZip = () => {
        if (shouldSkip) {
            setShouldSkip(false);
        } else {
            reExportZip();
        }
    };
    const renderGridSettings = () => (
        <>
            <CreateButton
              showModal={openCreateModal}
              showModalFunc={setOpenCreateModal}
              styles={{ width: '40px', height: '40px', color: 'rgb(25,118,210)' }}
            />
            <Tooltip title="Delete All">
                <IconButton onClick={() => setShowDeleteAllConfirm(!showDeleteAllConfirm)}>
                    <MdDeleteForever style={{ width: '40px', height: '40px', color: 'red' }} />
                </IconButton>
            </Tooltip>
            <Tooltip title="Import tests">
                <IconButton onClick={() => setShowImportModal(!showImportModal)}>
                    <FaFileImport style={{ width: '30px', height: '30px', color: 'rgb(25,118,210)' }} />
                </IconButton>
            </Tooltip>
            <Tooltip title="Export as zip">
                <IconButton onClick={() => exportZip()}>
                    <RiFolderZipFill style={{ width: '40px', height: '40px', color: 'rgb(25,118,210)' }} />
                </IconButton>
            </Tooltip>

            <SubmitSolution canBeCompeted={canBeCompeted} contestId={contestId} />
        </>
    );

    const renderDeleteAllModal = (index: number) => (
        <ConfirmDialog
          key={index}
          text={`Are you sure you want to delete all tests for ${testsData?.items
              ? testsData?.items[0].problemName
              : ''}`}
          title="Delete All Tests"
          declineButtonText="Close"
          confirmButtonText="Delete"
          declineFunction={() => setShowDeleteAllConfirm(!showDeleteAllConfirm)}
          confirmFunction={() => {
              deleteByProblem(problemId);
              setShowDeleteAllConfirm(!showDeleteAllConfirm);
          }}
        />
    );

    const onClose = (isEditMode: boolean) => {
        if (isEditMode) {
            setOpenEditTestModal(false);
        } else {
            setOpenCreateModal(false);
        }
        retakeTests();
    };

    const onSuccessDelete = () => {
        retakeTests();
    };

    const renderModal = (index: number, isEditMode: boolean) => (
        <AdministrationModal
          key={index}
          index={index}
          onClose={() => onClose(isEditMode)}
          open={isEditMode
              ? openEditTestModal
              : openCreateModal}
        >
            <TestForm
              id={isEditMode
                  ? testId!
                  : 0}
              isEditMode={isEditMode}
              problemName={problemName}
              problemId={problemId}
              onSuccess={() => onClose(isEditMode)}
              setParentSuccessMessage={setSuccessMessage}
            />
        </AdministrationModal>
    );

    const renderImportModal = (index: number) => (
        <AdministrationModal
          index={index}
          key={index}
          onClose={() => {
              setTestsToUpload({ ...defaultStateForUploadTests });
              setShowImportModal(false);
          }}
          open={showImportModal}
        >
            <form className={formStyles.form}>
                <Typography variant="h4">Import tests</Typography>
                <FormControl className={formStyles.inputRow}>
                    <FileUpload
                      handleFileUpload={handleFileUpload}
                      propName="tests"
                      setSkipDownload={() => {}}
                      showDownloadButton={false}
                      disableClearButton={testsToUpload.tests === null}
                      uploadButtonName={testsToUpload?.tests?.name}
                      onClearSelectionClicked={() => setTestsToUpload(defaultStateForUploadTests)}
                      buttonLabel="Import"
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <FormControlLabel
                      control={<Checkbox checked={testsToUpload.retestProblem ?? false} />}
                      label="Retest Problem"
                      name="retestProblem"
                      onChange={(e) => onFileUploadModelChange(e)}
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <FormControlLabel
                      control={<Checkbox checked={testsToUpload.deleteOldTests ?? false} />}
                      label="Delete old tests"
                      name="deleteOldTests"
                      onChange={(e) => onFileUploadModelChange(e)}
                    />
                </FormControl>
                <FormActionButton
                  className={formStyles.buttonsWrapper}
                  buttonClassName={formStyles.button}
                  onClick={() => {
                      const formData = new FormData();
                      formData.append('tests', testsToUpload.tests || 'null');
                      formData.append('problemId', testsToUpload.problemId.toString() ?? '0');
                      formData.append('retestProblem', testsToUpload.retestProblem.toString() ?? 'false');
                      formData.append('deleteOldTests', testsToUpload.deleteOldTests.toString() ?? 'false');

                      importTests(formData);
                      setShowImportModal(false);
                  }}
                  name="Import"
                />
            </form>
        </AdministrationModal>
    );

    if (isGettingData || isDeletingAll || isImporting) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderErrorMessagesAlert(errorMessages)}
            {renderSuccessfullAlert(successMessage)}
            <AdministrationGridView
              filterableGridColumnDef={testsFilterableColums}
              notFilterableGridColumnDef={
                returnTestsNonFilterableColumns(
                    onEditClick,
                    onSuccessDelete,
                )
}
              data={testsData}
              renderActionButtons={renderGridSettings}
              error={error}
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              withSearchParams={false}
              defaultSorter={defaultTestsSorterToAdd}
              modals={[
                  { showModal: openCreateModal, modal: (i) => renderModal(i, false) },
                  { showModal: openEditTestModal, modal: (i) => renderModal(i, true) },
                  { showModal: showDeleteAllConfirm, modal: (i) => renderDeleteAllModal(i) },
                  { showModal: showImportModal, modal: (i) => renderImportModal(i) },
              ]}
            />
        </>
    );
};
export default TestsInProblemView;
