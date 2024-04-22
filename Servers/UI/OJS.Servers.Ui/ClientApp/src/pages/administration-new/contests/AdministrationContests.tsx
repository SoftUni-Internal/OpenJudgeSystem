import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Typography } from '@mui/material';

import { DOWNLOAD } from '../../../common/labels';
import { CONTEST_IS_DELETED, CONTEST_IS_NOT_VISIBLE } from '../../../common/messages';
import { IGetAllAdminParams } from '../../../common/types';
import CreateButton from '../../../components/administration/common/create/CreateButton';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import ContestCompetePracticeButtons from '../../../components/administration/contests/contest-compete-practce-buttons/ContestCompetePracticeButtons';
import ContestDownloadSubmissions from '../../../components/administration/contests/contest-download-submissions/ContestDownloadSubmissions';
import ContestEdit from '../../../components/administration/contests/contest-edit/ContestEdit';
import FormActionButton from '../../../components/administration/form-action-button/FormActionButton';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { getColors } from '../../../hooks/use-administration-theme-provider';
import { useDownloadResultsMutation, useGetAllAdminContestsQuery, useLazyExportContestsToExcelQuery } from '../../../redux/services/admin/contestsAdminService';
import { useAppSelector } from '../../../redux/store';
import downloadFile from '../../../utils/file-download-utils';
import { getAndSetExceptionMessage } from '../../../utils/messages-utils';
import { flexCenterObjectStyles } from '../../../utils/object-utils';
import { renderErrorMessagesAlert } from '../../../utils/render-utils';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultFilterToAdd, defaultSorterToAdd } from '../AdministrationGridView';

import contestFilterableColumns, { returnContestsNonFilterableColumns } from './contestsGridColumns';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../../components/administration/common/styles/FormStyles.module.scss';

const AdministrationContestsPage = () => {
    const [ searchParams ] = useSearchParams();
    const themeMode = useAppSelector((x) => x.theme.administrationMode);
    const [ openEditContestModal, setOpenEditContestModal ] = useState(false);
    const [ openShowCreateContestModal, setOpenShowCreateContestModal ] = useState<boolean>(false);
    const [ contestId, setContestId ] = useState<number>();
    const [ showDownloadSubsModal, setShowDownloadSubsModal ] = useState<boolean>(false);
    const [ showExportExcelModal, setShowExportExcelModal ] = useState<boolean>(false);

    const [ excelExportType, setExcelExportType ] = useState<number>(0);
    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(defaultFilterToAdd, defaultSorterToAdd, searchParams));

    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);

    const {
        refetch: retakeContests,
        data,
        error,
        isLoading,
    } = useGetAllAdminContestsQuery(queryParams);

    const [
        exportResutls,
        {
            data: file,
            isSuccess: isSuccessfullyDownloaded,
            error: downloadError,
            reset: resetExport,
        },
    ] = useDownloadResultsMutation();

    const onEditClick = (id: number) => {
        setOpenEditContestModal(true);
        setContestId(id);
    };

    useEffect(() => {
        if (isSuccessfullyDownloaded) {
            if (file) {
                downloadFile(file.blob, file.filename);
            } else {
                setErrorMessages([ 'The required file is empty.' ]);
            }
            resetExport();
        }
    }, [ file, isSuccessfullyDownloaded, resetExport ]);

    useEffect(() => {
        getAndSetExceptionMessage([ downloadError ], setErrorMessages);
    }, [ downloadError ]);

    const onClose = (isEditMode: boolean) => {
        if (isEditMode) {
            setOpenEditContestModal(false);
        } else {
            setOpenShowCreateContestModal(false);
        }
        retakeContests();
    };

    const onClickExcel = (exelExportContestId: number) => {
        setShowExportExcelModal(true);
        setContestId(exelExportContestId);
    };

    const onDownloadSubmissionClick = (contestToDownloadSubs: number) => {
        setShowDownloadSubsModal(true);
        setContestId(contestToDownloadSubs);
    };

    const renderDownloadSubsModal = (index: number) => (
        <AdministrationModal
          key={index}
          index={index}
          open={showDownloadSubsModal}
          onClose={() => setShowDownloadSubsModal(false)}
        >
            <ContestDownloadSubmissions contestid={contestId!} />
        </AdministrationModal>
    );

    const renderExcelExportModal = (index: number) => (
        <AdministrationModal
          key={index}
          index={index}
          open={showExportExcelModal}
          onClose={() => setShowExportExcelModal(false)}
        >
            <>
                <Typography className={formStyles.centralize} variant="h4">Export contest results</Typography>
                <form className={formStyles.form}>
                    <ContestCompetePracticeButtons value={excelExportType} setStateFunc={setExcelExportType} />
                    <FormActionButton
                      className={formStyles.buttonsWrapper}
                      buttonClassName={formStyles.button}
                      onClick={() => exportResutls({ id: contestId!, type: excelExportType })}
                      name={DOWNLOAD}
                    />
                </form>
            </>
        </AdministrationModal>
    );

    const renderContestModal = (index: number, isEditMode: boolean) => (
        <AdministrationModal
          key={index}
          index={index}
          open={isEditMode
              ? openEditContestModal
              : openShowCreateContestModal}
          onClose={() => onClose(isEditMode)}
        >
            <ContestEdit
              contestId={isEditMode
                  ? Number(contestId)
                  : null}
              isEditMode={isEditMode}
              skipGettingContest={!isEditMode}
              onSuccess={() => onClose(isEditMode)}
              onDeleteSuccess={() => onClose(isEditMode)}
            />
        </AdministrationModal>
    );

    const renderGridActions = () => (
        <CreateButton
          showModal={openShowCreateContestModal}
          showModalFunc={setOpenShowCreateContestModal}
          styles={{ width: '40px', height: '40px' }}
        />
    );

    if (isLoading) {
        return <div style={{ ...flexCenterObjectStyles }}><SpinningLoader /></div>;
    }

    return (
        <>
            {renderErrorMessagesAlert(errorMessages)}
            <AdministrationGridView
              data={data}
              error={error}
              filterableGridColumnDef={contestFilterableColumns}
              notFilterableGridColumnDef={returnContestsNonFilterableColumns(
                  onEditClick,
                  retakeContests,
                  onClickExcel,
                  onDownloadSubmissionClick,
              )}
              renderActionButtons={renderGridActions}
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              modals={[
                  { showModal: openShowCreateContestModal, modal: (i) => renderContestModal(i, false) },
                  { showModal: openEditContestModal, modal: (i) => renderContestModal(i, true) },
                  { showModal: showDownloadSubsModal, modal: (i) => renderDownloadSubsModal(i) },
                  { showModal: showExportExcelModal, modal: (i) => renderExcelExportModal(i) },
              ]}
              legendProps={[
                  { color: getColors(themeMode).palette.deleted, message: CONTEST_IS_DELETED },
                  { color: getColors(themeMode).palette.visible, message: CONTEST_IS_NOT_VISIBLE } ]}
              excelMutation={useLazyExportContestsToExcelQuery}
            />
        </>
    );
};

export default AdministrationContestsPage;
