import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { IGetAllAdminParams } from '../../../common/types';
import CreateButton from '../../../components/administration/common/create/CreateButton';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import ParticipantForm from '../../../components/administration/participants/form/ParticipantForm';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { useDeleteParticipantMutation, useGetAllParticipantsQuery, useLazyExportParticipantsToExcelQuery } from '../../../redux/services/admin/participantsAdminService';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultSorterToAdd } from '../AdministrationGridView';

import participantsFilteringColumns, { returnparticipantsNonFilterableColumns } from './participantsGridColumns';

const ParticipantsAdministrationPage = () => {
    const [ searchParams ] = useSearchParams();

    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString('', defaultSorterToAdd, searchParams));

    const [ openCreateModal, setOpenCreateModal ] = useState<boolean>(false);

    const {
        refetch: retakeParticipants,
        data: participantsData,
        isLoading: isLoadingParticipants,
        error,
    } = useGetAllParticipantsQuery(queryParams);

    const onModalClose = () => {
        setOpenCreateModal(false);
        retakeParticipants();
    };

    const renderGridSettings = () => (
        <CreateButton
          showModal={openCreateModal}
          showModalFunc={setOpenCreateModal}
          styles={{ width: '40px', height: '40px', color: 'rgb(25,118,210)' }}
        />
    );

    const renderParticipantModal = (index: number) => (
        <AdministrationModal
          key={index}
          index={index}
          open={openCreateModal}
          onClose={() => onModalClose()}
        >
            <ParticipantForm />
        </AdministrationModal>
    );

    if (isLoadingParticipants) {
        return <SpinningLoader />;
    }

    return (
        <AdministrationGridView
          filterableGridColumnDef={participantsFilteringColumns}
          notFilterableGridColumnDef={returnparticipantsNonFilterableColumns(useDeleteParticipantMutation, retakeParticipants)}
          data={participantsData}
          error={error}
          queryParams={queryParams}
          setQueryParams={setQueryParams}
          renderActionButtons={renderGridSettings}
          modals={[
              { showModal: openCreateModal, modal: (i) => renderParticipantModal(i) },
          ]}
          excelMutation={useLazyExportParticipantsToExcelQuery}
        />
    );
};
export default ParticipantsAdministrationPage;
