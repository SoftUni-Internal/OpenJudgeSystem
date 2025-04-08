import React from 'react';

import { flexCenterObjectStyles } from '../../../utils/object-utils';
import ErrorIcon from '../../guidelines/icons/ErrorIcon';

import styles from './ErrorResult.module.scss';

const ErrorResult = () => 
    <div style={{ ...flexCenterObjectStyles }}>
        <ErrorIcon />
        <span className={styles.compileAndUnknownError}>
            {' '}
            Compile time error
        </span>
    </div>
;

export default ErrorResult;
