/* eslint-disable jsx-a11y/click-events-have-key-events */
/* eslint-disable jsx-a11y/no-static-element-interactions */
/* eslint-disable jsx-a11y/click-events-have-key-events */

import { useEffect, useMemo } from 'react';
import { useLocation, useParams } from 'react-router-dom';
import { IContestsSolutionSubmitPageUrlParams } from 'src/common/app-url-types';
import { ContestParticipationType } from 'src/common/constants';
import { CONTESTS_PATH } from 'src/common/urls/client-urls';
import Breadcrumbs, { IPageBreadcrumbsItem } from 'src/components/guidelines/breadcrumb/Breadcrumbs';

import { ContestBreadcrumb } from '../../../common/contest-types';
import { getAllContestsPageUrl, getContestsSolutionSubmitPageUrl } from '../../../common/urls/compose-client-urls';
import {
    clearContestCategoryBreadcrumbItems,
    setContestCategories,
    updateContestCategoryBreadcrumbItem,
} from '../../../redux/features/contestsSlice';
import { useGetContestCategoriesQuery } from '../../../redux/services/contestsService';
import { useAppDispatch, useAppSelector } from '../../../redux/store';
import trimBreadcrumbItems from '../../../utils/breadcrumb-utils';
import { findContestCategoryByIdRecursive, findParentNames } from '../contest-categories/ContestCategories';

interface IContestBreadcrumbsProps {
    isHidden?: boolean;
}

const ContestBreadcrumbs = ({ isHidden = false }: IContestBreadcrumbsProps) => {
    const dispatch = useAppDispatch();
    const location = useLocation();
    const { categoryId } = useParams();
    const {
        breadcrumbItems,
        contestCategories,
        contestDetails,
    } = useAppSelector((state) => state.contests);

    const { data, isLoading, refetch } = useGetContestCategoriesQuery();

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
        // contests page have directly category id in the url
        // if we make request for it, we go into recursion
        const selectedCategoryId = categoryId || contestDetails?.categoryId;

        if (!selectedCategoryId) {
            dispatch(clearContestCategoryBreadcrumbItems());
            return;
        }

        const selectedCategory = findContestCategoryByIdRecursive(contestCategories, Number(selectedCategoryId));
        if (selectedCategory) {
            const selectedCategoryBreadcrumbItems = findParentNames(contestCategories, selectedCategory.id);

            dispatch(updateContestCategoryBreadcrumbItem({ elements: selectedCategoryBreadcrumbItems }));
        }
    }, [ contestCategories, contestDetails, categoryId, dispatch ]);

    const contestBreadCrumbsItems = useMemo(() => {
        let items = [
            // Initial value
            { text: 'Contests', to: '/contests' } as IPageBreadcrumbsItem,
        ];

        // Concat categories tree
        items = items
            .concat(trimBreadcrumbItems(breadcrumbItems)?.map((item: ContestBreadcrumb) => ({
                text: item.name,
                to: getAllContestsPageUrl({
                    categoryId: item.id,
                    categoryName: item.name,
                }),
            } as IPageBreadcrumbsItem)));

        const isResultsPage = () => location.pathname.toLowerCase().includes(`/${CONTESTS_PATH.toLowerCase()}`) &&
            location.pathname.toLowerCase().includes('/results');

        if (contestDetails && !isResultsPage()) {
            // For pages that populate contestDetails (submit/contest details/submission details),
            // the contest name is appended as last element and its not an active link
            items = items.concat({ text: contestDetails.name } as IPageBreadcrumbsItem);
        }

        if (contestDetails && isResultsPage()) {
            const isCompete = location.pathname
                .toLowerCase()
                .includes(`/${ContestParticipationType.Compete.toLowerCase()}`);

            // Results page appends contest name as link
            items = items.concat([
                {
                    text: contestDetails.name,
                    to: getContestsSolutionSubmitPageUrl({
                        isCompete,
                        contestId: contestDetails!.id,
                        contestName: contestDetails!.name,
                    } as IContestsSolutionSubmitPageUrlParams),
                },
                // Results page appends 'Results' in the end
                { text: 'Results' },
            ] as IPageBreadcrumbsItem[]);
        }

        return items;
    }, [ breadcrumbItems, contestDetails, location.pathname ]);

    return (
        <Breadcrumbs
          keyPrefix="contest"
          items={contestBreadCrumbsItems}
          isHidden={isHidden}
          isLoading={isLoading}
        />
    );
};

export default ContestBreadcrumbs;
