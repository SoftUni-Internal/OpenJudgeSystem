import React, { memo } from 'react';
import { AiOutlineFileText } from 'react-icons/ai';

import concatClassNames from '../../../../utils/class-names';
import IconSize from '../common/icon-sizes';
import Icon, { IIconProps } from '../Icon';

import styles from './StrategyIcon.module.scss';

type IPlainTextIconProps = IIconProps

const PlainTextIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IPlainTextIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      size={size}
      helperText={helperText}
      Component={AiOutlineFileText}
    />
;

export default memo(PlainTextIcon);
