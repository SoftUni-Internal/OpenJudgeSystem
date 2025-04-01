import { IDropdownItem } from 'src/common/types';
import { setContestStrategy } from 'src/redux/features/contestsSlice';
import { useDropdownUrlSync } from 'src/utils/url-utils';

import { IContestStrategyFilter } from '../../../common/contest-types';
import { useGetContestStrategiesQuery } from '../../../redux/services/contestsService';
import { useAppDispatch, useAppSelector } from '../../../redux/store';
import Dropdown from '../../guidelines/dropdown/Dropdown';

import styles from './ContestStrategies.module.scss';

const ContestStrategies = () => {
    const { selectedCategory } = useAppSelector((state) => state.contests);
    const { selectedStrategy } = useAppSelector((state) => state.contests);
    const dispatch = useAppDispatch();

    const {
        data: contestStrategies,
        isLoading: areStrategiesLoading,
        error: strategiesError,
    } = useGetContestStrategiesQuery({ contestCategoryId: selectedCategory?.id ?? 0 });

    const { updateUrl } = useDropdownUrlSync<IContestStrategyFilter>(
        'strategy',
        contestStrategies,
        setContestStrategy,
    );

    const handleStrategySelect = (item: IDropdownItem | undefined) => {
        const strategy = item
            ? contestStrategies?.find((s) => s.id === item.id)
            : undefined;

        dispatch(setContestStrategy(strategy ?? null));
        updateUrl(strategy);
    };

    const handleStrategyClear = () => {
        dispatch(setContestStrategy(null));
        updateUrl(undefined);
    };

    if (strategiesError) { return <div>Error loading strategies...</div>; }
    if (areStrategiesLoading) { return <div>Loading strategies...</div>; }

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
