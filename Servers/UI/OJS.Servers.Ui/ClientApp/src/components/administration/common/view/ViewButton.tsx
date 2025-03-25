import { Link } from 'react-router-dom';
import VisibilityIcon from '@mui/icons-material/Visibility';
import { IconButton } from '@mui/material';
import StyledTooltip from 'src/components/administration/common/styled-tooltip/StyledTooltip';

interface IViewButtonProps {
    path?: string;
    onClick?: Function;
    text: string;
}

const ViewButton = (props: IViewButtonProps) => {
    const { path, text, onClick } = props;

    if (path === undefined && onClick === undefined) {

        return <></>;
    }

    return (
        <StyledTooltip
          placement="left"
          arrow
          title={text}
        >
            {path
                ? <Link to={path}>
                    <IconButton>
                        <VisibilityIcon color="primary" />
                    </IconButton>
                </Link>

                : <IconButton onClick={() => onClick!()}>
                    <VisibilityIcon color="primary" />
                </IconButton>
                }
        </StyledTooltip>
    );
};

export default ViewButton;
