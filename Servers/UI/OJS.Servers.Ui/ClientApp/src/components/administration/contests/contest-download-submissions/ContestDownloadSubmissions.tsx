import { ChangeEvent, useEffect, useState } from 'react';
import { FormControl, FormControlLabel, FormLabel, Radio, RadioGroup, Typography } from '@mui/material';

import { DOWNLOAD } from '../../../../common/labels';
import { useDownloadSubmissionsMutation } from '../../../../redux/services/admin/contestsAdminService';
import downloadFile from '../../../../utils/file-download-utils';
import { getAndSetExceptionMessage } from '../../../../utils/messages-utils';
import { renderErrorMessagesAlert } from '../../../../utils/render-utils';
import SpinningLoader from '../../../guidelines/spinning-loader/SpinningLoader';
import FormActionButton from '../../form-action-button/FormActionButton';
import ContestCompetePracticeButtons from '../contest-compete-practce-buttons/ContestCompetePracticeButtons';

import formStyles from '../../common/styles/FormStyles.module.scss';

enum DownloadContestSubmissionType {
    AllSubmissions = 1,
    BestSubmissions = 2,
    LastSubmissions = 3,
}

interface IDownloadSubmissionModel {
    contestExportResultType: number;
    submissionExportType: number;
    contestId: number;
}

interface IContestDownloadSubmissionsProps{
    contestid: number;
}

const ContestDownloadSubmissions = (props: IContestDownloadSubmissionsProps) => {
    const { contestid } = props;

    const [ errorMessage, setErrorMessages ] = useState<Array<string>>([]);

    const [ model, setModel ] = useState<IDownloadSubmissionModel>({
        contestExportResultType: 0,
        contestId: contestid,
        submissionExportType: DownloadContestSubmissionType.AllSubmissions,
    });

    const [
        downloadSubmissions,
        {
            data,
            isSuccess,
            error,
            isLoading,
            reset,
        },
    ] = useDownloadSubmissionsMutation();

    useEffect(() => {
        getAndSetExceptionMessage([ error ], setErrorMessages);
    }, [ error, reset ]);

    useEffect(() => {
        if (isSuccess) {
            if (data) {
                downloadFile(data.blob, data.filename);
            } else {
                setErrorMessages([ 'The required file is empty.' ]);
            }
            reset();
        }
    }, [ data, isSuccess, reset ]);

    const handleTypeChange = (value:number) => {
        const contestExportResultType = value;
        setModel((prevModel) => ({
            ...prevModel,
            contestExportResultType,
        }));
    };

    const handleSubmissionTypeChange = (event: ChangeEvent<HTMLInputElement>) => {
        const submissionExportType = Number(event.target.value);
        setModel((prevModel) => ({
            ...prevModel,
            submissionExportType,
        }));
    };

    if (isLoading) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderErrorMessagesAlert(errorMessage)}
            <Typography className={formStyles.centralize} variant="h4">Download contest submissions</Typography>
            <form className={formStyles.form}>
                <ContestCompetePracticeButtons value={model.contestExportResultType} setStateFunc={handleTypeChange} />
                <FormControl className={formStyles.inputRow}>
                    <FormLabel id="submission-types">Choose submission type</FormLabel>
                    <RadioGroup
                      aria-labelledby="submission-types"
                      value={model.submissionExportType}
                      onChange={handleSubmissionTypeChange}
                    >
                        <FormControlLabel
                          value={DownloadContestSubmissionType.AllSubmissions}
                          control={<Radio />}
                          label="All submissions"
                        />
                        <FormControlLabel
                          value={DownloadContestSubmissionType.BestSubmissions}
                          control={<Radio />}
                          label="Best submissions"
                        />
                        <FormControlLabel
                          value={DownloadContestSubmissionType.LastSubmissions}
                          control={<Radio />}
                          label="Latest submissions"
                        />
                    </RadioGroup>
                </FormControl>
                <FormActionButton
                  className={formStyles.buttonsWrapper}
                  buttonClassName={formStyles.button}
                  onClick={() => downloadSubmissions(model)}
                  name={DOWNLOAD}
                />
            </form>
        </>
    );
};
export default ContestDownloadSubmissions;
