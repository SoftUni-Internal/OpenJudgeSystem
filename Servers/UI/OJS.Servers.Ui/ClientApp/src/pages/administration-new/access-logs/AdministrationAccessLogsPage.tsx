import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import AdministrationModal from 'src/components/administration/common/modals/administration-modal/AdministrationModal';
import accessLogsFilterableColumns, { returnNonFilterableColumns } from 'src/pages/administration-new/access-logs/accessLogsGridColumns';
import AdministrationAccessLogViewPage
    from 'src/pages/administration-new/access-logs-view/AdministrationAccessLogViewPage';
import { useGetAllAccessLogsQuery } from 'src/redux/services/admin/accessLogsAdminService';

import { IGetAllAdminParams } from '../../../common/types';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView from '../AdministrationGridView';

const defaultAccessLogsSorterToAdd = 'createdon=DESC';

const AdministrationSubmissionTypeDocumentsPage = () => {
    const [ searchParams ] = useSearchParams();

    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString('', '', searchParams));
    const [ showViewModal, setShowViewModal ] = useState<boolean>(false);
    const [ accessLogId, setAccessLogId ] = useState<number>();

    const {
        data: accessLogsData,
        isLoading: isDataLoading,
        error,
    } = useGetAllAccessLogsQuery(queryParams);

    const renderViewModal = (index: number) => 
        <AdministrationModal
          key={index}
          index={index}
          open={showViewModal}
          onClose={() => setShowViewModal(false)}
        >
            <AdministrationAccessLogViewPage accessLogId={accessLogId} />
        </AdministrationModal>
    ;

    const onViewClick = (id: number) => {
        setAccessLogId(id);
        setShowViewModal(true);
    };

    if (isDataLoading) {
        return <SpinningLoader />;
    }

    return (
        <AdministrationGridView
          filterableGridColumnDef={accessLogsFilterableColumns}
          notFilterableGridColumnDef={returnNonFilterableColumns(onViewClick)}
          data={accessLogsData}
          error={error}
          queryParams={queryParams}
          setQueryParams={setQueryParams}
          defaultSorter={defaultAccessLogsSorterToAdd}
          defaultFilter=""
          modals={[
              { showModal: showViewModal, modal: (i) => renderViewModal(i) },
          ]}
        />
    );
};

export default AdministrationSubmissionTypeDocumentsPage;
