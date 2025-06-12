/* eslint-disable @typescript-eslint/ban-types */
import React, { useEffect, useMemo, useState } from 'react';
import { MdOutlineManageHistory } from 'react-icons/md';
import { useLocation, useNavigate } from 'react-router-dom';
import { ExcelFilterOperators } from 'src/common/enums';
import ChangeParticipantsTime
    from 'src/components/administration/participants/change-participants-time/ChangeParticipantsTime';

import { IContestAdministration, IGetAllAdminParams } from '../../../../common/types';
import {
    applyDefaultFilterToQueryString,
} from '../../../../pages/administration-new/administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultSorterToAdd } from '../../../../pages/administration-new/AdministrationGridView';
import participantsFilteringColumns, { returnparticipantsNonFilterableColumns } from '../../../../pages/administration-new/participants/participantsGridColumns';
import { useGetByContestIdQuery, useLazyExportParticipantsToExcelQuery } from '../../../../redux/services/admin/participantsAdminService';
import { renderSuccessfullAlert } from '../../../../utils/render-utils';
import CreateButton from '../../common/create/CreateButton';
import AdministrationModal from '../../common/modals/administration-modal/AdministrationModal';
import ParticipantForm from '../../participants/form/ParticipantForm';

// eslint-disable-next-line css-modules/no-unused-class
import styles from '../../../../pages/administration-new/AdministrationStyles.module.scss';

interface IParticipantsInContestView {
    contestId: number;
    contestName: string;
    contest: IContestAdministration;
    setParentSuccessMessage: Function;
}

const ParticipantsInContestView = (props: IParticipantsInContestView) => {
    const { contestId, contestName, contest, setParentSuccessMessage } = props;
    const location = useLocation();
    const params = useMemo(() => new URLSearchParams(location.search), [ location.search ]);
    const navigate = useNavigate();

    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ openCreateModal, setOpenCreateModal ] = useState<boolean>(false);
    const [ openChangeTimeModal, setOpenChangeTimeModal ] = useState<boolean>(false);

    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString('', defaultSorterToAdd));

    const { refetch, data, error } = useGetByContestIdQuery({ contestId: Number(contestId), ...queryParams });

    const onModalClose = () => {
        setOpenCreateModal(false);
        refetch();
    };

    const renderActions = () => (
        <>
            <CreateButton
              showModal={openCreateModal}
              showModalFunc={setOpenCreateModal}
              styles={{ width: '40px', height: '40px' }}
            />
            <CreateButton
              showModal={openChangeTimeModal}
              showModalFunc={setOpenChangeTimeModal}
              tooltipLabel="Change Participation Time"
              Icon={MdOutlineManageHistory}
              styles={{ width: '40px', height: '40px' }}
            />
        </>
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

    const renderChangeParticipantsTimeModal = (index: number) => (
        <AdministrationModal
          index={index}
          open={openChangeTimeModal}
          onClose={() => setOpenChangeTimeModal(false)}
        >
            <ChangeParticipantsTime
              contest={contest}
              setParentSuccessMessage={setParentSuccessMessage}
              onSuccess={() => setOpenChangeTimeModal(false)}
            />
        </AdministrationModal>
    );

    useEffect(() => {
        if (params.get('openChangeParticipantsTime') === 'true') {
            setOpenChangeTimeModal(true);
            params.delete('openChangeParticipantsTime');
            navigate(`${location.pathname}?${params.toString()}#tab-participants`, { replace: true });
        }
    }, [ location.pathname, navigate, params ]);

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            <div className={styles.container}>
                <AdministrationGridView
                  data={data}
                  error={error}
                  filterableGridColumnDef={participantsFilteringColumns}
                  notFilterableGridColumnDef={returnparticipantsNonFilterableColumns(refetch, setParentSuccessMessage)}
                  queryParams={queryParams}
                  renderActionButtons={renderActions}
                  modals={[
                      { showModal: openCreateModal, modal: (i) => renderParticipantModal(i) },
                      { showModal: openChangeTimeModal, modal: (i) => renderChangeParticipantsTimeModal(i) },
                  ]}
                  setQueryParams={setQueryParams}
                  withSearchParams={false}
                  excelMutation={useLazyExportParticipantsToExcelQuery}
                  excelFilters={[ { propertyName: 'contestId', operator: ExcelFilterOperators.Equals, value: contestId } ]}
                />
            </div>
        </>
    );
};

export default ParticipantsInContestView;
