/* eslint-disable import/prefer-default-export */
import { ReactNode } from 'react';
import { GridRenderCellParams } from '@mui/x-data-grid';

const whenNotDeleted = (renderFunction: (params: GridRenderCellParams) => ReactNode) => (params: GridRenderCellParams) => {
    if (params.row.isDeleted) {
        return null;
    }

    return renderFunction(params);
};

export {
    whenNotDeleted,
};
