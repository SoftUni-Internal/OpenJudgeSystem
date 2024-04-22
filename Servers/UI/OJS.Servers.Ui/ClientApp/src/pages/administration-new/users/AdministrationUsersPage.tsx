import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { IGetAllAdminParams } from '../../../common/types';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import UserForm from '../../../components/administration/users/form/UserForm';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { getColors } from '../../../hooks/use-administration-theme-provider';
import { useGetAllUsersQuery, useLazyExportUsersToExcelQuery } from '../../../redux/services/admin/usersAdminService';
import { useAppSelector } from '../../../redux/store';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultFilterToAdd, defaultSorterToAdd } from '../AdministrationGridView';

import usersFilterableColumns, { returnUsersNonFilterableColumns } from './usersGridColumns';

const AdministrationUsersPage = () => {
    const [ searchParams ] = useSearchParams();
    const themeMode = useAppSelector((x) => x.theme.administrationMode);
    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(defaultFilterToAdd, defaultSorterToAdd, searchParams));

    const [ showEditModal, setShowEditModal ] = useState<boolean>(false);
    const [ userId, setUserId ] = useState<string>('');

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

    const renderUserModal = (index: number) => (
        <AdministrationModal
          key={index}
          index={index}
          open={showEditModal}
          onClose={() => {
              setShowEditModal(false);
              retakeUsers();
          }}
        >
            <UserForm id={userId} />
        </AdministrationModal>
    );

    if (isLoadingUsers) {
        return <SpinningLoader />;
    }

    return (
        <AdministrationGridView
          filterableGridColumnDef={usersFilterableColumns}
          notFilterableGridColumnDef={returnUsersNonFilterableColumns(onEditClick)}
          data={usersData}
          error={error}
          queryParams={queryParams}
          setQueryParams={setQueryParams}
          legendProps={[ { color: getColors(themeMode).palette.deleted, message: 'User is deleted.' } ]}
          modals={
            [
                { showModal: showEditModal, modal: (i) => renderUserModal(i) },
            ]
          }
          excelMutation={useLazyExportUsersToExcelQuery}
        />
    );
};
export default AdministrationUsersPage;
