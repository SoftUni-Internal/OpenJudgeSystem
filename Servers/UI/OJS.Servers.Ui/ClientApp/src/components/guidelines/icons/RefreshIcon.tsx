import React, { memo } from 'react';
import { MdOutlineRefresh } from 'react-icons/md';

import IconSize from './common/icon-sizes';
import Icon, { IIconProps } from './Icon';

type IRefreshIconProps = IIconProps

const RefreshIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}:IRefreshIconProps) => 
    <Icon
      className={className}
      size={size}
      helperText={helperText}
      Component={MdOutlineRefresh}
    />
;

export default memo(RefreshIcon);
