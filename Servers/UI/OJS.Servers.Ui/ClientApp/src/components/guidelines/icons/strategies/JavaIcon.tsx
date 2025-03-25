import React, { memo } from 'react';
import { FaJava } from 'react-icons/fa';

import concatClassNames from '../../../../utils/class-names';
import IconSize from '../common/icon-sizes';
import Icon, { IIconProps } from '../Icon';

import styles from './StrategyIcon.module.scss';

type IJavaIconProps = IIconProps

const JavaIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IJavaIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      size={size}
      helperText={helperText}
      Component={FaJava}
    />
;

export default memo(JavaIcon);
