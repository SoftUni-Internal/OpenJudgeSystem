import { useState } from 'react';

import { IGetAllAdminParams } from '../../../../common/types';
import {
    applyDefaultFilterToQueryString,
} from '../../../../pages/administration-new/administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultSorterToAdd } from '../../../../pages/administration-new/AdministrationGridView';
import participantsFilteringColumns, { returnparticipantsNonFilterableColumns } from '../../../../pages/administration-new/participants/participantsGridColumns';
import { useGetByContestIdQuery } from '../../../../redux/services/admin/participantsAdminService';
import { renderSuccessfullAlert } from '../../../../utils/render-utils';
import CreateButton from '../../common/create/CreateButton';
import AdministrationModal from '../../common/modals/administration-modal/AdministrationModal';
import ParticipantForm from '../../participants/form/ParticipantForm';

// eslint-disable-next-line css-modules/no-unused-class
import styles from '../../../../pages/administration-new/AdministrationStyles.module.scss';

interface IParticipantsInContestView {
    contestId: number;
    contestName: string;
}

const ParticipantsInContestView = (props: IParticipantsInContestView) => {
    const { contestId, contestName } = props;

    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ openCreateModal, setOpenCreateModal ] = useState<boolean>(false);

    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString('', defaultSorterToAdd));

    const { refetch, data, error } = useGetByContestIdQuery({ contestId: Number(contestId), ...queryParams });

    const onModalClose = () => {
        setOpenCreateModal(false);
        refetch();
    };

    const renderActions = () => (
        <CreateButton
          showModal={openCreateModal}
          showModalFunc={setOpenCreateModal}
          styles={{ width: '40px', height: '40px' }}
        />
    );

    const renderParticipantModal = (index: number) => (
        <AdministrationModal
          key={index}
          index={index}
          open={openCreateModal}
          onClose={() => onModalClose()}
        >
            <ParticipantForm
              contestId={contestId}
              contestName={contestName}
              onSuccess={onModalClose}
              setParentSuccessMessage={setSuccessMessage}
            />
        </AdministrationModal>
    );

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            <div className={styles.container}>
                <AdministrationGridView
                  data={data}
                  error={error}
                  filterableGridColumnDef={participantsFilteringColumns}
                  notFilterableGridColumnDef={returnparticipantsNonFilterableColumns(refetch)}
                  queryParams={queryParams}
                  renderActionButtons={renderActions}
                  modals={[
                      { showModal: openCreateModal, modal: (i) => renderParticipantModal(i) },
                  ]}
                  setQueryParams={setQueryParams}
                  withSearchParams={false}
                />
            </div>
        </>
    );
};

export default ParticipantsInContestView;
