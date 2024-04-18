/* eslint-disable @typescript-eslint/no-use-before-define */

import { useEffect, useRef, useState } from 'react';
import { FaCheckDouble, FaLayerGroup, FaUserCircle, FaUsers } from 'react-icons/fa';
import { GiFiles } from 'react-icons/gi';
import { IoSettingsSharp } from 'react-icons/io5';
import { MdOutlineAirlineStops, MdOutlineRememberMe } from 'react-icons/md';
import { Link, Navigate, Route, Routes, useLocation } from 'react-router-dom';
import AutoStoriesIcon from '@mui/icons-material/AutoStories';
import BookmarksIcon from '@mui/icons-material/Bookmarks';
import BorderAllIcon from '@mui/icons-material/BorderAll';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import DataSaverOnIcon from '@mui/icons-material/DataSaverOn';
import NotListedLocationIcon from '@mui/icons-material/NotListedLocation';
import PlaylistAddCheckCircleIcon from '@mui/icons-material/PlaylistAddCheckCircle';
import ScienceIcon from '@mui/icons-material/Science';
import TableViewIcon from '@mui/icons-material/TableView';
import { Box, CSSObject, Divider, IconButton, List, ListItem, ListItemButton, ListItemIcon, ListItemText, Menu, MenuItem, styled, Theme, Tooltip, Typography } from '@mui/material';
import MuiDrawer from '@mui/material/Drawer';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';

import 'dayjs/locale/bg';

import { CHECKERS_PATH, CONTEST_CATEGORIES_PATH, CONTESTS_PATH, EXAM_GROUPS_PATH, NEW_ADMINISTRATION_PATH, PARTICIPANTS_PATH, PROBLEM_GROUPS_PATH, PROBLEM_RESOURCES_PATH, PROBLEMS_PATH, ROLES_PATH, SETTINGS_PATH, SUBMISSION_TYPES_PATH, SUBMISSIONS_FOR_PROCESSING_PATH, SUBMISSIONS_PATH, TESTS_PATH, USERS_PATH } from '../../../common/urls/administration-urls';
import AdministrationPage from '../../../pages/administration/AdministrationPage';
import AdministrationContestCategories from '../../../pages/administration-new/contest-categories/AdministrationContestCategories';
import AdministrationContestsPage from '../../../pages/administration-new/contests/AdministrationContests';
import AdministrationExamGroupsPage from '../../../pages/administration-new/exam-groups/AdministrationExamGroups';
import ParticipantsAdministrationPage from '../../../pages/administration-new/participants/ParticipantsAdministrationPage';
import AdministrationProblemGroupsPage from '../../../pages/administration-new/problem-groups/AdministrationProblemGroupsPage';
import AdministrationProblemResourcesPage from '../../../pages/administration-new/problem-resources/AdministrationProblemResourcesPage';
import AdministrationProblemsPage from '../../../pages/administration-new/problems/AdministrationProblemsPage';
import AdministrationRolesPage from '../../../pages/administration-new/roles/AdministrationRolesPage';
import AdministrationSettingsPage from '../../../pages/administration-new/settings/SettingsAdministrationPage';
import AdministrationSubmissionTypesPage from '../../../pages/administration-new/submission-types/AdministrationSubmissionTypesPage';
import AdministrationSubmissionsPage from '../../../pages/administration-new/submissions/AdministrationSubmissionsPage';
import AdminSubmissionForProcessingDetails
    from '../../../pages/administration-new/submissions-for-processing/AdministrationSubmissionForProcessing';
import AdministrationSubmissionsForProcessingPage from '../../../pages/administration-new/submissions-for-processing/AdministrationSubmissionsForProcessingPage';
import AdministrationTestsPage from '../../../pages/administration-new/tests/AdministrationTestsPage';
import AdministrationUsersPage from '../../../pages/administration-new/users/AdministrationUsersPage';
import AdministrationCheckersPage from '../../../pages/checkers/AdministrationCheckersPage';
import NotFoundPage from '../../../pages/not-found/NotFoundPage';
import { useAppSelector } from '../../../redux/store';
import AdministrationContestPage from '../../administration/contests/AdministrationContestPage';
import AdministrationExamGroupPage from '../../administration/exam-groups/AdministrationExamGroupPage';
import AdministrationProblemGroup from '../../administration/problem-groups/AdministrationProblemGroup';
import AdministrationProblemResource from '../../administration/problem-resources/AdministrationProblemResource';
import AdministrationProblem from '../../administration/problems/AdministrationProblem';
import AdministrationRole from '../../administration/roles/AdministrationRole';
import AdministrationTest from '../../administration/tests/AdministrationTest';
import AdministrationUser from '../../administration/users/AdministrationUser';

import styles from './AdministrationPortal.module.scss';

const drawerWidth = 240;

const administrationItems = [
    {
        name: 'Contests',
        icon: <AutoStoriesIcon />,
        path: `${CONTESTS_PATH}`,
        visibleOnlyForAdmin: false,
    },
    {
        name: 'Contest Categories',
        icon: <BookmarksIcon />,
        path: `${CONTEST_CATEGORIES_PATH}`,
        visibleOnlyForAdmin: false,

    },
    {
        name: 'Submissions',
        icon: <PlaylistAddCheckCircleIcon />,
        path: `${SUBMISSIONS_PATH}`,
        visibleOnlyForAdmin: false,
    },
    {
        name: 'Submissions For Processing',
        icon: <DataSaverOnIcon />,
        path: `${SUBMISSIONS_FOR_PROCESSING_PATH}`,
        visibleOnlyForAdmin: true,

    },
    {
        name: 'Tests',
        icon: <ScienceIcon />,
        path: `${TESTS_PATH}`,
        visibleOnlyForAdmin: false,

    },
    {
        name: 'Problems',
        icon: <NotListedLocationIcon />,
        path: `${PROBLEMS_PATH}`,
        visibleOnlyForAdmin: false,

    },
    {
        name: 'Problem Groups',
        icon: <TableViewIcon />,
        path: `${PROBLEM_GROUPS_PATH}`,
        visibleOnlyForAdmin: false,

    },
    {
        name: 'Problem Resources',
        icon: <GiFiles />,
        path: `${PROBLEM_RESOURCES_PATH}`,
        visibleOnlyForAdmin: false,

    },
    {
        name: 'Submission Types',
        icon: <BorderAllIcon />,
        path: `${SUBMISSION_TYPES_PATH}`,
        visibleOnlyForAdmin: true,

    },
    {
        name: 'Checkers',
        icon: <FaCheckDouble />,
        path: `${CHECKERS_PATH}`,
        visibleOnlyForAdmin: true,

    },
    {
        name: 'Participants',
        icon: <MdOutlineRememberMe />,
        path: `${PARTICIPANTS_PATH}`,
        visibleOnlyForAdmin: false,

    },
    {
        name: 'Roles',
        icon: <MdOutlineAirlineStops />,
        path: `${ROLES_PATH}`,
        visibleOnlyForAdmin: true,

    },
    {
        name: 'Users',
        icon: <FaUsers />,
        path: `${USERS_PATH}`,
        visibleOnlyForAdmin: true,

    },
    {
        name: 'Exam Groups',
        icon: <FaLayerGroup />,
        path: `${EXAM_GROUPS_PATH}`,
        visibleOnlyForAdmin: false,
    },
    {
        name: 'Settings',
        icon: <IoSettingsSharp />,
        path: `${SETTINGS_PATH}`,
        visibleOnlyForAdmin: true,
    },
];
const openedMixin = (theme: Theme): CSSObject => ({
    width: drawerWidth,
    transition: theme.transitions.create('width', {
        easing: theme.transitions.easing.sharp,
        duration: theme.transitions.duration.enteringScreen,
    }),
    overflowX: 'visible',
    overflowY: 'visible',
});

const closedMixin = (theme: Theme): CSSObject => ({
    transition: theme.transitions.create('width', {
        easing: theme.transitions.easing.sharp,
        duration: theme.transitions.duration.leavingScreen,
    }),
    overflowX: 'visible',
    overflowY: 'visible',
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

const Drawer = styled(MuiDrawer, { shouldForwardProp: (prop) => prop !== 'open' })(({ theme, open }) => ({
    width: drawerWidth,
    flexShrink: 0,
    whiteSpace: 'nowrap',
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

const mobileBreak = 1300;

const AdministrationPortal = () => {
    const location = useLocation();

    const user = useAppSelector((x) => x.authorization.internalUser);

    const [ open, setOpen ] = useState(true);
    const [ showMenu, setShowMenu ] = useState<boolean>(false);
    const iconButtonRef = useRef(null);

    useEffect(() => {
        const locationPathnameElements = location.pathname.split('/');
        const lastElementOfThePathname = locationPathnameElements[locationPathnameElements.length - 1];
        let pageTitle = '';
        if (!/^\d+$/.test(lastElementOfThePathname)) {
            const section = administrationItems.find((x) => x.path.split('/').pop() === lastElementOfThePathname);
            pageTitle = capitalizeFirstLetter(section
                ? section.name
                : '');
        } else {
            pageTitle = capitalizeFirstLetter(`${locationPathnameElements[locationPathnameElements.length - 2]}
            Id: ${lastElementOfThePathname}`);
        }

        document.title = `Administration ${pageTitle} - SoftUni Judge`;
    }, [ location.pathname ]);

    useEffect(() => {
        window.addEventListener('resize', handleResize);
        return function cleanUp() {
            window.removeEventListener('resize', handleResize);
        };
    }, []);

    const handleResize = () => {
        setOpen(!(window.innerWidth < mobileBreak));
    };

    const capitalizeFirstLetter = (value: string) => value.charAt(0).toUpperCase() + value.slice(1);

    const isSelected = (pathName: string) => {
        const sectionPathName = location.pathname.split('/');
        if (sectionPathName[sectionPathName.length - 1].toLocaleLowerCase() === pathName.toLocaleLowerCase()) {
            return true;
        }
        return false;
    };

    const handleDrawerOpen = () => {
        setOpen(true);
    };

    const handleDrawerClose = () => {
        setOpen(false);
    };

    const adminRoutes = [
        {
            path: `${CONTESTS_PATH}`,
            Element: AdministrationContestsPage,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${CONTESTS_PATH}/:id`,
            Element: AdministrationContestPage,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${CONTEST_CATEGORIES_PATH}`,
            Element: AdministrationContestCategories,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${SUBMISSIONS_PATH}`,
            Element: AdministrationSubmissionsPage,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${SUBMISSIONS_FOR_PROCESSING_PATH}`,
            Element: AdministrationSubmissionsForProcessingPage,
            visibleOnlyForAdmin: true,
        },
        {
            path: `${SUBMISSIONS_FOR_PROCESSING_PATH}/:id`,
            Element: AdminSubmissionForProcessingDetails,
            visibleOnlyForAdmin: true,
        },
        {
            path: `${TESTS_PATH}`,
            Element: AdministrationTestsPage,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${TESTS_PATH}/:id`,
            Element: AdministrationTest,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${PROBLEMS_PATH}`,
            Element: AdministrationProblemsPage,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${PROBLEMS_PATH}/:id`,
            Element: AdministrationProblem,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${PROBLEM_GROUPS_PATH}`,
            Element: AdministrationProblemGroupsPage,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${PROBLEM_GROUPS_PATH}/:id`,
            Element: AdministrationProblemGroup,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${PROBLEM_RESOURCES_PATH}`,
            Element: AdministrationProblemResourcesPage,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${PROBLEM_RESOURCES_PATH}/:id`,
            Element: AdministrationProblemResource,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${SUBMISSION_TYPES_PATH}`,
            Element: AdministrationSubmissionTypesPage,
            visibleOnlyForAdmin: true,
        },
        {
            path: `${CHECKERS_PATH}`,
            Element: AdministrationCheckersPage,
            visibleOnlyForAdmin: true,
        },
        {
            path: `${PARTICIPANTS_PATH}`,
            Element: ParticipantsAdministrationPage,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${ROLES_PATH}`,
            Element: AdministrationRolesPage,
            visibleOnlyForAdmin: true,
        },
        {
            path: `${ROLES_PATH}/:id`,
            Element: AdministrationRole,
            visibleOnlyForAdmin: true,
        },
        {
            path: `${USERS_PATH}`,
            Element: AdministrationUsersPage,
            visibleOnlyForAdmin: true,
        },
        {
            path: `${USERS_PATH}/:id`,
            Element: AdministrationUser,
            visibleOnlyForAdmin: true,
        },
        {
            path: `${EXAM_GROUPS_PATH}`,
            Element: AdministrationExamGroupsPage,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${EXAM_GROUPS_PATH}/:id`,
            Element: AdministrationExamGroupPage,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${SETTINGS_PATH}`,
            Element: AdministrationSettingsPage,
            visibleOnlyForAdmin: true,
        },
        {

            path: '/administration',
            Element: AdministrationPage,
            title: 'Administration',
        },
    ];

    const renderSectionicon = (name: string, icon: any) => {
        if (!open) {
            return (
                <Tooltip title={name}>
                    <div>{icon}</div>
                </Tooltip>
            );
        }
        return icon;
    };

    return (
        <Box sx={{ zIndex: 0 }}>
            <Box sx={{ display: 'flex', zIndex: 0 }}>
                <Drawer
                  variant="permanent"
                  open={open}
                  sx={{ '& .MuiDrawer-paper': { borderTopRightRadius: '16px', borderBottomRightRadius: '16px' } }}
                  className={styles.drawer}
                >
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
                    <DrawerHeader className={styles.drawerHeader}>
                        <IconButton ref={iconButtonRef} onClick={() => setShowMenu(!showMenu)}>
                            <FaUserCircle className={styles.profileIcon} />
                        </IconButton>
                        {open && (
                            <>
                                <Typography variant="subtitle1" className={styles.userName}>
                                    {user.userName}
                                </Typography>
                                <Divider sx={{ color: 'red', width: '90%' }} />
                            </>
                        )}
                        <Menu
                          anchorEl={iconButtonRef.current}
                          open={showMenu}
                          onClose={() => setShowMenu(false)}
                        >
                            <MenuItem>
                                <Link to="/logout" className={styles.adminHeaderLink}>Sign out</Link>
                            </MenuItem>
                        </Menu>
                    </DrawerHeader>
                    <List sx={{ overflow: 'hidden' }}>
                        <Divider />
                        {administrationItems.map((item) => (user.isAdmin || !item.visibleOnlyForAdmin) && (
                            <Box key={item.path}>
                                <ListItem key={item.name} disablePadding>
                                    <Link
                                      to={item.path}
                                      className={`${isSelected(item.path)
                                          ? styles.activeAdminNavLink
                                          : ''} ${styles.adminNavLink}`}
                                    >
                                        <ListItemButton className={isSelected(item.path)
                                            ? styles.selectedSection
                                            : ''}
                                        >
                                            <ListItemIcon className={isSelected(item.path)
                                                ? styles.listItemIcon
                                                : ''}
                                            >
                                                {renderSectionicon(item.name, item.icon)}
                                            </ListItemIcon>
                                            <ListItemText primary={item.name} />
                                        </ListItemButton>
                                    </Link>
                                </ListItem>
                                <Divider />
                            </Box>
                        ))}
                    </List>
                </Drawer>
                <Box className={styles.main} component="main" sx={{ flexGrow: 1 }}>
                    <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="bg">
                        <Routes>
                            {adminRoutes.map(({ path, Element, visibleOnlyForAdmin }) => {
                                if (user.isAdmin || !visibleOnlyForAdmin) {
                                    return <Route key={path} path={path} element={<Element />} />;
                                }
                                return null;
                            })}
                            <Route path="/" element={<Navigate to={`/${NEW_ADMINISTRATION_PATH}/${CONTESTS_PATH}`} replace />} />
                            <Route path="*" element={<NotFoundPage />} />
                        </Routes>
                    </LocalizationProvider>
                </Box>
            </Box>
        </Box>
    );
};
export default AdministrationPortal;
