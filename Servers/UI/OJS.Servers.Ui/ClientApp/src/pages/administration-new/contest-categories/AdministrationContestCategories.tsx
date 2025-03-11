/* eslint-disable no-restricted-globals */
import React, { useCallback, useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAppSelector } from 'src/redux/store';
import { CONTESTS_BULK_EDIT } from 'src/utils/constants';

import { IContestCategory, IGetAllAdminParams } from '../../../common/types';
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
    const { contestCategories } = useAppSelector((state) => state.contests);
    const navigate = useNavigate();
    const { themeMode } = useAdministrationTheme();
    // eslint-disable-next-line max-len
    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString(defaultFilterToAdd, defaultSorterToAdd, searchParams));

    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);
    const [ openEditContestCategoryModal, setOpenEditContestCategoryModal ] = useState(false);
    const [ openShowCreateContestCategoryModal, setOpenShowCreateContestCategoryModal ] = useState<boolean>(false);
    const [ openShowContestsBulkEditModal, setOpenShowContestsBulkEditModal ] = useState<boolean>(false);
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
        setOpenShowContestsBulkEditModal(true);
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

    const renderCategoryModal = (index: number, isEditMode: boolean) => (
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
    );

    const renderContestsBulkEditModal = (index: number) => (
        <AdministrationModal
          index={index}
          open={openShowContestsBulkEditModal}
          onClose={() => setOpenShowContestsBulkEditModal(false)}
          className={styles.administrationModal}
        >
            <ContestsBulkEdit
              categoryId={contestCategoryId}
              categoryName={contestCategoryName}
              setParentSuccessMessage={setSuccessMessage}
              onSuccess={() => setOpenShowContestsBulkEditModal(false)}
            />
        </AdministrationModal>
    );

    const renderGridActions = () => (
        <CreateButton
          showModal={openShowCreateContestCategoryModal}
          showModalFunc={setOpenShowCreateContestCategoryModal}
          styles={{ width: '40px', height: '40px' }}
        />
    );

    const getContestCategory = useCallback(
        (category: IContestCategory, categoryId: number): IContestCategory | null => {
            if (!category) {
                return null;
            }

            if (category.id === categoryId) {
                return category;
            }

            return (
                category.children
                    .map((child) => getContestCategory(child, categoryId))
                    .find((foundCategory) => foundCategory !== null) || null
            );
        },
        [],
    );

    useEffect(() => {
        const categoryId = Number(searchParams.get(CONTESTS_BULK_EDIT));
        if (categoryId) {
            onContestsBulkEditClick(
                categoryId,
                getContestCategory({ id: 0, children: [ ...contestCategories ] } as IContestCategory, categoryId)?.name ?? '',
            );

            const newParams = new URLSearchParams(searchParams);
            newParams.delete(CONTESTS_BULK_EDIT);
            navigate({ search: newParams.toString() }, { replace: true });
        }
    }, [ contestCategories, getContestCategory, navigate, searchParams ]);

    if (isLoading) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            <AdministrationGridView
              data={data}
              error={error}
              filterableGridColumnDef={categoriesFilterableColumns}
              notFilterableGridColumnDef={returnCategoriesNonFilterableColumns(onEditClick, onContestsBulkEditClick)}
              renderActionButtons={renderGridActions}
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              modals={[
                  { showModal: openShowCreateContestCategoryModal, modal: (i) => renderCategoryModal(i, false) },
                  { showModal: openEditContestCategoryModal, modal: (i) => renderCategoryModal(i, true) },
                  { showModal: openShowContestsBulkEditModal, modal: (i) => renderContestsBulkEditModal(i) },
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
