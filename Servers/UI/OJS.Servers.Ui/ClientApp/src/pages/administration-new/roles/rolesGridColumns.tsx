/* eslint-disable @typescript-eslint/ban-types */
import React from 'react';
import { TbBinaryTree } from 'react-icons/tb';
import AddIcon from '@mui/icons-material/Add';
import AutoStoriesIcon from '@mui/icons-material/AutoStories';
import RemoveIcon from '@mui/icons-material/Remove';
import { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';

import { EDIT } from '../../../common/labels';
import { DELETE_CONFIRMATION_MESSAGE } from '../../../common/messages';
import { NEW_ADMINISTRATION_PATH, ROLES_PATH } from '../../../common/urls/administration-urls';
import AdministrationGridDropdown
    from '../../../components/administration/common/administration-grid-dropdown/AdministrationGridDropdown';
import DeleteButton from '../../../components/administration/common/delete/DeleteButton';
import QuickEditButton from '../../../components/administration/common/edit/QuickEditButton';
import RedirectButton from '../../../components/administration/common/edit/RedirectButton';
import { AdministrationGridColDef } from '../../../components/administration/utils/mui-utils';
import { useDeleteRolesMutation } from '../../../redux/services/admin/rolesAdminService';

import styles from './AdministrationRolesPage.module.scss';

const rolesFilterableColumns: AdministrationGridColDef[] = [
    {
        field: 'id',
        headerName: 'Id',
        flex: 2,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
    {
        field: 'name',
        headerName: 'Name',
        flex: 3,
        type: 'string',
        filterable: false,
        sortable: false,
        align: 'center',
        headerAlign: 'center',
    },
];

export const returnRolesNonFilterableColumns = (
    onEditClick: Function,
    onAddLecturerToCategory: Function,
    onRemoveLecturerFromCategory: Function,
    onAddLecturerToContest: Function,
    onRemoveLecturerFromContest: Function,
    onSuccessfulDelete:() => void,
    setParentSuccessMessage: Function,
) => [
    {
        field: 'actions',
        headerName: 'Actions',
        flex: 1,
        minWidth: 250,
        headerAlign: 'center',
        align: 'center',
        filterable: false,
        sortable: false,
        renderCell: (params: GridRenderCellParams) => (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <QuickEditButton onEdit={() => onEditClick(params.row.id)} />
                <RedirectButton path={`/${NEW_ADMINISTRATION_PATH}/${ROLES_PATH}/${params.row.id}`} location={`${EDIT} page`} />
                <DeleteButton
                  id={params.row.id}
                  name={params.row.name}
                  text={DELETE_CONFIRMATION_MESSAGE}
                  mutation={useDeleteRolesMutation}
                  onSuccess={onSuccessfulDelete}
                  setParentSuccessMessage={setParentSuccessMessage}
                />
                {params.row.name === 'Lecturer' && (
                    <>
                        <AdministrationGridDropdown
                          icon={<TbBinaryTree />}
                          tooltipTitle="Contest Category Actions"
                          sections={
                            [
                                {
                                    icon: <AddIcon className={styles.button} />,
                                    label: 'Add Lecturer To Category',
                                    handleClick: () => onAddLecturerToCategory(params.row.id),
                                },
                                {
                                    icon: <RemoveIcon className={styles.button} />,
                                    label: 'Remove Lecturer From Category',
                                    handleClick: () => onRemoveLecturerFromCategory(params.row.id),
                                },
                            ]
                        }
                          id={Number(params.row.id)}
                        />
                        <AdministrationGridDropdown
                          icon={<AutoStoriesIcon />}
                          tooltipTitle="Contest Actions"
                          sections={
                            [
                                {
                                    icon: <AddIcon className={styles.button} />,
                                    label: 'Add Lecturer To Contest',
                                    handleClick: () => onAddLecturerToContest(params.row.id),
                                },
                                {
                                    icon: <RemoveIcon className={styles.button} />,
                                    label: 'Remove Lecturer From Contest',
                                    handleClick: () => onRemoveLecturerFromContest(params.row.id),
                                },
                            ]
                        }
                          id={Number(params.row.id)}
                        />
                    </>
                )}
            </div>
        ),
    },
] as GridColDef[];

export default rolesFilterableColumns;
