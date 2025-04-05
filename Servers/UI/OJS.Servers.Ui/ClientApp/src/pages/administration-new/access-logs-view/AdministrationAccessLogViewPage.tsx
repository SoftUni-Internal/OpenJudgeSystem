import React from 'react';
import { Box, TextField, Typography } from '@mui/material';
import { DateTimePicker } from '@mui/x-date-pickers';
import {
    CREATED_ON,
    IP_ADDRESS,
    POST_PARAMS,
    REQUEST_TYPE,
    USER_ID,
    USERNAME,
} from 'src/common/labels';
import SpinningLoader from 'src/components/guidelines/spinning-loader/SpinningLoader';
import { useGetAccessLogByIdQuery } from 'src/redux/services/admin/accessLogsAdminService';
import { getDateAsLocal } from 'src/utils/administration/administration-dates';

import formStyles from '../../../components/administration/common/styles/FormStyles.module.scss';

interface IAdministrationAccessLogViewPageProps {
    accessLogId: number | undefined;
}

const AdministrationAccessLogViewPage = ({ accessLogId }: IAdministrationAccessLogViewPageProps) => {
    const {
        data: accessLogData,
        isLoading: isDataLoading,
        isFetching: isDataFetching,
    } = useGetAccessLogByIdQuery(Number(accessLogId), { skip: accessLogId === undefined });

    if (isDataLoading || isDataFetching) {
        return <SpinningLoader />;
    }

    if (!accessLogData) {
        return (
            <Typography variant="h6" className={formStyles.centralize} color="error">
                No Access Log Data Found
            </Typography>
        );
    }

    return (
        <Box className={formStyles.form}>
            <Typography variant="h4" className={formStyles.centralize}>
                Access Log Details
            </Typography>
            <Box className={formStyles.fieldBox}>
                <Box className={formStyles.fieldBoxElement}>
                    <TextField
                      label={USER_ID}
                      value={accessLogData.userId}
                      disabled
                      fullWidth
                      variant="outlined"
                      className={formStyles.inputRow}
                    />
                </Box>
                <Box className={formStyles.fieldBoxElement}>
                    <TextField
                      label={USERNAME}
                      value={accessLogData.userUserName}
                      disabled
                      fullWidth
                      variant="outlined"
                      className={formStyles.inputRow}
                    />
                </Box>
                <Box className={formStyles.fieldBoxElement}>
                    <DateTimePicker
                      name="createdOn"
                      label={CREATED_ON}
                      value={getDateAsLocal(accessLogData.createdOn)}
                      className={formStyles.inputRow}
                      disabled
                    />
                </Box>
                <Box className={formStyles.fieldBoxElement}>
                    <TextField
                      label={IP_ADDRESS}
                      value={accessLogData.ipAddress}
                      disabled
                      fullWidth
                      variant="outlined"
                      className={formStyles.inputRow}
                    />
                </Box>
                <Box className={formStyles.fieldBoxElement}>
                    <TextField
                      label={REQUEST_TYPE}
                      value={accessLogData.requestType}
                      disabled
                      fullWidth
                      variant="outlined"
                      className={formStyles.inputRow}
                    />
                </Box>
                <Box className={formStyles.fieldBoxElement}>
                    <TextField
                      label="URL"
                      value={accessLogData.url}
                      disabled
                      fullWidth
                      variant="outlined"
                      className={formStyles.inputRow}
                    />
                </Box>
                <Box className={formStyles.fieldBoxElement}>
                    <TextField
                      label={POST_PARAMS}
                      value={accessLogData.postParams}
                      disabled
                      multiline
                      fullWidth
                      variant="outlined"
                      className={formStyles.inputRow}
                    />
                </Box>
            </Box>
        </Box>
    );
};

export default AdministrationAccessLogViewPage;
