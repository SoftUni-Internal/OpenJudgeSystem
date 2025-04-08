import { useState } from 'react';
import { FaChevronDown, FaChevronUp } from 'react-icons/fa';

import concatClassNames from '../../utils/class-names';
import Button, { ButtonType } from '../guidelines/buttons/Button';

import styles from './MultiLineTextDisplay.module.scss';

interface IMultiLineTextProps {
    text: string | null | undefined;
    className?: string;
    maxVisibleLines?: number;
}

const MultiLineTextDisplay = ({ text, className, maxVisibleLines = 1000 } : IMultiLineTextProps) => {
    const [ showAll, setShowAll ] = useState(false);

    if (!text) {
        return null;
    }

    const lines = text.split('\n');
    const shouldShowReadMore = lines.length > maxVisibleLines;
    const visibleLines = showAll
        ? lines
        : lines.slice(0, maxVisibleLines);

    const internalClassName = concatClassNames(styles.multiLineTextContainer, className);

    return (
        <div className={internalClassName}>
            {visibleLines.map((line: string, index: number) => 
                <p key={index} className={styles.multiLineTextParagraph}>{line}</p>)}
            {shouldShowReadMore && 
                <Button onClick={() => setShowAll(!showAll)} className={styles.readMoreButton} type={ButtonType.plain}>
                    {showAll
                        ? 'Show less'
                        : 'Read more'}
                    {showAll
                        ? <FaChevronUp className={styles.iconAdjustment} />
                        : <FaChevronDown className={styles.iconAdjustment} />}
                </Button>
            }
        </div>
    );
};

export default MultiLineTextDisplay;
