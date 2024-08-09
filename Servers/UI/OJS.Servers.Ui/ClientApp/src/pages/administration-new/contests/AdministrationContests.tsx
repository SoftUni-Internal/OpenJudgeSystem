import React, { useEffect, useState } from 'react';
import { concatClassnames } from 'react-alice-carousel/lib/utils';
import { useSearchParams } from 'react-router-dom';
import { Typography } from '@mui/material';

import { DOWNLOAD, TRANSFER } from '../../../common/labels';
import { CONTEST_IS_DELETED, CONTEST_IS_NOT_VISIBLE } from '../../../common/messages';
import { IGetAllAdminParams } from '../../../common/types';
import CreateButton from '../../../components/administration/common/create/CreateButton';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import ContestCompetePracticeButtons from '../../../components/administration/contests/contest-compete-practce-buttons/ContestCompetePracticeButtons';
import ContestDownloadSubmissions from '../../../components/administration/contests/contest-download-submissions/ContestDownloadSubmissions';
import ContestEdit from '../../../components/administration/contests/contest-edit/ContestEdit';
import FormActionButton from '../../../components/administration/form-action-button/FormActionButton';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import useSuccessMessageEffect from '../../../hooks/common/use-success-message-effect';
import { getColors } from '../../../hooks/use-administration-theme-provider';
import {
    useDownloadResultsMutation,
    useGetAllAdminContestsQuery,
    useLazyExportContestsToExcelQuery,
    useTransferParticipantsMutation,
} from '../../../redux/services/admin/contestsAdminService';
import { useAppSelector } from '../../../redux/store';
import downloadFile from '../../../utils/file-download-utils';
import { getAndSetExceptionMessage } from '../../../utils/messages-utils';
import { flexCenterObjectStyles } from '../../../utils/object-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../utils/render-utils';
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
    const [ contestName, setContestName ] = useState<string>();
    const [ categoryName, setCategoryName ] = useState<string>();
    const [ contestOfficialParticipants, setContestOfficialParticipants ] = useState<number>();
    const [ showDownloadSubsModal, setShowDownloadSubsModal ] = useState<boolean>(false);
    const [ showExportExcelModal, setShowExportExcelModal ] = useState<boolean>(false);
    const [ showTransferParticipantsModal, setShowTransferParticipantsModal ] = useState<boolean>(false);

    const [ excelExportType, setExcelExportType ] = useState<number>(0);
    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(defaultFilterToAdd, defaultSorterToAdd, searchParams));

    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);

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

    const [
        transfer,
        {
            data: transferParticipantsData,
            isSuccess: isTransferParticipantsSuccess,
            isLoading: isTransferParticipantsLoading,
            error: transferParticipantsError,
        },
    ] = useTransferParticipantsMutation();

    const onEditClick = (id: number) => {
        setOpenEditContestModal(true);
        setContestId(id);
    };

    useSuccessMessageEffect({
        data: [
            { message: transferParticipantsData as string, shouldGet: isTransferParticipantsSuccess },
        ],
        setSuccessMessage,
    });

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

    useEffect(() => {
        getAndSetExceptionMessage([ transferParticipantsError ], setErrorMessages);
    }, [ transferParticipantsError ]);

    const onClose = (isEditMode: boolean) => {
        if (isEditMode) {
            setOpenEditContestModal(false);
        } else {
            setOpenShowCreateContestModal(false);
        }
        retakeContests();
    };

    const onExcelClick = (excelExportContestId: number) => {
        setShowExportExcelModal(true);
        setContestId(excelExportContestId);
    };

    const onDownloadSubmissionClick = (contestToDownloadSubs: number) => {
        setShowDownloadSubsModal(true);
        setContestId(contestToDownloadSubs);
    };

    const onTransferParticipantsClick = (
        transferParticipantsContestId: number,
        transferParticipantsContestName: string,
        transferParticipantsCategoryName: string,
        transferParticipantsContestOfficialParticipants: number,
    ) => {
        setShowTransferParticipantsModal(true);
        setContestId(transferParticipantsContestId);
        setContestName(transferParticipantsContestName);
        setCategoryName(transferParticipantsCategoryName);
        setContestOfficialParticipants(transferParticipantsContestOfficialParticipants);
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

    const renderTransferParticipantsModal = (index: number) => (
        <AdministrationModal
          key={index}
          index={index}
          open={showTransferParticipantsModal}
          onClose={() => setShowTransferParticipantsModal(false)}
        >
            <>
                <Typography className={formStyles.centralize} variant="h4">Transfer participants</Typography>
                <Typography className={concatClassnames(formStyles.centralize, formStyles.spacing)} variant="h6">
                    {/* eslint-disable-next-line react/jsx-one-expression-per-line */}
                    Are you sure you want to transfer <b>{contestOfficialParticipants}</b> participants from <b>Compete</b>{' '}
                    {/* eslint-disable-next-line react/jsx-one-expression-per-line */}
                    to <b>Practice</b> for the contest <b>{contestName}</b> from the category <b>{categoryName}</b>?
                </Typography>
                <form className={formStyles.form}>
                    <FormActionButton
                      className={formStyles.buttonsWrapper}
                      buttonClassName={formStyles.button}
                      onClick={() => transfer(contestId!)}
                      name={TRANSFER}
                      disabled={isTransferParticipantsLoading}
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
              setParentSuccessMessage={setSuccessMessage}
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
            {renderSuccessfullAlert(successMessage)}
            {renderErrorMessagesAlert(errorMessages)}
            <AdministrationGridView
              data={data}
              error={error}
              filterableGridColumnDef={contestFilterableColumns}
              notFilterableGridColumnDef={returnContestsNonFilterableColumns(
                  onEditClick,
                  retakeContests,
                  onExcelClick,
                  onDownloadSubmissionClick,
                  onTransferParticipantsClick,
              )}
              renderActionButtons={renderGridActions}
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              modals={[
                  { showModal: openShowCreateContestModal, modal: (i) => renderContestModal(i, false) },
                  { showModal: openEditContestModal, modal: (i) => renderContestModal(i, true) },
                  { showModal: showDownloadSubsModal, modal: (i) => renderDownloadSubsModal(i) },
                  { showModal: showExportExcelModal, modal: (i) => renderExcelExportModal(i) },
                  { showModal: showTransferParticipantsModal, modal: (i) => renderTransferParticipantsModal(i) },
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
