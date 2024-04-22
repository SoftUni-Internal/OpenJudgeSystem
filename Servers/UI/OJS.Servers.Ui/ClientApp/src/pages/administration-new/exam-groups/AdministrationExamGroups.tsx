/* eslint-disable no-restricted-globals */
import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { IGetAllAdminParams } from '../../../common/types';
import CreateButton from '../../../components/administration/common/create/CreateButton';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import ExamGroupEdit from '../../../components/administration/exam-groups/exam-group-edit/ExamGroupEdit';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import {
    useGetAllAdminExamGroupsQuery,
    useLazyExportExamGroupsToExcelQuery,
} from '../../../redux/services/admin/examGroupsAdminService';
import { flexCenterObjectStyles } from '../../../utils/object-utils';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultSorterToAdd } from '../AdministrationGridView';

import examGroupsFilterableColumns, { returnExamGroupsNonFilterableColumns } from './examGroupsGridColumns';

const AdministrationExamGroupsPage = () => {
    const [ searchParams ] = useSearchParams();

    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString('', defaultSorterToAdd, searchParams));

    const [ openEditExamGroupModal, setOpenEditExamGroupModal ] = useState(false);
    const [ openShowCreateExamGroupModal, setOpenShowCreateExamGroupModal ] = useState<boolean>(false);
    const [ examGroupId, setExamGroupId ] = useState<number>();
    const {
        data,
        error,
        isLoading,
    } = useGetAllAdminExamGroupsQuery(queryParams);

    const onEditClick = (id: number) => {
        setOpenEditExamGroupModal(true);
        setExamGroupId(id);
    };

    const renderEditExamGroupModal = (index: number) => (
        <AdministrationModal
          key={index}
          index={index}
          open={openEditExamGroupModal}
          onClose={() => setOpenEditExamGroupModal(false)}
        >
            <ExamGroupEdit examGroupId={Number(examGroupId)} />
        </AdministrationModal>
    );

    const renderCreateExamGroupModal = (index: number) => (
        <AdministrationModal
          index={index}
          key={index}
          open={openShowCreateExamGroupModal}
          onClose={() => setOpenShowCreateExamGroupModal(!openShowCreateExamGroupModal)}
        >
            <ExamGroupEdit examGroupId={null} isEditMode={false} />
        </AdministrationModal>
    );

    const renderGridActions = () => (
        <CreateButton
          showModal={openShowCreateExamGroupModal}
          showModalFunc={setOpenShowCreateExamGroupModal}
          styles={{ width: '40px', height: '40px' }}
        />
    );

    if (isLoading) {
        return <div style={{ ...flexCenterObjectStyles }}><SpinningLoader /></div>;
    }

    return (
        <AdministrationGridView
          data={data}
          error={error}
          filterableGridColumnDef={examGroupsFilterableColumns}
          notFilterableGridColumnDef={returnExamGroupsNonFilterableColumns(onEditClick)}
          renderActionButtons={renderGridActions}
          queryParams={queryParams}
          setQueryParams={setQueryParams}
          modals={[
              { showModal: openShowCreateExamGroupModal, modal: (i) => renderCreateExamGroupModal(i) },
              { showModal: openEditExamGroupModal, modal: (i) => renderEditExamGroupModal(i) },
          ]}
          excelMutation={useLazyExportExamGroupsToExcelQuery}
        />
    );
};

export default AdministrationExamGroupsPage;
