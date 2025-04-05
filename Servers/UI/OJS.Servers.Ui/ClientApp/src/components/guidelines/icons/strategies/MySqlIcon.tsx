import React, { memo } from 'react';
import { GrMysql } from 'react-icons/gr';

import concatClassNames from '../../../../utils/class-names';
import IconSize from '../common/icon-sizes';
import Icon, { IIconProps } from '../Icon';

import styles from './StrategyIcon.module.scss';

type IMySqlIconProps = IIconProps

const MySqlIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IMySqlIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      size={size}
      helperText={helperText}
      Component={GrMysql}
    />
;

export default memo(MySqlIcon);
