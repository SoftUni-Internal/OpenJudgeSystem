import React, { memo } from 'react';
import { FaHtml5 } from 'react-icons/fa';

import concatClassNames from '../../../../utils/class-names';
import IconSize from '../common/icon-sizes';
import Icon, { IIconProps } from '../Icon';

import styles from './StrategyIcon.module.scss';

type IHtmlCssIconProps = IIconProps

const HtmlCssIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IHtmlCssIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      size={size}
      helperText={helperText}
      Component={FaHtml5}
    />
;

export default memo(HtmlCssIcon);
