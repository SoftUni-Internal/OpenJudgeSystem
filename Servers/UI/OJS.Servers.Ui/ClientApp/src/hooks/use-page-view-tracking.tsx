import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';

const usePageViewTracking = () => {
    const location = useLocation();
    const GA_ID = import.meta.env.VITE_GA_ID;

    useEffect(() => {
        if (!GA_ID || import.meta.env.DEV) {
            return;
        }

        if (window.gtag) {
            window.gtag(
                'config', GA_ID,
                { page_path: location.pathname + location.search },
            );
        }
    }, [ GA_ID, location ]);
};

export default usePageViewTracking;
