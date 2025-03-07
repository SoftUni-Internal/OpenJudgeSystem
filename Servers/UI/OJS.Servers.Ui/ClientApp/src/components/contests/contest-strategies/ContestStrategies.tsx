import { useEffect, useMemo } from 'react';
import { NavigateOptions, URLSearchParamsInit } from 'react-router-dom';
import { IDropdownItem } from 'src/common/types';
import { setContestStrategy } from 'src/redux/features/contestsSlice';

import { IContestStrategyFilter } from '../../../common/contest-types';
import { useGetContestStrategiesQuery } from '../../../redux/services/contestsService';
import { useAppDispatch, useAppSelector } from '../../../redux/store';
import Dropdown from '../../guidelines/dropdown/Dropdown';

import styles from './ContestStrategies.module.scss';

interface IContestStrategiesProps {
    setSearchParams: (newParams: URLSearchParamsInit, navigateOpts?: NavigateOptions) => void;
    searchParams: URLSearchParams;
}

const ContestStrategies = ({ setSearchParams, searchParams }: IContestStrategiesProps) => {
    const { selectedCategory } = useAppSelector((state) => state.contests);
    const dispatch = useAppDispatch();
    const { selectedStrategy } = useAppSelector((state) => state.contests);
    const selectedId = useMemo(() => searchParams.get('strategy'), [ searchParams ]);

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
                searchParams.set('page', '1');
                setSearchParams(searchParams);
            }
        } else {
            dispatch(setContestStrategy(null));
        }
    };

    const handleStrategyClear = () => {
        dispatch(setContestStrategy(null));
        searchParams.set('page', '1');
        setSearchParams(searchParams);
    };

    useEffect(() => {
        if (selectedId && contestStrategies) {
            const selected = contestStrategies.find((s) => s.id.toString() === selectedId);

            if (selected) {
                dispatch(setContestStrategy(selected));
                searchParams.set('page', '1');
                setSearchParams(searchParams);
            } else {
                dispatch(setContestStrategy(null));
            }
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [ selectedId, contestStrategies, dispatch ]);

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
              value={selectedStrategy ?? { id: 0, name: '' }}
              placeholder="Select strategy"
              noOptionsFoundText="No strategies found"
              handleDropdownItemClick={handleStrategySelect}
              handleDropdownItemClear={handleStrategyClear}
              isSearchable
            />
        </div>
    );
};

export default ContestStrategies;
