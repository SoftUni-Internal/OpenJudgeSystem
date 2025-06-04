import React, { useState } from 'react';
import { useGetContestResourcesQuery } from 'src/redux/services/admin/contestsAdminService';
import { useGetProblemResourcesQuery } from 'src/redux/services/admin/problemsAdminService';

import { IGetAllAdminParams } from '../../../../common/types';
import { getColors, useAdministrationTheme } from '../../../../hooks/use-administration-theme-provider';
import {
    applyDefaultFilterToQueryString,
} from '../../../../pages/administration-new/administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultSorterToAdd } from '../../../../pages/administration-new/AdministrationGridView';
import problemResourceFilterableColumns, {
    returnProblemResourceNonFilterableColumns,
} from '../../../../pages/administration-new/problem-resources/problemResourcesGridColumns';
import { renderSuccessfullAlert } from '../../../../utils/render-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import CreateButton from '../../common/create/CreateButton';
import AdministrationModal from '../../common/modals/administration-modal/AdministrationModal';
import ResourceForm from '../../problem-resources/problem-resource-form/ResourceForm';

interface IResourceInProblemViewProps {
    parentId: number;
    isForContest: boolean;
}

const ResourcesInProblemView = (props : IResourceInProblemViewProps) => {
    const { parentId, isForContest } = props;
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ openEditModal, setOpenEditModal ] = useState<boolean>(false);
    const [ showCreateModal, setShowCreateModal ] = useState<boolean>(false);
    const { themeMode } = useAdministrationTheme();
    const [ problemResourceId, setProblemResourceId ] = useState<number>(0);
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString('', defaultSorterToAdd));

    const {
        refetch: retakeProblemResourcesData,
        data: problemResourcesData,
        isLoading: isGettingProblemResources,
        error: problemResourcesError,
        // eslint-disable-next-line no-undef
    } = useGetProblemResourcesQuery({ parentId: Number(parentId), ...queryParams });

    const {
        refetch: retakeContestResourcesData,
        data: contestResourcesData,
        isLoading: isGettingContestResources,
        error: contestResourcesError,
    } = useGetContestResourcesQuery({ parentId: Number(parentId), ...queryParams });

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

            if (isForContest) {
                retakeProblemResourcesData();
            } else {
                retakeContestResourcesData();
            }
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
                <ResourceForm
                  id={problemResourceId}
                  isForContest={isForContest}
                  isEditMode={!isCreate}
                  problemId={parentId}
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

    if (isGettingProblemResources || isGettingContestResources) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            <AdministrationGridView
              filterableGridColumnDef={problemResourceFilterableColumns}
              notFilterableGridColumnDef={returnProblemResourceNonFilterableColumns(onEditClick, isForContest
                  ? retakeContestResourcesData
                  : retakeProblemResourcesData, setSuccessMessage)}
              data={isForContest
                  ? contestResourcesData
                  : problemResourcesData}
              error={isForContest
                  ? contestResourcesError
                  : problemResourcesError}
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
