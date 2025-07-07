import React, { memo } from 'react';
import { FaPhp } from 'react-icons/fa';

import concatClassNames from '../../../../utils/class-names';
import IconSize from '../common/icon-sizes';
import Icon, { IIconProps } from '../Icon';

import styles from './StrategyIcon.module.scss';

type IPhpIconProps = IIconProps

const PhpIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IPhpIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      size={size}
      helperText={helperText}
      Component={FaPhp}
    />
;

export default memo(PhpIcon);
