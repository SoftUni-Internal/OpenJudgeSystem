/* eslint-disable jsx-a11y/click-events-have-key-events */
/* eslint-disable jsx-a11y/no-static-element-interactions */
/* eslint-disable jsx-a11y/click-events-have-key-events */

import { useEffect } from 'react';
import { useParams } from 'react-router';
import { Link, useLocation, useSearchParams } from 'react-router-dom';

import { ContestBreadcrumb } from '../../../common/contest-types';
import { getAllContestsUrl } from '../../../common/urls/compose-client-urls';
import useTheme from '../../../hooks/use-theme';
import {
    setContestCategories,
    updateContestCategoryBreadcrumbItem,
} from '../../../redux/features/contestsSlice';
import { useGetContestCategoriesQuery } from '../../../redux/services/contestsService';
import { useAppDispatch, useAppSelector } from '../../../redux/store';
import { findContestCategoryByIdRecursive, findParentNames } from '../contest-categories/ContestCetegories';

import styles from './ContestBreadcrumbs.module.scss';

const ContestBreadcrumbs = () => {
    const dispatch = useAppDispatch();
    const { pathname } = useLocation();
    const { contestId } = useParams();
    const [ searchParams, setSearchParams ] = useSearchParams();
    const { themeColors, getColorClassName } = useTheme();
    const { breadcrumbItems, contestCategories, contestDetails } = useAppSelector((state) => state.contests);
    const { data, isLoading, refetch } = useGetContestCategoriesQuery();

    const textColorClassName = getColorClassName(themeColors.textColor);
    const backgroundColorClassName = getColorClassName(themeColors.baseColor500);

    // fetch contests data, if it's not present beforehand
    useEffect(() => {
        if (contestCategories?.length === 0) {
            refetch();
        }
        /* eslint-disable-next-line react-hooks/exhaustive-deps */
    }, []);

    useEffect(() => {
        dispatch(setContestCategories({ contestCategories: data || [] }));
    }, [ data, dispatch ]);

    // set selected category when loading from specific url and no data is present beforehand
    useEffect(() => {
        // contestId should be part of the check only when
        // on contests page, otherwise causes endless recursion
        if (pathname.includes('/contest')
            ? contestId
            : breadcrumbItems.length === 0 && contestDetails) {
            // eslint-disable-next-line @typescript-eslint/no-non-null-asserted-optional-chain
            const selectedCategory = findContestCategoryByIdRecursive(contestCategories, contestDetails?.categoryId!);
            if (selectedCategory) {
                const selectedCategoryBreadcrumbItems = findParentNames(contestCategories, selectedCategory.id);

                dispatch(updateContestCategoryBreadcrumbItem({ elements: selectedCategoryBreadcrumbItems }));
            }
        }
    }, [ contestCategories, breadcrumbItems.length, contestDetails, pathname, contestId, dispatch ]);

    const renderBreadcrumbItems = (breadcrumbItem: ContestBreadcrumb, isLast: boolean) => (
        <Link
          key={`contest-breadcrumb-item-${breadcrumbItem.id}`}
          to={getAllContestsUrl(breadcrumbItem.id)}
        >
            <div
              onClick={() => {
                  searchParams.set('category', breadcrumbItem.id.toString());
                  setSearchParams(searchParams);
              }}
              className={`${styles.item} ${isLast
                  ? textColorClassName
                  : ''}`}
            >
                <div>
                    {breadcrumbItem.name}
                    {' '}
                    {!isLast && '/'}
                </div>
            </div>
        </Link>
    );

    if (isLoading) {
        return <div className={getColorClassName(themeColors.textColor)}>Loading breadcrumbs...</div>;
    }

    if (breadcrumbItems.length === 0) {
        return null;
    }

    return (
        <div className={`${styles.breadcrumbsWrapper} ${textColorClassName} ${backgroundColorClassName}`}>
            {breadcrumbItems
                .map((item: ContestBreadcrumb, idx: number) => renderBreadcrumbItems(item, idx === breadcrumbItems.length - 1))}
        </div>
    );
};

export default ContestBreadcrumbs;
