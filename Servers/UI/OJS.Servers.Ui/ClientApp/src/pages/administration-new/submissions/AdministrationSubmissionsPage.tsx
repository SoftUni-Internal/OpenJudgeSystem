import { useCallback, useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { IGetAllAdminParams } from '../../../common/types';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { useDownloadFileSubmissionQuery,
    useGetAllSubmissionsQuery,
    useLazyExportSubmissionsToExcelQuery,
    useRetestMutation } from '../../../redux/services/admin/submissionsAdminService';
import downloadFile from '../../../utils/file-download-utils';
import { getAndSetExceptionMessage, getAndSetSuccesfullMessages } from '../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../utils/render-utils';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultFilterToAdd, defaultSorterToAdd } from '../AdministrationGridView';

import dataColumns, { returnSubmissionsNonFilterableColumns } from './admin-submissions-grid-def';

const AdministrationSubmissionsPage = () => {
    const [ searchParams ] = useSearchParams();
    const [ exceptionMessages, setExceptionMessages ] = useState<Array<string>>([]);
    const [ successfullMessage, setSuccessfullMessage ] = useState<string | null>(null);

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

    useEffect(() => {
        const message = getAndSetSuccesfullMessages([
            { message: retestData, shouldGet: isSuccessFullyRetested },
        ]);
        setSuccessfullMessage(message);
    }, [ isSuccessFullyRetested, retestData ]);

    useEffect(() => {
        getAndSetExceptionMessage([ retestError, downloadError ], setExceptionMessages);
    }, [ downloadError, retestError ]);

    const startDownload = useCallback((id: number) => {
        setSubmissionToDownload(id);
        setShouldSkipDownloadOfSubmission(false);
    }, []);

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
            {renderSuccessfullAlert(successfullMessage)}
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
              legendProps={[ { color: '#FFA1A1', message: 'Submission is deleted.' } ]}
              excelMutation={useLazyExportSubmissionsToExcelQuery}

            />
        </>
    );
};

export default AdministrationSubmissionsPage;
