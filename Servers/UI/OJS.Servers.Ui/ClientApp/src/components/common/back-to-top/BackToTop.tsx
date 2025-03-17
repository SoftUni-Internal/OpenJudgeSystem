import { useEffect, useState } from 'react';

import useTheme from '../../../hooks/use-theme';

import styles from './BackToTop.module.scss';

interface IBackToTopProps {
    rightPosition?: number;
}

const BackToTop = ({ rightPosition = 20 }: IBackToTopProps) => {
    const [ isVisible, setIsVisible ] = useState(false);
    const { isDarkMode } = useTheme();

    const toggleVisibility = () => {
        const scrolled = document.documentElement.scrollTop || document.body.scrollTop;
        if (scrolled > 200) {
            setIsVisible(true);
        } else {
            setIsVisible(false);
        }
    };

    const scrollToTop = () => {
        window.scrollTo({
            top: 0,
            behavior: 'smooth',
        });
    };

    useEffect(() => {
        window.addEventListener('scroll', toggleVisibility);
        toggleVisibility();

        return () => {
            window.removeEventListener('scroll', toggleVisibility);
        };
    }, []);

    if (!isVisible) { return null; }

    return (
        <button
          id="back-to-top"
          type="button"
          style={{ right: `${rightPosition}px` }}
          className={`${styles.backToTop} ${isDarkMode
              ? styles.darkTheme
              : styles.lightTheme}`}
          onClick={scrollToTop}
          aria-label="Scroll to top"
        >
            <svg
              width="24"
              height="24"
              viewBox="0 0 24 24"
              fill="none"
              stroke="white"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
                <path d="M12 19V5M5 12l7-7 7 7" />
            </svg>
        </button>
    );
};

export default BackToTop;
