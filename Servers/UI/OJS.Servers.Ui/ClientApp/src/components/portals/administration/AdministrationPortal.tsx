/* eslint-disable @typescript-eslint/no-use-before-define */

import { ChangeEvent, useEffect, useRef, useState } from 'react';
import { FaCheckDouble, FaLayerGroup, FaUsers } from 'react-icons/fa';
import { GiFiles } from 'react-icons/gi';
import { HiChatBubbleLeft } from 'react-icons/hi2';
import { IoIosLock, IoMdCheckbox } from 'react-icons/io';
import { IoSettingsSharp } from 'react-icons/io5';
import { MdOutlineAirlineStops, MdOutlineRememberMe } from 'react-icons/md';
import { TbBinaryTree } from 'react-icons/tb';
import { Link, Navigate, Route, Routes, useLocation } from 'react-router-dom';
import ArticleIcon from '@mui/icons-material/Article';
import AutoStoriesIcon from '@mui/icons-material/AutoStories';
import BookmarksIcon from '@mui/icons-material/Bookmarks';
import BorderAllIcon from '@mui/icons-material/BorderAll';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import DataSaverOnIcon from '@mui/icons-material/DataSaverOn';
import NotListedLocationIcon from '@mui/icons-material/NotListedLocation';
import PlaylistAddCheckCircleIcon from '@mui/icons-material/PlaylistAddCheckCircle';
import ScienceIcon from '@mui/icons-material/Science';
import SmartToyIcon from '@mui/icons-material/SmartToy';
import TableViewIcon from '@mui/icons-material/TableView';
import {
    Box,
    CssBaseline,
    CSSObject,
    Divider,
    IconButton,
    List,
    ListItem,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    styled,
    Theme,
    Typography,
} from '@mui/material';
import MuiDrawer from '@mui/material/Drawer';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import StyledTooltip from 'src/components/administration/common/styled-tooltip/StyledTooltip';
import UserActions from 'src/components/administration/common/user-actions/UserActions';
import AdministrationAccessLogsPage from 'src/pages/administration-new/access-logs/AdministrationAccessLogsPage';
import AdministrationMentorPromptTemplatesPage
    from 'src/pages/administration-new/mentor-prompt-templates/AdministrationMentorPromptTemplatesPage';
import AdministrationUsersMentorsPage from 'src/pages/administration-new/users-mentors/AdministrationUsersMentorsPage';

import 'dayjs/locale/bg';

import { ThemeMode } from '../../../common/enums';
import {
    ACCESS_LOGS_PATH,
    CHECKERS_PATH,
    CONTEST_CATEGORIES_HIERARCHY_PATH,
    CONTEST_CATEGORIES_PATH,
    CONTESTS_PATH,
    EXAM_GROUPS_PATH, MENTOR_PROMPT_TEMPLATES_PATH,
    NEW_ADMINISTRATION_PATH,
    PARTICIPANTS_PATH,
    PROBLEM_GROUPS_PATH,
    PROBLEM_RESOURCES_PATH,
    PROBLEMS_PATH,
    ROLES_PATH,
    SETTINGS_PATH,
    SUBMISSION_TYPE_DOCUMENTS_PATH,
    SUBMISSION_TYPE_DOCUMENTS_VIEW_PATH,
    SUBMISSION_TYPES_PATH,
    SUBMISSIONS_FOR_PROCESSING_PATH,
    SUBMISSIONS_PATH,
    SUBMISSIONS_SIMILLARITY,
    TESTS_PATH, USERS_MENTORS_PATH,
    USERS_PATH,
} from '../../../common/urls/administration-urls';
import {
    getColors,
    useAdministrationTheme,
} from '../../../hooks/use-administration-theme-provider';
import AdministrationContestCategories
    from '../../../pages/administration-new/contest-categories/AdministrationContestCategories';
import AdministrationContestCategoriesHierarchy
    from '../../../pages/administration-new/contest-categories-hierarchy/AdministrationContestCategoriesHierarchy';
import AdministrationContestsPage from '../../../pages/administration-new/contests/AdministrationContests';
import AdministrationExamGroupsPage from '../../../pages/administration-new/exam-groups/AdministrationExamGroups';
import ParticipantsAdministrationPage
    from '../../../pages/administration-new/participants/ParticipantsAdministrationPage';
import AdministrationProblemGroupsPage
    from '../../../pages/administration-new/problem-groups/AdministrationProblemGroupsPage';
import AdministrationProblemResourcesPage
    from '../../../pages/administration-new/problem-resources/AdministrationProblemResourcesPage';
import AdministrationProblemsPage from '../../../pages/administration-new/problems/AdministrationProblemsPage';
import AdministrationRolesPage from '../../../pages/administration-new/roles/AdministrationRolesPage';
import AdministrationSettingsPage from '../../../pages/administration-new/settings/SettingsAdministrationPage';
import AdministrationSubmissionTypeDocumentsPage
    from '../../../pages/administration-new/submission-type-documents/AdministrationSubmissionTypeDocumentsPage';
import AdministrationSubmissionTypeDocumentViewPage
    from '../../../pages/administration-new/submission-type-documents-view/AdministrationSubmissionTypeDocumentViewPage';
import AdministrationReplaceDeleteSubmissionTypesPage
    from '../../../pages/administration-new/submission-types/AdministrationReplaceDeleteSubmissionTypesPage';
import AdministrationSubmissionTypesPage
    from '../../../pages/administration-new/submission-types/AdministrationSubmissionTypesPage';
import AdministrationSubmissionsPage from '../../../pages/administration-new/submissions/AdministrationSubmissionsPage';
import AdminSubmissionForProcessingDetails
    from '../../../pages/administration-new/submissions-for-processing/AdministrationSubmissionForProcessing';
import AdministrationSubmissionsForProcessingPage
    from '../../../pages/administration-new/submissions-for-processing/AdministrationSubmissionsForProcessingPage';
import SubmissionsSimillarity from '../../../pages/administration-new/submissions-simillarity/SubmissionsSimillarity';
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
import AdministrationSubmissionTypeDocumentPage
    from '../../administration/submission-type-documents/AdministrationSubmissionTypeDocumentPage';
import AdministrationTest from '../../administration/tests/AdministrationTest';
import AdministrationUser from '../../administration/users/AdministrationUser';

import styles from './AdministrationPortal.module.scss';

const drawerWidth = 240;

const administrationItems = [
    {
        name: 'Contests',
        icon: <AutoStoriesIcon className={styles.iconSize} />,
        path: `${CONTESTS_PATH}`,
        visibleOnlyForAdmin: false,
    },
    {
        name: 'Contest Categories',
        icon: <BookmarksIcon className={styles.iconSize} />,
        path: `${CONTEST_CATEGORIES_PATH}`,
        visibleOnlyForAdmin: false,
    },
    {
        name: 'Contest Categories Hierarchy',
        icon: <TbBinaryTree className={styles.iconSize} />,
        path: `${CONTEST_CATEGORIES_HIERARCHY_PATH}`,
        visibleOnlyForAdmin: true,
    },
    {
        name: 'Submissions',
        icon: <PlaylistAddCheckCircleIcon className={styles.iconSize} />,
        path: `${SUBMISSIONS_PATH}`,
        visibleOnlyForAdmin: false,
    },
    {
        name: 'Submissions For Processing',
        icon: <DataSaverOnIcon className={styles.iconSize} />,
        path: `${SUBMISSIONS_FOR_PROCESSING_PATH}`,
        visibleOnlyForAdmin: true,
    },
    {
        name: 'Tests',
        icon: <ScienceIcon className={styles.iconSize} />,
        path: `${TESTS_PATH}`,
        visibleOnlyForAdmin: false,
    },
    {
        name: 'Problems',
        icon: <NotListedLocationIcon className={styles.iconSize} />,
        path: `${PROBLEMS_PATH}`,
        visibleOnlyForAdmin: false,
    },
    {
        name: 'Problem Groups',
        icon: <TableViewIcon className={styles.iconSize} />,
        path: `${PROBLEM_GROUPS_PATH}`,
        visibleOnlyForAdmin: false,
    },
    {
        name: 'Problem Resources',
        icon: <GiFiles className={styles.iconSize} />,
        path: `${PROBLEM_RESOURCES_PATH}`,
        visibleOnlyForAdmin: false,
    },
    {
        name: 'Submission Types',
        icon: <BorderAllIcon className={styles.iconSize} />,
        path: `${SUBMISSION_TYPES_PATH}`,
        visibleOnlyForAdmin: true,
    },
    {
        name: 'Submission Type Documents',
        icon: <ArticleIcon className={styles.iconSize} />,
        path: `${SUBMISSION_TYPE_DOCUMENTS_PATH}`,
        visibleOnlyForAdmin: true,
    },
    {
        name: 'Checkers',
        icon: <FaCheckDouble className={styles.iconSize} />,
        path: `${CHECKERS_PATH}`,
        visibleOnlyForAdmin: true,
    },
    {
        name: 'Participants',
        icon: <MdOutlineRememberMe className={styles.iconSize} />,
        path: `${PARTICIPANTS_PATH}`,
        visibleOnlyForAdmin: false,
    },
    {
        name: 'Roles',
        icon: <MdOutlineAirlineStops className={styles.iconSize} />,
        path: `${ROLES_PATH}`,
        visibleOnlyForAdmin: true,
    },
    {
        name: 'Users',
        icon: <FaUsers className={styles.iconSize} />,
        path: `${USERS_PATH}`,
        visibleOnlyForAdmin: true,
    },
    {
        name: 'Exam Groups',
        icon: <FaLayerGroup className={styles.iconSize} />,
        path: `${EXAM_GROUPS_PATH}`,
        visibleOnlyForAdmin: false,
    },
    {
        name: 'Settings',
        icon: <IoSettingsSharp className={styles.iconSize} />,
        path: `${SETTINGS_PATH}`,
        visibleOnlyForAdmin: true,
    },
    {
        name: 'Submissions Simillarity',
        icon: <IoMdCheckbox className={styles.iconSize} />,
        path: `${SUBMISSIONS_SIMILLARITY}`,
        visibleOnlyForAdmin: false,
    },
    {
        name: 'Mentor',
        icon: <SmartToyIcon className={styles.iconSize} />,
        path: `${USERS_MENTORS_PATH}`,
        visibleOnlyForAdmin: true,
    },
    {
        name: 'Mentor Prompt Templates',
        icon: <HiChatBubbleLeft className={styles.iconSize} />,
        path: `${MENTOR_PROMPT_TEMPLATES_PATH}`,
        visibleOnlyForAdmin: true,
    },
    {
        name: 'Access Logs',
        icon: <IoIosLock className={styles.iconSize} />,
        path: `${ACCESS_LOGS_PATH}`,
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
    ...theme.mixins.toolbar,
}));

const Drawer = styled(MuiDrawer, { shouldForwardProp: (prop) => prop !== 'open' })(({ theme, open }) => ({
    width: drawerWidth,
    flexShrink: 0,
    whiteSpace: 'nowrap',
    boxSizing: 'border-box',
    ...open && {
        ...openedMixin(theme),
        '& .MuiDrawer-docked': openedMixin(theme),
        '& .MuiDrawer-paper': openedMixin(theme),
    },
    ...!open && {
        ...closedMixin(theme),
        '& .MuiDrawer-docked': closedMixin(theme),
        '& .MuiDrawer-paper': closedMixin(theme),
    },
}));

const mobileBreak = 1300;

const AdministrationPortal = () => {
    const location = useLocation();
    const { toggleThemeMode, themeMode } = useAdministrationTheme();

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
            pageTitle =
                capitalizeFirstLetter(`${locationPathnameElements[locationPathnameElements.length - 2]} Id: ${lastElementOfThePathname}`);
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
        const isDesktop = window.innerWidth >= mobileBreak;
        setOpen(isDesktop);

        if (isDesktop) {
            setShowMenu(false);
        }
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

    const handleThemeChange = (event: ChangeEvent<HTMLInputElement>) => {
        const isChecked = event.target.checked;
        const newThemeMode = isChecked
            ? ThemeMode.Light
            : ThemeMode.Dark;

        toggleThemeMode(newThemeMode);
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
            path: `${CONTEST_CATEGORIES_HIERARCHY_PATH}`,
            Element: AdministrationContestCategoriesHierarchy,
            visibleOnlyForAdmin: true,
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
            path: `${SUBMISSION_TYPE_DOCUMENTS_PATH}`,
            Element: AdministrationSubmissionTypeDocumentsPage,
            visibleOnlyForAdmin: true,
        },
        {
            path: `${SUBMISSION_TYPE_DOCUMENTS_PATH}/:id`,
            Element: AdministrationSubmissionTypeDocumentPage,
            visibleOnlyForAdmin: true,
        },
        {
            path: `${SUBMISSION_TYPE_DOCUMENTS_VIEW_PATH}`,
            Element: AdministrationSubmissionTypeDocumentViewPage,
            visibleOnlyForAdmin: true,
        },
        {
            path: `${SUBMISSION_TYPES_PATH}/deleteReplaceSubmissionTypes`,
            Element: AdministrationReplaceDeleteSubmissionTypesPage,
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
            path: `${SUBMISSIONS_SIMILLARITY}`,
            Element: SubmissionsSimillarity,
            visibleOnlyForAdmin: false,
        },
        {
            path: `${USERS_MENTORS_PATH}`,
            Element: AdministrationUsersMentorsPage,
            visibleOnlyForAdmin: true,
        },
        {
            path: `${MENTOR_PROMPT_TEMPLATES_PATH}`,
            Element: AdministrationMentorPromptTemplatesPage,
            visibleOnlyForAdmin: true,
        },
        {
            path: `${ACCESS_LOGS_PATH}`,
            Element: AdministrationAccessLogsPage,
            visibleOnlyForAdmin: true,
        },
    ];

    return (
        <>
            <CssBaseline />
            <Box sx={{
                display: 'flex',
                overflowY: 'hidden',
                '&::-webkit-scrollbar': { display: 'none' },
                msOverflowStyle: 'none',
                scrollbarWidth: 'none',
            }}
            >
                <Drawer
                  variant="permanent"
                  open={open}
                >
                    {!open
                        ? <IconButton
                              sx={{ backgroundColor: getColors(themeMode).palette.secondary.main }}
                              className={`${styles.arrowRight} ${styles.arrowCommon}`}
                              color="primary"
                              onClick={handleDrawerOpen}
                            >
                            <ChevronRightIcon />
                        </IconButton>
                        
                        : <IconButton
                              className={`${styles.arrow} ${styles.arrowCommon}`}
                              sx={{ backgroundColor: getColors(themeMode).palette.secondary.main }}
                              color="primary"
                              onClick={handleDrawerClose}
                            >
                            <ChevronLeftIcon />
                        </IconButton>
                        }
                    <DrawerHeader className={styles.drawerHeader}>
                        <UserActions
                          isDropdown={!open}
                          handleThemeChange={handleThemeChange}
                          themeMode={themeMode}
                          showMenu={showMenu}
                          setShowMenu={setShowMenu}
                          iconButtonRef={iconButtonRef}
                        />
                        <DrawerHeader className={styles.drawerHeader}>
                            <UserActions
                              isDropdown={!open}
                              handleThemeChange={handleThemeChange}
                              themeMode={themeMode}
                              showMenu={showMenu}
                              setShowMenu={setShowMenu}
                              iconButtonRef={iconButtonRef}
                            />
                            <DrawerHeader className={styles.username}>
                                {open && 
                                    <>
                                        <Typography variant="subtitle1">
                                            {user.userName}
                                        </Typography>
                                        <Divider sx={{ color: 'red', width: '90%' }} />
                                    </>
                                }
                            </DrawerHeader>
                        </DrawerHeader>
                    </DrawerHeader>
                    <List className={styles.list}>
                        <Divider />
                        {administrationItems.map((item) => (user.isAdmin || !item.visibleOnlyForAdmin) && 
                            <StyledTooltip
                              key={item.path}
                              title={item.name}
                              placement="right"
                              arrow
                            >
                                <div>
                                    <ListItem key={item.name} disablePadding>
                                        <Link
                                          to={item.path}
                                          className={`${isSelected(item.path)
                                              ? styles.activeAdminNavLink
                                              : ''} ${
                                              styles.adminNavLink
                                          }`}
                                        >
                                            <ListItemButton
                                              className={isSelected(item.path)
                                                  ? styles.selectedSection
                                                  : ''}
                                            >
                                                <ListItemIcon
                                                  className={isSelected(item.path)
                                                      ? styles.listItemIcon
                                                      : ''}
                                                >
                                                    {item.icon}
                                                </ListItemIcon>
                                                <ListItemText primary={item.name} />
                                            </ListItemButton>
                                        </Link>
                                    </ListItem>
                                    <Divider />
                                </div>
                            </StyledTooltip>)}
                    </List>
                </Drawer>
                <Box
                  className={styles.main}
                  component="main"
                  sx={{
                      flexGrow: 1,
                      overflowY: 'scroll',
                      '&::-webkit-scrollbar': { display: 'none' },
                      msOverflowStyle: 'none',
                      scrollbarWidth: 'none',
                  }}
                >
                    <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="bg">
                        <Routes>
                            {adminRoutes.map(({ path, Element, visibleOnlyForAdmin }) => {
                                if (user.isAdmin || !visibleOnlyForAdmin) {
                                    return <Route key={path} path={path} element={<Element />} />;
                                }
                                return null;
                            })}
                            <Route
                              path="/"
                              element={(
                                  <Navigate
                                    to={`/${NEW_ADMINISTRATION_PATH}/${CONTESTS_PATH}`}
                                    replace
                                  />
                                )}
                            />
                            <Route path="*" element={<NotFoundPage />} />
                        </Routes>
                    </LocalizationProvider>
                </Box>
            </Box>
        </>
    );
};

export default AdministrationPortal;
