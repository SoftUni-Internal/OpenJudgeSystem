import { styled, Tooltip, tooltipClasses, TooltipProps } from '@mui/material';
import { ThemeMode } from 'src/common/enums';

interface IStyledTooltipProps extends TooltipProps {
    disabled?: boolean;
}

const StyledTooltip = styled(({ className, disabled, ...props }: IStyledTooltipProps) => 
    <Tooltip
      {...props}
      classes={{ popper: className }}
      disableHoverListener={disabled}
      disableFocusListener={disabled}
      disableTouchListener={disabled}
    />)(({ theme }) => ({
    [`& .${tooltipClasses.tooltip}`]: {
        backgroundColor: theme.palette.mode === ThemeMode.Light
            ? '#fff'
            : '#444',
        color: theme.palette.mode === ThemeMode.Light
            ? '#000'
            : '#fff',
        boxShadow: theme.shadows[1],
        fontSize: 14,
        border: theme.palette.mode === ThemeMode.Light
            ? '1px solid #ccc'
            : '1px solid #666',
    },
    [`& .${tooltipClasses.arrow}`]: {
        color: theme.palette.mode === ThemeMode.Light
            ? '#fff'
            : '#444',
        '&::before': {
            border: theme.palette.mode === ThemeMode.Light
                ? '1px solid #ccc'
                : '1px solid #666',
        },
    },
}));

export default StyledTooltip;
