import { useEffect, useState } from 'react';
import { Autocomplete, FormControl, MenuItem, TextField, Typography } from '@mui/material';

import { IGetAllAdminParams, IUserAutocompleteData } from '../../../../common/types';
import { IAdministrationFilter, mapFilterParamsToQueryString } from '../../../../pages/administration-new/administration-filters/AdministrationFilters';
import { IAdministrationSorter, mapSorterParamsToQueryString } from '../../../../pages/administration-new/administration-sorting/AdministrationSorting';
import AdministrationGridView from '../../../../pages/administration-new/AdministrationGridView';
import usersFilterableColumns, { returnUsersNonFilterableColumns } from '../../../../pages/administration-new/users/usersGridColumns';
import { useAddUserToRoleMutation, useRemoveUserFromRoleMutation } from '../../../../redux/services/admin/rolesAdminService';
import { useGetUsersAutocompleteQuery, useGetUsersByRoleQuery } from '../../../../redux/services/admin/usersAdminService';
import { DEFAULT_ITEMS_PER_PAGE } from '../../../../utils/constants';
import { getAndSetExceptionMessage, getAndSetSuccesfullMessages } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import ConfirmDialog from '../../../guidelines/dialog/ConfirmDialog';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import CreateButton from '../../common/create/CreateButton';
import AdministrationModal from '../../common/modals/administration-modal/AdministrationModal';
import FormActionButton from '../../form-action-button/FormActionButton';
import UserForm from '../../users/form/UserForm';
import { onAutocompleteInputChange } from '../../utils/mui-utils';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';

interface IUsersInRoleViewProps {
    roleId: string;
}

const UsersInRoleView = (props: IUsersInRoleViewProps) => {
    const { roleId } = props;

    const [ selectedFilters, setSelectedFilters ] = useState<Array<IAdministrationFilter>>([]);
    const [ selectedSorters, setSelectedSorters ] = useState<Array<IAdministrationSorter>>([]);

    const [ errorMessages, setErrorMessages ] = useState <Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState <string | null>(null);

    const [ userIdToAdd, setUserIdToAdd ] = useState<string | null>(null);

    const [ showUserEditModal, setShowUserEditModal ] = useState<boolean>(false);
    const [ userId, setUserId ] = useState<string | null>(null);

    const [ showConfirmDialog, setShowConfirmDialog ] = useState<boolean>(false);

    const [ usersAutocomplete, setUsersAutocomplete ] = useState<Array<IUserAutocompleteData>>([
        {
            id: '',
            userName: '',
        },
    ]);

    const [ usersSearchString, setUsersSearchString ] = useState<string>('');

    const [ showCreateModal, setShowCreateModal ] = useState<boolean>(false);

    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>({
        page: 1,
        itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
        filter: mapFilterParamsToQueryString(selectedFilters),
        sorting: mapSorterParamsToQueryString(selectedSorters),
    });

    const filtersQueryParams = mapFilterParamsToQueryString(selectedFilters);

    const sortersQueryParams = mapSorterParamsToQueryString(selectedSorters);

    const {
        refetch,
        data: usersData,
        error: getError,
        isLoading: isGetting,
    } = useGetUsersByRoleQuery({ roleId, ...queryParams });

    const [
        addUserToRole,
        {
            data: addData,
            error: addError,
            isSuccess: isSuccessfullyAddedToRole,
            isLoading: isAddingToRole,
        },
    ] = useAddUserToRoleMutation();

    const [
        removeFromRole,
        {
            data: removeData,
            error: removeError,
            isSuccess: isSuccessfullyRemoved,
            isLoading: isRemovingFromRole,
        },
    ] = useRemoveUserFromRoleMutation();

    const {
        data: usersAutocompleteData,
        error: getUsersDataError,
    } = useGetUsersAutocompleteQuery(usersSearchString);

    useEffect(() => {
        if (usersAutocompleteData) {
            setUsersAutocomplete(usersAutocompleteData);
        }
    }, [ usersAutocompleteData ]);

    useEffect(() => {
        getAndSetExceptionMessage([ getUsersDataError, addError, removeError ], setErrorMessages);
        setSuccessMessage(null);
    }, [ addError, getUsersDataError, removeError ]);

    useEffect(() => {
        const message = getAndSetSuccesfullMessages([
            {
                message: addData,
                shouldGet: isSuccessfullyAddedToRole,
            },
            {
                message: removeData,
                shouldGet: isSuccessfullyRemoved,
            } ]);

        setSuccessMessage(message);
    }, [ addData, isSuccessfullyAddedToRole, isSuccessfullyRemoved, removeData ]);

    useEffect(() => {
        setQueryParams((currentParams) => ({ ...currentParams, filter: filtersQueryParams }));
    }, [ filtersQueryParams ]);

    useEffect(() => {
        setQueryParams((currentParams) => ({ ...currentParams, sorting: sortersQueryParams }));
    }, [ sortersQueryParams ]);

    useEffect(() => {
        if (isSuccessfullyRemoved || isSuccessfullyAddedToRole) {
            refetch();
        }
    }, [ isSuccessfullyAddedToRole, isSuccessfullyRemoved, refetch ]);

    const onEditClick = (id: string) => {
        setUserId(id);
        setShowUserEditModal(true);
    };

    const onSelectUser = (user: IUserAutocompleteData) => {
        let uId = '';
        if (user) {
            uId = user.id;
        }

        setUserIdToAdd(uId);
    };

    const onRemoveFromRowClicked = (uId: string) => {
        setUserId(uId);
        setShowConfirmDialog(true);
    };

    const renderGridSettings = () => (
        <CreateButton
          showModal={showCreateModal}
          showModalFunc={setShowCreateModal}
          styles={{ width: '40px', height: '40px', color: 'rgb(25,118,210)' }}
          tooltipLabel="Add user to role"
        />
    );

    const renderConfirmDialog = (index: number) => (
        <ConfirmDialog
          key={index}
          text="Are you sure you want to remove user from role."
          title="Remove from role"
          declineButtonText="Close"
          confirmButtonText="Remove"
          declineFunction={() => setShowConfirmDialog(false)}
          confirmFunction={() => {
              removeFromRole({ userId: userId!, roleId });
              setShowConfirmDialog(false);
          }}
        />
    );
    const renderUserEditModal = (i: number) => (
        <AdministrationModal
          key={i}
          index={i}
          open={showUserEditModal}
          onClose={() => {
              setShowUserEditModal(false);
          }}
        >
            <UserForm id={userId!} />
        </AdministrationModal>
    );

    const renderCreateModal = (i: number) => (
        <AdministrationModal
          key={i}
          index={i}
          open={showCreateModal}
          onClose={() => {
              setShowCreateModal(false);
          }}
        >
            <form className={formStyles.form}>
                <Typography variant="h4" className="centralize">
                    Add user to role
                </Typography>
                <FormControl className={formStyles.inputRow}>
                    <Autocomplete
                      options={usersAutocomplete}
                      renderInput={(params) => <TextField {...params} label="Select User" key={params.id} />}
                      onChange={(event, newValue) => onSelectUser(newValue!)}
                      onInputChange={(e: any) => onAutocompleteInputChange(e, setUsersSearchString)}
                      isOptionEqualToValue={(option, value) => option.id === value.id}
                      getOptionLabel={(option) => option?.userName}
                      renderOption={(properties, option) => (
                          <MenuItem {...properties} key={option.id} value={option.id}>
                              {option.userName}
                          </MenuItem>
                      )}
                    />
                </FormControl>
                <FormActionButton
                  disabled={userIdToAdd === null}
                  className={formStyles.buttonsWrapper}
                  buttonClassName={formStyles.button}
                  onClick={() => addUserToRole({ userId: userIdToAdd!, roleId })}
                  name="Add"
                />
            </form>
        </AdministrationModal>
    );

    if (isGetting || isAddingToRole || isRemovingFromRole) {
        return (
            <SpinningLoader />
        );
    }

    return (
        <>
            {renderErrorMessagesAlert(errorMessages)}
            {renderSuccessfullAlert(successMessage)}
            <div style={{ marginTop: '2rem' }}>
                <AdministrationGridView
                  filterableGridColumnDef={usersFilterableColumns}
                  notFilterableGridColumnDef={returnUsersNonFilterableColumns(onEditClick, onRemoveFromRowClicked)}
                  data={usersData}
                  error={getError}
                  queryParams={queryParams}
                  selectedFilters={selectedFilters}
                  selectedSorters={selectedSorters}
                  setQueryParams={setQueryParams}
                  setSorterStateAction={setSelectedSorters}
                  setFilterStateAction={setSelectedFilters}
                  withSearchParams={false}
                  renderActionButtons={renderGridSettings}
                  legendProps={[ { color: '#FFA1A1', message: 'User is deleted.' } ]}
                  modals={[
                      { showModal: showCreateModal, modal: (i) => renderCreateModal(i) },
                      { showModal: showUserEditModal, modal: (i) => renderUserEditModal(i) },
                      { showModal: showConfirmDialog, modal: (i) => renderConfirmDialog(i) },
                  ]}
                />
            </div>
        </>
    );
};
export default UsersInRoleView;