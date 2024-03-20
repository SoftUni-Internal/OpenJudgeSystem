import { useEffect, useState } from 'react';
import { Autocomplete, Checkbox, debounce, FormControl, FormControlLabel, MenuItem, TextField, Typography } from '@mui/material';

import { CREATE, IS_OFFICIAL } from '../../../../common/labels';
import { IContestAutocomplete, IParticipantAdministrationModel, IUserAutocompleteData } from '../../../../common/types';
import { useGetContestAutocompleteQuery } from '../../../../redux/services/admin/contestsAdminService';
import { useCreateParticipantMutation } from '../../../../redux/services/admin/participantsAdminService';
import { useGetUsersAutocompleteQuery } from '../../../../redux/services/admin/usersAdminService';
import { getAndSetExceptionMessage, getAndSetSuccesfullMessages } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../../utils/render-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import FormActionButton from '../../form-action-button/FormActionButton';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../../common/styles/FormStyles.module.scss';

interface IParticipantFormProps {
    contestName?: string;
    contestId?: number;
}

const ParticipantForm = (props: IParticipantFormProps) => {
    const { contestName = '', contestId = 0 } = props;
    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ successMessages, setSuccessMessages ] = useState<string>('');
    const [ participant, setParticipant ] = useState<IParticipantAdministrationModel>({
        contestId,
        id: 0,
        contestName,
        isOfficial: false,
        userId: '',
        userName: '',
    });
    const [ contestAutocomplete, setContestsAutocomplete ] = useState<Array<IContestAutocomplete>>([
        {
            id: 0,
            name: '',
        },
    ]);
    const [ usersAutocomplete, setUsersAutocomplete ] = useState<Array<IUserAutocompleteData>>([
        {
            id: '',
            userName: '',
        },
    ]);

    const [ contestSearchString, setContestSearchString ] = useState<string>('');
    const [ usersSearchString, setUsersSearchString ] = useState<string>('');

    const {
        data: contestsAutocompleteData,
        error: getContestDataError,
    } = useGetContestAutocompleteQuery(contestSearchString, { skip: contestId > 0 });

    const {
        data: usersAutocompleteData,
        error: getUsersDataError,
    } = useGetUsersAutocompleteQuery(usersSearchString);

    const [
        createParticipant,
        {
            data: createData,
            isLoading: isCreating,
            error: createError,
            isSuccess: isSuccessfullyCreated,
        } ] = useCreateParticipantMutation();

    useEffect(() => {
        const successMessage = getAndSetSuccesfullMessages([
            {
                message: createData,
                shouldGet: isSuccessfullyCreated,
            },
        ]);

        if (successMessage) {
            setSuccessMessages(successMessage);
        }
    }, [ createData, isSuccessfullyCreated ]);

    useEffect(() => {
        if (contestsAutocompleteData) {
            setContestsAutocomplete(contestsAutocompleteData);
        }
        if (usersAutocompleteData) {
            setUsersAutocomplete(usersAutocompleteData);
        }
    }, [ contestsAutocompleteData, usersAutocompleteData ]);

    useEffect(() => {
        getAndSetExceptionMessage([ createError, getContestDataError, getUsersDataError ], setErrorMessages);
        setSuccessMessages('');
    }, [ createError, getContestDataError, getUsersDataError ]);

    const onInputChange = debounce((e: any) => {
        setContestSearchString(e.target.value);
    }, 300);

    const onUserInputChange = debounce((e: any) => {
        setUsersSearchString(e.target.value);
    }, 300);

    const onChange = (e: any) => {
        const { target } = e;
        const { name, type, value, checked } = target;
        setParticipant((prevState) => ({
            ...prevState,
            [name]: type === 'checkbox'
                ? checked
                : type === 'number'
                    ? value === ''
                        ? ''
                        : Number(value)
                    : value,
        }));
    };

    const onSelect = (contest: IContestAutocomplete) => {
        let currContestId = 0;
        let currContestName = '';
        if (contest) {
            currContestId = contest.id;
            currContestName = contest.name;
        }
        setParticipant((prevState) => ({
            ...prevState,
            contestId: currContestId,
            contestName: currContestName,
        }));
    };

    const onSelectUser = (user: IUserAutocompleteData) => {
        let userId = '';
        let userName = '';
        if (user) {
            userId = user.id;
            userName = user.userName;
        }
        setParticipant((prevState) => ({
            ...prevState,
            userId,
            userName,
        }));
    };

    if (isCreating) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderErrorMessagesAlert(errorMessages)}
            {renderSuccessfullAlert(successMessages)}
            <form className={formStyles.form}>
                <Typography variant="h4" className="centralize">
                    Participant Administration Form
                </Typography>
                <FormControl className={formStyles.inputRow}>
                    <Autocomplete
                      options={contestAutocomplete}
                      renderInput={(params) => <TextField {...params} label="Select Contest" key={params.id} />}
                      onChange={(event, newValue) => onSelect(newValue!)}
                      onInputChange={(event) => onInputChange(event)}
                      isOptionEqualToValue={(option, value) => option.id === value.id}
                      getOptionLabel={(option) => option?.name}
                      disabled={contestId > 0}
                      value={{ id: participant.contestId, name: participant.contestName } as IContestAutocomplete}
                      renderOption={(properties, option) => (
                          <MenuItem {...properties} key={option.id} value={option.id}>
                              {option.name}
                          </MenuItem>
                      )}
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <Autocomplete
                      options={usersAutocomplete}
                      renderInput={(params) => <TextField {...params} label="Select User" key={params.id} />}
                      onChange={(event, newValue) => onSelectUser(newValue!)}
                      onInputChange={(event) => onUserInputChange(event)}
                      isOptionEqualToValue={(option, value) => option.id === value.id}
                      getOptionLabel={(option) => option?.userName}
                      renderOption={(properties, option) => (
                          <MenuItem {...properties} key={option.id} value={option.id}>
                              {option.userName}
                          </MenuItem>
                      )}
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <FormControlLabel
                      control={<Checkbox checked={participant.isOfficial} />}
                      label={IS_OFFICIAL}
                      name="isOfficial"
                      onChange={(e) => onChange(e)}
                    />
                </FormControl>
                <FormActionButton
                  className={formStyles.buttonsWrapper}
                  buttonClassName={formStyles.button}
                  onClick={() => createParticipant(participant)}
                  name={CREATE}
                />
            </form>
        </>
    );
};
export default ParticipantForm;