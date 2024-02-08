/* eslint-disable prefer-destructuring */
/* eslint-disable max-len */
import React, { FC, useEffect, useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import AutoStoriesIcon from '@mui/icons-material/AutoStories';
import BookmarksIcon from '@mui/icons-material/Bookmarks';
import BorderAllIcon from '@mui/icons-material/BorderAll';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import DataSaverOnIcon from '@mui/icons-material/DataSaverOn';
import KeyboardReturnIcon from '@mui/icons-material/KeyboardReturn';
import NotListedLocationIcon from '@mui/icons-material/NotListedLocation';
import PlaylistAddCheckCircleIcon from '@mui/icons-material/PlaylistAddCheckCircle';
import ScienceIcon from '@mui/icons-material/Science';
import TableViewIcon from '@mui/icons-material/TableView';
import { Tooltip } from '@mui/material';
import MuiAppBar, { AppBarProps as MuiAppBarProps } from '@mui/material/AppBar';
import Box from '@mui/material/Box';
import MuiDrawer from '@mui/material/Drawer';
import IconButton from '@mui/material/IconButton';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import { CSSObject, styled, Theme } from '@mui/material/styles';
import Toolbar from '@mui/material/Toolbar';

import { Anything } from '../../common/common-types';

import styles from './set-admin-navigation.module.scss';

const drawerWidth = 240;

interface IAppBarProps extends MuiAppBarProps {
    open?: boolean;
}

const administrationItems = [
    {
        name: 'Contests',
        icon: <AutoStoriesIcon />,
        path: '/administration-new/contests',
    },
    {
        name: 'Contest Categories',
        icon: <BookmarksIcon />,
        path: '/administration-new/contestCategories',
    },
    {
        name: 'Submissions',
        icon: <PlaylistAddCheckCircleIcon />,
        path: '/administration-new/submissions',
    },
    {
        name: 'Submissions For Processing',
        icon: <DataSaverOnIcon />,
        path: '/administration-new/submissions-for-processing',
    },
    {
        name: 'Tests',
        icon: <ScienceIcon />,
        path: '/administration-new/tests',
    },
    {
        name: 'Problems',
        icon: <NotListedLocationIcon />,
        path: '/administration-new/problems',
    },
    {
        name: 'Problem Groups',
        icon: <TableViewIcon />,
        path: '/administration-new/problemGroups',
    },
    {
        name: 'Submission Types',
        icon: <BorderAllIcon />,
        path: '/administration-new/submissionTypes',
    },
];

const openedMixin = (theme: Theme): CSSObject => ({
    width: drawerWidth,
    transition: theme.transitions.create('width', {
        easing: theme.transitions.easing.sharp,
        duration: theme.transitions.duration.enteringScreen,
    }),
    overflowX: 'hidden',
});

const closedMixin = (theme: Theme): CSSObject => ({
    transition: theme.transitions.create('width', {
        easing: theme.transitions.easing.sharp,
        duration: theme.transitions.duration.leavingScreen,
    }),
    overflowX: 'hidden',
    width: `calc(${theme.spacing(7)} + 1px)`,
    [theme.breakpoints.up('sm')]: { width: `calc(${theme.spacing(8)} + 1px)` },
});

const DrawerHeader = styled('div')(({ theme }) => ({
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'flex-end',
    padding: theme.spacing(0, 1),
    // necessary for content to be below app bar
    ...theme.mixins.toolbar,
}));

const AppBar = styled(MuiAppBar, { shouldForwardProp: (prop) => prop !== 'open' })<IAppBarProps>(({ theme, open }) => ({
    zIndex: theme.zIndex.drawer + 1,
    transition: theme.transitions.create([ 'width', 'margin' ], {
        easing: theme.transitions.easing.sharp,
        duration: theme.transitions.duration.leavingScreen,
    }),
    ...open && {
        marginLeft: drawerWidth,
        transition: theme.transitions.create([ 'width', 'margin' ], {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.enteringScreen,
        }),
    },
}));

const Drawer = styled(MuiDrawer, { shouldForwardProp: (prop) => prop !== 'open' })(({ theme, open }) => ({
    width: drawerWidth,
    flexShrink: 0,
    whiteSpace: 'nowrap',
    boxShadow: '0px 0px 19px -4px rgba(22,0,0,0.75)',
    boxSizing: 'border-box',
    ...open && {
        ...openedMixin(theme),
        '& .MuiDrawer-paper': openedMixin(theme),
    },
    ...!open && {
        ...closedMixin(theme),
        '& .MuiDrawer-paper': closedMixin(theme),
    },
}));

const withAdministrationNav = (ComponentToWrap: FC) => (props: Anything) => {
    const location = useLocation();
    const [ open, setOpen ] = useState(true);
    const [ locationTitle, setLocationTitle ] = useState('');

    const capitalizeFirstLetter = (value: string) => value.charAt(0).toUpperCase() + value.slice(1);

    useEffect(() => {
        const locationPathnameElements = location.pathname.split('/');
        const lastElementOfThePathname = locationPathnameElements[locationPathnameElements.length - 1];
        let pageTitle = '';
        if (!/^\d+$/.test(lastElementOfThePathname)) {
            const section = administrationItems.find((x) => x.path.split('/').pop() === lastElementOfThePathname);
            pageTitle = capitalizeFirstLetter(section!.name);
            setLocationTitle(pageTitle);
        } else {
            pageTitle = capitalizeFirstLetter(`${locationPathnameElements[locationPathnameElements.length - 2]}
            Id: ${lastElementOfThePathname}`);

            setLocationTitle(pageTitle);
        }
        document.title = `Administration ${pageTitle} - SoftUni Judge`;
    }, [ location.pathname ]);

    const handleDrawerOpen = () => {
        setOpen(true);
    };

    const handleDrawerClose = () => {
        setOpen(false);
    };

    return (
        // eslint-disable-next-line @typescript-eslint/ban-ts-comment
        // @ts-ignore
        <Box sx={{ zIndex: 0 }}>
            {!open
                ? (
                    <IconButton className={`${styles.arrowRight} ${styles.arrowCommon}`} color="primary" onClick={handleDrawerOpen}>
                        <ChevronRightIcon />
                    </IconButton>
                )
                : (
                    <IconButton className={`${styles.arrow} ${styles.arrowCommon}`} color="primary" onClick={handleDrawerClose}>
                        <ChevronLeftIcon />
                    </IconButton>
                )}
            <Box sx={{ display: 'flex', zIndex: 0 }}>
                <AppBar position="fixed" open={open}>
                    <Toolbar>
                        <div className={styles.adminHeaderWrapper}>
                            <Link to="/" className={styles.adminHeaderLink}>
                                <Tooltip title="Return to client app">
                                    <KeyboardReturnIcon />
                                </Tooltip>
                            </Link>
                            <div className={styles.locationTitle}>{locationTitle}</div>
                            <Link to="/logout" className={styles.adminHeaderLink}>Sign out</Link>
                        </div>
                    </Toolbar>
                </AppBar>
                <Drawer
                  sx={{ zIndex: 0 }}
                  variant="permanent"
                  open={open}
                >
                    <DrawerHeader>
                        <div />
                    </DrawerHeader>

                    {/* <Box sx={{ overflow: 'auto', marginTop: '20px' }}> */}
                    <List>
                        {administrationItems.map((item) => (
                            <ListItem key={item.name} disablePadding>
                                <Link
                                  to={item.path}
                                  className={`${location.pathname === item.path
                                      ? styles.activeAdminNavLink
                                      : ''} ${styles.adminNavLink}`}
                                >
                                    <ListItemButton>
                                        <ListItemIcon style={{
                                            color: location.pathname === item.path
                                                ? '#42abf8'
                                                : '#3e4c5d',
                                        }}
                                        >
                                            {item.icon}
                                        </ListItemIcon>
                                        <ListItemText primary={item.name} />
                                    </ListItemButton>
                                </Link>
                            </ListItem>
                        ))}
                    </List>
                </Drawer>
                <Box component="main" sx={{ flexGrow: 1, p: 3 }}>
                    {/* eslint-disable-next-line react/jsx-props-no-spreading */}
                    <ComponentToWrap {...props} />
                </Box>
            </Box>
        </Box>
    );
};

export default withAdministrationNav;
