import { useEffect, useState } from 'react';
import { NavigateOptions, URLSearchParamsInit } from 'react-router-dom';
import { IDropdownItem } from 'src/common/types';

import { IContestStrategyFilter } from '../../../common/contest-types';
import { setContestStrategy } from '../../../redux/features/contestsSlice';
import { useGetContestStrategiesQuery } from '../../../redux/services/contestsService';
import { useAppDispatch, useAppSelector } from '../../../redux/store';
import Dropdown from '../../guidelines/dropdown/Dropdown';

import styles from './ContestStrategies.module.scss';

interface IContestStrategiesProps {
    setSearchParams: (newParams: URLSearchParamsInit, navigateOpts?: NavigateOptions) => void;
    searchParams: URLSearchParams;
}

const ContestStrategies = ({ setSearchParams, searchParams }: IContestStrategiesProps) => {
    const dispatch = useAppDispatch();
    const { selectedCategory, selectedStrategy } = useAppSelector((state) => state.contests);
    const [ currentStrategy, setCurrentStrategy ] = useState<IContestStrategyFilter | null>(null);

    const {
        data: contestStrategies,
        isLoading: areStrategiesLoading,
        error: strategiesError,
    } = useGetContestStrategiesQuery({ contestCategoryId: selectedCategory?.id ?? 0 });

    const handleStrategySelect = (item: IDropdownItem | undefined) => {
        if (item) {
            const strategy = contestStrategies?.find((s) => s.id === item.id);
            if (strategy) {
                dispatch(setContestStrategy(strategy));
                setCurrentStrategy(strategy);
                searchParams.set('page', '1');
                setSearchParams(searchParams);
            }
        } else {
            dispatch(setContestStrategy(null));
        }
    };

    // Used to reset the selected strategy when the user goes to another page
    useEffect(() => {
        if (selectedStrategy !== currentStrategy) {
            dispatch(setContestStrategy(currentStrategy));
        }
    }, [ selectedStrategy, currentStrategy, dispatch ]);

    if (strategiesError) {
        return <div>Error loading strategies...</div>;
    }

    if (areStrategiesLoading) {
        return <div>Loading strategies...</div>;
    }

    return (
        <div className={styles.selectWrapper}>
            <Dropdown<IContestStrategyFilter>
              dropdownItems={contestStrategies || []}
              value={currentStrategy ?? { id: 0, name: '' }}
              placeholder="Select strategy"
              noOptionsFoundText="No strategies found"
              handleDropdownItemClick={handleStrategySelect}
              isSearchable
            />
        </div>
    );
};

export default ContestStrategies;
