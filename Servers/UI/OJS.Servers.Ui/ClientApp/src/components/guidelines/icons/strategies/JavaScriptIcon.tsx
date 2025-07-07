import React, { memo } from 'react';
import { FaJs } from 'react-icons/fa';

import concatClassNames from '../../../../utils/class-names';
import IconSize from '../common/icon-sizes';
import Icon, { IIconProps } from '../Icon';

import styles from './StrategyIcon.module.scss';

type IJavaScriptIconProps = IIconProps

const JavaScriptIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IJavaScriptIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      size={size}
      helperText={helperText}
      Component={FaJs}
    />
;

export default memo(JavaScriptIcon);
