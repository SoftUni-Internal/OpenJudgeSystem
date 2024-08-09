import React, { useState } from 'react';
import { TbReplaceFilled } from 'react-icons/tb';
import { useSelector } from 'react-redux';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { IconButton, Tooltip } from '@mui/material';

import { IGetAllAdminParams } from '../../../common/types';
import { NEW_ADMINISTRATION_PATH, SUBMISSION_TYPES_PATH } from '../../../common/urls/administration-urls';
import CreateButton from '../../../components/administration/common/create/CreateButton';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import SubmissionTypesForm from '../../../components/administration/submission-types/form/SubmissionTypeForm';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { IAuthorizationReduxState } from '../../../redux/features/authorizationSlice';
import { useGetAllSubmissionTypesQuery, useLazyExportSubmissionTypesToExcelQuery } from '../../../redux/services/admin/submissionTypesAdminService';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultSorterToAdd } from '../AdministrationGridView';

import submissionTypesFilterableColumns, { returnNonFilterableColumns } from './submissionTypesGridColumns';

const AdministrationSubmissionTypesPage = () => {
    const [ searchParams ] = useSearchParams();
    const navigate = useNavigate();

    const [ showCreateModal, setShowCreateModal ] = useState<boolean>(false);
    const [ showEditModal, setShowEditModal ] = useState<boolean>(false);
    const [ submissionTypeId, setSubmissionTypeId ] = useState<number | null>(null);

    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString('', defaultSorterToAdd, searchParams));

    const {
        refetch,
        data: submissionTypesData,
        isLoading: isGettingData,
        error,
    } = useGetAllSubmissionTypesQuery(queryParams);

    const { internalUser: user } = useSelector((state: {authorization: IAuthorizationReduxState}) => state.authorization);

    const onEditClick = (id: number) => {
        setSubmissionTypeId(id);
        setShowEditModal(true);
    };

    if (isGettingData) {
        return <SpinningLoader />;
    }
    const onModalClose = (isEditMode : boolean) => {
        if (isEditMode) {
            setShowEditModal(false);
        } else {
            setShowCreateModal(false);
        }
        refetch();
    };
    const onSuccessFullDelete = () => {
        refetch();
    };

    const renderFormModal = (index: number, isEditMode: boolean) => (
        <AdministrationModal
          key={index}
          index={index}
          open={isEditMode
              ? showEditModal
              : showCreateModal}
          onClose={() => onModalClose(isEditMode)}
        >
            <SubmissionTypesForm id={submissionTypeId} isEditMode={isEditMode} />
        </AdministrationModal>
    );

    const renderGridSettings = () => (
        <>
            <CreateButton
              showModal={showCreateModal}
              showModalFunc={setShowCreateModal}
              styles={{ width: '40px', height: '40px' }}
            />
            {
                user.isDeveloper && (
                    <Tooltip title="Replace/Delete Submission Type">
                        <span>
                            <IconButton
                              disabled={!user.isDeveloper}
                              onClick={() => navigate(`/${NEW_ADMINISTRATION_PATH}/${SUBMISSION_TYPES_PATH}/deleteReplaceSubmissionTypes`)}
                            >
                                <TbReplaceFilled
                                  style={{ color: 'red' }}
                                />
                            </IconButton>
                        </span>
                    </Tooltip>
                )
            }
        </>

    );

    return (
        <AdministrationGridView
          filterableGridColumnDef={submissionTypesFilterableColumns}
          notFilterableGridColumnDef={returnNonFilterableColumns(onEditClick, onSuccessFullDelete)}
          data={submissionTypesData}
          error={error}
          queryParams={queryParams}
          setQueryParams={setQueryParams}
          modals={[
              { showModal: showEditModal, modal: (i) => renderFormModal(i, true) },
              { showModal: showCreateModal, modal: (i) => renderFormModal(i, false) },
          ]}
          renderActionButtons={renderGridSettings}
          excelMutation={useLazyExportSubmissionTypesToExcelQuery}
        />
    );
};
export default AdministrationSubmissionTypesPage;
