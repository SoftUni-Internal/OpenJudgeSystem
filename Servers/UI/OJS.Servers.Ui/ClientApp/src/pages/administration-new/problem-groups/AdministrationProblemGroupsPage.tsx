/* eslint-disable no-undefined */
import React, { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { IGetAllAdminParams } from '../../../common/types';
import CreateButton from '../../../components/administration/common/create/CreateButton';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import ProblemGroupForm from '../../../components/administration/problem-groups/problem-group-form/ProblemGroupForm';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { getColors } from '../../../hooks/use-administration-theme-provider';
import { useGetAllAdminProblemGroupsQuery, useLazyExportProblemGroupsToExcelQuery } from '../../../redux/services/admin/problemGroupsAdminService';
import { useAppSelector } from '../../../redux/store';
import { getAndSetExceptionMessage } from '../../../utils/messages-utils';
import { flexCenterObjectStyles } from '../../../utils/object-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../utils/render-utils';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultFilterToAdd, defaultSorterToAdd } from '../AdministrationGridView';

import filterableColumns, { returnNonFilterableColumns } from './problemGroupGridColumns';

const AdministrationProblemGroupsPage = () => {
    const [ searchParams ] = useSearchParams();
    const themeMode = useAppSelector((x) => x.theme.administrationMode);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ openEditModal, setOpenEditModal ] = useState<boolean>(false);
    const [ openCreateModal, setOpenCreateModal ] = useState<boolean>(false);
    const [ problemGroupId, setProblemGroupId ] = useState<number | undefined>(undefined);

    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(defaultFilterToAdd, defaultSorterToAdd, searchParams));

    const { refetch: retakeGroups, data, isLoading, error } = useGetAllAdminProblemGroupsQuery(queryParams);
    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);

    useEffect(() => {
        getAndSetExceptionMessage([ error ], setErrorMessages);
    }, [ error ]);

    const onEditClick = (id: number) => {
        setOpenEditModal(true);
        setProblemGroupId(id);
    };

    const onFormModalClose = (isCreate: boolean) => {
        if (isCreate) {
            setOpenCreateModal(false);
        } else {
            setOpenEditModal(false);
        }

        retakeGroups();
    };

    const renderGridSettings = () => (
        <div style={{ ...flexCenterObjectStyles, justifyContent: 'space-between' }}>
            <CreateButton
              showModal={openCreateModal}
              showModalFunc={setOpenCreateModal}
              styles={{ width: '40px', height: '40px' }}
            />
        </div>
    );

    if (isLoading) {
        return <div style={{ ...flexCenterObjectStyles }}><SpinningLoader /></div>;
    }

    const renderProblemModal = (index: number, isCreate: boolean) => (
        <AdministrationModal
          key={index}
          index={index}
          open={isCreate
              ? openCreateModal
              : openEditModal}
          onClose={() => onFormModalClose(isCreate)}
        >
            <ProblemGroupForm
              id={isCreate
                  ? 0
                  : problemGroupId}
              isEditMode={!isCreate}
              onSuccess={() => onFormModalClose(isCreate)}
              setParentSuccessMessage={setSuccessMessage}
            />
        </AdministrationModal>
    );

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            {renderErrorMessagesAlert(errorMessages)}
            <AdministrationGridView
              filterableGridColumnDef={filterableColumns}
              notFilterableGridColumnDef={returnNonFilterableColumns(onEditClick)}
              data={data}
              renderActionButtons={renderGridSettings}
              error={error}
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              modals={[
                  { showModal: openEditModal, modal: (i) => renderProblemModal(i, false) },
                  { showModal: openCreateModal, modal: (i) => renderProblemModal(i, true) },
              ]}
              legendProps={[ { color: getColors(themeMode).palette.deleted, message: 'Problem Group is deleted.' } ]}
              excelMutation={useLazyExportProblemGroupsToExcelQuery}
            />
        </>
    );
};

export default AdministrationProblemGroupsPage;
