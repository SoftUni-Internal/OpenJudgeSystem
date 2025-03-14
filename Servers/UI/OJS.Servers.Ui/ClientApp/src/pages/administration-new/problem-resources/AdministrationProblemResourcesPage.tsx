import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { IGetAllAdminParams } from '../../../common/types';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import ProblemResourceForm from '../../../components/administration/problem-resources/problem-resource-form/ProblemResourceForm';
import { getColors, useAdministrationTheme } from '../../../hooks/use-administration-theme-provider';
import { useGetAllAdminProblemResourcesQuery, useLazyExportProblemResourcesToExcelQuery } from '../../../redux/services/admin/problemResourcesAdminService';
import { renderSuccessfullAlert } from '../../../utils/render-utils';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultFilterToAdd, defaultSorterToAdd } from '../AdministrationGridView';

import problemResourceFilterableColumns, { returnProblemResourceNonFilterableColumns } from './problemResourcesGridColumns';

const AdministrationProblemResourcesPage = () => {
    const [ searchParams ] = useSearchParams();
    const { themeMode } = useAdministrationTheme();
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ openEditModal, setOpenEditModal ] = useState<boolean>(false);
    const [ problemResourceId, setProblemResourceId ] = useState<number>(0);

    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(defaultFilterToAdd, defaultSorterToAdd, searchParams));

    const { refetch: retakeData, data, error } = useGetAllAdminProblemResourcesQuery(queryParams);

    const onClose = () => {
        retakeData();
        setOpenEditModal(false);
    };

    const onEditClick = (id: number) => {
        setOpenEditModal(true);
        setProblemResourceId(id);
    };

    const renderProblemResourceModal = (index: number) => (
        <AdministrationModal
          key={index}
          index={index}
          open={openEditModal}
          onClose={onClose}
        >
            <ProblemResourceForm
              id={problemResourceId}
              onSuccess={onClose}
              setParentSuccessMessage={setSuccessMessage}
            />
        </AdministrationModal>
    );
    return (
        <>
            {renderSuccessfullAlert(successMessage, 7000)}
            <AdministrationGridView
              filterableGridColumnDef={problemResourceFilterableColumns}
              notFilterableGridColumnDef={returnProblemResourceNonFilterableColumns(onEditClick, retakeData, setSuccessMessage)}
              data={data}
              error={error}
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              legendProps={[ { color: getColors(themeMode).palette.deleted, message: 'Resource is deleted.' } ]}
              modals={[
                  { showModal: openEditModal, modal: (i) => renderProblemResourceModal(i) },
              ]}
              excelMutation={useLazyExportProblemResourcesToExcelQuery}
            />
        </>
    );
};
export default AdministrationProblemResourcesPage;
