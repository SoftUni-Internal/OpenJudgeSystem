import { MenuItem, Select } from '@mui/material';

import useTheme from '../../../hooks/use-theme';

import styles from './Dropdown.module.scss';

interface IDropdownItem {
    id: number;
    name: string;
}

interface IDropdownProps {
    dropdownItems: Array<IDropdownItem>;
    value: string;
    handleDropdownItemClick?: (arg?: any) => any;
    placeholder?: string;
    isDisabled?: boolean;
}

const Dropdown = (props: IDropdownProps) => {
    const {
        dropdownItems,
        value,
        handleDropdownItemClick,
        isDisabled = false,
        placeholder = 'Select element',
    } = props;

    const { getColorClassName, themeColors } = useTheme();

    const textColorClassName = getColorClassName(themeColors.textColor);

    return (
        <Select
          sx={{ '.MuiSvgIcon-root ': { fill: themeColors.textColor } }}
          className={`${styles.dropdown} ${textColorClassName}`}
          value={value}
          autoWidth
          displayEmpty
          disabled={isDisabled}
          MenuProps={{ MenuListProps: { disablePadding: true } }}
        >
            <MenuItem key="dropdown-default-item" value="" disabled>
                {placeholder}
            </MenuItem>
            {dropdownItems.map((item: IDropdownItem) => (
                <MenuItem
                  key={`dropdown-item-${item.id}`}
                  value={item.id}
                  selected
                  onClick={() => handleDropdownItemClick?.(item)}
                  className={textColorClassName}
                >
                    {item.name}
                </MenuItem>
            ))}
        </Select>
    );
};

export default Dropdown;
