import { Dispatch, ReactNode, SetStateAction, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import AddBoxIcon from '@mui/icons-material/AddBox';
import { Box, IconButton, Slide, Tooltip } from '@mui/material';
import { DataGrid, GridColDef, GridPaginationModel } from '@mui/x-data-grid';
import { SerializedError } from '@reduxjs/toolkit';
import { FetchBaseQueryError } from '@reduxjs/toolkit/query';

import { ACTION_NOT_ALLOWED_MESSAGE } from '../../common/messages';
import { ExceptionData, IGetAllAdminParams, IPagedResultType } from '../../common/types';
import ExportExcel from '../../components/administration/common/export-excel/ExportExcel';
import LegendBox from '../../components/administration/common/legend-box/LegendBox';
import { isDeletedClassName, isVisibleClassName } from '../../hooks/use-administration-theme-provider';
import { DEFAULT_ITEMS_PER_PAGE, DEFAULT_ROWS_PER_PAGE } from '../../utils/constants';
import { flexCenterObjectStyles } from '../../utils/object-utils';

import AdministrationFilters, { addDefaultFilter, IAdministrationFilter, IAdministrationSorter, mapGridColumnsToAdministrationFilterProps, mapGridColumnsToAdministrationSortingProps, mapUrlToSorters } from './administration-filters/AdministrationFilters';

import styles from './AdministrationStyles.module.scss';

interface IAdministrationGridViewProps<T> {

    filterableGridColumnDef: Array<GridColDef>;
    notFilterableGridColumnDef: Array<GridColDef>;
    data: IPagedResultType<T> | undefined;
    showFiltersAndSorters?: boolean;
    renderActionButtons?: () => ReactNode;
    modals?: Array<{showModal:boolean; modal: (index: number) => ReactNode}>;
    error: ExceptionData[] | FetchBaseQueryError | SerializedError | undefined;
    queryParams?: IGetAllAdminParams;
    setQueryParams?: Dispatch<SetStateAction<IGetAllAdminParams>>;
    withSearchParams?: boolean;
    legendProps?: Array<{color: string; message:string}>;
    specificRowIdName?: string | null;
    excelMutation?: any;
    defaultFilter?: string;
    defaultSorter?: string;

}

const defaultFilterToAdd = 'isdeleted~equals~false';
const defaultSorterToAdd = 'id=DESC';
const AdministrationGridView = <T extends object >(props: IAdministrationGridViewProps<T>) => {
    const {
        filterableGridColumnDef,
        notFilterableGridColumnDef,
        data,
        showFiltersAndSorters = true,
        renderActionButtons,
        modals = [],
        error,
        queryParams,
        setQueryParams,
        withSearchParams = true,
        legendProps,
        excelMutation,
        specificRowIdName: specifyColumnIdName,
        defaultFilter = defaultFilterToAdd,
        defaultSorter = defaultSorterToAdd,
    } = props;

    const [ searchParams, setSearchParams ] = useSearchParams();

    // eslint-disable-next-line max-len
    const [ selectedFilters, setSelectedFilters ] = useState<Array<IAdministrationFilter>>(addDefaultFilter(filterableGridColumnDef, searchParams, defaultFilter));

    const [ selectedSorters, setSelectedSorters ] = useState<Array<IAdministrationSorter>>(mapUrlToSorters(
        searchParams ?? '',
        mapGridColumnsToAdministrationSortingProps(filterableGridColumnDef),
        defaultSorter,
    ));

    const getRowClassName = (isDeleted: boolean, isVisible: boolean) => {
        if (isDeleted) {
            return isDeletedClassName;
        } if (isVisible === false) {
            return isVisibleClassName;
        }
        return '';
    };

    const renderActions = () => (
        <div style={{ ...flexCenterObjectStyles, justifyContent: 'space-between' }}>
            {renderActionButtons
                ? renderActionButtons()
                : (
                    <Tooltip title={ACTION_NOT_ALLOWED_MESSAGE}>
                        <Box>
                            <IconButton disabled>
                                {' '}
                                <AddBoxIcon sx={{ width: '40px', height: '40px' }} color="disabled" />
                            </IconButton>
                        </Box>
                    </Tooltip>
                )}
            <ExportExcel mutation={excelMutation} disabled={!excelMutation} queryParams={queryParams} />
        </div>
    );
    const renderGridSettings = () => {
        const sortingColumns = mapGridColumnsToAdministrationSortingProps(filterableGridColumnDef);
        const filtersColumns = mapGridColumnsToAdministrationFilterProps(filterableGridColumnDef);

        return (
            <div style={{ ...flexCenterObjectStyles, justifyContent: 'space-between' }}>
                { renderActions() }
                {showFiltersAndSorters && (
                <div style={{ ...flexCenterObjectStyles, width: '100%', gap: '20px' }}>
                    <AdministrationFilters
                      searchParams={searchParams}
                      setSearchParams={setSearchParams}
                      withSearchParams={withSearchParams}
                      setSelectedFilters={setSelectedFilters}
                      setQueryParams={setQueryParams}
                      selectedFilters={selectedFilters}
                      filterColumns={filtersColumns}
                      sortingColumns={sortingColumns}
                      selectedSorters={selectedSorters}
                      setSelectedSorters={setSelectedSorters}
                    />
                </div>
                )}
                <LegendBox renders={legendProps ?? []} />
            </div>
        );
    };

    const handlePaginationModelChange = (model: GridPaginationModel) => {
        if (setQueryParams) {
            setQueryParams({ ...queryParams, page: model.page + 1, itemsPerPage: model.pageSize });
        }
    };
    return (
        <Slide direction="left" in mountOnEnter unmountOnExit timeout={400}>
            <div>
                {modals.map((m, i) => m.showModal && m.modal(i))}
                { renderGridSettings() }
                { error
                    ? <div className={styles.errorText}>Error loading data</div>
                    : (
                        <DataGrid
                          columns={[ ...filterableGridColumnDef, ...notFilterableGridColumnDef ]}
                          rows={data?.items ?? []}
                          rowCount={data?.totalItemsCount ?? 0}
                          paginationMode="server"
                          autoHeight
                          onPaginationModelChange={handlePaginationModelChange}
                          pageSizeOptions={[ ...DEFAULT_ROWS_PER_PAGE ]}
                          disableRowSelectionOnClick
                          getRowId={(row) => specifyColumnIdName
                              ? row[specifyColumnIdName]
                              : row.id}
                          getRowClassName={(params) => getRowClassName(params.row.isDeleted, params.row.isVisible)}
                          initialState={{
                              columns: {
                                  columnVisibilityModel: {
                                      isDeleted: false,
                                      isVisible: false,
                                      createdOn: false,
                                      modifiedOn: false,
                                  },
                              },
                              pagination: {
                                  paginationModel: {
                                      page: 0,
                                      pageSize: DEFAULT_ITEMS_PER_PAGE,
                                  },
                              },
                          }}
                        />
                    )}
            </div>
        </Slide>
    );
};

export {
    defaultFilterToAdd,
    defaultSorterToAdd,
};

export default AdministrationGridView;
