import { NavigateOptions, URLSearchParamsInit, useSearchParams } from 'react-router-dom';

const usePreserveScrollOnSearchParamsChange = () => {
    const [ searchParams, updateSearchParams ] = useSearchParams();

    const setSearchParams = (newParams: URLSearchParamsInit, navigateOpts?: NavigateOptions) => {
        updateSearchParams(newParams, { preventScrollReset: true, ...navigateOpts });
    };

    return { searchParams, setSearchParams };
};

export default usePreserveScrollOnSearchParamsChange;
