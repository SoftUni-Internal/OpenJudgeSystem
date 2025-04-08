import React, { useCallback, useState } from 'react';
import { GoGear } from 'react-icons/go';
import { IoIosArrowDown } from 'react-icons/io';
import isNilOrEmpty from 'src/utils/check-utils';

import {
    CONTEST_CATEGORIES_HIERARCHY_PATH,
    CONTEST_CATEGORIES_PATH,
    CONTESTS_PATH,
    EXAM_GROUPS_PATH,
    NEW_ADMINISTRATION_PATH,
    PARTICIPANTS_PATH,
    PROBLEM_GROUPS_PATH,
    PROBLEMS_PATH,
    ROLES_PATH,
    SUBMISSION_TYPES_PATH,
    SUBMISSIONS_PATH,
    TESTS_PATH,
    USERS_PATH,
} from '../../common/urls/administration-urls';
import Button, { ButtonType } from '../../components/guidelines/buttons/Button';
import useNavigation from '../../hooks/common/use-routing';
import useTheme from '../../hooks/use-theme';
import { useAppSelector } from '../../redux/store';
import concatClassNames from '../../utils/class-names';

import styles from './AdministrationMenu.module.scss';

enum AdministrationMenuButtonType {
    gear = 0,
    text = 1,
}

interface IAdministrationMenuItem {
    text: string;
    link: string;
}

interface IAdministrationMenuProps {
    buttonType: AdministrationMenuButtonType;
    /* If no value is provided, default full menu items is rendered based on user role
    *  If items is array of arrays, menu items are rendered in sections */
    items?: IAdministrationMenuItem[] | IAdministrationMenuItem[][];
    /* Adds margin to align with header */
    isUsedInPageHeader?: boolean;
}

const AdministrationMenu = ({ buttonType, items, isUsedInPageHeader = false }: IAdministrationMenuProps) => {
    const [ isMenuVisible, setMenuVisible ] = useState(false);
    const { navigateInNewWindow } = useNavigation();
    const { internalUser: user } = useAppSelector((state) => state.authorization);

    const { isDarkMode, themeColors, getColorClassName } = useTheme();
    const backgroundColorClassname = getColorClassName(isDarkMode
        ? themeColors.baseColor200
        : themeColors.baseColor100);

    const menuSectionClassName = concatClassNames(
        styles.menuSection,
        isDarkMode
            ? styles.darkMenuSection
            : styles.lightMenuSection,
    );

    const handleMouseEnter = () => {
        setMenuVisible(true);
    };

    const handleMouseLeave = () => {
        setMenuVisible(false);
    };

    const onClickNavigate = useCallback(
        (administrationEntity: string) => navigateInNewWindow(`/${NEW_ADMINISTRATION_PATH}/${administrationEntity}`),
        [ navigateInNewWindow ],
    );

    const renderDefaultMenuItems = useCallback(
        () => 
            <>
                <div className={menuSectionClassName}>
                    <span onClick={() => onClickNavigate(CONTESTS_PATH)}>Contests</span>
                    <span onClick={() => onClickNavigate(EXAM_GROUPS_PATH)}>Exam Groups</span>
                    <span onClick={() => onClickNavigate(SUBMISSIONS_PATH)}>Submissions</span>
                    {user.isAdmin && 
                    <span onClick={() => onClickNavigate(CONTEST_CATEGORIES_PATH)}>Categories</span>}
                    {user.isAdmin && 
                    <span onClick={() => onClickNavigate(CONTEST_CATEGORIES_HIERARCHY_PATH)}>Categories Hierarchy</span>}
                    {user.isAdmin && 
                    <span onClick={() => onClickNavigate(PARTICIPANTS_PATH)}>Participants</span>}
                </div>
                {user.isAdmin && 
                <>
                    <div className={menuSectionClassName}>
                        <span onClick={() => onClickNavigate(PROBLEMS_PATH)}>Problems</span>
                        <span onClick={() => onClickNavigate(PROBLEM_GROUPS_PATH)}>Problem Groups</span>
                        <span onClick={() => onClickNavigate(TESTS_PATH)}>Tests</span>
                        <span onClick={() => onClickNavigate(SUBMISSION_TYPES_PATH)}>Submission Types</span>
                    </div>
                    <div className={menuSectionClassName}>
                        <span onClick={() => onClickNavigate(USERS_PATH)}>Users</span>
                        <span onClick={() => onClickNavigate(ROLES_PATH)}>Roles</span>
                        {/* TODO */}
                        {/* <span onClick={() => onClickNavigate(TESTS_PATH)}>
                                Lecturers in contests and categories</span> */}
                    </div>
                    <div className={menuSectionClassName}>
                        <span onClick={() => onClickNavigate('')}>All Administrations</span>
                    </div>
                </>
            }
            </>
        , [ menuSectionClassName, onClickNavigate, user ],
    );

    return (
        <div
          className={styles.adminMenuContainer}
          onMouseOver={handleMouseEnter}
          onMouseOut={handleMouseLeave}
          onBlur={() => {}}
          onFocus={() => {}}
        >
            <Button
              onClick={() => {
                  setMenuVisible(false);
                  navigateInNewWindow(`/${NEW_ADMINISTRATION_PATH}`);
              }}
              type={ButtonType.plain}
              internalClassName={styles.menuButton}
            >
                {buttonType === AdministrationMenuButtonType.text
                    ? <>
                        Administration
                        <IoIosArrowDown />
                    </>
                    
                    : <GoGear />}
            </Button>

            {/* Transparent spacer to cover the gap */}
            {isMenuVisible && <div className={styles.spacer} />}

            {isMenuVisible && 
            <div
              className={concatClassNames(
                  styles.dropdownMenu,
                  isUsedInPageHeader
                      ? styles.dropdownMenuInHeader
                      : '',
                  getColorClassName(themeColors.textColor),
                  backgroundColorClassname,
              )}
            >
                { isNilOrEmpty(items)
                    ? renderDefaultMenuItems()
                    : Array.isArray(items?.[0])
                    // Render sections if items is an array of arrays
                        ? (items as IAdministrationMenuItem[][]).map((section, sectionIndex) => 
                            <div key={sectionIndex} className={menuSectionClassName}>
                                {section.map((menuItem, itemIndex) => 
                                    <span key={itemIndex} onClick={() => onClickNavigate(menuItem.link)}>
                                        {menuItem.text}
                                    </span>)}
                            </div>)
                    // Render individual spans if items is a flat array or not provided
                        : <div key={1} className={menuSectionClassName}>
                            {(items as IAdministrationMenuItem[] || []).map((menuItem, itemIndex) => 
                                <span key={itemIndex} onClick={() => onClickNavigate(menuItem.link)}>
                                    {menuItem.text}
                                </span>)}
                        </div>
                        }
            </div>
            }
        </div>
    );
};

export default AdministrationMenu;

export { AdministrationMenuButtonType };
