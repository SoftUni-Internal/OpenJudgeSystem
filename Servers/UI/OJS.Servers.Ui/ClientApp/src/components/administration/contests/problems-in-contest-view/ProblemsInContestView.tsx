import React, { useEffect, useState } from 'react';
import { MdCopyAll, MdDeleteForever } from 'react-icons/md';
import { IconButton, Tooltip } from '@mui/material';

import { ContestVariation } from '../../../../common/contest-types';
import { IGetAllAdminParams } from '../../../../common/types';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import { getColors } from '../../../../hooks/use-administration-theme-provider';
import { applyDefaultFilterToQueryString } from '../../../../pages/administration-new/administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultFilterToAdd } from '../../../../pages/administration-new/AdministrationGridView';
import problemFilterableColums, { returnProblemsNonFilterableColumns } from '../../../../pages/administration-new/problems/problemGridColumns';
import { useDeleteByContestMutation, useGetContestProblemsQuery } from '../../../../redux/services/admin/problemsAdminService';
import { useAppSelector } from '../../../../redux/store';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import ConfirmDialog from '../../../guidelines/dialog/ConfirmDialog';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import CreateButton from '../../common/create/CreateButton';
import AdministrationModal from '../../common/modals/administration-modal/AdministrationModal';
import SubmitSolution from '../../common/submit-solution/SubmitSolution';
import CopyModal, { AllowedOperations } from '../../problems/copy/CopyModal';
import ProblemForm from '../../problems/problem-form/ProblemForm';
import ProblemRetest from '../../problems/retest/ProblemRetest';

interface IProblemsInContestViewProps {
    contestId: number;
    contestName?: string;
    contestType: ContestVariation | undefined;
    canContestBeCompeted: boolean;
}

const defaultProblemsSorterToAdd = 'problemgrouporderby=ASC';

const ProblemsInContestView = (props:IProblemsInContestViewProps) => {
    const { contestId, contestName, contestType, canContestBeCompeted } = props;

    const [ openEditModal, setOpenEditModal ] = useState<boolean>(false);
    const [ problemId, setProblemId ] = useState<number>(-1);
    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(defaultFilterToAdd, defaultProblemsSorterToAdd));
    const themeMode = useAppSelector((x) => x.theme.administrationMode);
    const [ errorMessages, setErrorMessages ] = useState <Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState <string | null>(null);

    const [ showDeleteAllConfirm, setShowDeleteAllConfirm ] = useState<boolean>(false);
    const [ showRetestModal, setShowRetestModal ] = useState<boolean>(false);
    const [ problemToRetestId, setProblemToRetestId ] = useState<number>(0);
    const [ openShowCreateProblemModal, setOpenShowCreateProblemModal ] = useState<boolean>(false);
    const [ showCopyAllModal, setShowCopyAllModal ] = useState<boolean>(false);
    const [ showCopyModal, setShowCopyModal ] = useState<boolean>(false);
    const [ problemToCopy, setProblemToCopy ] = useState<number>(0);

    const {
        refetch: retakeData,
        data: problemsData,
        error: getContestError,
    } = useGetContestProblemsQuery({ contestId: Number(contestId), ...queryParams });

    const [ deleteByContest,
        {
            data: deleteAllData,
            isSuccess: isSuccesfullyDeletedAll,
            isLoading: isDeletingAll,
            error: deleteAllError,
        } ] = useDeleteByContestMutation();

    useSuccessMessageEffect({
        data: [
            { message: deleteAllData, shouldGet: isSuccesfullyDeletedAll },
        ],
        setSuccessMessage,
    });

    useEffect(() => {
        getAndSetExceptionMessage([ deleteAllError, getContestError ], setErrorMessages);
        setSuccessMessage(null);
    }, [ deleteAllError, getContestError ]);

    const onEditClick = (id: number) => {
        setOpenEditModal(true);
        setProblemId(id);
    };

    useEffect(() => {
        if (isSuccesfullyDeletedAll) {
            retakeData();
        }
    }, [ isSuccesfullyDeletedAll, retakeData ]);

    const openCopyModal = (id: number) => {
        setShowCopyModal(true);
        setProblemToCopy(id);
    };

    const openRetestModal = (id: number) => {
        setShowRetestModal(true);
        setProblemToRetestId(id);
    };

    const renderProblemModal = (index: number, isCreate: boolean) => {
        const onClose = () => isCreate
            ? setOpenShowCreateProblemModal(!openShowCreateProblemModal)
            : setOpenEditModal(false);

        const onProblemCreate = () => {
            retakeData();
            onClose();
        };

        return (
            <AdministrationModal
              index={index}
              open={isCreate
                  ? openShowCreateProblemModal
                  : openEditModal}
              onClose={onClose}
            >
                {isCreate
                    ? (
                        <ProblemForm
                          contestId={Number(contestId)}
                          contestName={contestName}
                          problemId={null}
                          isEditMode={false}
                          contestType={contestType!}
                          onSuccess={onProblemCreate}
                          setParentSuccessMessage={setSuccessMessage}
                        />
                    )
                    : (
                        <ProblemForm
                          problemId={problemId}
                          contestType={null}
                        />
                    )}
            </AdministrationModal>
        );
    };

    const renderDeleteAllModal = (index: number) => (
        <ConfirmDialog
          key={index}
          text={`Are you sure you want to delete all problems ${problemsData?.items
              ? problemsData?.items[0].contest
              : ''}`}
          title="Delete All Problems"
          declineButtonText="Close"
          confirmButtonText="Delete"
          declineFunction={() => setShowDeleteAllConfirm(!showDeleteAllConfirm)}
          confirmFunction={() => {
              deleteByContest(contestId);
              setShowDeleteAllConfirm(!showDeleteAllConfirm);
          }}
        />
    );

    const onSuccessfulRetest = (message: string) => {
        setSuccessMessage(message);
        setShowRetestModal(false);
    };

    const renderRetestModal = (index: number) => (
        <ProblemRetest
          key={index}
          contestId={contestId}
          declineFunction={() => setShowRetestModal(!showRetestModal)}
          index={index}
          problemData={problemsData}
          problemName={problemsData?.items
              ? problemsData?.items.find((x) => x.id === problemToRetestId)?.name
              : 'problem'}
          problemToRetest={problemToRetestId}
          onSuccess={onSuccessfulRetest}
        />
    );

    const renderGridSettings = () => (
        <>
            <CreateButton
              showModal={openShowCreateProblemModal}
              showModalFunc={setOpenShowCreateProblemModal}
              styles={{ width: '40px', height: '40px' }}
            />
            <Tooltip title="Copy All">
                <IconButton onClick={() => {
                    setShowCopyAllModal(!showCopyAllModal);
                }}
                >
                    <MdCopyAll style={{ width: '40px', height: '40px' }} />
                </IconButton>
            </Tooltip>
            <Tooltip title="Delete All">
                <IconButton onClick={() => setShowDeleteAllConfirm(!showDeleteAllConfirm)}>
                    <MdDeleteForever style={{ width: '40px', height: '40px', color: 'red' }} />
                </IconButton>
            </Tooltip>

            <SubmitSolution contestId={contestId} canBeCompeted={canContestBeCompeted} contestName={contestName} />
        </>
    );

    const renderCopyModal = (index: number, operation: AllowedOperations) => (
        <CopyModal
          key={index + operation}
          index={index}
          operation={operation}
          setShowModal={operation === AllowedOperations.Copy
              ? setShowCopyModal
              : setShowCopyAllModal}
          sourceId={contestId}
          sourceName={problemsData?.items
              ? problemsData?.items[0].contest
              : ''}
          problemToCopy={problemToCopy}
          setParentSuccessMessage={setSuccessMessage}
        />
    );

    return (
        <div style={{ marginTop: '2rem' }}>
            {renderErrorMessagesAlert(errorMessages)}
            {renderSuccessfullAlert(successMessage)}
            { isDeletingAll
                ? <SpinningLoader />
                : (
                    <AdministrationGridView
                      data={problemsData}
                      error={getContestError}
                      filterableGridColumnDef={problemFilterableColums}
                      notFilterableGridColumnDef={
                        returnProblemsNonFilterableColumns(
                            onEditClick,
                            openCopyModal,
                            openRetestModal,
                            retakeData,
                        )
}
                      queryParams={queryParams}
                      setQueryParams={setQueryParams}
                      defaultSorter={defaultProblemsSorterToAdd}
                      modals={[
                          { showModal: openEditModal, modal: (i) => renderProblemModal(i, false) },
                          { showModal: openShowCreateProblemModal, modal: (i) => renderProblemModal(i, true) },
                          { showModal: showDeleteAllConfirm, modal: (i) => renderDeleteAllModal(i) },
                          {
                              showModal: showCopyAllModal,
                              modal: (i) => renderCopyModal(i, AllowedOperations.CopyAll),
                          },
                          {
                              showModal: showCopyModal,
                              modal: (i) => renderCopyModal(i, AllowedOperations.Copy),
                          },
                          {
                              showModal: showRetestModal,
                              modal: (i) => renderRetestModal(i),
                          },
                      ]}
                      renderActionButtons={renderGridSettings}
                      withSearchParams={false}
                      legendProps={[ { color: getColors(themeMode).palette.deleted, message: 'Problem is deleted.' } ]}
                    />
                )}
        </div>
    );
};

export default ProblemsInContestView;
