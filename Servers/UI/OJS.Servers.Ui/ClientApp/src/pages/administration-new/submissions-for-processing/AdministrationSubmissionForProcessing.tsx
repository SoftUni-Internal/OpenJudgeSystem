 
import React, { useEffect, useState } from 'react';
import ReactJson from 'react-json-view';
import { useLocation } from 'react-router-dom';
import {
    Box, FormControl, FormLabel,
    TextField,
} from '@mui/material';
import isNil from 'lodash/isNil';

import { ISubmissionForProcessingAdminGridViewType } from '../../../common/types';
import { useGetByIdQuery } from '../../../redux/services/admin/submissionsForProcessingAdminService';
import { preciseFormatDate } from '../../../utils/dates';

import styles from './AdminSubmissionForProcessingDetails.module.scss';

const AdministrationSubmissionForProcessing = () => {
    const { pathname } = useLocation();
    const [ , , , contestId ] = pathname.split('/');
    const [ submission, setSubmission ] = useState<ISubmissionForProcessingAdminGridViewType | null>(null);

    const { data } = useGetByIdQuery(
        { id: Number(contestId) },
        { skip: isNil(contestId) },
    );

    useEffect(
        () => {
            if (data) {
                setSubmission(data);
            }
        },
        [ data ],
    );

    return (
        !isNil(submission)
            ? <Box className={`${styles.fieldBox}`}>
                <TextField
                      className={styles.inputRow}
                      label="Submission Id"
                      variant="standard"
                      value={submission.submissionId}
                      disabled
                    />
                <TextField
                      className={styles.inputRow}
                      label="Is Processed"
                      variant="standard"
                      value={submission.processed}
                      disabled
                    />
                <TextField
                      className={styles.inputRow}
                      label="Is Processing"
                      variant="standard"
                      value={submission.processing}
                      disabled
                    />
                <FormControl className={styles.inputRow}>
                    <FormLabel>Serialized Exception</FormLabel>
                    {isNil(submission.serializedException)
                        ? 'No Value'
                        : <ReactJson
                                  src={JSON.parse(submission.serializedException)}
                                  theme="summerfruit:inverted"
                                  collapsed={false}
                                  quotesOnKeys
                                />
                            }
                </FormControl>
                <FormControl className={styles.inputRow}>
                    <FormLabel>Serialized Execution Details</FormLabel>
                    <ReactJson
                          src={JSON.parse(submission.serializedExecutionDetails)}
                          theme="summerfruit:inverted"
                          collapsed={false}
                          quotesOnKeys
                        />
                </FormControl>
                <FormControl className={styles.inputRow}>
                    <FormLabel>Serialized Execution Result</FormLabel>
                    {isNil(submission.serializedExecutionResult)
                        ? 'No Value'
                        : <ReactJson
                                  src={JSON.parse(submission.serializedExecutionResult)}
                                  theme="summerfruit:inverted"
                                  collapsed={false}
                                  quotesOnKeys
                                />
                            }
                </FormControl>
                <TextField
                      className={styles.inputRow}
                      label="Created On"
                      variant="standard"
                      value={preciseFormatDate(submission.createdOn)}
                      disabled
                    />
                <TextField
                      className={styles.inputRow}
                      label="Modified On"
                      variant="standard"
                      value={preciseFormatDate(submission.modifiedOn)}
                      disabled
                    />
            </Box>
            
            : null
    );
};
export default AdministrationSubmissionForProcessing;
