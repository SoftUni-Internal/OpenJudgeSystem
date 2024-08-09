import { useCallback, useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { IGetAllAdminParams } from '../../../common/types';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import useSuccessMessageEffect from '../../../hooks/common/use-success-message-effect';
import { getColors } from '../../../hooks/use-administration-theme-provider';
import { useDownloadFileSubmissionQuery,
    useGetAllSubmissionsQuery,
    useLazyExportSubmissionsToExcelQuery,
    useRetestMutation } from '../../../redux/services/admin/submissionsAdminService';
import { useAppSelector } from '../../../redux/store';
import downloadFile from '../../../utils/file-download-utils';
import { getAndSetExceptionMessage } from '../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../utils/render-utils';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultFilterToAdd, defaultSorterToAdd } from '../AdministrationGridView';

import dataColumns, { returnSubmissionsNonFilterableColumns } from './admin-submissions-grid-def';

const AdministrationSubmissionsPage = () => {
    const [ searchParams ] = useSearchParams();
    const [ exceptionMessages, setExceptionMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const themeMode = useAppSelector((x) => x.theme.administrationMode);
    const [ submissionToDownload, setSubmissionToDownload ] = useState<number | null>(null);
    const [ shouldSkipDownloadOfSubmission, setShouldSkipDownloadOfSubmission ] = useState<boolean>(true);

    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(defaultFilterToAdd, defaultSorterToAdd, searchParams));

    const {
        refetch: retakeSubmissions,
        data,
        error,
        isLoading,
    } = useGetAllSubmissionsQuery(queryParams);

    const [ retest,
        {
            data: retestData,
            error: retestError,
            isSuccess: isSuccessFullyRetested,
        },
    ] = useRetestMutation();

    const { data: fileSubmission, error: downloadError } = useDownloadFileSubmissionQuery(
        { id: submissionToDownload! },
        { skip: shouldSkipDownloadOfSubmission },
    );

    const startDownload = useCallback((id: number) => {
        setSubmissionToDownload(id);
        setShouldSkipDownloadOfSubmission(false);
    }, []);

    useSuccessMessageEffect({
        data: [
            { message: retestData, shouldGet: isSuccessFullyRetested },
        ],
        setSuccessMessage,
    });

    useEffect(() => {
        getAndSetExceptionMessage([ retestError, downloadError ], setExceptionMessages);
    }, [ downloadError, retestError ]);

    useEffect(() => {
        if (fileSubmission?.blob) {
            downloadFile(fileSubmission.blob, fileSubmission.filename);
        }
    }, [ fileSubmission ]);

    if (isLoading) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            {renderErrorMessagesAlert(exceptionMessages)}
            <AdministrationGridView
              data={data}
              error={error}
              filterableGridColumnDef={dataColumns}
              notFilterableGridColumnDef={
                returnSubmissionsNonFilterableColumns(retest, startDownload, retakeSubmissions)
            }
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              legendProps={[ { color: getColors(themeMode).palette.deleted, message: 'Submission is deleted.' } ]}
              excelMutation={useLazyExportSubmissionsToExcelQuery}
            />
        </>
    );
};

export default AdministrationSubmissionsPage;
