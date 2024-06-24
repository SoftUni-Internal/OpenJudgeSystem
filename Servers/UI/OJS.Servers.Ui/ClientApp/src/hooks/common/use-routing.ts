import { useCallback } from 'react';

const useNavigation = () => {
    const navigateInNewWindow = useCallback((url: string) => {
        window.open(`${window.location.origin}${url}`);
    }, []);

    return { navigateInNewWindow };
};

export default useNavigation;
