import React, { memo } from 'react';
import { TbBrandPython } from 'react-icons/tb';

import concatClassNames from '../../../../utils/class-names';
import IconSize from '../common/icon-sizes';
import Icon, { IIconProps } from '../Icon';

import styles from './StrategyIcon.module.scss';

type IPythonIconProps = IIconProps

const PythonIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IPythonIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      size={size}
      helperText={helperText}
      Component={TbBrandPython}
    />
;

export default memo(PythonIcon);
