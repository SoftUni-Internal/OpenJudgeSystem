import React, { useState } from 'react';
import { TbBinaryTree } from 'react-icons/tb';
import { useSearchParams } from 'react-router-dom';
import AddIcon from '@mui/icons-material/Add';
import AutoStoriesIcon from '@mui/icons-material/AutoStories';
import RemoveIcon from '@mui/icons-material/Remove';

import { IGetAllAdminParams } from '../../../common/types';
import AdministrationGridDropdown
    from '../../../components/administration/common/administration-grid-dropdown/AdministrationGridDropdown';
import LecturerActions from '../../../components/administration/common/lecturer-actions/LecturerActions';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import UserForm from '../../../components/administration/users/form/UserForm';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { getColors, useAdministrationTheme } from '../../../hooks/use-administration-theme-provider';
import { useGetAllUsersQuery, useLazyExportUsersToExcelQuery } from '../../../redux/services/admin/usersAdminService';
import { renderSuccessfullAlert } from '../../../utils/render-utils';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultFilterToAdd, defaultSorterToAdd } from '../AdministrationGridView';

import usersFilterableColumns, { returnUsersNonFilterableColumns } from './usersGridColumns';

import styles from './AdministrationUsersPage.module.scss';

const AdministrationUsersPage = () => {
    const [ searchParams ] = useSearchParams();
    const { themeMode } = useAdministrationTheme();

    const [ queryParams, setQueryParams ] =
        useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(
            defaultFilterToAdd,
            defaultSorterToAdd,
            searchParams,
        ));
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ showEditModal, setShowEditModal ] = useState<boolean>(false);
    const [ userId, setUserId ] = useState<string>('');

    const [ lecturerActionModal, setLecturerActionModal ] = useState({
        isVisible: false,
        isRemove: false,
        isContest: false,
    });

    const {
        refetch: retakeUsers,
        data: usersData,
        isLoading: isLoadingUsers,
        error,
    } = useGetAllUsersQuery(queryParams);

    const onEditClick = (id: string) => {
        setShowEditModal(true);
        setUserId(id);
    };

    const renderUserModal = (index: number) => 
        <AdministrationModal
          key={index}
          index={index}
          open={showEditModal}
          onClose={() => {
              setShowEditModal(false);
              retakeUsers();
          }}
        >
            <UserForm
              id={userId}
              onSuccess={() => {
                  setShowEditModal(false);
                  retakeUsers();
              }}
              setParentSuccessMessage={setSuccessMessage}
            />
        </AdministrationModal>
    ;

    const openLecturerActionModal = (isRemove: boolean, isContest: boolean) => {
        setLecturerActionModal({ isVisible: true, isRemove, isContest });
    };

    const onAddLecturerToCategory = () => openLecturerActionModal(false, false);
    const onRemoveLecturerFromCategory = () => openLecturerActionModal(true, false);
    const onAddLecturerToContest = () => openLecturerActionModal(false, true);
    const onRemoveLecturerFromContest = () => openLecturerActionModal(true, true);

    if (isLoadingUsers) {
        return <SpinningLoader />;
    }

    const renderLecturerActions = () => 
        <>
            <AdministrationGridDropdown
              icon={<TbBinaryTree className={styles.icon} />}
              tooltipTitle="Contest Category Actions"
              sections={[
                  {
                      icon: <AddIcon className={styles.button} />,
                      label: 'Add Lecturer To Category',
                      handleClick: onAddLecturerToCategory,
                  },
                  {
                      icon: <RemoveIcon className={styles.button} />,
                      label: 'Remove Lecturer From Category',
                      handleClick: onRemoveLecturerFromCategory,
                  },
              ]}
              id={1}
            />
            <AdministrationGridDropdown
              icon={<AutoStoriesIcon className={styles.icon} />}
              tooltipTitle="Contest Actions"
              sections={[
                  {
                      icon: <AddIcon className={styles.button} />,
                      label: 'Add Lecturer To Contest',
                      handleClick: onAddLecturerToContest,
                  },
                  {
                      icon: <RemoveIcon className={styles.button} />,
                      label: 'Remove Lecturer From Contest',
                      handleClick: onRemoveLecturerFromContest,
                  },
              ]}
              id={2}
            />
        </>
    ;

    return (
        <>
            {lecturerActionModal.isVisible && 
                <LecturerActions
                  index={
                        lecturerActionModal.isRemove
                            ? lecturerActionModal.isContest
                                ? 4
                                : 2
                            : lecturerActionModal.isContest
                                ? 3
                                : 1
                    }
                  showModal={lecturerActionModal.isVisible}
                  setShowModal={(isVisible: boolean) => setLecturerActionModal({ ...lecturerActionModal, isVisible })}
                  isRemove={lecturerActionModal.isRemove}
                  isContest={lecturerActionModal.isContest}
                  setParentSuccessMessage={setSuccessMessage}
                  onSuccess={() => setLecturerActionModal({ ...lecturerActionModal, isVisible: false })}
                />
            }
            {renderSuccessfullAlert(successMessage)}
            <AdministrationGridView
              filterableGridColumnDef={usersFilterableColumns}
              notFilterableGridColumnDef={returnUsersNonFilterableColumns(onEditClick)}
              data={usersData}
              error={error}
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              renderActionButtons={renderLecturerActions}
              legendProps={[
                  { color: getColors(themeMode).palette.deleted, message: 'User is deleted.' },
              ]}
              modals={[ { showModal: showEditModal, modal: (i) => renderUserModal(i) } ]}
              excelMutation={useLazyExportUsersToExcelQuery}
            />
        </>
    );
};

export default AdministrationUsersPage;
