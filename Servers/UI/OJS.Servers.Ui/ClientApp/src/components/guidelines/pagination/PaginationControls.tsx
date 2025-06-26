import { PaginationItem } from '@mui/material';
import Pagination from '@mui/material/Pagination';
import useTheme from 'src/hooks/use-theme';

import { PAGE_BOUNDARY_COUNT, PAGE_JUMP_COUNT, PAGE_SIBLING_COUNT } from '../../../common/constants';
import concatClassNames from '../../../utils/class-names';
import { IHaveOptionalClassName } from '../../common/Props';

import styles from './PaginationControls.module.scss';

interface IPaginationControlsProps extends IHaveOptionalClassName {
    count: number;
    page: number;
    onChange: (value: number) => void | undefined;
    isDataFetching: boolean;
}

const PaginationControls = ({
    count,
    page,
    onChange,
    isDataFetching,
    className = '',
} : IPaginationControlsProps) => {
    const { themeColors, getColorClassName } = useTheme();

    const paginationClassNames = concatClassNames(
        styles.paginationControlsMenu,
        getColorClassName(themeColors.textColor),
        className,
    );

    const handleEllipsisClick = (type: string) => {
        if (isDataFetching) {
            return;
        }

        let newPage;

        if (type === 'start-ellipsis') {
            newPage = Math.max(1, page - PAGE_JUMP_COUNT);
        } else {
            newPage = Math.min(count, page + PAGE_JUMP_COUNT);
        }

        if (newPage !== page) {
            onChange(newPage);
        }
    };

    return count > 1
        ? <Pagination
              count={count}
              siblingCount={PAGE_SIBLING_COUNT}
              boundaryCount={PAGE_BOUNDARY_COUNT}
              onChange={(ev, value) => onChange(value)}
              page={page}
              className={paginationClassNames}
              sx={{
                  ul: {
                      '& .MuiPaginationItem-root.Mui-selected': {
                          backgroundColor: '#44a9f8',
                          color: '#ffffff',
                      },
                      '& .MuiPaginationItem-root': { color: themeColors.textColor },
                      '& .MuiPaginationItem-ellipsis': { cursor: 'pointer' },
                      '& .Mui-disabled': { pointerEvents: 'none' },
                  },
              }}
              showFirstButton
              showLastButton
              disabled={isDataFetching}
              renderItem={(item) => {
                  if (item.type === 'start-ellipsis' || item.type === 'end-ellipsis') {
                      return (
                          <div
                            style={{ cursor: 'pointer', pointerEvents: 'auto' }}
                            onClick={() => handleEllipsisClick(item.type)}
                          >
                              <PaginationItem {...item}>...</PaginationItem>
                          </div>
                      );
                  }
                  return <PaginationItem {...item} />;
              }}
            />
        
        : null;
};

export default PaginationControls;
