import React, { memo } from 'react';
import { TbBrandCSharp } from 'react-icons/tb';

import concatClassNames from '../../../../utils/class-names';
import IconSize from '../common/icon-sizes';
import Icon, { IIconProps } from '../Icon';

import styles from './StrategyIcon.module.scss';

type IDotNetIconProps = IIconProps

const DotNetIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IDotNetIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      size={size}
      helperText={helperText}
      Component={TbBrandCSharp}
    />
;

export default memo(DotNetIcon);
