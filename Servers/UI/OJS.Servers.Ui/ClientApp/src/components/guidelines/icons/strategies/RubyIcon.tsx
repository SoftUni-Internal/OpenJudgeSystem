import React, { memo } from 'react';
import { DiRuby } from 'react-icons/di';

import concatClassNames from '../../../../utils/class-names';
import IconSize from '../common/icon-sizes';
import Icon, { IIconProps } from '../Icon';

import styles from './StrategyIcon.module.scss';

type IRubyIconProps = IIconProps

const RubyIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IRubyIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      size={size}
      helperText={helperText}
      Component={DiRuby}
    />
;

export default memo(RubyIcon);
