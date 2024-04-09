import React, { useState } from 'react';

import AdministrationGridView from '../../../../pages/administration-new/AdministrationGridView';
import problemResourceFilterableColumns, { returnProblemResourceNonFilterableColumns } from '../../../../pages/administration-new/problem-resources/problemResourcesGridColumns';
import { useGetResourcesQuery } from '../../../../redux/services/admin/problemsAdminService';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import CreateButton from '../../common/create/CreateButton';
import AdministrationModal from '../../common/modals/administration-modal/AdministrationModal';
import ProblemResourceForm from '../../problem-resources/problem-resource-form/ProblemResourceForm';

interface IResourceInproblemViewProps {
problemId: number;
}
const ResourcesInProblemView = (props : IResourceInproblemViewProps) => {
    const { problemId } = props;
    const [ openEditModal, setOpenEditModal ] = useState<boolean>(false);
    const [ showCreateModal, setShowCreateModal ] = useState<boolean>(false);

    const [ problemResourceId, setProblemResourceId ] = useState<number>(0);
    const {
        refetch: retakeData,
        data: resourcesData,
        isLoading: isGettingResources,
        error: resourcesError,
    } = useGetResourcesQuery(Number(problemId));

    const onEditClick = (id: number) => {
        setOpenEditModal(true);
        setProblemResourceId(id);
    };

    const renderProblemResourceModal = (index: number, isCreate: boolean) => (
        <AdministrationModal
          key={index}
          index={index}
          open={isCreate
              ? showCreateModal
              : openEditModal}
          onClose={() => isCreate
              ? setShowCreateModal(!showCreateModal)
              : setOpenEditModal(false)}
        >
            <ProblemResourceForm
              id={problemResourceId}
              isEditMode={!isCreate}
              problemId={problemId}
            />
        </AdministrationModal>
    );

    const renderGridSettings = () => (
        <CreateButton
          showModal={showCreateModal}
          showModalFunc={setShowCreateModal}
          styles={{ width: '40px', height: '40px', color: 'rgb(25,118,210)' }}
        />

    );

    if (isGettingResources) {
        return <SpinningLoader />;
    }
    return (
        <AdministrationGridView
          filterableGridColumnDef={problemResourceFilterableColumns}
          notFilterableGridColumnDef={returnProblemResourceNonFilterableColumns(onEditClick, retakeData)}
          data={resourcesData}
          error={resourcesError}
          selectedFilters={[]}
          selectedSorters={[]}
          showFiltersAndSorters={false}
          renderActionButtons={renderGridSettings}
          legendProps={[ { color: '#FFA1A1', message: 'Problem Resource is deleted.' } ]}
          modals={[
              { showModal: openEditModal, modal: (i) => renderProblemResourceModal(i, false) },
              { showModal: showCreateModal, modal: (i) => renderProblemResourceModal(i, true) },
          ]}
        />
    );
};

export default ResourcesInProblemView;
