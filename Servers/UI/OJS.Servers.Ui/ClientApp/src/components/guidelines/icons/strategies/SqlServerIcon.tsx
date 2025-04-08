import React, { memo } from 'react';
import { SiMicrosoftsqlserver } from 'react-icons/si';

import concatClassNames from '../../../../utils/class-names';
import IconSize from '../common/icon-sizes';
import Icon, { IIconProps } from '../Icon';

import styles from './StrategyIcon.module.scss';

type ISqlServerIconProps = IIconProps

const SqlServerIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: ISqlServerIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      size={size}
      helperText={helperText}
      Component={SiMicrosoftsqlserver}
    />
;

export default memo(SqlServerIcon);
