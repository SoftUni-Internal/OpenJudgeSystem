
import { MdEdit } from 'react-icons/md';
import { IconButton, Tooltip } from '@mui/material';

import { EDIT } from '../../../../common/labels';

interface IEditIconProps {
    onEdit: Function;
}
const QuickEditButton = (props: IEditIconProps) => {
    const { onEdit } = props;
    return (
        <Tooltip title={`Quick ${EDIT}`}>
            <IconButton onClick={() => onEdit()}>
                <MdEdit color="orange" />
            </IconButton>
        </Tooltip>
    );
};

export default QuickEditButton;
