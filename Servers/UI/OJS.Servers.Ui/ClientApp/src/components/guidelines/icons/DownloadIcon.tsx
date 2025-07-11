import React, { memo } from 'react';
import { GoDesktopDownload } from 'react-icons/go';

import IconSize from './common/icon-sizes';
import Icon, { IIconProps } from './Icon';

type IDownloadIconProps = IIconProps

const DownloadIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}:IDownloadIconProps) => 
    <Icon
      className={className}
      size={size}
      helperText={helperText}
      Component={GoDesktopDownload}
    />
;

export default memo(DownloadIcon);
