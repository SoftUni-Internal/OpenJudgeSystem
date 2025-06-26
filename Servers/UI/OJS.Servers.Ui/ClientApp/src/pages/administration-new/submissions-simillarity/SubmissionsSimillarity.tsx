import { useEffect, useState } from 'react';
import { Autocomplete, debounce, FormControl, InputLabel, MenuItem, Select, TextField, Typography } from '@mui/material';
import isNaN from 'lodash/isNaN';

import { TYPE } from '../../../common/labels';
import { IContestAutocomplete } from '../../../common/types';
import ExportExcel from '../../../components/administration/common/export-excel/ExportExcel';
import { useExportSimilaritiesToExcelMutation, useGetContestAutocompleteQuery } from '../../../redux/services/admin/contestsAdminService';
import { getAndSetExceptionMessage } from '../../../utils/messages-utils';
import { renderErrorMessagesAlert } from '../../../utils/render-utils';

import formStyles from '../../../components/administration/common/styles/FormStyles.module.scss';


export enum SimillarityType {
    Text = 3,
}

const SubmissionsSimillarity = () => {
    const [ contestAutocomplete, setContestsAutocomplete ] = useState<Array<IContestAutocomplete>>([]);
    const [ contestSearchString, setContestSearchString ] = useState<string>('');
    const [ contestId, setContestId ] = useState<number | null>(null);
    const [ similarityType, setSimillarityType ] = useState<SimillarityType>(SimillarityType.Text);
    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const {
        data: contestsAutocompleteData,
        error: getContestDataError,
    } = useGetContestAutocompleteQuery(contestSearchString);

    useEffect(() => {
        if (contestsAutocompleteData) {
            setContestsAutocomplete(contestsAutocompleteData);
        }
    }, [ contestsAutocompleteData ]);

    useEffect(() => {
        getAndSetExceptionMessage([ getContestDataError ], setErrorMessages);
    }, [ getContestDataError ]);

    const onInputChange = debounce((e: any) => {
        setContestSearchString(e.target.value);
    }, 300);

    const onSelect = (contest: IContestAutocomplete) => {
        setContestId(contest.id);
    };

    const onChange = (e: any) => {
        setSimillarityType(e.target.value as SimillarityType);
    };

    return (
        <>
            {renderErrorMessagesAlert(errorMessages)}
            <form className={`${formStyles.form}`}>
                <Typography variant="h4" className={formStyles.centralize}>
                    Submission similarity
                </Typography>
                <FormControl className={formStyles.inputRow}>
                    <Autocomplete
                      options={contestAutocomplete}
                      renderInput={(params) => <TextField {...params} label="Select Contest" key={params.id} />}
                      onChange={(event, newValue) => onSelect(newValue!)}
                      onInputChange={(event) => onInputChange(event)}
                      isOptionEqualToValue={(option, value) => option.id === value.id}
                      getOptionLabel={(option) => option?.name}
                      renderOption={(properties, option) =>
                          <MenuItem {...properties} key={option.id} value={option.id}>
                              {option.name}
                          </MenuItem>
                      }
                    />
                </FormControl>
                <FormControl className={formStyles.inputRow}>
                    <InputLabel id="similarity-type">{TYPE}</InputLabel>
                    <Select
                      sx={{ width: '100%' }}
                      variant="standard"
                      name="type"
                      value={similarityType}
                      labelId="similarity-type"
                      onChange={(e) => onChange(e)}
                      onBlur={(e) => onChange(e)}
                    >
                        {/* eslint-disable-next-line @typescript-eslint/no-unused-vars */}
                        {Object.entries(SimillarityType).filter(([ key, value ]) => !isNaN(Number(value))).map(([ key, value ]) =>
                            <MenuItem key={key} value={value}>
                                {key}
                            </MenuItem>)}
                    </Select>
                </FormControl>
                <ExportExcel
                  mutation={useExportSimilaritiesToExcelMutation}
                  contestIds={[ contestId! ]}
                  similarityCheckType={similarityType}
                  disabled={!contestId}
                />
            </form>
        </>
    );
};

export default SubmissionsSimillarity;
