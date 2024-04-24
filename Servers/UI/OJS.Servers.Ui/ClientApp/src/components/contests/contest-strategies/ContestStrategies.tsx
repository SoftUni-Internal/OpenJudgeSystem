import { useEffect, useMemo, useState } from 'react';
import { IoMdClose } from 'react-icons/io';
import { useSearchParams } from 'react-router-dom';

import { IContestStrategyFilter } from '../../../common/contest-types';
import { setContestStrategy } from '../../../redux/features/contestsSlice';
import { useGetContestStrategiesQuery } from '../../../redux/services/contestsService';
import { useAppDispatch, useAppSelector } from '../../../redux/store';
import Dropdown from '../../dropdown/Dropdown';

import styles from './ContestStrategies.module.scss';

const ContestStrategies = () => {
    const dispatch = useAppDispatch();
    const [ searchParams ] = useSearchParams();
    const { selectedStrategy, selectedCategory } = useAppSelector((state) => state.contests);
    const [ selectValue, setSelectValue ] = useState<string>('');

    const selectedId = useMemo(() => searchParams.get('strategy'), [ searchParams ]);

    const {
        data: contestStrategies,
        isLoading: areStrategiesLoading,
        error: strategiesError,
    } = useGetContestStrategiesQuery();

    useEffect(() => {
        if (!selectedStrategy) {
            setSelectValue('');
        }
    }, [ selectedStrategy ]);

    useEffect(() => {
        if (selectedId) {
            setSelectValue(selectedId);
        } else {
            setSelectValue('');
        }
    }, [ selectedId ]);

    const mapDataToDropdownItem = (el: IContestStrategyFilter) => ({ id: el.id.toString(), name: el.name });

    const dropdownItems = useMemo(
        () => !selectedCategory || selectedCategory?.allowedStrategyTypes?.length === 0
            ? (contestStrategies || []).map(mapDataToDropdownItem)
            : selectedCategory?.allowedStrategyTypes.map(mapDataToDropdownItem),
        [ contestStrategies, selectedCategory ],
    );

    const removeSelectedStrategy = () => {
        dispatch(setContestStrategy(null));
    };

    const handleStrategySelect = (s: IContestStrategyFilter) => {
        dispatch(setContestStrategy(s));
        setSelectValue(s.id.toString());
    };

    if (strategiesError) { return <div>Error loading strategies...</div>; }

    if (areStrategiesLoading) { return <div>Loading strategies...</div>; }

    return (
        <div className={styles.selectWrapper}>
            { selectedStrategy && <IoMdClose onClick={removeSelectedStrategy} />}
            <Dropdown dropdownItems={dropdownItems || []} value={selectValue} handleDropdownItemClick={handleStrategySelect} />
        </div>
    );
};

export default ContestStrategies;
