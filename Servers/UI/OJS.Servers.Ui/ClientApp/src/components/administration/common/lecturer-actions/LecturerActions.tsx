
import React, { useEffect, useState } from 'react';
import { Autocomplete, FormControl, MenuItem, TextField, Typography } from '@mui/material';

import { RolesEnum } from '../../../../common/enums';
import { SELECT_CATEGORY, SELECT_CONTEST } from '../../../../common/labels';
import {
    IContestAutocomplete,
    IContestCategories,
    IUserAutocompleteData,
} from '../../../../common/types';
import useDelayedSuccessEffect from '../../../../hooks/common/use-delayed-success-effect';
import useSuccessMessageEffect from '../../../../hooks/common/use-success-message-effect';
import {
    useGetCategoriesQuery,
    useGetForLecturerInCategoryQuery,
} from '../../../../redux/services/admin/contestCategoriesAdminService';
import {
    useGetContestAutocompleteQuery,
    useGetForLecturerInContestQuery,
} from '../../../../redux/services/admin/contestsAdminService';
import { useGetIdByNameQuery } from '../../../../redux/services/admin/rolesAdminService';
import {
    useAddLecturerToCategoryMutation, useAddLecturerToContestMutation,
    useGetUsersAutocompleteQuery,
    useRemoveLecturerFromCategoryMutation, useRemoveLecturerFromContestMutation,
} from '../../../../redux/services/admin/usersAdminService';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert } from '../../../../utils/render-utils';
import clearSuccessMessages from '../../../../utils/success-messages-utils';
import FormActionButton from '../../form-action-button/FormActionButton';
import { autocompleteNameIdFormatFilterOptions, onAutocompleteInputChange } from '../../utils/mui-utils';
import AdministrationModal from '../modals/administration-modal/AdministrationModal';

import formStyles from '../styles/FormStyles.module.scss';

interface ILecturerInCategoryActionsProps {
    index: number;
    roleId?: string | null;
    showModal: boolean;
    setShowModal: Function;
    isRemove: boolean;
    isContest: boolean;
    setParentSuccessMessage: Function;
    onSuccess: Function;
}

const LecturerActions = ({
    index,
    roleId,
    showModal,
    setShowModal,
    isRemove,
    isContest,
    setParentSuccessMessage,
    onSuccess,
}: ILecturerInCategoryActionsProps) => {
    const [ categoryToAdd, setCategoryToAdd ] = useState<number>(0);
    const [ contestToAdd, setContestToAdd ] = useState<number>(0);

    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);

    const [ contestSearchString, setContestSearchString ] = useState<string>('');
    const [ usersSearchString, setUsersSearchString ] = useState<string>('');

    const [ selectedUser, setSelectedUser ] = useState<IUserAutocompleteData | null>(null);
    const [ usersAutocomplete, setUsersAutocomplete ] = useState<Array<IUserAutocompleteData>>([]);

    const { data: lecturerRoleId } = useGetIdByNameQuery(RolesEnum.Lecturer.toString(), { skip: roleId !== undefined && roleId !== null });

    const { data: contests } = useGetContestAutocompleteQuery(contestSearchString, { skip: isRemove || !isContest });
    const { data: lecturerContests } = useGetForLecturerInContestQuery(
        selectedUser?.id ?? '',
        { skip: !isRemove || !isContest || !selectedUser },
    );

    const { data: contestCategories } = useGetCategoriesQuery(null, { skip: isRemove || isContest });
    const { data: lecturerContestCategories } = useGetForLecturerInCategoryQuery(
        selectedUser?.id ?? '',
        { skip: !isRemove || isContest || !selectedUser },
    );

    const { data: usersAutocompleteData } = useGetUsersAutocompleteQuery([ usersSearchString, roleId ?? lecturerRoleId! ]);

    const [ addLecturerToCategory,
        {
            data: addToCategoryData,
            error: addToCategoryError,
            isSuccess: isSuccessfullyAddedToCategory,
            isLoading: isAddingToCategory,
        },
    ] = useAddLecturerToCategoryMutation();

    const [ removeLecturerFromCategory,
        {
            data: removeFromCategoryData,
            error: removeFromCategoryError,
            isSuccess: isSuccessfullyRemovedFromCategory,
            isLoading: isRemovingFromCategory,
        },
    ] = useRemoveLecturerFromCategoryMutation();

    const [
        addLecturerToContest,
        {
            data: addToContestData,
            error: addToContestError,
            isSuccess: isSuccessfullyAddedToContest,
            isLoading: isAddingToContest,
        },
    ] = useAddLecturerToContestMutation();

    const [
        removeLecturerFromContest,
        {
            data: removeFromContestData,
            error: removeFromContestError,
            isSuccess: isSuccessfullyRemovedFromContest,
            isLoading: isRemovingFromContest,
        },
    ] = useRemoveLecturerFromContestMutation();

    useSuccessMessageEffect({
        data: [
            { message: addToContestData, shouldGet: isSuccessfullyAddedToContest },
            { message: removeFromContestData, shouldGet: isSuccessfullyRemovedFromContest },
            { message: addToCategoryData, shouldGet: isSuccessfullyAddedToCategory },
            { message: removeFromCategoryData, shouldGet: isSuccessfullyRemovedFromCategory },
        ],
        setParentSuccessMessage,
        clearFlags: [ isAddingToContest, isRemovingFromContest, isAddingToCategory, isRemovingFromCategory ],
    });

    useDelayedSuccessEffect({
        isSuccess: isSuccessfullyAddedToContest ||
            isSuccessfullyRemovedFromContest ||
            isSuccessfullyAddedToCategory ||
            isSuccessfullyRemovedFromCategory,
        onSuccess,
    });

    useEffect(() => {
        getAndSetExceptionMessage([
            addToContestError,
            removeFromContestError,
            addToCategoryError,
            removeFromCategoryError ], setErrorMessages);
        clearSuccessMessages({ setParentSuccessMessage });
    }, [ addToCategoryError, addToContestError, removeFromCategoryError, removeFromContestError, setParentSuccessMessage ]);

    useEffect(() => {
        if (usersAutocompleteData) {
            setUsersAutocomplete(usersAutocompleteData);
        }
    }, [ usersAutocompleteData ]);

    const onUserSelect = (user: IUserAutocompleteData | null) => {
        setSelectedUser(user);
    };

    const onCategorySelect = (category?: IContestCategories) => {
        setCategoryToAdd(category?.id ?? 0);
    };

    const onContestSelect = (contest?: IContestAutocomplete) => {
        setContestToAdd(contest?.id ?? 0);
    };

    const handleLecturerInCategory = () => {
        if (selectedUser && categoryToAdd) {
            if (isRemove) {
                removeLecturerFromCategory({ lecturerId: selectedUser.id, categoryId: categoryToAdd });
            } else {
                addLecturerToCategory({ lecturerId: selectedUser.id, categoryId: categoryToAdd });
            }
        }
    };

    const handleLecturerInContest = () => {
        if (selectedUser && contestToAdd) {
            if (isRemove) {
                removeLecturerFromContest({ lecturerId: selectedUser.id, contestId: contestToAdd });
            } else {
                addLecturerToContest({ lecturerId: selectedUser.id, contestId: contestToAdd });
            }
        }
    };

    return (
        <>
            {renderErrorMessagesAlert(errorMessages)}
            <AdministrationModal
              index={index}
              open={showModal}
              onClose={() => {
                  setShowModal(false);
              }}
            >
                <form className={formStyles.form}>
                    <Typography variant="h4" className="centralize">
                        {isRemove
                            ? 'Remove'
                            : 'Add'}
                        {' '}
                        Lecturer
                        {' '}
                        {isRemove
                            ? 'from'
                            : 'to'}
                        {' '}
                        {isContest
                            ? 'Contest'
                            : 'Category'}
                    </Typography>
                    <FormControl fullWidth margin="normal">
                        <Autocomplete
                          sx={{ width: '90%' }}
                          className={formStyles.centralize}
                          options={usersAutocomplete}
                          renderInput={(params) => <TextField {...params} label="Select User" />}
                          onChange={(event, newValue) => onUserSelect(newValue)}
                          onInputChange={(e: any) => onAutocompleteInputChange(e, setUsersSearchString)}
                          getOptionLabel={(option) => option?.userName || ''}
                          isOptionEqualToValue={(option, value) => option.id === value.id}
                          renderOption={(properties, option) =>
                              <MenuItem {...properties} key={option.id}>
                                  {option.userName}
                              </MenuItem>
                          }
                        />
                        {isContest
                            ? <FormControl fullWidth margin="normal" className={formStyles.inputRow}>
                                <Autocomplete<IContestAutocomplete>
                                      sx={{ width: '90%' }}
                                      disabled={!selectedUser && !contestToAdd}
                                      className={formStyles.centralize}
                                      onChange={(event, newValue) => onContestSelect(newValue!)}
                                      onInputChange={(e: any) => onAutocompleteInputChange(e, setContestSearchString)}
                                      options={contests || lecturerContests || []}
                                      filterOptions={autocompleteNameIdFormatFilterOptions}
                                      renderInput={(params) => <TextField {...params} label={SELECT_CONTEST} key={params.id} />}
                                      getOptionLabel={(option) => option?.name}
                                      renderOption={(properties, option) =>
                                          <MenuItem {...properties} key={option.id} value={option.id}>
                                              #
                                              {option.id}
                                              {' '}
                                              {option.name}
                                          </MenuItem>
                                      }
                                    />
                            </FormControl>

                            : <FormControl fullWidth margin="normal" className={formStyles.inputRow}>
                                <Autocomplete<IContestAutocomplete>
                                      sx={{ width: '90%' }}
                                      disabled={!selectedUser && !categoryToAdd}
                                      className={formStyles.centralize}
                                      onChange={(event, newValue) => onCategorySelect(newValue!)}
                                      options={contestCategories || lecturerContestCategories || []}
                                      filterOptions={autocompleteNameIdFormatFilterOptions}
                                      renderInput={(params) => <TextField {...params} label={SELECT_CATEGORY} />}
                                      getOptionLabel={(option) => option?.name || ''}
                                      renderOption={(properties, option) =>
                                          <MenuItem {...properties} key={option.id}>
                                              #
                                              {option.id}
                                              {' '}
                                              {option.name}
                                          </MenuItem>
                                      }
                                    />
                            </FormControl>
                            }
                    </FormControl>
                    <FormControl fullWidth margin="normal">
                        <FormActionButton
                          disabled={!selectedUser || isContest && contestToAdd === 0 || !isContest && categoryToAdd === 0}
                          className={formStyles.buttonsWrapper}
                          buttonClassName={formStyles.button}
                          onClick={isContest
                              ? handleLecturerInContest
                              : handleLecturerInCategory}
                          name={isRemove
                              ? 'Remove'
                              : 'Add'}
                        />
                    </FormControl>
                </form>
            </AdministrationModal>
        </>
    );
};

export default LecturerActions;
