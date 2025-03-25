 
import React, { useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { IGetAllAdminParams } from '../../../common/types';
import CreateButton from '../../../components/administration/common/create/CreateButton';
import AdministrationModal from '../../../components/administration/common/modals/administration-modal/AdministrationModal';
import CategoryEdit from '../../../components/administration/contest-categories/CategoryEdit';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { getColors, useAdministrationTheme } from '../../../hooks/use-administration-theme-provider';
import { useGetAllAdminContestCategoriesQuery, useLazyExportContestCategoriesToExcelQuery } from '../../../redux/services/admin/contestCategoriesAdminService';
import { renderSuccessfullAlert } from '../../../utils/render-utils';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView, { defaultFilterToAdd, defaultSorterToAdd } from '../AdministrationGridView';

import ContestsBulkEdit from './contests-bulk-edit/ContestsBulkEdit';
import categoriesFilterableColumns, { returnCategoriesNonFilterableColumns } from './contestCategoriesGridColumns';

import styles from './AdministrationContestCategories.module.scss';

const AdministrationContestCategoriesPage = () => {
    const [ searchParams ] = useSearchParams();
    const { themeMode } = useAdministrationTheme();
    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(defaultFilterToAdd, defaultSorterToAdd, searchParams));

    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ openEditContestCategoryModal, setOpenEditContestCategoryModal ] = useState(false);
    const [ openShowCreateContestCategoryModal, setOpenShowCreateContestCategoryModal ] = useState<boolean>(false);
    const [ showContestsBulkEditModal, setShowContestsBulkEditModal ] = useState<boolean>(false);
    const [ contestCategoryId, setContestCategoryId ] = useState<number>();
    const [ contestCategoryName, setContestCategoryName ] = useState<string>();

    const {
        refetch: retakeData,
        data,
        error,
        isLoading,
    } = useGetAllAdminContestCategoriesQuery(queryParams);

    const onEditClick = (id: number) => {
        setOpenEditContestCategoryModal(true);
        setContestCategoryId(id);
    };

    const onContestsBulkEditClick = (id: number, name: string) => {
        setShowContestsBulkEditModal(true);
        setContestCategoryId(id);
        setContestCategoryName(name);
    };

    const onCloseModal = (isEditMode: boolean) => {
        if (isEditMode) {
            setOpenEditContestCategoryModal(false);
        } else {
            setOpenShowCreateContestCategoryModal(false);
        }
        retakeData();
    };

    const renderCategoryModal = (index: number, isEditMode: boolean) => 
        <AdministrationModal
          index={index}
          open={isEditMode
              ? openEditContestCategoryModal
              : openShowCreateContestCategoryModal}
          onClose={() => onCloseModal(isEditMode)}
        >
            <CategoryEdit
              contestCategoryId={isEditMode
                  ? Number(contestCategoryId)
                  : null}
              isEditMode={isEditMode}
              onSuccess={() => onCloseModal(isEditMode)}
              setParentSuccessMessage={setSuccessMessage}
            />
        </AdministrationModal>
    ;

    const renderContestsBulkEditModal = (index: number) => 
        <AdministrationModal
          index={index}
          open={showContestsBulkEditModal}
          onClose={() => setShowContestsBulkEditModal(false)}
          className={styles.administrationModal}
        >
            <ContestsBulkEdit
              categoryId={contestCategoryId}
              categoryName={contestCategoryName}
              setParentSuccessMessage={setSuccessMessage}
              onSuccess={() => setShowContestsBulkEditModal(false)}
            />
        </AdministrationModal>
    ;

    const renderGridActions = () => 
        <CreateButton
          showModal={openShowCreateContestCategoryModal}
          showModalFunc={setOpenShowCreateContestCategoryModal}
          styles={{ width: '40px', height: '40px' }}
        />
    ;

    if (isLoading) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderSuccessfullAlert(successMessage, 7000)}
            <AdministrationGridView
              data={data}
              error={error}
              filterableGridColumnDef={categoriesFilterableColumns}
              notFilterableGridColumnDef={returnCategoriesNonFilterableColumns(
                  onEditClick,
                  onContestsBulkEditClick,
                  retakeData,
                  setSuccessMessage,
              )}
              renderActionButtons={renderGridActions}
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              modals={[
                  { showModal: openShowCreateContestCategoryModal, modal: (i) => renderCategoryModal(i, false) },
                  { showModal: openEditContestCategoryModal, modal: (i) => renderCategoryModal(i, true) },
                  { showModal: showContestsBulkEditModal, modal: (i) => renderContestsBulkEditModal(i) },
              ]}
              legendProps={[
                  { color: getColors(themeMode).palette.deleted, message: 'Category is deleted.' },
                  { color: getColors(themeMode).palette.visible, message: 'Category is not visible' } ]}
              excelMutation={useLazyExportContestCategoriesToExcelQuery}
            />
        </>
    );
};

export default AdministrationContestCategoriesPage;
