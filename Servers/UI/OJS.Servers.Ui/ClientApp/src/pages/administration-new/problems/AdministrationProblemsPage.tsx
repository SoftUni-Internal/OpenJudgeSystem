import { useCallback, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { IGetAllAdminParams } from '../../../common/types';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import CopyModal, { AllowedOperations } from '../../../components/administration/problems/copy/CopyModal';
import ProblemForm from '../../../components/administration/problems/problem-form/ProblemForm';
import ProblemRetest from '../../../components/administration/problems/retest/ProblemRetest';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { getColors, useAdministrationTheme } from '../../../hooks/use-administration-theme-provider';
import { useGetAllAdminProblemsQuery, useLazyExportProblemsToExcelQuery } from '../../../redux/services/admin/problemsAdminService';
import { flexCenterObjectStyles } from '../../../utils/object-utils';
import { renderSuccessfullAlert } from '../../../utils/render-utils';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultFilterToAdd, defaultSorterToAdd } from '../AdministrationGridView';

import filterableColumns, { returnProblemsNonFilterableColumns } from './problemGridColumns';

const AdministrationProblemsPage = () => {
    const [ searchParams ] = useSearchParams();
    const [ openEditProblemModal, setOpenEditProblemModal ] = useState(false);
    const { themeMode } = useAdministrationTheme();
    const [ problemId, setProblemId ] = useState<number | null>(null);
    const [ showRetestModal, setShowRetestModal ] = useState<boolean>(false);
    const [ successMessage, setSuccessMessage ] = useState <string | null>(null);
    const [ showCopyModal, setShowCopyModal ] = useState<boolean>(false);

    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(defaultFilterToAdd, defaultSorterToAdd, searchParams));

    const {
        data: problemsData,
        refetch: retakeProblems,
        isLoading: isLoadingProblems,
        error,
    } = useGetAllAdminProblemsQuery(queryParams);

    const onEditClick = (id: number) => {
        setOpenEditProblemModal(true);
        setProblemId(id);
    };

    const openRetestModal = (id: number) => {
        setShowRetestModal(true);
        setProblemId(id);
    };

    const onSuccessOperation = (message: string) => {
        setSuccessMessage(message);
        setShowRetestModal(false);
    };

    const openCopyModal = (id: number) => {
        setShowCopyModal(true);
        setProblemId(id);
    };

    const onEditModalClose = () => {
        setOpenEditProblemModal(false);
        retakeProblems();
    };

    const findProblemInData = useCallback(() => problemsData?.items?.find((x) => x.id === problemId), [ problemId, problemsData ]);

    const renderRetestModal = (index: number) => (
        <ProblemRetest
          key={index}
          contestId={findProblemInData()?.contestId || 0}
          declineFunction={() => setShowRetestModal(!showRetestModal)}
          index={index}
          problemData={problemsData}
          problemName={problemsData?.items
              ? findProblemInData()?.name
              : 'problem'}
          problemToRetest={problemId!}
          onSuccess={onSuccessOperation}
        />
    );

    const renderProblemsEditModal = (index: number) => (
        <AdministrationModal
          index={index}
          open={openEditProblemModal}
          onClose={() => onEditModalClose()}
        >
            <ProblemForm
              problemId={Number(problemId)}
              isEditMode
              contestId={null}
              contestName={null}
              onSuccess={() => onEditModalClose()}
              setParentSuccessMessage={setSuccessMessage}
            />
        </AdministrationModal>
    );

    const renderCopyModal = (index: number) => (
        <CopyModal
          key={index}
          index={index}
          operation={AllowedOperations.Copy}
          setShowModal={setShowCopyModal}
          sourceContestId={findProblemInData()?.contestId || 0}
          sourceContestName={findProblemInData()?.contest || ''}
          problemToCopyName={findProblemInData()?.name || ''}
          problemToCopyId={problemId}
          setParentSuccessMessage={setSuccessMessage}
        />
    );

    if (isLoadingProblems) {
        return <div style={{ ...flexCenterObjectStyles }}><SpinningLoader /></div>;
    }

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            <AdministrationGridView
              filterableGridColumnDef={filterableColumns}
              notFilterableGridColumnDef={
                returnProblemsNonFilterableColumns(
                    onEditClick,
                    openCopyModal,
                    openRetestModal,
                    retakeProblems,
                )
}
              data={problemsData}
              error={error}
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              modals={[
                  { showModal: openEditProblemModal, modal: (i) => renderProblemsEditModal(i) },
                  { showModal: showRetestModal, modal: (i) => renderRetestModal(i) },
                  { showModal: showCopyModal, modal: (i) => renderCopyModal(i) },
              ]}
              legendProps={[ { color: getColors(themeMode).palette.deleted, message: 'Problem is deleted.' } ]}
              excelMutation={useLazyExportProblemsToExcelQuery}
            />
        </>
    );
};

export default AdministrationProblemsPage;
