import { setContests, setContestsCacheIsReset } from './features/contestsSlice';
import { useAppDispatch } from './store';

interface ICacheServiceContext {
    resetCache: () => void;
}

const CacheService = (): ICacheServiceContext => {
    const dispatch = useAppDispatch();

    const resetContests = () => {
        dispatch(setContests(null));
        dispatch(setContestsCacheIsReset(true));
    };

    const resetCache = () => {
        resetContests();
    };

    return { resetCache };
};

export default CacheService;
