import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { renderSuccessfullAlert } from 'src/utils/render-utils';

import { IGetAllAdminParams } from '../../../common/types';
import { NEW_ADMINISTRATION_PATH, SUBMISSION_TYPE_DOCUMENTS_PATH } from '../../../common/urls/administration-urls';
import CreateButton from '../../../components/administration/common/create/CreateButton';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import {
    useGetAllSubmissionTypesInSubmissionDocumentsQuery,
} from '../../../redux/services/admin/submissionTypesInSubmissionDocumentsAdminService';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView from '../AdministrationGridView';

import submissionTypeDocumentsFilterableColumns, {
    returnNonFilterableColumns,
} from './submissionTypeDocumentsGridColumns';

const AdministrationSubmissionTypeDocumentsPage = () => {
    const [ searchParams ] = useSearchParams();
    const navigate = useNavigate();

    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString('', '', searchParams));
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);

    const {
        refetch: refetchSubmissionTypesInSubmissionDocuments,
        data: submissionTypesInSubmissionDocumentsData,
        isLoading: isDataLoading,
        error,
    } = useGetAllSubmissionTypesInSubmissionDocumentsQuery(queryParams);

    if (isDataLoading) {
        return <SpinningLoader />;
    }

    const onDelete = () => {
        refetchSubmissionTypesInSubmissionDocuments();
    };

    const renderGridSettings = () => 
        // The 'showModal' parameter is not needed, since we want to use the button for navigation
        <CreateButton
          showModal={false}
          showModalFunc={() => navigate(`/${NEW_ADMINISTRATION_PATH}/${SUBMISSION_TYPE_DOCUMENTS_PATH}/0?isEditMode=false`)}
          styles={{ width: '40px', height: '40px' }}
        />
    ;

    return (
        <>
            {renderSuccessfullAlert(successMessage, 7000)}
            <AdministrationGridView
              filterableGridColumnDef={submissionTypeDocumentsFilterableColumns}
              notFilterableGridColumnDef={returnNonFilterableColumns(onDelete, setSuccessMessage)}
              data={submissionTypesInSubmissionDocumentsData}
              error={error}
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              renderActionButtons={renderGridSettings}
              defaultSorter=""
              defaultFilter=""
              specificRowIdName={[ 'submissionTypeDocumentId', 'submissionTypeId' ]}
            />
        </>
    );
};

export default AdministrationSubmissionTypeDocumentsPage;
