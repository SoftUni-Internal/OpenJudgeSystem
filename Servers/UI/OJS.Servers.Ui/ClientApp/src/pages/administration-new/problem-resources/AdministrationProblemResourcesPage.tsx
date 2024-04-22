import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { IGetAllAdminParams } from '../../../common/types';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import ProblemResourceForm from '../../../components/administration/problem-resources/problem-resource-form/ProblemResourceForm';
import { getColors } from '../../../hooks/use-administration-theme-provider';
import { useGetAllAdminProblemResourcesQuery, useLazyExportProblemResourcesToExcelQuery } from '../../../redux/services/admin/problemResourcesAdminService';
import { useAppSelector } from '../../../redux/store';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultFilterToAdd, defaultSorterToAdd } from '../AdministrationGridView';

import problemResourceFilterableColumns, { returnProblemResourceNonFilterableColumns } from './problemResourcesGridColumns';

const AdministrationProblemResourcesPage = () => {
    const [ searchParams ] = useSearchParams();
    const themeMode = useAppSelector((x) => x.theme.administrationMode);
    const [ openEditModal, setOpenEditModal ] = useState<boolean>(false);
    const [ problemResourceId, setProblemResourceId ] = useState<number>(0);

    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(defaultFilterToAdd, defaultSorterToAdd, searchParams));

    const { refetch: retakeData, data, error } = useGetAllAdminProblemResourcesQuery(queryParams);

    const onEditClick = (id: number) => {
        setOpenEditModal(true);
        setProblemResourceId(id);
    };

    const renderProblemResourceModal = (index: number) => (
        <AdministrationModal
          key={index}
          index={index}
          open={openEditModal}
          onClose={() => setOpenEditModal(false)}
        >
            <ProblemResourceForm
              id={problemResourceId}
            />
        </AdministrationModal>
    );
    return (
        <AdministrationGridView
          filterableGridColumnDef={problemResourceFilterableColumns}
          notFilterableGridColumnDef={returnProblemResourceNonFilterableColumns(onEditClick, retakeData)}
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
    );
};
export default AdministrationProblemResourcesPage;
