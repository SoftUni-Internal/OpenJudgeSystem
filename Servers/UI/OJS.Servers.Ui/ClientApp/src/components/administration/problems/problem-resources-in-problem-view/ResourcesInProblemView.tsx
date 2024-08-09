import React, { useState } from 'react';

import { IGetAllAdminParams } from '../../../../common/types';
import { getColors } from '../../../../hooks/use-administration-theme-provider';
import {
    applyDefaultFilterToQueryString,
} from '../../../../pages/administration-new/administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultSorterToAdd } from '../../../../pages/administration-new/AdministrationGridView';
import problemResourceFilterableColumns, {
    returnProblemResourceNonFilterableColumns,
} from '../../../../pages/administration-new/problem-resources/problemResourcesGridColumns';
import { useGetResourcesQuery } from '../../../../redux/services/admin/problemsAdminService';
import { useAppSelector } from '../../../../redux/store';
import { renderSuccessfullAlert } from '../../../../utils/render-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import CreateButton from '../../common/create/CreateButton';
import AdministrationModal from '../../common/modals/administration-modal/AdministrationModal';
import ProblemResourceForm from '../../problem-resources/problem-resource-form/ProblemResourceForm';

interface IResourceInproblemViewProps {
    problemId: number;
}

const ResourcesInProblemView = (props : IResourceInproblemViewProps) => {
    const { problemId } = props;
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ openEditModal, setOpenEditModal ] = useState<boolean>(false);
    const [ showCreateModal, setShowCreateModal ] = useState<boolean>(false);
    const themeMode = useAppSelector((x) => x.theme.administrationMode);
    const [ problemResourceId, setProblemResourceId ] = useState<number>(0);
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString('', defaultSorterToAdd));

    const {
        refetch: retakeData,
        data: resourcesData,
        isLoading: isGettingResources,
        error: resourcesError,
    } = useGetResourcesQuery({ problemId: Number(problemId), ...queryParams });

    const onEditClick = (id: number) => {
        setOpenEditModal(true);
        setProblemResourceId(id);
    };

    const renderProblemResourceModal = (index: number, isCreate: boolean) => {
        const onClose = () => isCreate
            ? setShowCreateModal(!showCreateModal)
            : setOpenEditModal(false);

        const onProblemCreate = () => {
            onClose();
            retakeData();
        };

        return (
            <AdministrationModal
              key={index}
              index={index}
              open={isCreate
                  ? showCreateModal
                  : openEditModal}
              onClose={onClose}
            >
                <ProblemResourceForm
                  id={problemResourceId}
                  isEditMode={!isCreate}
                  problemId={problemId}
                  onSuccess={onProblemCreate}
                  setParentSuccessMessage={setSuccessMessage}
                />
            </AdministrationModal>
        );
    };

    const renderGridSettings = () => (
        <CreateButton
          showModal={showCreateModal}
          showModalFunc={setShowCreateModal}
          styles={{ width: '40px', height: '40px' }}
        />

    );

    if (isGettingResources) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            <AdministrationGridView
              filterableGridColumnDef={problemResourceFilterableColumns}
              notFilterableGridColumnDef={returnProblemResourceNonFilterableColumns(onEditClick, retakeData)}
              data={resourcesData}
              error={resourcesError}
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              withSearchParams={false}
              renderActionButtons={renderGridSettings}
              legendProps={[ { color: getColors(themeMode).palette.deleted, message: 'Problem Resource is deleted.' } ]}
              modals={[
                  { showModal: openEditModal, modal: (i) => renderProblemResourceModal(i, false) },
                  { showModal: showCreateModal, modal: (i) => renderProblemResourceModal(i, true) },
              ]}
            />
        </>
    );
};

export default ResourcesInProblemView;
