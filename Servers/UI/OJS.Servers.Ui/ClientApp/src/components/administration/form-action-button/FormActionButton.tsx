
import { Box, Button } from '@mui/material';

interface IFormActionButton {
    name: string;
    onClick: Function;
    className?: string;
    disabled?: boolean;
    buttonClassName?: string;
}
const FormActionButton = (props: IFormActionButton) => {
    const { name, onClick, className, disabled, buttonClassName } = props;
    return (
        <Box className={className}>
            <Button
              variant="contained"
              onClick={() => onClick()}
              className={buttonClassName}
              disabled={disabled ?? false}
            >
                {name}
            </Button>
        </Box>
    );
};
export default FormActionButton;
