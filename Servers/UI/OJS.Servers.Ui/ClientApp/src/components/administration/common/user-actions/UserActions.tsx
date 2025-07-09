import React, { ChangeEvent, FC, RefObject, useState } from 'react';
import { CiSquareMore } from 'react-icons/ci';
import HomeIcon from '@mui/icons-material/Home';
import LogoutIcon from '@mui/icons-material/Logout';
import { FormControlLabel, IconButton, Menu, MenuItem, styled, Switch, Tooltip } from '@mui/material';
import { ThemeMode } from 'src/common/enums';
import { LinkButton, LinkButtonType } from 'src/components/guidelines/buttons/Button';
import ConfirmDialog from 'src/components/guidelines/dialog/ConfirmDialog';
import concatClassNames from 'src/utils/class-names';

import styles from './UserActions.module.scss';

interface IUserActionsProps {
    isDropdown: boolean;
    handleThemeChange: (event: ChangeEvent<HTMLInputElement>) => void;
    themeMode: ThemeMode;
    showMenu: boolean;
    setShowMenu: (show: boolean) => void;
    iconButtonRef: RefObject<HTMLButtonElement | null>;
}

const MaterialUISwitch = styled(Switch)(({ theme }) => ({
    width: 52,
    height: 26,
    padding: 7,
    '& .MuiSwitch-switchBase': {
        margin: 1,
        padding: 0,
        transform: 'translateX(6px)',
        '&.Mui-checked': {
            color: '#fff',
            transform: 'translateX(22px)',
            '& .MuiSwitch-thumb:before': { backgroundImage: `url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" height="20" width="20" viewBox="0 0 20 20"><path fill="${encodeURIComponent('#fff')}" d="M4.2 2.5l-.7 1.8-1.8.7 1.8.7.7 1.8.6-1.8L6.7 5l-1.9-.7-.6-1.8zm15 8.3a6.7 6.7 0 11-6.6-6.6 5.8 5.8 0 006.6 6.6z"/></svg>')` },
            '& + .MuiSwitch-track': {
                opacity: 1,
                backgroundColor: theme.palette.mode === 'dark'
                    ? '#8796A5'
                    : '#aab4be',
            },
        },
    },
    '& .MuiSwitch-thumb': {
        backgroundColor: theme.palette.mode === 'dark'
            ? '#003892'
            : '#001e3c',
        width: 24,
        height: 24,
        '&::before': {
            content: '\'\'',
            position: 'absolute',
            width: '100%',
            height: '100%',
            left: 0,
            top: 0,
            backgroundRepeat: 'no-repeat',
            backgroundPosition: 'center',
            backgroundImage: `url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" height="20" width="20" viewBox="0 0 20 20"><path fill="${encodeURIComponent('#fff')}" d="M9.305 1.667V3.75h1.389V1.667h-1.39zm-4.707 1.95l-.982.982L5.09 6.072l.982-.982-1.473-1.473zm10.802 0L13.927 5.09l.982.982 1.473-1.473-.982-.982zM10 5.139a4.872 4.872 0 00-4.862 4.86A4.872 4.872 0 0010 14.862 4.872 4.872 0 0014.86 10 4.872 4.872 0 0010 5.139zm0 1.389A3.462 3.462 0 0113.471 10a3.462 3.462 0 01-3.473 3.472A3.462 3.462 0 016.527 10 3.462 3.462 0 0110 6.528zM1.665 9.305v1.39h2.083v-1.39H1.666zm14.583 0v1.39h2.084v-1.39h-2.084zM5.09 13.928L3.616 15.4l.982.982 1.473-1.473-.982-.982zm9.82 0l-.982.982 1.473 1.473.982-.982-1.473-1.473zM9.305 16.25v2.083h1.389V16.25h-1.39z"/></svg>')`,
        },
    },
    '& .MuiSwitch-track': {
        opacity: 1,
        backgroundColor: theme.palette.mode === 'dark'
            ? '#8796A5'
            : '#aab4be',
        borderRadius: 20 / 2,
    },
}));

const UserActions: FC<IUserActionsProps> = ({
    isDropdown,
    handleThemeChange,
    themeMode,
    showMenu,
    setShowMenu,
    iconButtonRef,
}) => {
    const [ showConfirmLogout, setShowConfirmLogout ] = useState<boolean>(false);

    const onLogoutClick = () => {
        setShowConfirmLogout(!showConfirmLogout);
    };

    const onLogout = () => {
        setShowConfirmLogout(false);
        setShowMenu(false);
        window.open('/logout', '_blank');
    };

    const onHomeRedirection = () => {
        setShowMenu(false);
        window.open('/', '_blank');
    };

    const actions = 
        <>
            <FormControlLabel
              control={(
                  <MaterialUISwitch
                    onChange={handleThemeChange}
                    checked={themeMode === ThemeMode.Light}
                  />
                  )}
              label=""
              sx={{
                  position: 'absolute',
                  top: 1,
                  right: 1,
                  marginRight: '0.5rem',
                  marginTop: '0.5rem',
              }}
            />
            <div className={styles.buttons}>
                <Tooltip title="Go to home page" className={styles.actionsWrapper}>
                    <span>
                        <LinkButton to="/" isToExternal type={LinkButtonType.plain}>
                            <HomeIcon className={concatClassNames(styles.icon, styles.activeColor)} />
                        </LinkButton>
                    </span>
                </Tooltip>
                <Tooltip title="Logout" className={styles.actionsWrapper}>
                    <span>
                        <LogoutIcon onClick={onLogoutClick} className={concatClassNames(styles.icon, styles.activeColor)} />
                    </span>
                </Tooltip>
            </div>
        </>
    ;

    const dropdownActions = 
        <div className={styles.dropdown}>
            <IconButton
              className={styles.dropdownIcon}
              ref={iconButtonRef}
              aria-controls={showMenu
                  ? 'menu-list'
                  : undefined}
              aria-haspopup="true"
              onClick={() => setShowMenu(!showMenu)}
            >
                <CiSquareMore />
            </IconButton>
            <Menu
              id="menu-list"
              anchorEl={iconButtonRef.current}
              open={showMenu}
              onClose={() => setShowMenu(false)}
            >
                <div className={styles.actionsWrapper}>
                    <MenuItem className={styles.dropdownRow}>
                        <FormControlLabel
                          control={(
                              <MaterialUISwitch
                                onChange={handleThemeChange}
                                checked={themeMode === ThemeMode.Light}
                              />
                            )}
                          label="Theme"
                        />
                    </MenuItem>
                    <MenuItem
                      onClick={onHomeRedirection}
                      className={styles.dropdownRow}
                    >
                        <HomeIcon className={concatClassNames(styles.icon, styles.activeColor)} />
                        Home
                    </MenuItem>
                    <MenuItem
                      onClick={onLogoutClick}
                      className={styles.dropdownRow}
                    >
                        <LogoutIcon className={concatClassNames(styles.icon, styles.activeColor)} />
                        Logout
                    </MenuItem>
                </div>
            </Menu>
        </div>
    ;

    return (
        <>
            {isDropdown
                ? dropdownActions
                : actions}
            {showConfirmLogout && 
                <ConfirmDialog
                  title="Logout"
                  text="Are you sure you want to logout?"
                  confirmButtonText="Logout"
                  declineButtonText="Cancel"
                  onClose={onLogoutClick}
                  confirmFunction={onLogout}
                />
            }
        </>
    );
};

export default UserActions;
